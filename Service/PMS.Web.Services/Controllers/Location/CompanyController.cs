using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using AM.EFCore.Services;
using PMS.EFCore.Services.Location;
using PMS.Shared.Models;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Location
{
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "Company";
        private Company _service;
        AuthenticationServiceEstate _authenticationService;

        public CompanyController(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new Company(context, _authenticationService, auditContext);
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

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterCompany param)
        {
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GetListCount([FromQuery]FilterCompany param)
        {
            return Ok(_service.GetListCount(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterCompany param)
        {
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.LEGALID, Text = c.LEGALID + " - " + c.NAME }));
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
    }
}