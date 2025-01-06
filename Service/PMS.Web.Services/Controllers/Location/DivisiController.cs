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

namespace PMS.Web.Services.Controllers
{
    
    [ApiController]
    public class DivisiController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "Divisi"; //Replace with your own route name
        private Divisi _service;
        private Unit _unitService;
        private Block _blockService;

        private AuthenticationServiceEstate _authenticationService;

        public DivisiController (AuthenticationServiceEstate authenticationService, PMSContextEstate context,AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new Divisi(context,_authenticationService, auditContext);
            _unitService = new Unit(context,_authenticationService, auditContext);
            _blockService = new Block(context,_authenticationService, auditContext);
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
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            return Ok(_service.GetSingle(Id));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getparent/{Id}")]
        public IActionResult GetParent([FromRoute]string Id)
        {

            VDIVISI divisi = _service.GetSingle(Id);
            return Ok(_unitService.GetSingle(divisi.UNITCODE));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getblocks/{Id}")]
        public IActionResult GetBlocks([FromRoute]string Id)
        {
            
            FilterBlock filter = new FilterBlock { DivisionID = Id };
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