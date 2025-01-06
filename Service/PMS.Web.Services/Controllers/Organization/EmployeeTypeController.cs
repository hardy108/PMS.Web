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
    public class EmployeeTypeController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "employeeType";
        private EmployeeType _service;
        private AuthenticationServiceEstate _authenticationService;

        public EmployeeTypeController(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new EmployeeType(context, _authenticationService, auditContext);
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]GeneralFilter param)
        {
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]GeneralFilter param)
        {
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.ID, Text = c.NAME }));            
        }


        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/initnew")]
        public IActionResult InitNew()
        {
            return Ok(_service.NewRecord(string.Empty));
        }


        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            return Ok(_service.GetSingle(Id));
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

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/GetHarvestingType")]
        public IActionResult GetHarvestingType()
        {
            return Ok(_service.GetHarvestingType());
        }
    }
}