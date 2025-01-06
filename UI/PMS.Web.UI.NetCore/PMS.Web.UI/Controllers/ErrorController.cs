using Microsoft.AspNetCore.Mvc;

namespace PMS.Web.UI.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult InternalServerError()
        {
            ViewBag.Error = RouteData.Values["error"];
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }
    }
}
