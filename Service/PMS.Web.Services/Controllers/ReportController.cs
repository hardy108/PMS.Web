using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AM.EFCore.Services;
using FastReport.Web;
using PMS.EFCore.Model;
using Microsoft.AspNetCore.Authorization;
using PMS.EFCore.Model.Filter;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using FastReport.Export;
using FastReport.Export.PdfSimple;
using FastReport.Export.Html;
using FastReport.Export.Image;
using FastReport.Utils;
using FastReport.Data;
using Microsoft.AspNetCore.Html;
using PMS.EFCore.Services.Utilities;
using PMS.Shared.Models;
using PMS.EFCore.Helper;
using System.Data;
using System.Text;
using System.Data.Common;

using PMS.Web.Services.Reports.Models;
using PMS.Web.Services.Reports;

namespace PMS.Web.Services.Controllers
{    
    [ApiController]
    public class ReportController : Controller
    {
        private const string _CONTROLLER_NAME = "Report"; //Replace with your own route name
        private AuthenticationServiceEstate _authenticationService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private PMSContextEstate _context;

        public ReportController(PMSContextEstate context, AuthenticationServiceEstate authenticationService,IHostingEnvironment environment)
        {
            _authenticationService = authenticationService;
            _hostingEnvironment = environment;
            _context = context;
            _authenticationService = authenticationService;
        }        

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/accountbalance")]
        public IActionResult AccountBalance([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "AccountBalanceRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/GL/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("EDate", filter.StartDate);

            var AccType = filter.Type;
            if (AccType == null)
            {
                AccType = "0";
            }
            else
            {
                AccType = filter.Type;
            }
            reportParameters.Add("AccountType", AccType);

            var AccNo = filter.Id;
            if (AccNo == null)
            {
                AccNo = "*";
            }
            else
            {
                AccNo = filter.Id;
            }
            reportParameters.Add("AccountCode", AccNo);

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/accounthistory")]
        public IActionResult AccountHistory([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "AccountHistoryRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/GL/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);

            var AccType = filter.Type;
            if (AccType == null)
            {
                AccType = "0";
            }
            else
            {
                AccType = filter.Type;
            }
            reportParameters.Add("AccountType", AccType);

            var AccNo = filter.Id;
            if (AccNo == null)
            {
                AccNo = "*";
            }
            else
            {
                AccNo = filter.Id;
            }
            reportParameters.Add("AccountCode", AccNo);

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/attendanceperemployee")]
        public IActionResult AttendancePerEmployee([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "AttendancePerEmployeeRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Attendance/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("EmployeeId", filter.Id);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/attendancesummary")]
        public IActionResult AttendanceSummary([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "AttendanceSummaryRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Attendance/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivId", filter.DivisionID);
            reportParameters.Add("Type", filter.Type);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/balancesheet")]
        public IActionResult BalanceSheet([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "BalanceSheetRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Financial/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.Id);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/dailypanenloading")]
        public IActionResult DailyPanenLoading([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "DailyPanenLoading.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/EDP/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("EDate", filter.Date);

            if (filter.Format == "data")
                return Ok(GetReportData<rptDailyPanenLoading>(reportPath, context, reportParameters));

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/dailyupkeep")]
        public IActionResult DailyUpkeep([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "DailyUpkeep.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/EDP/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("EDate", filter.Date);

            if (filter.Format == "data")
                return Ok(GetReportData<rptUpkeepDaily>(reportPath, context, reportParameters));

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/dailyharvestupkeepdivision")]
        public IActionResult Daily([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "DailyLHD.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/EDP/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("EDate", filter.StartDate);

            if (filter.Format == "data")
                return Ok(GetReportData<rptHarvestingUpkeepDailyDivision>(reportPath, context, reportParameters));

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);            
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/employeejoint")]
        public IActionResult EmployeeJoint([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "EmployeeJointRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            if (filter.Id == null)
                reportParameters.Add("Id", '*');
            else
                reportParameters.Add("Id", filter.Id);
                reportParameters.Add("EDate", filter.StartDate);

            if (filter.Format == "data")
                return Ok(GetReportData<rpt_EmployeeJoint>(reportPath,context,reportParameters));
            
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/employeemutation")]
        public IActionResult EmployeeMutation([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "EmployeeMutationRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            if (filter.Id == null)
                reportParameters.Add("Id", '*');
            else
                reportParameters.Add("Id", filter.Id);
                reportParameters.Add("EDate", filter.StartDate);

            if (filter.Format == "data")
                return Ok(GetReportData<rpt_EmployeeMutation>(reportPath, context, reportParameters));

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/employeeout")]
        public IActionResult EmployeeOut([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "EmployeeOutRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            if (filter.Id == null)
                reportParameters.Add("Id", '*');
            else
                reportParameters.Add("Id", filter.Id);
                reportParameters.Add("EDate", filter.StartDate);

            if (filter.Format == "data")
                return Ok(GetReportData<rpt_EmployeeOut>(reportPath, context, reportParameters));

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/equipmenttimesheet")]
        public IActionResult EquipmentTimeSheet([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "EquipmentTimeSheetRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/goodissue")]
        public IActionResult GoodIssue([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "GoodIssueRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Inventory/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/goodmutation")]
        public IActionResult GoodMutation([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "GoodMutationRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Inventory/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("EDate", filter.StartDate);

            var MatId = filter.Id;
            if (MatId == null)
            {
                MatId = "*";
            }
            else
            {
                MatId = filter.Id;
            }
            reportParameters.Add("MaterialId", MatId);

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/goodreceipt")]
        public IActionResult GoodReceipt([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "GoodReceiptRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Inventory/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/rekappph21")]
        public IActionResult RekapPPH21([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "RekapPPH21Rpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Attendance/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/goodreceiptvendor")]
        public IActionResult GoodReceiptPerVendor([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "GoodReceiptVendorRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Inventory/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/goodstock")]
        public IActionResult GoodStock([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "GoodStockRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Inventory/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("MaterialId", filter.Id);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/goodvalue")]
        public IActionResult GoodValue([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "GoodValueRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Inventory/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("EDate", filter.StartDate);

            var MatId = filter.Id;
            if (MatId == null)
            {
                MatId = "*";
            }
            else
            {
                MatId = filter.Id;
            }
            reportParameters.Add("MaterialId", MatId);

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/harvestingdetail")]
        public IActionResult HarvestingDetail([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "HarvestingDetailRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/harvestingfine")]
        public IActionResult HarvestingFine([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "HarvestingFineRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/harvestinglebihbasis")]
        public IActionResult HarvestingLebihBasis([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "HarvestingLebihBasisAndMaxBrondolRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);

            if (filter.Format == "data")
                return Ok(GetReportData<rptHarvestingLebihBasisAndMaxBrondol>(reportPath, context, reportParameters));

            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/journaldetail")]
        public IActionResult JournalDetail([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "JournalDetailRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/GL/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("Id", filter.Id);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/kontanan")]
        public IActionResult Kontanan([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "KontananRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/lhd")]
        public IActionResult LHD([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "LHDRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionID", filter.DivisionID);
            reportParameters.Add("Type", filter.Type);
            reportParameters.Add("Form", filter.Id);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/lhp")]
        public IActionResult LHP([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "LHPRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.UnitID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/paymentfp")]
        public IActionResult PaymentFP([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "PaymentFPRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Attendance/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/paymentperiod")]
        public IActionResult PaymentPeriod([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "PaymentPeriod2Rpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Attendance/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionID", filter.DivisionID);
            reportParameters.Add("Type", filter.Type);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/paymentslip")]
        public IActionResult PaymentSlip([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "PaymentSlipRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Attendance/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/produksiperblock")]
        public IActionResult ProduksiPerBlock([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "ProduksiPerBlockRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/profitloss")]
        public IActionResult ProfitLoss([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "ProfitLossRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Financial/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.Id);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/rekappayroll2")]
        public IActionResult RekapPayroll2([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "RekapPayroll2Rpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Attendance/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("UnitId", filter.Id);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }
        
        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/rkh")]
        public IActionResult RKH([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "RKHRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("EDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/summarykaryawan")]
        public IActionResult SummaryKaryawan([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "SummaryKaryawanRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("Id", filter.Id);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/suratperingatan")]
        public IActionResult SuratPeringatan([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string typeSp = filter.Type;
            string reportFile = "";
            if (typeSp == "1")
                reportFile = "SuratPeringatan1Rpt.frx";
            else if (typeSp == "2")
                reportFile = "SuratPeringatan2Rpt.frx";
            else
                reportFile = "SuratPeringatan3Rpt.frx";

            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Attendance/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("Id", filter.Id);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/spb")]
        public IActionResult SPB([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "SPBRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/tanamancost")]
        public IActionResult TanamanCost([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "TanamanCostSingleRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Tanaman/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            reportParameters.Add("Type", filter.Type);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/thr")]
        public IActionResult THR([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "THRRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/natura")]
        public IActionResult NATURA([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "NaturaRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/kuponnatura")]
        public IActionResult KUPONNATURA([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "KuponNaturaRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/preminonpanen")]
        public IActionResult PREMINONPANEN([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "PremiNonPanenRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/premipanendetailall")]
        public IActionResult PremiPanenDetailAll([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string type = "";
            string reportFile = "";
            if (filter.Type == "1")
            {
                reportFile = "PremiPanenDetailRpt.frx";
                type = "SKU";
            }
            else if (filter.Type == "2")
            {
                reportFile = "PremiPanenDetailRpt.frx";
                type = "BHL";
            }
            else if (filter.Type == "3")
                reportFile = "PremiPanenPemanenRpt.frx";
            else if (filter.Type == "4")
                reportFile = "PremiPanenMandorRpt.frx";
            else if (filter.Type == "5")
                reportFile = "PremiPanenKraniRpt.frx";
            else if (filter.Type == "6")
                reportFile = "PremiPanenMandor1Rpt.frx";
            else if (filter.Type == "7")
                reportFile = "PremiPanenCheckerRpt.frx";

            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            reportParameters.Add("Type", type);
            reportParameters.Add("PaymentType", Convert.ToInt32(filter.PaymentType));
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/premioperatordetailall")]
        public IActionResult PremiOperatorDetailAll([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string type = "";
            string reportFile = "";
            if (filter.Type == "1")
            {
                reportFile = "PremiOperatorDetailRpt.frx";
                type = "SKU";
            }
            else if (filter.Type == "2")
            {
                reportFile = "PremiOperatorDetailRpt.frx";
                type = "BHL";
            }
            

            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            reportParameters.Add("Type", type);
            reportParameters.Add("PaymentType", Convert.ToInt32(filter.PaymentType));
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/premimuatdetailall")]
        public IActionResult PremiMuatDetailAll([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string type = "";
            string reportFile = "";
            if (filter.Type == "1")
            {
                reportFile = "PremiMuatDetailRpt.frx";
                type = "SKU";
            }
            else if (filter.Type == "2")
            {
                reportFile = "PremiMuatDetailRpt.frx";
                type = "BHL";
            }


            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            reportParameters.Add("Type", type);
            reportParameters.Add("PaymentType", Convert.ToInt32(filter.PaymentType));
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/upkeepdetail")]
        public IActionResult UpkeepDetail([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "UpkeepDetailRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivisionId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        //[Authorize]]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/upkeephk")]
        public IActionResult UpkeepHk([FromServices]PMSContextEstate context, [FromQuery]FilterReport filter)
        {
            string reportFile = "UpkeepHkRpt.frx";
            string reportPath = _hostingEnvironment.ContentRootPath + "/Reports/Logistic/" + reportFile;
            Dictionary<string, object> reportParameters = new Dictionary<string, object>();
            reportParameters.Add("DivId", filter.DivisionID);
            reportParameters.Add("StartDate", filter.StartDate);
            reportParameters.Add("EndDate", filter.EndDate);
            return GetReport(reportPath, filter.Format, filter.Inline, context, reportParameters);
        }

        private IActionResult GetReport(string reportPath, string format, bool inline, PMSContextEstate context, Dictionary<string,object> reportParameters)
        {
            string mime = string.Empty;
            string fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");            
            if (string.IsNullOrWhiteSpace(format))
                format = "image";
            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));

            WebReport webReport = new WebReport();
            webReport.Report.Load(reportPath);

            if (format == "html" && inline)
            {
                
                webReport.Report.Dictionary.Connections[0].ConnectionString = context.ConnectionString;
                
                if (reportParameters != null)
                {
                    foreach (string key in reportParameters.Keys)
                    {
                        webReport.Report.SetParameterValue(key, reportParameters[key]);
                    }
                }
                ViewBag.WebReport = webReport;
                return View("Report");
            }
            else if (format == "csv")
            {
                
                var parameters = webReport.Report.Dictionary.Connections[0].Tables[0].Parameters;
                var sqlCommand = webReport.Report.Dictionary.Connections[0].Tables[0].SelectCommand;
                var executor = context.ExecuteSqlText(sqlCommand);
                

                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    string paramSource = string.Empty;
                    if (!string.IsNullOrWhiteSpace(parameter.Expression))
                    {
                        paramSource = parameter.Expression;
                        int start = paramSource.IndexOf("[") + 1;
                        int end = paramSource.IndexOf("]", start);
                        paramSource = paramSource.Substring(start, end - start);
                    }
                    if (string.IsNullOrWhiteSpace(paramSource))
                        paramSource = parameter.Name;

                    switch (parameter.DataType)
                    {
                        case 0: //BigInt
                            executor.AddParam(parameter.Name, (long)reportParameters[paramSource]);
                            break;
                        case 2: //Bit
                            executor.AddParam(parameter.Name, (bool)reportParameters[paramSource]);
                            break;
                        case 3: //Char
                        case 22://varchar
                        case 10://NChar
                        case 11://NText
                        case 12://NVarchar
                        case 18://Text
                            executor.AddParam(parameter.Name, reportParameters[paramSource].ToString());
                            break;
                        case 4://DateTime
                        case 15://Small DateTime
                        case 33://DateTime2
                            executor.AddParam(parameter.Name, (DateTime)reportParameters[paramSource]);
                            break;
                        case 5: //Decimal                        
                            executor.AddParam(parameter.Name, (decimal)reportParameters[paramSource]);
                            break;
                        case 6: //Float                        
                        case 9: //Money
                        case 13: //Real
                        case 17: //Small Money
                            executor.AddParam(parameter.Name, (float)reportParameters[paramSource]);
                            break;
                        case 8://Int
                        case 16://Small Int
                        case 20://TinyInt
                            executor.AddParam(parameter.Name, (int)reportParameters[paramSource]);
                            break;
                        case 31://DAte
                            executor.AddParam(parameter.Name, ((DateTime)reportParameters[paramSource]).Date);
                            break;
                        case 32://Time
                            executor.AddParam(parameter.Name, ((DateTime)reportParameters[paramSource]).TimeOfDay);
                            break;
                    }
                }

                
                string csvString = string.Empty;

                executor.Exec(dr => {
                    if (dr != null)
                    {

                        bool addComma = false;
                        csvString +="\"";

                        var dtSchema = dr.GetSchemaTable();
                        foreach (DataRow schemarow in dtSchema.Rows)
                        {   
                            if (addComma)                            
                                csvString += "\",\"";                            
                            else                            
                                addComma = true;
                            csvString += schemarow.ItemArray[0].ToString();

                        }
                        csvString += "\"";
                        csvString += "\r\n";

                        while (dr.Read())
                        {

                            addComma = false;
                            csvString += "\"";
                            for(int i = 0; i < dr.FieldCount; i++)
                            {
                            
                                if (dr[i] != null)                                
                                    csvString  += Convert.ToString(dr[i]).Replace("\"", String.Empty);

                                if (addComma)
                                {
                                    csvString += "\",\"";
                                }
                                else
                                {
                                    addComma = true;
                                }
                            }
                            csvString += "\"";
                            csvString += "\r\n";
                        }
                        dr.Close();
                    }
                });

                var csvBytes = Encoding.ASCII.GetBytes(csvString);
                var result = new FileContentResult(csvBytes, "application/octet-stream");
                result.FileDownloadName = $"report.{fileName}.csv";
                return result;
            }

            //using(var export = new FastReport.Export.oox)

            using (MemoryStream stream = new MemoryStream())
            {
                using (FastReport.Report report = new FastReport.Report())
                {
                    report.Load(reportPath);
                    report.Dictionary.Connections[0].ConnectionString = context.ConnectionString;

                    if (reportParameters != null)
                    {
                        foreach (string key in reportParameters.Keys)
                        {
                            report.SetParameterValue(key, reportParameters[key]);
                        }
                    }

                    report.Prepare();

                    switch (format)
                    {
                        case "pdf":
                            PDFSimpleExport pdf = new PDFSimpleExport();
                            mime = "application/pdf";
                            fileName = $"EmployeeSummary.{fileName}.pdf";
                            report.Export(pdf, stream);
                            break;
                        case "html":
                            
                            HTMLExport html = new HTMLExport();
                            html.SinglePage = true;
                            mime = "text/html";
                            fileName = $"EmployeeSummary.{fileName}.html";
                            report.Export(html, stream);
                            break;
                        default:
                            ImageExport image = new ImageExport();
                            image.ImageFormat = ImageExportFormat.Png;
                            image.SeparateFiles = false;
                            mime = "application/image";
                            fileName = $"EmployeeSummary.{fileName}.png";
                            report.Export(image, stream);
                            break;
                    }

                    if (inline)
                        return File(stream.ToArray(), mime);
                    else
                        // Otherwise download the report file 
                        return File(stream.ToArray(), mime, fileName); // attachment
                }
            }
        }

        private string GetReportData<T>(string reportPath, PMSContextEstate context, Dictionary<string, object> reportParameters)         
            where T:class,new()
        {
            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport webReport = new WebReport();
            webReport.Report.Load(reportPath);

         
            var parameters = webReport.Report.Dictionary.Connections[0].Tables[0].Parameters;
            var sqlCommand = webReport.Report.Dictionary.Connections[0].Tables[0].SelectCommand;
            var executor = context.ExecuteSqlText(sqlCommand);


            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                string paramSource = string.Empty;
                if (!string.IsNullOrWhiteSpace(parameter.Expression))
                {
                    paramSource = parameter.Expression;
                    int start = paramSource.IndexOf("[") + 1;
                    int end = paramSource.IndexOf("]", start);
                    paramSource = paramSource.Substring(start, end - start);
                }
                if (string.IsNullOrWhiteSpace(paramSource))
                    paramSource = parameter.Name;

                switch (parameter.DataType)
                {
                    case 0: //BigInt
                        executor.AddParam(parameter.Name, (long)reportParameters[paramSource]);
                        break;
                    case 2: //Bit
                        executor.AddParam(parameter.Name, (bool)reportParameters[paramSource]);
                        break;
                    case 3: //Char
                    case 22://varchar
                    case 10://NChar
                    case 11://NText
                    case 12://NVarchar
                    case 18://Text
                        executor.AddParam(parameter.Name, reportParameters[paramSource].ToString());
                        break;
                    case 4://DateTime
                    case 15://Small DateTime
                    case 33://DateTime2
                        executor.AddParam(parameter.Name, (DateTime)reportParameters[paramSource]);
                        break;
                    case 5: //Decimal                        
                        executor.AddParam(parameter.Name, (decimal)reportParameters[paramSource]);
                        break;
                    case 6: //Float                        
                    case 9: //Money
                    case 13: //Real
                    case 17: //Small Money
                        executor.AddParam(parameter.Name, (float)reportParameters[paramSource]);
                        break;
                    case 8://Int
                    case 16://Small Int
                    case 20://TinyInt
                        executor.AddParam(parameter.Name, (int)reportParameters[paramSource]);
                        break;
                    case 31://DAte
                        executor.AddParam(parameter.Name, ((DateTime)reportParameters[paramSource]).Date);
                        break;
                    case 32://Time
                        executor.AddParam(parameter.Name, ((DateTime)reportParameters[paramSource]).TimeOfDay);
                        break;
                }
            }



            List<T> result = null;
            executor.Exec(rows => {                
                result = rows.ToList<T>();
            });


            
            var excelStream = ExcelGenerator.GetExcelStream(result, "data", new List<string> { });
            return Convert.ToBase64String(excelStream);
            
        }

        #region ReportParameter Helper

        #endregion
    }
}