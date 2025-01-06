using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using PMS.EFCore.Services.Entities;

using AM.EFCore.Services;
using Microsoft.AspNetCore.Authorization;

using WF.EFCore.Services;
using PMS.EFCore.Services.Approval;
using PMS.Shared.Models;
using WF.EFCore.Data;
using WF.EFCore.Models.Filters;
using Microsoft.CodeAnalysis;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers
{
    
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "Workflow";
        private AuthenticationServiceEstate _authenticationService;
        private WFContext _wfContext;
        private PMSContextHO _pmsContext;
        private PMSWorkflow _workflowService;
        private DocumentService _documentService;
        public WorkflowController(AuthenticationServiceEstate authenticationService, AuthenticationServiceHO authenticationServiceHO, WFContext wfContext, PMSContextHO pmsContext, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _pmsContext = pmsContext;
            _wfContext = wfContext;
            _workflowService = new PMSWorkflow(_pmsContext, _wfContext, authenticationServiceHO,auditContext);
            _documentService = new DocumentService(_wfContext, authenticationServiceHO,auditContext);
        }

        [Authorize]        
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/inbox/list")]
        public IActionResult Inbox([FromQuery]FilterWorkflow parameter)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            parameter.UserName = webSession.DecodedToken.UserName;
            var inbox = _workflowService.GetInbox(parameter);
            return Ok(inbox);
            
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/inbox2/list")]
        public IActionResult Inbox2([FromQuery]FilterWorkflow parameter)
        {

            var webSession = _authenticationService.LoginBySession(Request.Headers);
            parameter.UserName = webSession.DecodedToken.UserName;
            var inbox = _workflowService.GetInbox2(parameter);
            return Ok(inbox);

        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/inbox/listcount")]
        public IActionResult InboxCount([FromQuery]FilterWorkflow parameter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            parameter.UserName = webSession.DecodedToken.UserName;
            parameter.PageSize = 0;
            return Ok(_workflowService.GetInboxCount(parameter));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/inbox2/listcount")]
        public IActionResult Inbox2Count([FromQuery]FilterWorkflow parameter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            parameter.UserName = webSession.DecodedToken.UserName;
            parameter.PageSize = 0;
            return Ok(_workflowService.GetInbox2Count(parameter));
        }


        

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/outbox/list")]
        public IActionResult Outbox([FromQuery]FilterWorkflow parameter)
        {

            var webSession = _authenticationService.LoginBySession(Request.Headers);
            parameter.UserName = webSession.DecodedToken.UserName;
            return Ok(_workflowService.GetOutbox(parameter));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/outbox/listcount")]
        public IActionResult OutboxCount([FromQuery]FilterWorkflow parameter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            parameter.UserName = webSession.DecodedToken.UserName;
            parameter.PageSize = 0;
            return Ok(_workflowService.GetOutboxCount(parameter));
        }



        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/outbox2/list")]
        public IActionResult Outbox2([FromQuery]FilterWorkflow parameter)
        {

            var webSession = _authenticationService.LoginBySession(Request.Headers);
            parameter.UserName = webSession.DecodedToken.UserName;
            return Ok(_workflowService.GetOutbox2(parameter));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/outbox2/listcount")]
        public IActionResult Outbox2Count([FromQuery]FilterWorkflow parameter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            parameter.UserName = webSession.DecodedToken.UserName;
            parameter.PageSize = 0;
            return Ok(_workflowService.GetOutbox2Count(parameter));
        }




        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/authorizeddoctypes")]
        public IActionResult GetAuthorizedDocumentTypes([FromRoute]long Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);            
            return Ok
                (
                    _workflowService.GetDocumentTypesByUserName(webSession.DecodedToken.UserName)
                    .Select(c => new SelectOptionItem() { Id = c.DocTypeID, Text = c.DocTypeID.Trim() + " - " + c.Description })
                );
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getnextactions/{Id}")]
        public IActionResult GetListNextActions([FromRoute]long Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);            
            return Ok(_documentService.GetNextActions(Id,webSession.DecodedToken.UserName));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getapprovalhistories/{Id}")]
        public IActionResult GetListApprovalHistories([FromRoute]long Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_documentService.GetApprovalHistories(Id, webSession.DecodedToken.UserName));
        }
    }
}