using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using AM.EFCore.Services;

using Microsoft.AspNetCore.Hosting;
using PMS.Shared.Models;
using Microsoft.Extensions.Options;

namespace PMS.Web.Services.Controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public class DoketGalleryController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "doketgallery"; //Replace with your own route name
        private string imageFolder = string.Empty;
        

        

        private AuthenticationServiceEstate _authenticationService;
        IOptions<AppSetting> _appSetting;
        PMSContextEstate _context;
        public DoketGalleryController(AuthenticationServiceEstate authenticationService,PMSContextEstate context,IOptions<AppSetting> appSetting)
        {
            _authenticationService = authenticationService;
            _context = context;
            imageFolder = appSetting.Value.DHSPhotoFolder;
            _appSetting = appSetting;
        }

       

        //GetDoketImageThumbnail
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/imagetn")]
        public HttpResponseMessage GetImageThumbnail([FromQuery]string divisionId, [FromQuery]string fileName, [FromQuery]int? width, [FromQuery]int? height,[FromServices]IHostingEnvironment env)
        {
            string filePath = string.Empty;
            if (width == null)
                width = 200;
            else if (!width.HasValue)
                width = 200;
            else if (width.Value <= 0)
                width = 200;


            //if (string.IsNullOrWhiteSpace(imageFolder))
            //{
            //    try
            //    {
            //        /*/*/*/*imageFolder = db.DHS_SYSVALUES.SingleOrDefault(d => d.FLD_ID.Equals("APIPHOTODIR")).FLD_VALUE1;*/*/*/*/
            //    }
            //    catch (Exception ex)
            //    {
            //        imageFolder = string.Empty;
            //    }

            //}

            try
            {
                if (string.IsNullOrWhiteSpace(imageFolder))
                    imageFolder = _appSetting.Value.DHSPhotoFolder;
            }
            catch { imageFolder = string.Empty; }

            if (string.IsNullOrWhiteSpace(fileName))
                filePath = env.WebRootPath +  "\\Content\\no-image.png";
            else
            {
                if (string.IsNullOrWhiteSpace(divisionId))
                    filePath = imageFolder.Replace("\\\\", "\\") + fileName;
                else
                    filePath = imageFolder.Replace("\\\\", "\\") + divisionId + "\\" + fileName;

                if (!System.IO.File.Exists(filePath))
                    filePath = env.WebRootPath + "\\Content\\no-image.png";
            }
            filePath = filePath.ToLower().Trim();
            NetVips.Image image = NetVips.Image.Thumbnail(filePath, width.Value, height);
            byte[] buffer = image.PngsaveBuffer();
            MemoryStream ms = new MemoryStream(buffer);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(ms);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            return response;
        }
        //GetDoketImage
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/image")]
        public HttpResponseMessage GetImage([FromQuery]string divisionId, [FromQuery]string fileName, [FromServices]IHostingEnvironment env)
        {
            string filePath = string.Empty;

            //if (string.IsNullOrWhiteSpace(imageFolder))
            //{
            //    try
            //    {
            //        imageFolder = db.DHS_SYSVALUES.SingleOrDefault(d => d.FLD_ID.Equals("APIPHOTODIR")).FLD_VALUE1;
            //    }
            //    catch (Exception ex)
            //    {
            //        imageFolder = string.Empty;
            //    }

            //}

            try
            {
                if (string.IsNullOrWhiteSpace(imageFolder))
                    imageFolder = _appSetting.Value.DHSPhotoFolder;
            }
            catch { imageFolder = string.Empty; }

            if (string.IsNullOrWhiteSpace(fileName))
                filePath = env.WebRootPath + "\\Content\\no-image.png";
            else
            {
                if (string.IsNullOrWhiteSpace(divisionId))
                    filePath = imageFolder.Replace("\\\\", "\\") + fileName;
                else
                    filePath = imageFolder.Replace("\\\\", "\\") + divisionId + "\\" + fileName;
                if (!System.IO.File.Exists(filePath))
                    filePath = env.WebRootPath + "\\Content\\no-image.png";
            }
            filePath = filePath.ToLower().Trim();
            NetVips.Image image = NetVips.Image.NewFromFile(filePath);
            byte[] buffer = image.PngsaveBuffer();
            MemoryStream ms = new MemoryStream(buffer);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(ms);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            return response;
        }

        //GetDoketCount
        [HttpGet]
        [Authorize]
        [Route("api/" + _CONTROLLER_NAME + "/count")]
        public int GetDoketCount([FromQuery]FilterDoket param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            try
            {
                var doketListCount = _context.fn_DHS_Doket_ListCount(param.UnitID, param.DivisionID, param.BlokId, param.StartDate, param.EndDate, param.Status).FirstOrDefault();
                if (doketListCount != null)
                {
                    if (doketListCount.DoketCount.HasValue)
                        return doketListCount.DoketCount.Value;
                }
                return 0;
            }
            catch { return 0; }
        }
        //GetDoketList
        [HttpGet]
        [Authorize]
        [Route("api/" + _CONTROLLER_NAME + "/list")]
        public IEnumerable<DHS_DOKET> GetDoketList([FromQuery]FilterDoket param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            
            try
            {
                return _context.fn_DHS_Doket_List(param.UnitID, param.DivisionID, param.BlokId, param.StartDate, param.EndDate, param.Status, param.RowNoStart, param.RowNoEnd).ToList();
            }
            catch { return new List<DHS_DOKET>(); }
        }

        [HttpGet]
        [Authorize]
        [Route("api/" + _CONTROLLER_NAME + "/get")]
        public fn_DHS_Doket_GetById_Result GetDoket([FromQuery]FilterDoket param)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            
            try
            {
                return _context.fn_DHS_Doket_GetById(param.DivisionID, param.DoketId).FirstOrDefault();
            }
            catch { return new fn_DHS_Doket_GetById_Result(); }
        }
    }
}