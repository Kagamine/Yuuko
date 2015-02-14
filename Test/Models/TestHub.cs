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
            TestContext db = new TestContext();
            UsersSource = db.Users;
        }

        [Binding("UsersSource")]
        [OrderBy("ID desc")]
        [Skip("$vskip")]
        [Paging(10)]
        public List<UserViewModel> Users { get; set; }

        [OrderBy("ID asc")]
        [Skip("$skip")]
        public DbSet<User> UsersSource { get; set; }
    }
}