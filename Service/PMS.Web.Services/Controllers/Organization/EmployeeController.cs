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
using PMS.Shared.Models;
using PMS.EFCore.Services;
using AM.EFCore.Services;
using PMS.EFCore.Services.Organization;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Organization
{
    
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "Employee"; //Replace with your own route name


        private AuthenticationServiceEstate _authenticationService;
        private EFCore.Services.Organization.Employee _service;

        public EmployeeController(AuthenticationServiceEstate tokenService,PMSContextEstate context,AuditContext auditContext)
        {
            _authenticationService = tokenService;
            _service = new EFCore.Services.Organization.Employee(context,_authenticationService,auditContext);
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
            var record = _service.GetSingle(Id);
            if (record != null)
                record.EmployeeChangePermission = _service.GetEmployeeChangePermission(webSession.DecodedToken.UserName);
            return Ok(record);
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getdetails/{Id}")]
        public IActionResult GetDetails([FromRoute]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetDetails(Id));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterEmployee param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            if (param.ByUserName)
            {
                param.UserName = webSession.DecodedToken.UserName;
            }
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GetListCount([FromQuery]FilterEmployee param)
        {
            param.PageSize = 0;
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            if (param.ByUserName)
            {
                param.UserName = webSession.DecodedToken.UserName;
            }            
            return Ok(_service.GetListCount(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterEmployee param)
        {
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.EMPID, Text = c.EMPCODE.Trim() + " - " + c.EMPNAME }));
        }



        //[Authorize]
        //[HttpPost]
        //[Route("api/" + _CONTROLLER_NAME + "/new")]
        //[Consumes("application/x-www-form-urlencoded")]
        //public IActionResult SaveNew([FromForm]IFormCollection data)
        //{
        //    var webSession = _authenticationService.LoginBySession(Request.Headers);
        //    return Ok(_service.SaveInsert(data, webSession.DecodedToken.UserName));
            
        //}

        //[Authorize]
        //[HttpPost]
        //[Route("api/" + _CONTROLLER_NAME + "/edit")]
        //[Consumes("application/x-www-form-urlencoded")]
        //public IActionResult SaveEdit([FromForm]IFormCollection data)
        //{
        //    var webSession = _authenticationService.LoginBySession(Request.Headers);
            
        //    return Ok(_service.SaveUpdate(data, webSession.DecodedToken.UserName));
        //}


        //[Authorize]
        //[HttpPost]
        //[Route("api/" + _CONTROLLER_NAME + "/delete")]
        //[Consumes("application/x-www-form-urlencoded")]
        //public IActionResult Delete([FromForm]IFormCollection data)
        //{
        //    var webSession = _authenticationService.LoginBySession(Request.Headers);
            
        //    return Ok(_service.Delete(data, webSession.DecodedToken.UserName));
        //}

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getattachments/{Id}")]
        public IActionResult GetAttachments([FromRoute]string Id, [FromServices]FileStorage.EFCore.FilestorageContext fileContext)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetAttachments(Id, fileContext));
        }


        //[Authorize]
        //[HttpGet]
        //[Route("api/" + _CONTROLLER_NAME + "/listdeletedbypayroll")]
        //public IActionResult GetListDeletedByPayroll([FromQuery]string keyword,[FromQuery]bool? status, [FromServices]PMSContextHO contextHO)
        //{
        //    var webSession = _authenticationService.LoginBySession(Request.Headers);
        //    return Ok(_service.ListDeleteEmployeeByPayroll(contextHO, keyword, status, webSession.DecodedToken.UserName));
        //}

        //[Authorize]
        //[HttpGet]
        //[Route("api/" + _CONTROLLER_NAME + "/processdeletebypayroll")]
        //public IActionResult ProcessListDeletedByPayroll([FromQuery]string keyword, [FromQuery]bool? status, [FromServices]PMSContextHO contextHO)
        //{
        //    var webSession = _authenticationService.LoginBySession(Request.Headers);
        //    _service.ProcessDeleteEmployeeByPayroll(contextHO, keyword, webSession.DecodedToken.UserName);
        //    return Ok(_service.ListDeleteEmployeeByPayroll(contextHO, keyword, status, webSession.DecodedToken.UserName));
        //}


    }
}