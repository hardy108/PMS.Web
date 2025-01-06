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

using PMS.Shared.Models;
using PMS.EFCore.Services.Logistic;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Logistic
{
    
    [ApiController]
    public class HarvestingController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "Harvesting"; //Replace with your own route name
        private Harvesting _service;
        private AuthenticationServiceEstate _authenticationService;

        public HarvestingController(AuthenticationServiceEstate authenticationService, PMSContextEstate context, AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _service = new Harvesting(context, _authenticationService, auditContext);
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterHarvest param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            param.UserName = webSession.DecodedToken.UserName;
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listcount")]
        public IActionResult GetListCount([FromQuery]FilterHarvest param)
        {
            return Ok(_service.GetListCount(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterHarvest param)
        {
            return Ok(_service.GetList(param).Select(c => new SelectOptionItem() { Id = c.HARVESTCODE, Text = c.HARVESTCODE }));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {

            return Ok(_service.GetSingle(Id));
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
        [Route("api/" + _CONTROLLER_NAME + "/generatebp")]
        public IActionResult GenerateBP([FromQuery]GeneralFilter filter)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            List<string> deletedBP = new List<string>(), insertedBP = new List<string>();
             _service.GenerateBP(filter.DivisionID, filter.Date, webSession.DecodedToken.UserName,deletedBP,insertedBP);                        
            return Ok();
        }


        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/employeecandidate")]
        public IActionResult GetEmployeeCandidate([FromQuery]string UnitID, [FromQuery]string SupervisorID, [FromQuery]string Keyword, [FromQuery]string CardType, [FromQuery]string EmployeeID, [FromQuery]DateTime date)
        {
            if (string.IsNullOrWhiteSpace(UnitID))
                return null;

            if (date == new DateTime())
                date = DateTime.Today;
            
            List<string> unitIds = _authenticationService.GetAuthorizedUnit(Request.Headers, UnitID).Select(d => d.UNITCODE).Distinct().ToList();
            if (unitIds == null || !unitIds.Any())
                return null;
            return Ok(_service.GetEmployeeCandidate(UnitID,SupervisorID,EmployeeID,Keyword, date, CardType));
            
        }


    }
}