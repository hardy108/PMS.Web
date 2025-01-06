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


using PMS.EFCore.Services.Location;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Location
{
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "Organizations"; //Replace with your own route name
        private Organizations _service;
        private AuthenticationServiceEstate _authenticationService;

        public OrganizationsController(PMSContextEstate context,AuthenticationServiceEstate authenticationService, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new Organizations(context,_authenticationService, auditContext);
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            return Ok(_service.GetSingle(Id));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterMorganization param)
        {
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.ID, Text = c.ID+"-"+c.NAME }));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterMorganization param)
        {
            return Ok(_service.GetList(param));
        }


    }
}