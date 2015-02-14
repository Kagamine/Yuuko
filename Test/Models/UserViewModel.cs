using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class UserViewModel
    {
        public UserViewModel() { }

        public UserViewModel(User Model)
        {
            ID = Model.ID;
            Username = Model.Username;
        }

        public int ID { get; set; }

        public string Username { get; set; }
    }
}