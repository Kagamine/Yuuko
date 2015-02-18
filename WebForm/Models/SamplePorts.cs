using System.Collections.Generic;
using CodeComb.Yuuko;
using CodeComb.Yuuko.Schema;

namespace WebFrom.Models
{
    public class SamplePorts : PortsContext
    {
        public SamplePorts()
        {
            UserSource = new List<User>();
            UserSource.Add(new User { Name = "Amamiya Yuuko", Role = Role.Root });
            UserSource.Add(new User { Name = "Gasai Yuno", Role = Role.Manager });
            UserSource.Add(new User { Name = "Kasugano Sora", Role = Role.Manager });
            UserSource.Add(new User { Name = "Hatsune Miku", Role = Role.Member });
        }

        [GroupBy("it.Role", "it")]
        public List<User> UserSource { get; set; } // Data Source

        [Binding("UserSource")]
        [Select("new (Key.ToString() as Role, Count() as Count)")]
        public List<object> UserStatisticsPort { get; set; } //Collection Port

        [Binding("UserSource")]
        public List<User> UserPort { get; set; } //Collection Port
    }
}