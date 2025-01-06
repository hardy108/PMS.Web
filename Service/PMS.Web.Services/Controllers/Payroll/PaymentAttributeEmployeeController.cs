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
using PMS.Shared.Models;
using PMS.Shared.Utilities;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Payroll
{
    [ApiController]
    public class PaymentAttributeEmployeeController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "PaymentAttributeEmployeeController"; //Replace with your own route name
        private PaymentAttributeEmployee _service;
        private AuthenticationServiceEstate _authenticationService;

        public PaymentAttributeEmployeeController(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new PaymentAttributeEmployee(context, _authenticationService, auditContext);
        }


        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/initnew")]
        public IActionResult InitNew()
        {
            return Ok(_service.NewRecord(string.Empty));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterSPB param)
        {
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GeListCount([FromQuery]FilterSPB filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            filter.PageSize = 0;
            return Ok(_service.GetListCount(filter));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]GeneralFilter param)
        {
            return Ok(_service.GetAll(param));
        }


        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id, [FromQuery]bool withAccount = false)
        {
            return Ok(_service.GetSingle(Id, withAccount));
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




    }
}