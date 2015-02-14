using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class TmpModel
    {
        public string Identity { get; set; }
    }

    public class UserViewModel
    {
        public UserViewModel() { }

        public UserViewModel(User Model)
        {
            ID = Model.ID;
            Username = Model.Username;
        }

        public UserViewModel(dynamic Model)
        {
            ID = Convert.ToInt32(Model.Identity);
            Username = "Null"; 
        }

        public int ID { get; set; }

        public string Username { get; set; }
    }
}