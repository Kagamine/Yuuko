using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodeComb.Yuuko.Schema;

namespace Test.Models
{
    public class User
    {
        [WhereOptional("ID = $id")]
        public int ID { get; set; }

        [SingleBy]
        [WhereOptional("Username = $name")]
        public string Username { get; set; }

        public string Password { get; set; }

    }
}