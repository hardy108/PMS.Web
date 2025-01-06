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
    public class EMInspectionController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "em-inspection"; //Replace with your own route name
        

        private AuthenticationServiceEstate _authenticationService;
        private EMInspection _service;
        
        public EMInspectionController(AuthenticationServiceEstate authenticationService,PMSContextEstate context,AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new EMInspection(context,authenticationService,auditContext);
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
            return Ok(_service.GetSingleByUsername(webSession.DecodedToken.UserName, Id));
        }

        

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GeList([FromQuery]GeneralFilter filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            filter.UserName = webSession.DecodedToken.UserName;
            return Ok(_service.GetList(filter));
        }

        
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GeListCount([FromQuery]GeneralFilter filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            filter.PageSize = 0;
            return Ok(_service.GetListCount(filter));
        }

        
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/new")]        
        public IActionResult SaveNew([FromBody]EM_TINSPECTION data)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.SaveInsert(data, webSession.DecodedToken.UserName));
        }

        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/bulkinsert")]
        public IActionResult SaveNewMultiple([FromBody]List<EM_TINSPECTION> data)
        {

            var webSession = _authenticationService.LoginBySession(Request.Headers);
            _service.SaveInsertMultiple(data, webSession.DecodedToken.UserName, true);
            return Ok();
        }


        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/edit")]        
        public IActionResult SaveEdit([FromBody]EM_TINSPECTION data)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.SaveUpdate(data, webSession.DecodedToken.UserName));
        }


        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/bulkupdate")]
        public IActionResult SaveUpdateMultiple([FromBody] List<EM_TINSPECTION> data)
        {

            var webSession = _authenticationService.LoginBySession(Request.Headers);
            _service.SaveUpdateMultiple(data, webSession.DecodedToken.UserName, true);
            return Ok();
        }



        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/delete")]        
        public IActionResult Delete([FromBody] EM_TINSPECTION data)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.Delete(data, webSession.DecodedToken.UserName));
        }

        
    }
}