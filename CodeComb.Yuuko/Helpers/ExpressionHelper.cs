using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Yuuko.Helpers
{
    public static class ExpressionHelper
    {
        private const string LegalChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890_";
        private class NameValueItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public static string ReplaceQueryString(NameValueCollection QueryString, string Expression, Type[] Types, Type DefaultType, ref object[] Values)
        {
            var Pos = 0;
            var Dic = new Dictionary<string, int>();
            var reading = false;
            string name = "";
            for (var i = 0; i < Expression.Length - 1;i++)
            {
                if (reading)
                {
                    if (LegalChars.Contains(Expression[i]))
                    {
                        name += Expression[i];
                    }
                    if (!LegalChars.Contains(Expression[i + 1]))
                    {
                        reading = false;
                        if (!Dic.ContainsKey(name))
                        {
                            Dic.Add(name, Pos++);
                        }
                    }
                }
                else if (Expression[i] == '$')
                {
                    reading = true;
                    name = "";
                    continue;
                }
            }

            //单独建立一个NV类是为了防止比如先匹配了$user而导致无法匹配$username的问题，虽然这种情况可能很少发生
            var tmp = new List<NameValueItem>();
            for (var i = 0; i < QueryString.Count; i++)
            {
                tmp.Add(new NameValueItem
                {
                    Name = QueryString.GetKey(i),
                    Value = QueryString.GetValues(i).Single()
                });
            }
            tmp = tmp.OrderByDescending(x => x.Name).ToList();
            foreach (var t in tmp)
            {
                if (Expression.IndexOf("$" + t.Name) >= 0)
                {
                    int pos;
                    Dic.TryGetValue("$" + t.Name, out pos);
                    Expression = Expression.Replace("$" + t.Name, "@" + pos);
                    var lst = Values == null ? new List<object>() : Values.ToList();
                    Type typeofpos;
                    if (pos >= Types.Length)
                    {
                        typeofpos = DefaultType;
                    }
                    else
                    {
                        typeofpos = Types[pos];
                    }
                    lst.Add(Convert.ChangeType(t.Value, typeofpos));
                    Values = lst.ToArray();
                }
            }
            return Expression.Replace("'", "\"");
        }
    }
}
