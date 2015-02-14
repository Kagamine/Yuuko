using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using CodeComb.Yuuko;
using CodeComb.Yuuko.Schema;

namespace Test.Models
{
    public class TestYuukoContext : YuukoContext
    {
        public TestYuukoContext()
        {
            DB = new TestContext();
            UsersSource = DB.Users;
        }

        [DbContext]
        public TestContext DB { get; set; }

        [Binding("UsersSource")]
        [CollectionPort]
        public List<UserViewModel> Users { get; set; }

        [Binding("UsersSource")]
        [DetailPort(DetailPortFunction.Edit ,DetailPortFunction.Delete, DetailPortFunction.Insert)]
        public UserViewModel User { get; set; }

        public DbSet<User> UsersSource { get; set; }
    }
}