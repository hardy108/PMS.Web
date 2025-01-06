using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileStorage.EFCore;
using AM.EFCore.Services;
using Microsoft.AspNetCore.Authorization;
using PMS.EFCore.Model;
using System.IO;

namespace PMS.Web.Services.Controllers
{
    
    [ApiController]
    public class FileController : ControllerBase
    {
        private const string _CONTROLLER_NAME = "File"; //Replace with your own route name
        private FilestorageContext _context;
        private AuthenticationServiceEstate _authenticationService;
        public FileController(AuthenticationServiceEstate authenticationService, FilestorageContext context)
        {
            _authenticationService = authenticationService;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/get/{Id}")]
        public FileResult Get([FromRoute]long? Id)
        {
            if (!Id.HasValue)
                throw new Exception("Invalid file Id");
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            string fileName = Request.Query["FileName"];
            FileStorage.EFCore.File file = _context.GetFile(Id.Value);
            if (file == null)
                throw new Exception("$File {Id} is not valid");
            if (file.FileContent == null || file.FileContent.Length<=0)
                throw new Exception("$File {Id} has no contents");
            System.IO.Stream stream = new System.IO.MemoryStream(file.FileContent);
            if (string.IsNullOrWhiteSpace(fileName))
                return File(stream, "application/octet-stream");
            return File(stream, "application/octet-stream", fileName);
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getinfo/{Id}")]
        public object GetFileInfo([FromRoute] long Id)
        {
            var file = _context.File.Where(d => d.FileID == Id)
                .Select(d => new FileStorage.EFCore.File
                {
                    FileID = d.FileID,
                    FileType = d.FileType,
                    Created = d.Created,
                    CreatedBy = d.CreatedBy,
                    UsedBy  = d.UsedBy
                });
            if (file == null)
                throw new Exception("$File {Id} is not valid");
            return file.FirstOrDefault();
        }

        [Authorize]
        [HttpGet]
        [Route("api/" + _CONTROLLER_NAME + "/getbase64/{Id}")]
        public Base64Model GetBase64([FromRoute]long? Id)
        {
            if (!Id.HasValue)
                throw new Exception("Invalid file Id");
            var webSession = _authenticationService.LoginBySession(Request.Headers);
            string fileName = Request.Query["FileName"];
            FileStorage.EFCore.File file = _context.GetFile(Id.Value);
            if (file == null)
                throw new Exception("$File {Id} is not valid");
            if (file.FileContent == null || file.FileContent.Length <= 0)
                throw new Exception("$File {Id} has no contents");
            var contents = Convert.ToBase64String(file.FileContent);
            return new Base64Model { Base64 = contents, Description = file.FileID.ToString(), MimeType = file.FileType, Name = file.FileID.ToString() };
        }

        [Authorize]
        [HttpPost]        
        [Route("api/" + _CONTROLLER_NAME + "/upload")]
        [Consumes("multipart/form-data")]
        public ActionResult UploadFile([FromForm(Name = "fileContents")]IFormFile file)
        {
            // Extract file name from whatever was posted by browser
            string fileType = file.ContentType;

            byte[] contents = null;
            using (var fileContents = file.OpenReadStream())
            {
                contents = new byte[fileContents.Length];
                fileContents.Read(contents, 0, contents.Length);
            }
            if (contents == null)
                throw new Exception("Invalid file contents");

            var webSession = _authenticationService.LoginBySession(Request.Headers);
            return Ok(_context.UploadFile(contents,  fileType,webSession.DecodedToken.UserName));
        }

        [HttpPost]
        [Authorize]
        [Route("api/" + _CONTROLLER_NAME + "/uploadjson")]
        [Produces("application/json")]
        public ActionResult UploadFileJson(FormFileModel model)
        {


            var webSession = _authenticationService.LoginBySession(Request.Headers);
            

            if (model == null || model.File == null || model.File.Length <= 0)
                return null;

            using (var ms = new MemoryStream())
            {
                model.File.CopyTo(ms);
                var contents = ms.ToArray();
                return Ok(_context.UploadFile(contents, model.File.ContentType, webSession.DecodedToken.UserName));
            }

        }


        [Authorize]
        [HttpPost]
        [Route("api/" + _CONTROLLER_NAME + "/delete")]        
        public ActionResult Delete([FromBody]FileStorage.EFCore.File file)
        {
            
            var webSession = _authenticationService.LoginBySession(Request.Headers);

            return Ok(_context.DeleteFile(file.FileID,webSession.DecodedToken.UserName));
        }
    }

   
}