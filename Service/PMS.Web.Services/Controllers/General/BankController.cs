using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using PMS.EFCore.Services.General;
using PMS.Shared.Models;
using AM.EFCore.Services;
using Microsoft.AspNetCore.Authorization;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.General
{
    [ApiController]
    public class BankController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "bank";
        private Bank _service;
        private AuthenticationServiceEstate _authenticationService;
        
        public BankController(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new Bank(context, _authenticationService, auditContext);
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterPosition param)
        {
            try
            {
                return Ok(_service.GetList(param));
            }
            catch(Exception ex) { return BadRequest(ex); }
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GetListCount([FromQuery]FilterPosition param)
        {
            return Ok(_service.GetListCount(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterPosition param)
        {
            try
            {
                return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.BANKID, Text = c.BANKNAME }));
            }
            catch (Exception ex) { return BadRequest(ex); }
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            try
            {
                return Ok(_service.GetSingle(Id));
            }
            catch (Exception ex) { return BadRequest(ex); }
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
    }
}