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
using PMS.EFCore.Services.Attendances;


using PMS.EFCore.Services.GL;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.GL
{
    [ApiController]
    public class JournalTypeController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "JournalType"; //Replace with your own route name
        private JournalType _service;
        private AuthenticationServiceEstate _authenticationService;

        public JournalTypeController(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new JournalType(context, _authenticationService, auditContext);
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterJournal param)
        {
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterJournal param)
        {
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.CODE, Text = c.CODE + " - " + c.NAME }));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            return Ok(_service.GetSingle(Id));
        }


    }
}