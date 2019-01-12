using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCinema.Security
{
    public static class SessionPersister
    {
        static string usernameSessionVar = "username";
        static string adminSessionVar = "admin";

        public static string Username
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    return string.Empty;
                }

                var sessionVar = HttpContext.Current.Session[usernameSessionVar];
                if(sessionVar != null)
                {
                    return sessionVar as string;
                }

                return null;
            }
            set
            {
                HttpContext.Current.Session[usernameSessionVar] = value;
            }
        }

        public static string Admin
        {
            get
            {
                if(HttpContext.Current == null)
                {
                    return string.Empty;
                }

                var sessionVar = HttpContext.Current.Session[adminSessionVar];
                if(sessionVar != null)
                {
                    return sessionVar as string;
                }

                return null;
            }

            set
            {
                HttpContext.Current.Session[adminSessionVar] = value;
            }
        }

    }
}