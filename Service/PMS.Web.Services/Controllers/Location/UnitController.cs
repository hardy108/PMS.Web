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


using PMS.EFCore.Services.Location;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Location
{
    
    [ApiController]
    public class UnitController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "Unit"; //Replace with your own route name
        private Unit _service;
        private Block _blockService;
        private Divisi _divisiService;

        private AuthenticationServiceEstate _authenticationService;

        public UnitController(AuthenticationServiceEstate authenticationService, PMSContextEstate context,AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new Unit(context, _authenticationService, auditContext);
            _blockService = new Block(context, _authenticationService, auditContext);
            _divisiService = new Divisi(context, _authenticationService, auditContext);
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            return Ok(_service.GetSingle(Id));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]GeneralFilter param)
        {
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GetListCount([FromQuery]GeneralFilter param)
        {
            param.PageSize = 0;
            return Ok(_service.GetListCount(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getdivisions/{Id}")]
        public IActionResult GetUnits([FromRoute]string Id)
        {
            
            GeneralFilter filter = new FilterBlock { UnitID = Id };
            return Ok(_divisiService.GetList(filter));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getblocks/{Id}")]
        public IActionResult GetBlocks([FromRoute]string Id)
        {
            string divisiId = Request.Query["DivisionID"];
            
            FilterBlock filter = new FilterBlock { DivisionID = divisiId,UnitID = Id };
            return Ok(_blockService.GetList(filter));
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
    }
}