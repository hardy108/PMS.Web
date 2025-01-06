using Microsoft.AspNetCore.Mvc;

namespace PMS.Web.UI.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(string returnURL)
        {
            ViewBag.ReturnURL = returnURL;
            return View();
        }

        public ActionResult LoginByToken()
        {
            ViewBag.Token = Request.Query["Token"];
            ViewBag.DestType = Request.Query["destType"];
            ViewBag.Destination = Request.Query["destination"];
            ViewBag.MenuID = Request.Query["MenuID"];
            ViewBag.Url = Request.Query["Url"];

            


            return View();
        }

        public ActionResult LoginFromExternal(string userName, string password)
        {
            ViewBag.UserName = userName;
            ViewBag.Password = password;
            return View();
        }

        public ActionResult LogoutFromExternal()
        {
            return View();
        }

        public ActionResult ChangePassword(string returnURL)
        {
            ViewBag.ReturnURL = returnURL;
            return View();
        }

        public ActionResult ResetPasswordRequest()
        {
            return View();
        }

        public ActionResult ResetPassword([FromQuery]string token)
        {
            ViewBag.ResetPasswordToken = token;
            return View();
        }

        public ActionResult ServerList()
        {
            
            return View();
        }
    }
}