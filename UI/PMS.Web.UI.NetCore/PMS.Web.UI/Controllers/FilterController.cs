using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PMS.Shared.Models;
using PMS.Shared.Utilities;
using PMS.Web.UI.Models;

namespace PMS.Web.UI.Controllers
{
    public class FilterController : Controller
    {
        // GET: Filter
        
        
        public ActionResult Index(string filterName, string prefix)
        {
            ViewBag.Prefix = String.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix + "_";
            ViewBag.MenuId = filterName;
            return View(filterName);
        }

        public ActionResult FilterJson(string filterName)
        {

            ViewBag.Prefix = Request.Query["Prefix"];            
            ViewBag.FilterName = filterName;
            return View();
        }


        public ActionResult ReportParameter(string filterName)
        {

            ViewBag.Prefix = Request.Query["Prefix"];
            ViewBag.FilterName = filterName;
            return View();
        }


        public ActionResult ShowFilter([FromQuery]string filterId, [FromQuery]string prefixId,[FromForm]string filterRows)
        {
            ViewBag.FilterID = filterId;
            ViewBag.Prefix = prefixId;
            FilterJson filterJsons = filterRows.Deserialize<FilterJson>();
            return View("_Filter");

        }
    }
}