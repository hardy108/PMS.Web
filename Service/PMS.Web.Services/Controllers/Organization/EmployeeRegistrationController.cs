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
using PMS.EFCore.Services.Organization;


using WF.EFCore.Models;
using WF.EFCore.Services;
using PMS.Shared.Models;
using PMS.EFCore.Services.Utilities;
using WF.EFCore.Data;
using PMS.Shared.Services;
using PMS.EFCore.Helper;


namespace PMS.Web.Services.Controllers.Organization
{

    [ApiController]
    public class EmployeeRegistrationController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "EmployeeRegistration"; //Replace with your own route name
        

        private AuthenticationServiceEstate _authenticationService;
        private EmployeeRegistration _service;
        private PMSContextHO _contextHO;
        private PMSContextEstate _contextEstate;
        //private PMS.EFCore.Services.Organization.Employee _employeeService;

        public EmployeeRegistrationController(AuthenticationServiceEstate tokenService,AuthenticationServiceHO authenticationServiceHO, PMSContextHO context,PMSContextEstate contextEstate, WFContext wfContext,IBackgroundTaskQueue taskQueue,IEmailSender emailSender, AuditContext auditContext)
        {
            _authenticationService = tokenService;
            _contextEstate = contextEstate;
            _contextHO = context;
            _service = new EmployeeRegistration(context,wfContext, authenticationServiceHO, taskQueue,emailSender,auditContext);
            //_employeeService = new EFCore.Services.Organization.Employee(_contextHO, _authenticationService, auditContext);
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]GeneralFilter param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            //_employeeService.UpdateEmployeeStatusFromSP3(_contextEstate, _contextHO);
            return Ok(_service.GetList(param));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GetListCount([FromQuery]GeneralFilter param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            param.PageSize = 0;
            return Ok(_service.GetListCount(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/initnew")]
        public IActionResult InitNew()
        {
            return Ok(_service.NewRecord(string.Empty));        
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetSingleByUsername(webSession.DecodedToken.UserName, Id));
        }




        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getforapproval/{Id}")]
        public IActionResult GetForApproval ([FromRoute]long Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetForApproval(Id,webSession.DecodedToken.UserName));
        }


        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/new")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult SaveNew([FromForm]IFormCollection data)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            
            return Ok(_service.SaveInsert(data, webSession.DecodedToken.UserName));
        }

        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/edit")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult SaveEdit([FromForm]IFormCollection data)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            string errorMessage = string.Empty;
            return Ok(_service.SaveUpdate(data, webSession.DecodedToken.UserName));
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

        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/approval")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult DoApproval([FromForm]IFormCollection data, [FromQuery]string approvalCode, [FromQuery] string approvalNotes,[FromQuery]bool NewRecord)
        {

            var webSession = _authenticationService.LoginBySession(Request.Headers);

            if (string.IsNullOrWhiteSpace(approvalCode))
                approvalCode = data["APRVCODE"];
            if (string.IsNullOrWhiteSpace(approvalNotes))
                approvalNotes = data["APPRVNOTES"];//Custom

            return Ok(_service.WFSendApproval(data, webSession.DecodedToken.UserName, approvalCode, approvalNotes,NewRecord));
        }


        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getnextactions/{Id}")]
        public IActionResult GetListNextActions([FromRoute]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetNextActions(_service.GetSingleByUsername(webSession.DecodedToken.UserName, Id), webSession.DecodedToken.UserName));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getapprovalhistories/{Id}")]
        public IActionResult GetListApprovalHistories([FromRoute]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetApprovalHistory(_service.GetSingleByUsername(webSession.DecodedToken.UserName, Id), webSession.DecodedToken.UserName)); ;
        }
    }
}