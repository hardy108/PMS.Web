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
using PMS.EFCore.Services.Logistic;
using PMS.EFCore.Services;
using AM.EFCore.Services;
using PMS.EFCore.Services.Upkeep;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers
{

    [ApiController]
    [Authorize]
    public class EMTrackingController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "em-tracking"; //Replace with your own route name
        

        private AuthenticationServiceHO _authenticationService;
        private EMTracking _service;
        
        public EMTrackingController(AuthenticationServiceHO authenticationService,PMSContextHO context,AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new EMTracking(context,authenticationService,auditContext);
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
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetTrackingInfo(Id));
        }

        

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GeList([FromQuery]GeneralFilter filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            filter.UserName = webSession.DecodedToken.UserName;
            return Ok(_service.GetListTrackingInfo(filter));
        }

        
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GeListCount([FromQuery]GeneralFilter filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            filter.PageSize = 0;
            return Ok(_service.GetListCountTrackingInfo(filter));
        }

        
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/new")]        
        public IActionResult SaveNew([FromBody]EM_TTRACKING data)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.SaveInsert(data, webSession.DecodedToken.UserName));
        }

        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/bulkinsert")]
        public IActionResult SaveNewMultiple([FromBody]List<EM_TTRACKING> data)
        {

            var webSession = _authenticationService.LoginBySession(Request.Headers);
            _service.SaveInsertMultiple(data, webSession.DecodedToken.UserName, true);
            return Ok();
        }


        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/edit")]        
        public IActionResult SaveEdit([FromBody] EM_TTRACKING data)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.SaveUpdate(data, webSession.DecodedToken.UserName));
        }


        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/bulkupdate")]
        public IActionResult SaveUpdateMultiple([FromBody] List<EM_TTRACKING> data)
        {

            var webSession = _authenticationService.LoginBySession(Request.Headers);
            _service.SaveUpdateMultiple(data, webSession.DecodedToken.UserName, true);
            return Ok();
        }



        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/delete")]        
        public IActionResult Delete([FromBody] EM_TTRACKING data)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.Delete(data, webSession.DecodedToken.UserName));
        }


        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/linestring")]
        public IActionResult GetLineString([FromQuery]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);            
            return Ok(_service.GenerateGeoJsonLineStringByTrackingId(Id));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/points")]
        public IActionResult GetTrackingPOints([FromQuery]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetTrackingPoints(Id));
        }

    }
}