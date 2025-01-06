using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;

using PMS.EFCore.Services;
using AM.EFCore.Services;


using PMS.EFCore.Services.Payroll;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Payroll
{
    [ApiController]
    public class ProcessDailyController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "ProcessDaily";
        private ProcessDaily _service;
        private AuthenticationServiceEstate _authenticationService;

        public ProcessDailyController(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new ProcessDaily(context, _authenticationService, auditContext);
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/process")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult DownloadSPB([FromQuery]string divId, DateTime date)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.Process(divId, date,webSession.DecodedToken.UserName));
        }

    }
}