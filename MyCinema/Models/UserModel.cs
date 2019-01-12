using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCinema.Models
{
    public class UserModel
    {
        private List<User> usersList = new List<User>();

        public UserModel()
        {
            CinemaDBConnection db = new CinemaDBConnection();
            foreach (var user in db.User)
            {
                usersList.Add(user);
            }
            db.Dispose();
        }

        public User FindUser(string name)
        {
            return usersList.Where(usr => usr.name.Equals(name)).FirstOrDefault();
        }

        public User LoginUser(string name, string password)
        {
            return usersList.Where(usr => usr.name.Equals(name) && usr.password.Equals(password)).FirstOrDefault();
        }
    }
}