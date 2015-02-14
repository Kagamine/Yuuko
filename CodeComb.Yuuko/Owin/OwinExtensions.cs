using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CodeComb.Yuuko;
using CodeComb.Yuuko.Schema;
using CodeComb.Yuuko.Helpers;

namespace Owin
{
    public static class OwinExtensions
    {
        public static IEnumerable<PropertyInfo> Properties;
        public static IAppBuilder MapYuuko(this IAppBuilder builder)
        {
            return builder.Map("/yuuko", app => {
                #region 获取Yuuko上下文
                //获取程序集
                var assemblies = (from t in AppDomain.CurrentDomain.GetAssemblies()
                                  select t);
                Type ContextType = null;
                foreach (var asm in assemblies)
                {
                    var classes = (from t in asm.GetTypes()
                                   where t.IsClass && t.BaseType == typeof(YuukoContext)
                                   select t).SingleOrDefault();
                    if (classes != null)
                        ContextType = classes;
                }
                var Instance = Activator.CreateInstance(ContextType);
                Properties = Instance.GetType().GetProperties();
                #endregion

                foreach (var p in Properties) //遍历上下文属性
                {
                    app.Map("/" + p.Name, innerApp =>
                    {
                        innerApp.Run(cxt => {
                            var BindingAttribute = p.GetCustomAttribute<BindingAttribute>(); //获取数据源绑定特性
                            if (BindingAttribute != null)
                            {
                                #region 分析Yuuko上下文
                                //获取数据源
                                var DataSourceType = (from ds in Properties
                                                      where ds.Name == BindingAttribute._DataSource
                                                      select ds).Single();
                                var DataSource = DataSourceType.GetValue(Instance);
                                var DataModel = DataSource.GetType().GetGenericArguments().Single();
                                string Json = "";
                                var Attributes = p.GetCustomAttributes(); //获取该Port的所有特性
                                var AccessToAttribute = (from a in Attributes
                                                         where a.GetType().BaseType == typeof(AccessToAttribute)
                                                         select a).SingleOrDefault(); //获取AccessTo特性
                                if (AccessToAttribute != null)//处理权限校验逻辑
                                {
                                    Type AccessToAttributeType = typeof(AccessToAttribute);
                                    var flag = (bool)AccessToAttributeType.GetMethod("AccessCore").Invoke(AccessToAttribute, new object[] { cxt.Authentication.User }); //调用AccessCore校验
                                    if (!flag)
                                    {
                                        cxt.Response.ContentType = "text/json";
                                        return cxt.Response.WriteAsync("null");
                                    }
                                }
                                #endregion
                                #region 处理集合
                                if (p.PropertyType.IsGenericType) //处理集合型逻辑
                                {
                                    #region 准备数据
                                    var ViewModelType = p.PropertyType.GetGenericArguments().Single();
                                    var fields = from f in DataModel.GetProperties()
                                                 select f;//获取数据模型字段
                                    var tmp = ((IEnumerable)DataSource);
                                    NameValueCollection QueryString = new NameValueCollection(); //创建容器用以存储QueryString
                                    if (cxt.Request.QueryString.HasValue)
                                    {
                                        QueryString = HttpUtility.ParseQueryString(cxt.Request.QueryString.Value);
                                    }
                                    #endregion
                                    #region Where与WhereOptional特性处理
                                    foreach (var f in fields.Where(x=>x.GetCustomAttribute<WhereAttribute>() != null)) //处理Where逻辑
                                    {
                                        var WhereAttribute = f.GetCustomAttribute<WhereAttribute>(); //获取Where特性
                                        var expression = ExpressionHelper.ReplaceQueryString(QueryString, WhereAttribute.predicate, WhereAttribute.types, f.PropertyType, ref WhereAttribute.values); //获取表达式
                                        tmp = tmp.Where(expression, WhereAttribute.values);
                                    }
                                    foreach (var f in fields.Where(x => x.GetCustomAttribute<WhereOptionalAttribute>() != null)) //处理WhereOptional逻辑
                                    {
                                        var WhereOptionalAttribute = f.GetCustomAttribute<WhereOptionalAttribute>(); //获取WhereOptional特性
                                        var expression = ExpressionHelper.ReplaceQueryString(QueryString, WhereOptionalAttribute.predicate, WhereOptionalAttribute.types, f.PropertyType, ref WhereOptionalAttribute.values); //获取表达式
                                        if (expression.IndexOf("$") < 0)
                                            tmp = tmp.Where(expression, WhereOptionalAttribute.values);
                                    }
                                    #endregion
                                    #region OrderBy特性处理
                                    //处理针对数据源集合的OrderBy
                                    var DataSourceOrderByAttribute = DataSourceType.GetCustomAttribute<OrderByAttribute>();
                                    if (DataSourceOrderByAttribute != null)
                                    {
                                        tmp = tmp.OrderBy(DataSourceOrderByAttribute.ordering);
                                    }
                                    #endregion
                                    #region Skip特性处理
                                    var DataSourceSkipAttribute = DataSourceType.GetCustomAttribute<SkipAttribute>();
                                    if (DataSourceSkipAttribute != null)
                                    {
                                        if (DataSourceSkipAttribute.requestKey == null || QueryString[DataSourceSkipAttribute.requestKey.Trim('$')] == null)
                                        {
                                            if (DataSourceSkipAttribute.defaultSkipCount < 0)
                                            {
                                                throw new ArgumentNullException(QueryString[DataSourceSkipAttribute.requestKey.Trim('$')]);
                                            }
                                            else
                                            {
                                                tmp = tmp.Skip(DataSourceSkipAttribute.defaultSkipCount);
                                            }
                                        }
                                        else
                                        {
                                            tmp = tmp.Skip(int.Parse(QueryString[DataSourceSkipAttribute.requestKey.Trim('$')]));
                                        }
                                    }
                                    #endregion
                                    #region Take特性处理
                                    var DataSourceTakeAttribute = DataSourceType.GetCustomAttribute<TakeAttribute>();
                                    if (DataSourceTakeAttribute != null)
                                    {
                                        if (DataSourceTakeAttribute.requestKey == null || QueryString[DataSourceTakeAttribute.requestKey.Trim('$')] == null)
                                        {
                                            if (DataSourceSkipAttribute.defaultSkipCount < 0)
                                            {
                                                throw new ArgumentNullException(QueryString[DataSourceTakeAttribute.requestKey.Trim('$')]);
                                            }
                                            else
                                            {
                                                tmp = tmp.Take(DataSourceTakeAttribute.defaultTakeCount);
                                            }
                                        }
                                        else
                                        {
                                            tmp = tmp.Take(int.Parse(QueryString[DataSourceTakeAttribute.requestKey.Trim('$')]));
                                        }
                                    }
                                    #endregion
                                    #region Select特性处理
                                    var DataSourceSelectAttribute = DataSourceType.GetCustomAttribute<SelectAttribute>();
                                    if (DataSourceSelectAttribute != null)
                                    {
                                        tmp = tmp.Select(DataSourceSelectAttribute.selector, DataSourceSelectAttribute.values);
                                    }
                                    #endregion
                                    #region 输出JSON
                                    tmp = ((IEnumerable<object>)tmp).ToList();
                                    //考虑是否需要转换为视图模型
                                    var ret = (IEnumerable)Activator.CreateInstance((typeof(List<>).MakeGenericType(ViewModelType))); //使用反射创建泛型容器
                                    if (ViewModelType != DataModel)
                                    {
                                        #region 将数据源数据转换为视图模型集合
                                        foreach (var o in (IEnumerable)tmp)
                                            ((IList)ret).Add(Activator.CreateInstance(ViewModelType, o));
                                        #endregion
                                        #region 处理针对视图模型集合的OrderBy
                                        var ViewsCollectionOrderByAttribute = p.GetCustomAttribute<OrderByAttribute>();
                                        if (ViewsCollectionOrderByAttribute != null)
                                        {
                                            ret = ret.OrderBy(ViewsCollectionOrderByAttribute.ordering, ViewsCollectionOrderByAttribute.values);
                                        }
                                        #endregion
                                        #region 处理针对视图模型集合的Skip
                                        var ViewsCollectionSkipAttribute = p.GetCustomAttribute<SkipAttribute>();
                                        if (ViewsCollectionSkipAttribute != null)
                                        {
                                            if (ViewsCollectionSkipAttribute.requestKey == null || QueryString[ViewsCollectionSkipAttribute.requestKey.Trim('$')] == null)
                                            {
                                                if (ViewsCollectionSkipAttribute.defaultSkipCount < 0)
                                                {
                                                    throw new ArgumentNullException(QueryString[ViewsCollectionSkipAttribute.requestKey.Trim('$')]);
                                                }
                                                else
                                                {
                                                    ret = ret.Skip(ViewsCollectionSkipAttribute.defaultSkipCount);
                                                }
                                            }
                                            else
                                            {
                                                ret = ret.Skip(int.Parse(QueryString[ViewsCollectionSkipAttribute.requestKey.Trim('$')]));
                                            }
                                        }
                                        #endregion
                                        #region 处理针对视图模型集合的Take
                                        var ViewsCollectionTakeAttribute = p.GetCustomAttribute<TakeAttribute>();
                                        if (ViewsCollectionTakeAttribute != null)
                                        {
                                            if (ViewsCollectionTakeAttribute.requestKey == null || QueryString[ViewsCollectionTakeAttribute.requestKey.Trim('$')] == null)
                                            {
                                                if (ViewsCollectionTakeAttribute.defaultTakeCount < 0)
                                                {
                                                    throw new ArgumentNullException(QueryString[ViewsCollectionTakeAttribute.requestKey.Trim('$')]);
                                                }
                                                else
                                                {
                                                    ret = ret.Take(ViewsCollectionTakeAttribute.defaultTakeCount);
                                                }
                                            }
                                            else
                                            {
                                                ret = ret.Take(int.Parse(QueryString[ViewsCollectionTakeAttribute.requestKey.Trim('$')]));
                                            }
                                        }
                                        #endregion
                                        #region 处理针对视图模型集合的Select
                                        var ViewsCollectionSelectAttribute = p.GetCustomAttribute<SelectAttribute>();
                                        if (ViewsCollectionTakeAttribute != null)
                                        {
                                            ret = ret.Select(DataSourceSelectAttribute.selector, DataSourceSelectAttribute.values);
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        ret = tmp;
                                    }
                                    Json = JsonConvert.SerializeObject(ret);
                                    cxt.Response.ContentType = "text/json";
                                    return cxt.Response.WriteAsync(Json);
                                    #endregion
                                }
                                #endregion
                            }
                            return Task.FromResult(0);
                        });
                    });
                }
            });
        }
    }
}
