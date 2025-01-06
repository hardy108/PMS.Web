using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using AM.EFCore.Services;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Hosting;
using PMS.Shared.Models;

using Microsoft.Extensions.Options;

namespace PMS.Web.Services.Controllers
{
    
    [ApiController]
    public class LoginController : ControllerBase
    {
        

        private AuthenticationServiceEstate _service;
        
        

        public LoginController(AuthenticationServiceEstate authenticationService)
        {
            _service = authenticationService;
            
        }

        [Authorize]
        [HttpGet]
        [Route("api/login/useraccess")]
        // GET api/<controller>
        public List<USERACCESS> UserAccess()
        {
            string menuID = Request.Query["MenuID"];
            return _service.GetAuthorizedMenu(Request.Headers, menuID, null);
        }

        [Authorize]
        [HttpGet]
        [Route("api/login/reportaccess")]
        // GET api/<controller>
        public List<string> ReportAccess()
        {
            string reportId = Request.Query["ReportID"];
            return _service.GetAuthorizedReport(Request.Headers, reportId, null);
        }

        [Authorize]
        [HttpGet]
        [Route("api/login/ismenuauthorized")]
        // GET api/<controller>
        public USERACCESS IsMenuAuthorized()
        {
            string menuID = Request.Query["MenuID"],
                   viewPath = Request.Query["ViewPath"];

           

            return _service.GetAuthorizedMenu(Request.Headers, menuID,null).FirstOrDefault();
            
        }


        [Authorize]
        [HttpGet]
        [Route("api/login/isreportauthorized")]
        // GET api/<controller>
        public string IsReportAuthorized()
        {
            string reportID = Request.Query["ReportID"];


            return _service.GetAuthorizedReport(Request.Headers, reportID, null).FirstOrDefault();

        }







        [Authorize]
        [HttpGet]
        [Route("api/login/getunits")]
        public IEnumerable<VUNIT> GetUnits()
        {
            string unitId = Request.Query["Id"];
            if (string.IsNullOrWhiteSpace(unitId))
                unitId = Request.Query["UnitID"];
            return _service.GetAuthorizedUnit(Request.Headers, unitId);
        }

        [Authorize]
        [HttpGet]
        [Route("api/login/getunitsforselect")]
        public IEnumerable<SelectOptionItem> GetUnitsForSelect()
        {
            string unitId = Request.Query["Id"];
            if (string.IsNullOrWhiteSpace(unitId))
                unitId = Request.Query["UnitID"];

            return _service.GetAuthorizedUnit(Request.Headers, unitId).Select(c => new SelectOptionItem() { Id = c.UNITCODE, Text = c.UNITCODE.Trim() + " - " +  c.NAME });
        }


        #region Get Division
        [Authorize]
        [HttpGet]
        [Route("api/login/getdivisions")]
        public IEnumerable<VDIVISI> GetDivisions()
        {
            string unitId = Request.Query["UnitID"],
                   divisiId = Request.Query["Id"];
            if (string.IsNullOrWhiteSpace(divisiId))
                divisiId = Request.Query["DivisionID"];
            return _service.GetAuthorizedDivisi(Request.Headers, unitId, divisiId);            
        }

        [Authorize]
        [HttpGet]
        [Route("api/login/getdivisionsforselect")]
        public  IEnumerable<SelectOptionItem> GetDivisionsForSelect()
        {
            string unitId = Request.Query["UnitID"],
                   divisiId = Request.Query["Id"];
            if (string.IsNullOrWhiteSpace(divisiId))
                divisiId = Request.Query["DivisionID"];

            return _service.GetAuthorizedDivisi(Request.Headers, unitId, divisiId)
                .Select(c => new SelectOptionItem() { Id = c.DIVID, Text = c.NAME });
        }
        #endregion

        #region Get Blocks
        [Authorize]
        [HttpGet]
        [Route("api/login/getblocks")]
        public IEnumerable<VBLOCK> GetBlocks()
        {
            string unitId = Request.Query["UnitID"],
                   divisiId = Request.Query["DivisionID"],
                   blockId = Request.Query["Id"];
            if (string.IsNullOrWhiteSpace(blockId))
                blockId = Request.Query["BlockID"];


            return _service.GetAuthorizedBlock(Request.Headers, unitId, divisiId, blockId);
        }

        [Authorize]
        [HttpGet]
        [Route("api/login/getblocksforselect")]
        public IEnumerable<SelectOptionItem> GetBlocksForSelect()
        {
            string unitId = Request.Query["UnitID"],
                   divisiId = Request.Query["DivisionID"],
                   blockId = Request.Query["Id"];
            if (string.IsNullOrWhiteSpace(blockId))
                blockId = Request.Query["BlockID"];


            return _service.GetAuthorizedBlock(Request.Headers, unitId, divisiId, blockId)
                .Select(c => new SelectOptionItem() { Id = c.BLOCKID, Text = c.NAME });

        }

        #endregion


    }
}