using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CodeComb.Yuuko
{
    public static class Yuuko
    {
        public static IEnumerable<PropertyInfo> Properties { get; set; }
        public static void Register()
        {
            //获取注册者程序集中Yuuko上下文类
            var classes = (from t in Assembly.GetCallingAssembly().GetTypes()
                           where t.IsClass && t.BaseType == typeof(YuukoContext)
                           select t).Single();

            //获取上下文类中的所有属性
            var properties = from p in classes.GetProperties()
                             select p;
            Properties = properties;
        }
    }
}
