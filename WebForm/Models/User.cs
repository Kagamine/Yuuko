using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebFrom.Models
{
    public enum Role
    {
        Member,
        Manager,
        Root
    }

    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Role Role { get; set; }
    }
}