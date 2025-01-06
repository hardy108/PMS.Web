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
using Microsoft.AspNetCore.Authorization;
using PMS.EFCore.Services.Integration;
using PMS.EFCore.Helper;
using Microsoft.Extensions.Options;
using PMS.Shared.Models;

namespace PMS.Web.Services.Controllers.integration
{
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "Integration"; //Replace with your own route name
        private Integration _service;
        IOptions<AppSetting> _appsetting;

        private AuthenticationServiceEstate _authenticationService;

        public IntegrationController(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext, IOptions<AppSetting> appSetting)
        {
            _authenticationService = authenticationService;
            _service = new Integration(context,authenticationService,auditContext,appSetting);
            _appsetting = appSetting;
        }

        //[HttpGet]
        //[Route("api/" + _CONTROLLER_NAME + "/initnew")]
        //public IActionResult InitNew()
        //{
        //    return Ok(_service.NewRecord(string.Empty));
        //}

        [HttpGet]
        [Route("api/[controller]/compname/{Code}")]
        public Task<string> GetCompanyCodeDetailAsync([FromRoute]string Code)
        {

            //var service = new SAPIntegration.SAPConnectorInterface(_appsetting);
            var test = _service.CompName(Code);
            //var test = "testing";
            return test;
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/sapposting")]
        public ActionResult<bool> SapPosting()
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.PMSToSapRFC("test"));
        }

        //[Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/TesRFC")]
        public ActionResult<bool> TesRFC()
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.TesRFC("test"));
        }

        //[HttpGet]
        //[Route("api/" + _CONTROLLER_NAME + "/list")]
        //public IActionResult GetList([FromQuery]FilterBlock param)
        //{
        //    return Ok(_service.GetList(param));
        //}

        //[HttpGet]
        //[Route("api/" + _CONTROLLER_NAME + "/listcount")]
        //public IActionResult GetListCount([FromQuery]FilterBlock param)
        //{
        //    return Ok(_service.GetListCount(param));
        //}

        //[HttpGet]
        //[Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        //public IActionResult Get([FromRoute]string Id)
        //{
        //    return Ok(_service.GetSingle(Id));
        //}

        //[HttpGet]
        //[Route("api/" + _CONTROLLER_NAME + "/getparent/{Id}")]
        //public IActionResult GetParent([FromRoute]string Id)
        //{
        //    return Ok(_service.GetParent(Id));
        //}

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
    }
}