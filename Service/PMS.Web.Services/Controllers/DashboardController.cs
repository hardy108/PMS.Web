using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using AM.EFCore.Services;

namespace PMS.Web.Services.Controllers
{
    
    [ApiController]
    public class DashboardController : ControllerBase
    {

        private AuthenticationServiceEstate _authenticationService;
        private PMSContextEstate _context;
        public DashboardController(AuthenticationServiceEstate authenticationService,PMSContextEstate context)
        {
            _authenticationService = authenticationService;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        [Route("api/dashboard/getsummarydata")]
        // GET api/<controller>2
        public sp_DHS_DashBoard_GetSummaryData_Result GetSummaryData([FromQuery]string estateCode, [FromQuery]DateTime endDate)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            DateTime startDate;
            DateTime.TryParse(endDate.Year.ToString() + "-01-01", out startDate);
            return _context.sp_DHS_DashBoard_GetSummaryData(estateCode, startDate, endDate).First();
        }
    }
}