using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Helpers;

namespace PQT.Domain.Concrete
{
    public class InMemLoginTracker : EFMembershipService, ILoginTracker
    {
        public static readonly List<InMemLoginRecord> LoginRecords = new List<InMemLoginRecord>();

        public InMemLoginTracker(DbContext db)
            : base(db)
        {
        }

        #region ILoginTracker Members

        public virtual void SignIn(string email, string hashKey)
        {
            if (email == null)
                throw new ArgumentNullException("email");

            if (CanSignIn(email))
                SignIn(GetUserByEmail(email), hashKey);
        }

        public User RetrieveUser(string hashKey)
        {
            InMemLoginRecord record = LoginRecords.FirstOrDefault(r => r.SessionID == hashKey);
            return record != null ? record.User : null;
        }

        public User ReloadUser(string oldEmail, User info)
        {
            if (string.IsNullOrEmpty(oldEmail))
                throw new ArgumentNullException(oldEmail);

            foreach (InMemLoginRecord record in LoginRecords)
            {
                if (record.User.Email == oldEmail)
                {
                    record.User = info;
                }
            }
            return info;
        }

        /// <summary>
        ///     Sign out a specific user
        /// </summary>
        /// <param name="username">Is username or email</param>
        /// <param name="hashKey">Session ID</param>
        public void SignOut(string username, string hashKey = null)
        {
            if (username == null)
                throw new ArgumentNullException("username");

            if (hashKey != null)
            {
                LoginRecords.RemoveAll(r => r.User != null &&
                                            (r.User.Email.ToLower() == username.ToLower()) &&
                                            r.SessionID == hashKey);
            }
            else
            {
                LoginRecords.RemoveAll(r => r.User != null &&
                                            (r.User.Email.ToLower() == username.ToLower()));
            }
        }

        public bool IsLogged(string username)
        {
            return LoginRecords.Any(r => r.User.Email.ToLower() == username.ToLower());
        }

        #endregion

        protected void SignIn(User user, string hashKey)
        {
            InMemLoginRecord existingRecord = LoginRecords.FirstOrDefault(r => r.SessionID == hashKey);
            if (existingRecord != null)
            {
                existingRecord.User = user;
            }
            else
                LoginRecords.Add(new InMemLoginRecord(user, hashKey));
        }

        protected virtual bool CanSignIn(string username)
        {
            return true;
        }

        public override User GetUser(int id)
        {
            InMemLoginRecord loginRecord =
                LoginRecords.FirstOrDefault(
                    r => r.User.ID == id);
            if (loginRecord != null)
                return loginRecord.User;
            return base.GetUser(id);
        }
        public override bool UpdateUser(User userInfo)
        {
            var loginRecords =
                LoginRecords.Where(
                    r => r.User.ID == userInfo.ID).ToList();
            foreach (var inMemLoginRecord in loginRecords)
            {
                inMemLoginRecord.User = userInfo;
                return base.UpdateUser(userInfo);
            }
            return true;
        }
    }

    public class InMemLoginRecord
    {
        public InMemLoginRecord(User user, string sessionId)
        {
            User = user;
            SessionID = sessionId;
        }

        public User User { get; set; }
        public string SessionID { get; set; }
    }
}
