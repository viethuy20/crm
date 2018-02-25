using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime.Misc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Web.Infrastructure.Utility
{
    public class CurrentUser
    {
        #region Repository

        private static IAuthenticationService AuthenticationService
        {
            get { return DependencyResolver.Current.GetService<IAuthenticationService>(); }
        }

        private static IAuthorizationService AuthorizationService
        {
            get { return DependencyResolver.Current.GetService<IAuthorizationService>(); }
        }

        private static IUnitRepository UnitRepo
        {
            get { return DependencyHelper.GetService<IUnitRepository>(); }
        }

        #endregion

        public static bool IsAuthenticated
        {
            get
            {
                return Identity != null;
            }
        }

        public static User Identity
        {
            get
            {
                var user = LoginPersister.RetrieveUser();
                return user;
            }
        }

        public static bool HasRole(string role)
        {
            if (Identity == null) return false;

            return Identity.Roles.Any(Role.HasName(role));
        }
        public static bool HasContainRole(string role)
        {
            if (Identity == null) return false;
            return Identity.Roles.Any(Role.ContainName(role));
        }
        public static RoleLevel CurrentRoleLevel()
        {
            if (Identity != null && HasRoleLevel(RoleLevel.AdminLevel))
            {
                return RoleLevel.AdminLevel;
            }
            if (Identity != null && HasRoleLevel(RoleLevel.ManagerLevel))
            {
                return RoleLevel.ManagerLevel;
            }
            return RoleLevel.SalesLevel;
        }
        public static bool HasRoleLevel(RoleLevel roleLevel)
        {
            if (Identity == null) return false;

            return Identity.Roles.Any(m => m.RoleLevel == roleLevel);
        }

        public static bool HasPermission(string controller, string action)
        {
            if (Identity == null) return false;
            return AuthorizationService.CheckAccess(Identity.ID, controller + "." + action);
        }

        public static bool HasSettingPermission(string module, string name)
        {
            if (Identity == null) return false;
            return AuthorizationService.CheckAccess(Identity.ID, module + "." + name, AreaType.SettingPermission);
        }

        //public static bool Can<TController>(Expression<Func<TController, object>> action) where TController : IController
        //{
        //    string actionName = action.GetActionName();
        //    if (string.IsNullOrEmpty(actionName))
        //        return true;

        //    string controllerName = typeof(TController).GetControllerName();

        //    return HasPermission(controllerName, actionName);
        //}
        public static bool Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(email)) return false;

            User user = AuthenticationService.ValidateLogin(email, password);
            if (user != null)
            {
                LoginPersister.SignIn(email, rememberMe);
                //PermissionCurrent.SetPermissionUser(AuthenticationService.PermissionUserInfo(user));
            }

            return Identity != null;
        }

        public static void Logout()
        {
            //PermissionCurrent.RemovePermissinUser();
            LoginPersister.SignOut();
        }

        //// Dirty hacks
        //// It's time racing. I can't help it :(
        //public static RuleResult<Indent> Can(IndentStatus status, Indent indent)
        //{
        //    return new IndentRule(indent).Check(status);
        //}
        public static bool IsCorrect(string email, string password)
        {
            User user = AuthenticationService.ValidateLogin(email, password);
            return user != null;
        }


    }
}
