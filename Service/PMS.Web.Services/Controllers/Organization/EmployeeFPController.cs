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
using PMS.EFCore.Services.Organization;



using FP.EFCore.Model;
using FP.EFCore.Services;
using PMS.EFCore.Helper;

namespace PMS.Web.Services.Controllers.Organization
{
    
    [ApiController]
    public class EmployeeFPController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "EmployeeFP"; //Replace with your own route name


        private AuthenticationServiceEstate _authenticationService;
        private EmployeeFingerPrint _service;

        public EmployeeFPController(AuthenticationServiceEstate tokenService, FPContext fpContext, PMSContextEstate pmsContext, AuditContext auditContext)
        {
            _authenticationService = tokenService;
            _service = new EmployeeFingerPrint(fpContext, pmsContext,_authenticationService, auditContext);
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/initnew")]
        public IActionResult InitNew()
        {
            return Ok(_service.NewRecord(string.Empty));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public IActionResult Get([FromRoute]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetSingle(Id));
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getdetails/{Id}")]
        public IActionResult GetDetails([FromRoute]string Id)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_service.GetDetails(Id));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IActionResult GetList([FromQuery]FilterEmployee param)
        {
            if (param.WithDetails)
                return Ok(_service.GetListWithDetails(param));
            return Ok(_service.GetList(param));
        }

        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/listforselect")]
        public IActionResult GetListForSelect([FromQuery]FilterEmployee param)
        {
            return Ok(_service.GetListWithDetails(param).Select(c => new SelectOptionItem() { Id = c.EMPID, Text = c.EMPNAME }));
        }



     

        

        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/edit")]
        [Consumes("multipart/form-data")]
        public ActionResult SaveEdit([FromForm(Name = "photo")]IFormFile photo, [FromForm(Name = "fp1")]IFormFile fp1, [FromForm(Name = "fp2")]IFormFile fp2, [FromForm(Name = "fp3")]IFormFile fp3,[FromForm]IFormCollection data)
        {
            // Extract file name from whatever was posted by browser


            byte[] photoContents = null, fp1Contents = null, fp2Contents = null, fp3Contents = null;
            if (photo != null)
            {
                using (var fileContents = photo.OpenReadStream())
                {
                    photoContents = new byte[fileContents.Length];
                    fileContents.Read(photoContents, 0, photoContents.Length);
                }
            }


            if (fp1 != null)
            {
                using (var fileContents = fp1.OpenReadStream())
                {
                    fp1Contents = new byte[fileContents.Length];
                    fileContents.Read(fp1Contents, 0, fp1Contents.Length);
                }
            }

            if (fp2 != null)
            {
                using (var fileContents = fp2.OpenReadStream())
                {
                    fp2Contents = new byte[fileContents.Length];
                    fileContents.Read(fp2Contents, 0, fp2Contents.Length);
                }
            }

            if (fp3 != null)
            {
                using (var fileContents = fp3.OpenReadStream())
                {
                    fp3Contents = new byte[fileContents.Length];
                    fileContents.Read(fp3Contents, 0, fp3Contents.Length);
                }
            }



            var webSession = _authenticationService.LoginBySession(Request.Headers);

            return Ok(_service.SaveUpdate(data, photoContents, fp1Contents, fp2Contents, fp3Contents,webSession.DecodedToken.UserName));
        }


        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/new")]
        [Consumes("multipart/form-data")]
        public ActionResult SaveNew([FromForm(Name = "photo")]IFormFile photo, [FromForm(Name = "fp1")]IFormFile fp1, [FromForm(Name = "fp2")]IFormFile fp2, [FromForm(Name = "fp3")]IFormFile fp3, [FromForm]IFormCollection data)
        {
            // Extract file name from whatever was posted by browser


            byte[] photoContents = null, fp1Contents = null, fp2Contents = null, fp3Contents = null;
            if (photo != null)
            {
                using (var fileContents = photo.OpenReadStream())
                {
                    photoContents = new byte[fileContents.Length];
                    fileContents.Read(photoContents, 0, photoContents.Length);
                }
            }


            if (fp1 != null)
            {
                using (var fileContents = fp1.OpenReadStream())
                {
                    fp1Contents = new byte[fileContents.Length];
                    fileContents.Read(fp1Contents, 0, fp1Contents.Length);
                }
            }

            if (fp2 != null)
            {
                using (var fileContents = fp2.OpenReadStream())
                {
                    fp2Contents = new byte[fileContents.Length];
                    fileContents.Read(fp2Contents, 0, fp2Contents.Length);
                }
            }

            if (fp3 != null)
            {
                using (var fileContents = fp3.OpenReadStream())
                {
                    fp3Contents = new byte[fileContents.Length];
                    fileContents.Read(fp3Contents, 0, fp3Contents.Length);
                }
            }



            var webSession = _authenticationService.LoginBySession(Request.Headers);

            return Ok(_service.SaveInsert(data, photoContents, fp1Contents, fp2Contents, fp3Contents, webSession.DecodedToken.UserName));
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

        

    }
}