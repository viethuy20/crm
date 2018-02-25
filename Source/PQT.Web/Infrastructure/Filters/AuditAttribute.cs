using System.Web.Mvc;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Infrastructure.Filters
{
    public class AuditAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Log.Info();
            base.OnActionExecuting(filterContext);
        }
    }
}
