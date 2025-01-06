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
using PMS.EFCore.Services.Upkeep;
using AM.EFCore.Services;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers
{
    
    [ApiController]
    public class DoketController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "doket"; //Replace with your own route name
        private Doket _service;
        private AuthenticationServiceEstate _authenticationService;

        public DoketController(AuthenticationServiceEstate authenticationService,PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new Doket(context, _authenticationService, auditContext);
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public VDOKET Get([FromQuery]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return _service.GetSingle(Id);
           
        }

        
    }
}