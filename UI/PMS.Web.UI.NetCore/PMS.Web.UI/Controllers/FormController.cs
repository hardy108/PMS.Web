using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PMS.Web.UI.Controllers
{
    public class FormController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("form/{viewName}/{mode}/{id}")]
        public IActionResult ShowForm([FromRoute] string viewName,[FromRoute] string mode, [FromRoute] string id)
        {
            ViewBag.Key = id;
            ViewBag.Mode = mode;
            return View(viewName);
        }
    }
}