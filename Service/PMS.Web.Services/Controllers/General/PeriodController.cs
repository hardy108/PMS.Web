using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using AM.EFCore.Services;
using PMS.EFCore.Services.General;
using PMS.Shared.Models;
using System.Linq;
using System;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.General
{
    //[Route("api/[controller]")]
    [ApiController]
    public class PeriodController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "Period";
        private Period _service;
        AuthenticationServiceEstate _authenticationService;

        public PeriodController(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new Period(context, _authenticationService, auditContext);
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            return Ok(_service.GetSingle(Id));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterPeriod param)
        {
            var period = _service.GetList(param).ToList();
            return Ok(period);
        }

        

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GetListCount([FromQuery]FilterPeriod param)
        {
            
            return Ok(_service.GetListCount(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterPeriod param)
        {
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.PERIODCODE, Text = c.REMARK }));
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
        public IActionResult Delete([FromForm]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.Delete(webSession.DecodedToken.UserName, Id));
        }
        
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/GetBy")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult GetBy([FromQuery]FilterPeriod param)
        {
            return Ok(_service.GetSingleBy(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/CheckValidPeriod")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult CheckPeriod([FromForm]string UnitCode, DateTime dateTime)
        {
            return Ok(_service.CheckPeriod(UnitCode, dateTime));
        }

    }
}