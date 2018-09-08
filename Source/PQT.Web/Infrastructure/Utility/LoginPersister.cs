﻿using System;
using System.Web;
using System.Web.Security;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Web.Infrastructure.Utility
{
    public class LoginPersister
    {
        #region Repository

        private static ILoginTracker LoginTracker
        {
            get { return DependencyHelper.GetService<ILoginTracker>(); }
        }

        #endregion
        public static void SignIn(string email, bool persistent = false)
        {
            LoginTracker.SignIn(email, HttpContext.Current.Session.SessionID);
            //Log.Debug(LoginTracker.IsLogged("admin@localhost"),"is login");
            FormsAuthentication.SetAuthCookie(email, persistent);
        }

        public static void SignOut()
        {
            if (HttpContext.Current != null)
            {
                LoginTracker.SignOut(HttpContext.Current.User.Identity.Name, HttpContext.Current.Session.SessionID);
                FormsAuthentication.SignOut();
            }
        }

        public static User RetrieveUser()
        {
            User user = null;
            try
            {
                //Log.Debug("Debug");
                user = LoginTracker.RetrieveUser(HttpContext.Current.Session.SessionID);
                //Log.Debug("Debug 2");
                // login if cookie exists
                if (user == null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    LoginTracker.SignIn(HttpContext.Current.User.Identity.Name, HttpContext.Current.Session.SessionID);
                    //Log.Debug("Login by email");
                    user = LoginTracker.RetrieveUser(HttpContext.Current.Session.SessionID);
                    //Log.Debug("Retrieve User");
                }
                else
                {

                }          //Log.Debug(user==null,"User null");

            }
            catch (Exception e)
            {
            }
            return user;
        }

        public static bool IsLogged(string username)
        {
            return LoginTracker.IsLogged(username);
        }
    }
}
