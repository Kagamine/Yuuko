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
        public List<User> Users { get; set; }

        [Binding("UsersSource")]
        [DetailPort(DetailPortFunction.Edit ,DetailPortFunction.Delete, DetailPortFunction.Insert)]
        public User User { get; set; }

        [OrderBy("ID")]
        [Paging(10)]
        public DbSet<User> UsersSource { get; set; }
    }
}