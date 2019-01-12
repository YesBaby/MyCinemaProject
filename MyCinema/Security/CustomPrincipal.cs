using MyCinema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace MyCinema.Security
{
    public class CustomPrincipal : IPrincipal
    {
        private User user;

        public CustomPrincipal(User user)
        {
            this.user = user;
            this.Identity = new GenericIdentity(user.name);
        }
        public IIdentity Identity
        {
            get;
            set;
        }

        public bool IsInRole(string role)
        {
            string userRoles = user.is_admin ? "Admin, User" : "User";

            var roles = role.Split(new char[] { ',' });

            return roles.Any(r => userRoles.Contains(r));
        }
    }
}