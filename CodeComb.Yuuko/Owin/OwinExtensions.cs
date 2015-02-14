using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Owin.Extensions;
using CodeComb.Yuuko;
using CodeComb.Yuuko.Schema;
using CodeComb.Yuuko.Helpers;
using CodeComb.Yuuko.Exceptions;

namespace Owin
{
    public static class OwinExtensions
    {
        public static IEnumerable<PropertyInfo> Properties;
        public static IAppBuilder MapYuuko(this IAppBuilder builder)
        {
            builder.RequireAspNetSession();
            return builder.Map("/yuuko/gets", app => {
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
                #region GET型逻辑处理
                foreach (var p in Properties) //遍历上下文属性
                {
                    var BindingAttribute = p.GetCustomAttribute<BindingAttribute>(); //获取数据源绑定特性
                    if (BindingAttribute != null)
                    {
                        app.Map("/" + p.Name, innerApp =>
                        {
                            innerApp.Run(cxt => {
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
                                NameValueCollection QueryString = new NameValueCollection(); //创建容器用以存储QueryString
                                if (cxt.Request.QueryString.HasValue)
                                {
                                    QueryString = HttpUtility.ParseQueryString(cxt.Request.QueryString.Value);
                                }
                                var tmp = ((IEnumerable)DataSource);
                                #endregion
                                #region 处理CollectionPort
                                if (p.GetCustomAttribute<CollectionPortAttribute>() != null) //处理集合型逻辑
                                {
                                    #region 准备数据
                                    var ViewModelType = p.PropertyType.GetGenericArguments().Single();
                                    var fields = from f in DataModel.GetProperties()
                                                 select f;//获取数据模型字段
                                    #endregion
                                    #region Where与WhereOptional特性处理
                                    foreach (var f in fields.Where(x => x.GetCustomAttribute<WhereAttribute>() != null)) //处理Where逻辑
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
                                    #region GroupBy特性处理
                                    var DataSourceGroupByAttribute = DataSourceType.GetCustomAttribute<GroupByAttribute>();
                                    if (DataSourceGroupByAttribute != null)
                                    {
                                        tmp = tmp.GroupBy(DataSourceGroupByAttribute.keySelector, DataSourceGroupByAttribute.elementSelector, DataSourceGroupByAttribute.values);
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
                                    #region Select特性处理
                                    var DataSourceSelectAttribute = DataSourceType.GetCustomAttribute<SelectAttribute>();
                                    if (DataSourceSelectAttribute != null)
                                    {
                                        tmp = tmp.Select(DataSourceSelectAttribute.selector, DataSourceSelectAttribute.values);
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
                                    #region Paging特性处理
                                    var DataSourcePagingAttribute = DataSourceType.GetCustomAttribute<PagingAttribute>();
                                    if (DataSourcePagingAttribute != null)
                                    {
                                        var Page = int.Parse(QueryString[DataSourcePagingAttribute.requestKey]);
                                        tmp = tmp.Skip(Page).Take(DataSourcePagingAttribute.pageSize);
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
                                        #region 处理针对视图模型集合的GroupBy
                                        var ViewsCollectionGroupByAttribute = p.GetCustomAttribute<GroupByAttribute>();
                                        if (ViewsCollectionGroupByAttribute != null)
                                        {
                                            ret = ret.Select(ViewsCollectionGroupByAttribute.keySelector, ViewsCollectionGroupByAttribute.elementSelector, ViewsCollectionGroupByAttribute.values);
                                        }
                                        #endregion
                                        #region 处理针对视图模型集合的OrderBy
                                        var ViewsCollectionOrderByAttribute = p.GetCustomAttribute<OrderByAttribute>();
                                        if (ViewsCollectionOrderByAttribute != null)
                                        {
                                            ret = ret.OrderBy(ViewsCollectionOrderByAttribute.ordering, ViewsCollectionOrderByAttribute.values);
                                        }
                                        #endregion
                                        #region 处理针对视图模型集合的Select
                                        var ViewsCollectionSelectAttribute = p.GetCustomAttribute<SelectAttribute>();
                                        if (ViewsCollectionSelectAttribute != null)
                                        {
                                            ret = ret.Select(ViewsCollectionSelectAttribute.selector, ViewsCollectionSelectAttribute.values);
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
                                        #region 处理针对视图模型集合的Paging
                                        var ViewsCollectionPaggingAttribute = DataSourceType.GetCustomAttribute<PagingAttribute>();
                                        if (ViewsCollectionPaggingAttribute != null)
                                        {
                                            var Page = int.Parse(QueryString[ViewsCollectionPaggingAttribute.requestKey]);
                                            tmp = tmp.Skip(Page).Take(ViewsCollectionPaggingAttribute.pageSize);
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
                                #region 处理DetailPort
                                else if (p.GetCustomAttribute<DetailPortAttribute>() != null) //处理DetailPort
                                {
                                    #region 准备数据
                                    var DetailPortAttribute = p.GetCustomAttribute<DetailPortAttribute>();
                                    var ViewModelType = p.PropertyType;
                                    var fields = from f in DataModel.GetProperties()
                                                 select f;//获取数据模型字段
                                    var KeyField = (from f in fields
                                                    where f.GetCustomAttribute<SingleByAttribute>() != null
                                                    select f).Single();
                                    #endregion
                                    #region 处理SingleBy特性
                                    var SingleByAttribute = KeyField.GetCustomAttribute<SingleByAttribute>();
                                    string requestKey = QueryString[SingleByAttribute.requestKey.Trim('$')];
                                    tmp = tmp.Where(KeyField.Name + " =@0", Convert.ChangeType(requestKey, KeyField.PropertyType));
                                    var tmp2 = ((IEnumerable<dynamic>)tmp).SingleOrDefault();
                                    #endregion
                                    #region 输出JSON
                                    //考虑是否需要把数据源单体转换成视图模型类型
                                    dynamic ret;
                                    if (ViewModelType != DataModel)
                                    {
                                        ret = Activator.CreateInstance(ViewModelType, tmp2);
                                    }
                                    else
                                    {
                                        ret = tmp2;
                                    }
                                    if (tmp2 == null)
                                    {
                                        ret = null;
                                    }
                                    Json = JsonConvert.SerializeObject(ret);
                                    cxt.Response.ContentType = "text/json";
                                    return cxt.Response.WriteAsync(Json);
                                    #endregion
                                }
                                #endregion
                                return Task.FromResult(0);
                            });
                        });
                    }
                }
                #endregion
                #region SET型逻辑处理
                foreach (var p in Properties.Where(x => x.GetCustomAttribute<DetailPortAttribute>() != null))
                {
                    #region 分析Yuuko上下文
                    var BindingAttribute = p.GetCustomAttribute<BindingAttribute>(); //获取数据源绑定特性
                    //获取数据源
                    var DataSourceType = (from ds in Properties
                                          where ds.Name == BindingAttribute._DataSource
                                          select ds).Single();
                    var DataSource = DataSourceType.GetValue(Instance);
                    var DataModel = DataSource.GetType().GetGenericArguments().Single();
                    var Attributes = p.GetCustomAttributes(); //获取该Port的所有特性
                    var AccessToAttribute = (from a in Attributes
                                             where a.GetType().BaseType == typeof(AccessToAttribute)
                                             select a).SingleOrDefault(); //获取AccessTo特性
                    #endregion
                    #region 准备数据
                    var DetailPortAttribute = p.GetCustomAttribute<DetailPortAttribute>();
                    var ViewModelType = p.PropertyType;
                    var fields = from f in DataModel.GetProperties()
                                 select f;//获取数据模型字段
                    var KeyField = (from f in fields
                                    where f.GetCustomAttribute<SingleByAttribute>() != null
                                    select f).Single();
                    #endregion
                    #region 处理删除操作
                    if (DetailPortAttribute.detailPortFunctions.Contains(DetailPortFunction.Delete))
                    {
                        builder.Map("/yuuko/sets/" + p.Name+ "/Delete", innerApp=>
                        {
                            innerApp.Run(cxt => {
                                #region 处理请求
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
                                var tmp = ((IEnumerable)DataSource);
                                var frm = cxt.Request.ReadFormAsync();
                                frm.Wait();
                                var Form = frm.Result;
                                var YuukoToken = Form.Get("YuukoToken");
                                if (HttpContext.Current.Session["YuukoToken"] == null || YuukoToken != HttpContext.Current.Session["YuukoToken"].ToString())
                                    throw new YuukoTokenException();
                                #endregion
                                #region 处理SingleBy特性
                                var SingleByAttribute = KeyField.GetCustomAttribute<SingleByAttribute>();
                                string requestKey = Form.Get(SingleByAttribute.requestKey.Trim('$'));
                                tmp = tmp.Where(KeyField.Name + " =@0", Convert.ChangeType(requestKey, KeyField.PropertyType));
                                var tmp2 = ((IEnumerable<dynamic>)tmp).SingleOrDefault();
                                if (tmp2 == null)
                                {
                                    cxt.Response.ContentType = "text/json";
                                    
                                    return cxt.Response.WriteAsync("false");
                                }
                                #endregion
                                #region 删除操作
                                DataSource.GetType().GetMethod("Remove").Invoke(DataSource, new object[] { tmp2 });
                                var DbContext = (from pro in Properties
                                                 where pro.GetCustomAttribute<DbContextAttribute>() != null
                                                 select pro).SingleOrDefault();
                                if (DbContext != null)
                                {
                                    cxt.Response.ContentType = "text/json";
                                    try
                                    {
                                        DbContext.PropertyType.GetMethod("SaveChanges").Invoke(DbContext.GetValue(Instance), null);
                                    }
                                    catch
                                    {
                                        return cxt.Response.WriteAsync("false");
                                    }
                                    return cxt.Response.WriteAsync("true");
                                }
                                #endregion
                                return Task.FromResult(0);
                            });
                        });
                    }
                    #endregion
                    #region 处理添加操作
                    if (DetailPortAttribute.detailPortFunctions.Contains(DetailPortFunction.Insert))
                    {
                        builder.Map("/yuuko/sets/" + p.Name + "/Insert", innerApp =>
                        {
                            innerApp.Run(cxt => {
                                #region 处理请求
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
                                var tmp = ((IEnumerable)DataSource);
                                var frm = cxt.Request.ReadFormAsync();
                                frm.Wait();
                                var Form = frm.Result;
                                var YuukoToken = Form.Get("YuukoToken");
                                if (HttpContext.Current.Session["YuukoToken"] == null || YuukoToken != HttpContext.Current.Session["YuukoToken"].ToString())
                                    throw new YuukoTokenException();
                                #endregion
                                #region 插入操作
                                var NewItem = Activator.CreateInstance(DataModel);
                                var NewItemProperties = NewItem.GetType().GetProperties();
                                foreach (var np in NewItemProperties)
                                {
                                    if (Form.Get(np.Name) != null)
                                    {
                                        if (np.PropertyType == typeof(string))
                                        {
                                            np.SetValue(NewItem, Form.Get(np.Name).ToString());
                                        }
                                        else
                                        {
                                            np.SetValue(NewItem, Convert.ChangeType(Form.Get(np.Name).ToString(), np.PropertyType));
                                        }
                                    }
                                    else if (np.PropertyType == typeof(Guid)) //特别处理一下GUID问题
                                    {
                                        np.SetValue(NewItem, Guid.NewGuid());
                                    }
                                }
                                DataSource.GetType().GetMethod("Add").Invoke(DataSource, new object[] { NewItem });
                                var DbContext = (from pro in Properties
                                                 where pro.GetCustomAttribute<DbContextAttribute>() != null
                                                 select pro).SingleOrDefault();
                                if (DbContext != null)
                                {
                                    cxt.Response.ContentType = "text/json";
                                    try
                                    {
                                        DbContext.PropertyType.GetMethod("SaveChanges").Invoke(DbContext.GetValue(Instance), null);
                                    }
                                    catch
                                    {
                                        return cxt.Response.WriteAsync("false");
                                    }
                                    return cxt.Response.WriteAsync("true");
                                }
                                #endregion
                                return Task.FromResult(0);
                            });
                        });
                    }
                    #endregion
                    #region 处理修改操作
                    builder.Map("/yuuko/sets/" + p.Name + "/Delete", innerApp =>
                    {
                        innerApp.Run(cxt => {
                            #region 处理请求
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
                            var tmp = ((IEnumerable)DataSource);
                            var frm = cxt.Request.ReadFormAsync();
                            frm.Wait();
                            var Form = frm.Result;
                            var YuukoToken = Form.Get("YuukoToken");
                            if (HttpContext.Current.Session["YuukoToken"] == null || YuukoToken != HttpContext.Current.Session["YuukoToken"].ToString())
                                throw new YuukoTokenException();
                            #endregion
                            #region 处理SingleBy特性
                            var SingleByAttribute = KeyField.GetCustomAttribute<SingleByAttribute>();
                            string requestKey = Form.Get(SingleByAttribute.requestKey.Trim('$'));
                            tmp = tmp.Where(KeyField.Name + " =@0", Convert.ChangeType(requestKey, KeyField.PropertyType));
                            var tmp2 = ((IEnumerable<dynamic>)tmp).SingleOrDefault();
                            if (tmp2 == null)
                            {
                                cxt.Response.ContentType = "text/json";

                                return cxt.Response.WriteAsync("false");
                            }
                            #endregion
                            #region 修改操作
                            var ItemProperties = ((object)tmp2).GetType().GetProperties();
                            foreach (var ip in ItemProperties)
                            {
                                if (Form.Get(ip.Name) != null)
                                {
                                    if (ip.PropertyType == typeof(string))
                                    {
                                        ip.SetValue(tmp2, Form.Get(ip.Name).ToString());
                                    }
                                    else
                                    {
                                        ip.SetValue(tmp2, Convert.ChangeType(Form.Get(ip.Name).ToString(), ip.PropertyType));
                                    }
                                }
                            }
                            var DbContext = (from pro in Properties
                                             where pro.GetCustomAttribute<DbContextAttribute>() != null
                                             select pro).SingleOrDefault();
                            if (DbContext != null)
                            {
                                cxt.Response.ContentType = "text/json";
                                try
                                {
                                    DbContext.PropertyType.GetMethod("SaveChanges").Invoke(DbContext.GetValue(Instance), null);
                                }
                                catch
                                {
                                    return cxt.Response.WriteAsync("false");
                                }
                                return cxt.Response.WriteAsync("true");
                            }
                            #endregion
                            return Task.FromResult(0);
                        });
                    });
                    #endregion
                }
                #endregion
            });
        }
    }

    public static class AspNetSessionExtensions
    {
        public static IAppBuilder RequireAspNetSession(this IAppBuilder app)
        {
            app.Use((context, next) =>
            {
                // Depending on the handler the request gets mapped to, session might not be enabled. Force it on.
                HttpContextBase httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                httpContext.SetSessionStateBehavior(SessionStateBehavior.Required);
                return next();
            });
            // SetSessionStateBehavior must be called before AcquireState
            app.UseStageMarker(PipelineStage.MapHandler);
            return app;
        }
    }
}
