using System.Web.Mvc;
using PQT.Web.Infrastructure.Filters;

namespace PQT.Web.Controllers
{
    public class ErrorsController : Controller
    {
        //
        // GET: /Errors/
        public ActionResult NotFound()
        {
            return View();
        }
        public ActionResult Error500()
        {
            return View();
        }

    }
}
