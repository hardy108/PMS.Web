using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using PMS.Shared.Models;

namespace PMS.Web.UI.Services
{
    public interface IHttpReportServices
    {
        List<Report> GetReports(string reportGroupId);
        Report GetReport(string reportId);
        ReportGroup GetReportGroup(string groupId);
        
        List<Report> GetAllReports();
        List<ReportGroup> GetAllReportGroups();

        Report GetReportAccess(string menuId, UserAccess userAccess);

    }
    public class HttpReportServices:IHttpReportServices
    {
        private PMS.Shared.Services.ReportServices _service;
        public HttpReportServices(IHostingEnvironment hostingEnvirontment, IOptions<AppSetting> appSettings)
        {
            string reportFile = hostingEnvirontment.ContentRootPath + appSettings.Value.ReportJsonFile,
                reportGroupFile = hostingEnvirontment.ContentRootPath + appSettings.Value.ReportGroupJsonFile,
                filterFolder = hostingEnvirontment.ContentRootPath + appSettings.Value.FilterFolder;
            _service = new Shared.Services.ReportServices(reportFile, reportGroupFile, filterFolder);
        }
        public List<Report> GetReports(string reportGroupId)
        {
            return _service.GetReports(reportGroupId);
        }

        public Report GetReport(string reportId)
        {
            return _service.GetReport(reportId);
        }
        public ReportGroup GetReportGroup(string groupId)
        {
            return _service.GetReportGroup(groupId);
        }

        public List<Report> GetAllReports()
        {
            return _service.GetAllReports();
        }

        public List<ReportGroup> GetAllReportGroups()
        {
            return _service.GetAllReportGroups();
        }

        public Report GetReportAccess(string menuId, UserAccess userAccess)
        {
            return _service.GetReportAccess(menuId, userAccess);
        }
    }
}
