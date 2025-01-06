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
using PMS.EFCore.Helper;
using PMS.EFCore.Services.Payroll;
using AM.EFCore.Services;
using PMS.Shared.Utilities;

namespace PMS.Web.Services.Controllers.Payroll
{
    [ApiController]
    public class ProcessMonthlyController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "ProcessMonthly";
        private ProcessMonthly _service;
        private AuthenticationServiceEstate _authenticationService;

        public ProcessMonthlyController(AuthenticationServiceEstate authenticationService, PMSContextEstate context,AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new ProcessMonthly(context,_authenticationService,auditContext);
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]GeneralFilter param)
        {
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GeListCount([FromQuery]GeneralFilter filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            filter.PageSize = 0;
            return Ok(_service.GetListCount(filter));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/process")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Process([FromQuery]string docNo, string unitId, DateTime date)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.Process(docNo, unitId, date, webSession.DecodedToken.UserName));
        }

        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/delete")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Delete([FromForm]IFormCollection data)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.Delete(data, webSession.DecodedToken.UserName));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            return Ok(_service.GetSingle(Id));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/initnew")]
        public IActionResult InitNew()
        {
            return Ok(_service.NewRecord(string.Empty));
        }

        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/approve")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Approve([FromForm]IFormCollection data)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.Approve(data, webSession.DecodedToken.UserName));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/paymentjournal")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult PaymentJournal([FromQuery]int period,string unitId, DateTime date)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetPaymentJournal( period,unitId, date));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/paymentjournalcsv")]
        public IActionResult PaymentJournalCsv([FromQuery]int period, string unitId, DateTime date)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetPaymentJournal(period, unitId, date).ToCSV(","));

        }
    }
}