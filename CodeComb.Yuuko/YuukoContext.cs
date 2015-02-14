using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using CodeComb.Yuuko.Schema;

namespace CodeComb.Yuuko
{
    public abstract class YuukoContext
    {
        public static void Register()
        {
            //获取注册者程序集中Yuuko上下文类
            var classes = (from t in Assembly.GetCallingAssembly().GetTypes()
                                                   where t.IsClass && t.BaseType == typeof(YuukoContext)
                                                   select t).Single();

            //获取上下文类中的所有属性
            var properties = from p in classes.GetProperties()
                             select p;

            //遍历出口成员
            foreach (var p in properties.Where(x=> x.GetCustomAttribute<BindingAttribute>() != null))
            {
                //处理集合
                if (p.GetType().IsGenericType)
                {
                    var BindingAttribute = p.GetCustomAttribute<BindingAttribute>(); //获取BindingAttribute特性
                    var DataSource = properties.Where(x => x.Name == BindingAttribute._DataSource);//获取数据源
                }

            }
        }
    }
}
