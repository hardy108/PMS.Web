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
using PMS.EFCore.Services.Attendances;
using PMS.Shared.Models;

using PMS.EFCore.Services.Logistic;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Logistic
{
    [ApiController]
    public class RKH1Controller : ControllerBase
    {
        private const string _CONTROLLER_NAME = "RKH1";
        private RKH1 _service;
        private AuthenticationServiceEstate _authenticationService;

        public RKH1Controller(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new RKH1(context, _authenticationService, auditContext);
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterRKH1 param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            param.UserName = webSession.DecodedToken.UserName;
            return Ok(_service.GetList(param));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GetListCount([FromQuery]FilterRKH1 param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            param.UserName = webSession.DecodedToken.UserName;
            return Ok(_service.GetListCount(param));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterRKH1 param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            param.UserName = webSession.DecodedToken.UserName;
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.RKH_CODE, Text = c.RKH_CODE }));
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
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/cancelapprove")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult CancelApprove([FromForm]IFormCollection data)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.CancelApprove(data, webSession.DecodedToken.UserName));
        }

    }
}
