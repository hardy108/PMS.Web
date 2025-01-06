using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PMS.Web.UI.Code;
using Microsoft.Extensions.Options;
using System.IO;
using PMS.Shared.Models;
using PMS.Web.UI.Services;
using PMS.Shared.Utilities;
using PMS.Shared.Services;

namespace PMS.Web.UI.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        readonly IOptions<AppSetting> _config;

        public ReportController(IOptions<AppSetting> config)
        {
            _config = config;
        }
        public async System.Threading.Tasks.Task<ActionResult> Index([FromServices]IHttpReportServices reportServices)
        {
            List<string> authorizedReportIds = new List<string>();
            string reportString = await GetReportAccess(string.Empty);
            if (!string.IsNullOrWhiteSpace(reportString))
            {
                authorizedReportIds = reportString.Deserialize<List<string>>();
                if (authorizedReportIds == null)
                    authorizedReportIds = new List<string>();
            }

            ViewBag.ReportGroups = reportServices.GetAllReportGroups();
            var dictReportGroup = string.Empty;
            var dictReport = string.Empty;
            foreach (ReportGroup group in ViewBag.ReportGroups)
            {
                string reports = string.Empty;
                int i = 0;
                foreach (Report report in reportServices.GetReports(group.Id))
                {
                    bool authorized = false;
                    if (report.NeedAUthentication)
                        authorized = authorizedReportIds.Contains(report.ReportID);
                    else
                        authorized = true;

                    if (authorized)
                    {
                        var reportData = $"ReportID:'{report.ReportID}',ReportPath:'{report.ReportPath}',ReportName:'{report.ReportName}'";
                        reportData = "{" + reportData + "}";
                        reports += reportData + ",";
                        dictReport += $"dictReports['{report.ReportID}']={reportData};";
                        i++;
                    }
                }
                if (i > 0)
                {
                    reports = "[" + reports.Substring(0, reports.Length - 1) + "]";
                    dictReportGroup += $"dictReportGroups['{group.Id}']={reports};";
                }
            }
            ViewBag.DicReportGroups = dictReportGroup;
            ViewBag.DicReports = dictReport;
            return View();
        }

        // GET: Report/Show/5
        public async System.Threading.Tasks.Task<ActionResult> Show(string MenuID,[FromServices]IHttpReportServices reportServices)
        {
            string reportId = MenuID;
            Report reportInfo = reportServices.GetReport(reportId);
            if (reportInfo == null)
            {
                ViewBag.Error = "Invalid report";
                return Error();
            }
            if (reportInfo.NeedAUthentication)
            {
                string reportString = await GetReportAccess(reportId);
                if (string.IsNullOrWhiteSpace(reportString))
                {
                    ViewBag.Error = "You are not authorized";
                    return Error();

                }
            }

            ViewBag.ReportID = reportInfo.ReportID;
            ViewBag.ReportPath = reportInfo.ReportPath;
            ViewBag.FilterID = "Filter_" + reportInfo.ReportID;
            ViewBag.FilterRows = reportInfo.FilterRows;
            ViewBag.HasFilter = (reportInfo.FilterRows != null && reportInfo.FilterRows.Any());
            string mandatoryFilterElementIds = string.Empty;
            ViewBag.Prefix = reportInfo.ReportID;
            if (reportInfo.MandatoryFilterItems != null && reportInfo.MandatoryFilterItems.Any())
            {
                reportInfo.MandatoryFilterItems.ForEach(d =>
                {
                    mandatoryFilterElementIds += "'" + d + "',";
                });
                mandatoryFilterElementIds = "[" + mandatoryFilterElementIds.Substring(0, mandatoryFilterElementIds.Length - 1) + "]";
            }
            else
                mandatoryFilterElementIds = "[]";
            ViewBag.MandatoryFilterItems = mandatoryFilterElementIds;            
            ViewBag.FilterValidation = reportInfo.FilterValidation;
            

            if (reportInfo.FilterRows != null && reportInfo.FilterRows.Any())
            {
                reportInfo.FilterRows.ForEach(d => 
                {
                    d.ForEach(c=> {
                        if (c.Ascendants != null && c.Ascendants.Any())
                        {
                            c.Ascendants.ForEach(f => { f.ElementID = ViewBag.Prefix + f.ElementID; });
                        }
                    });
                });
            }
            return View();
        }

        private async System.Threading.Tasks.Task<string> GetReportAccess(string reportId)
        {
            string menuInfoApi = _config.Value.WebApiUrl + "api/login/reportaccess?ReportID=" + reportId;
            RestClient restClient = new RestClient(menuInfoApi, Request.Cookies["BearerToken"]);
            return (await restClient.GetStringAsync());
        }

        private async System.Threading.Tasks.Task<Report> GetReportInfoAsync(string Id)
        {

            string menuString = await GetReportInfoStringAsync(Id);
            return menuString.Deserialize<Report>();

        }

        private async System.Threading.Tasks.Task<string> GetReportInfoStringAsync(string Id)
        {
            string reportInfoApi = _config.Value.WebApiUrl + "api/report/info/" + Id;
            RestClient restClient = new RestClient(reportInfoApi, Request.Cookies["BearerToken"]);
            return (await restClient.GetStringAsync());
        }

        

        private async System.Threading.Tasks.Task<string> GetAuthorizationAsync(string Id)
        {
            string reportString = string.Empty;
            try
            {
                reportString = await GetReportInfoStringAsync(Id);
            }
            catch { }
            return reportString;
        }

        public ActionResult Error()
        {
            return View("/Views/Error/InternalServerError.cshtml");
        }
    }
}