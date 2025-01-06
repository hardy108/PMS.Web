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
    public class ContractController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "contract"; //Replace with your own route name
        

        private AuthenticationServiceEstate _authenticationService;
        private Contract _contractService;
        
        public ContractController(AuthenticationServiceEstate authenticationService,PMSContextEstate context,AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _contractService = new Contract(context,authenticationService,auditContext);
        }

        
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/initnew")]
        public IActionResult InitNew()
        {
            return Ok(_contractService.NewRecord(string.Empty));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_contractService.GetSingleByUsername(webSession.DecodedToken.UserName, Id));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getitem")]
        public IActionResult GetItem([FromQuery]FilterContractItem filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_contractService.GetContractItem(filter));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GeList([FromQuery]GeneralFilter filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            filter.UserName = webSession.DecodedToken.UserName;
            return Ok(_contractService.GetList(filter));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GeListCount([FromQuery]GeneralFilter filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            filter.PageSize = 0;
            return Ok(_contractService.GetListCount(filter));
        }

        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/new")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult SaveNew([FromForm]IFormCollection data)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_contractService.SaveInsert(data, webSession.DecodedToken.UserName));
        }

        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/edit")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult SaveEdit([FromForm]IFormCollection data)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_contractService.SaveUpdate(data, webSession.DecodedToken.UserName));
        }


        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/delete")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Delete([FromForm]IFormCollection data)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_contractService.Delete(data, webSession.DecodedToken.UserName));
        }

        
    }
}