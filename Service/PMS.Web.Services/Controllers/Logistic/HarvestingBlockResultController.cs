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


using PMS.EFCore.Services.Logistic;
using PMS.Shared.Models;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Logistic
{
    [ApiController]
    public class HarvestingBlockResultController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "HarvestingBlockResult"; //Replace with your own route name
        private HarvestingBlockResult _service;
        private AuthenticationServiceEstate _authenticationService;

        public HarvestingBlockResultController(AuthenticationServiceEstate authenticationService, PMSContextEstate context,AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new HarvestingBlockResult(context, _authenticationService, auditContext);
        }


        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterHarvestBlockResult param)
        {
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterHarvestBlockResult param)
        {
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.ID, Text = c.NOSPB}));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            FilterHarvestBlockResult param = new FilterHarvestBlockResult {NoSPB= Id};
            return Ok(_service.GetList(param));
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

        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/approve")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Approve([FromForm]IFormCollection data)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.Approve(data, webSession.DecodedToken.UserName));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/downloadspb")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult DownloadSPB([FromQuery]string source, string unitId, DateTime from, DateTime to)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.DownloadDataSPB(source,unitId,from,to,webSession.DecodedToken.UserName));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/servermill")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult ServerMill()
        {
            return Ok(_service.GetServerMill().Select(c => new SelectOptionItem() { Id = c.ToString(), Text = c.ToString() }));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getlistspb")]
        public IActionResult GetListForSelectSPB([FromQuery]FilterHarvestBlockResult param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            param = new FilterHarvestBlockResult{ UserName=webSession.DecodedToken.UserName } ;           
            return Ok(_service.GetListSPB(param));
        }
        

    }
}