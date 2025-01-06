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
    public class AccountTypeController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "AccountType"; //Replace with your own route name
        private AccountType _service;
        private AuthenticationServiceEstate _authenticationService;

        public AccountTypeController(AuthenticationServiceEstate authenticationService, PMSContextEstate context,AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new AccountType(context, _authenticationService,auditContext);
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]GeneralFilter param)
        {
            //string stringvalue = Status.Approved.ToString();
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]GeneralFilter param)
        {
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.ID.ToString(), Text = c.ID + " - " + c.NAME }));
        }

    }
}