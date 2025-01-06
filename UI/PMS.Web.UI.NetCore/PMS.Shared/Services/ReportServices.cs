using System;
using System.Collections.Generic;
using System.Text;
using PMS.Shared.Models;
using System.Linq;
using Newtonsoft.Json;
using System.IO;



namespace PMS.Shared.Services
{
    

    public class ReportServices
    {
        private List<Report> _reports = new List<Report>();
        private List<ReportGroup> _reportGroup = new List<ReportGroup>();
        string _filterFolder = string.Empty;

        public void InitServices(string reportFile, string reportGroupFile, string filterFolder)
        {
            using (StreamReader stream = System.IO.File.OpenText(reportFile))
            {
                using (JsonTextReader reader = new JsonTextReader(stream))
                {
                    var x = Newtonsoft.Json.JsonSerializer.Create();
                    _reports.AddRange(x.Deserialize<IEnumerable<Report>>(reader));
                }
            }


            using (StreamReader stream = System.IO.File.OpenText(reportGroupFile))
            {
                using (JsonTextReader reader = new JsonTextReader(stream))
                {
                    var x = Newtonsoft.Json.JsonSerializer.Create();
                    _reportGroup.AddRange(x.Deserialize<IEnumerable<ReportGroup>>(reader));
                }
            }
            _reports = _reports.OrderBy(d => d.DisplayOrder).ToList();
            _reportGroup = _reportGroup.OrderBy(d => d.DisplayOrder).ToList();
            _filterFolder = filterFolder;
        }

       

        public ReportServices(string reportFile, string reportGroupFile, string filterFolder)
        {
            InitServices(reportFile, reportGroupFile,filterFolder);
        }

        public List<Report> AllReports
        {
            get { return _reports; }
        }

        public List<Report> GetAllReports()
        {
            return AllReports;
        }

        public List<Report> GetReports(string reportGroupId)
        {
            if (string.IsNullOrWhiteSpace(reportGroupId))
                return AllReports;
            return _reports.Where(d => d.ParentID.Equals(reportGroupId)).ToList();
        }

        public List<ReportGroup> AllReportGroups
        {
            get { return _reportGroup; }
        }

        public List<ReportGroup> GetAllReportGroups()
        {
            return AllReportGroups;
        }

        public Report GetReport(string reportId)
        {
            
            Report report = _reports.SingleOrDefault(d => d.ReportID.Equals(reportId));
            report.FilterRows = new FilterJson();
            if (!string.IsNullOrWhiteSpace(report.FilterJson))
            {
                FilterServices filter = new FilterServices(_filterFolder + "/" + report.FilterJson + ".json");
                report.FilterRows = filter.AllRows;
            }
            return report;
        }


        public Report GetReportAccess(string reportId, UserAccess userAccess)
        {
            Report report = GetReport(reportId);
            if (report.NeedAUthentication)
            {
                if (userAccess == null)
                    throw new Exception("Anda tidak memiliki akses ke report ini");

                if (!userAccess.ACTIVE)
                    throw new Exception("Anda tidak memiliki akses ke report ini");
            }
            return report;
        }

        public ReportGroup GetReportGroup(string groupId)
        {
            return _reportGroup.SingleOrDefault(d => d.Id.Equals(groupId));
        }
    }
}
