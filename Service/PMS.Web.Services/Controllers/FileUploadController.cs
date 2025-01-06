using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AM.EFCore.Services;
using FileStorage.EFCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace PMS.Web.Services.Controllers
{
    [Route("api/fileupload")]
    [Produces("application/json")]
    [Authorize]
    public class FileUploadController : Controller
    {

        const int BUF_SIZE = 4096;
        
        private FilestorageContext _context;
        private AuthenticationServiceEstate _authenticationService;

        public FileUploadController(AuthenticationServiceEstate authenticationService, FilestorageContext context)
        {
            _authenticationService = authenticationService;
            _context = context;
        }

       

        [HttpPost("base64")]
        public UploadResult UploadBase64([FromBody]Base64Model model)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);

            var bytes = Convert.FromBase64String(model.Base64);

            File file = new File
            {

                FileContent = bytes,
                FileType = model.MimeType,
                Created = DateTime.Now,
                CreatedBy = webSession.DecodedToken.UserName
            };

            _context.File.Add(file);
            _context.SaveChanges();

            return new UploadResult { ID = file.FileID, Name = model.Name, Length = bytes.Length };
        }

        [HttpPost("base64multi")]
        public IEnumerable<UploadResult> UploadBase64Multi([FromBody] IEnumerable<Base64Model> models)
        {
            var webSession = _authenticationService.LoginBySession(Request.Headers);

            if (models == null || models.Count() <= 0)
                return new List<UploadResult>();

            List<File> files = new List<File>();
            foreach (var model in models)
            {
                var bytes = Convert.FromBase64String(model.Base64);

                File file = new File
                {
                    FileContent = bytes,
                    FileType = model.MimeType,
                    Created = DateTime.Now,
                    CreatedBy = webSession.DecodedToken.UserName
                };
                files.Add(file);
            }
            _context.File.AddRange(files);
            _context.SaveChanges();

            return files.Select(d => new UploadResult { ID = d.FileID, Length = d.FileContent.Length, Name = d.FileID.ToString() }).ToList();
        }

        [HttpPost("files")]
        public IEnumerable<UploadResult> UploadFiles(IEnumerable<FormFileModel> models)
        {


            var webSession = _authenticationService.LoginBySession(Request.Headers);

            List<File> files = new List<File>();
            foreach (var model in models)
            {
                if (model.File == null || model.File.Length <= 0)
                    continue;

                using (var ms = new System.IO.MemoryStream())
                {
                    model.File.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    files.Add(new File
                    {
                        FileContent = fileBytes,
                        FileType = model.File.ContentType,
                        Created = DateTime.Now,
                        CreatedBy = webSession.DecodedToken.UserName
                    });
                }

            }

            if (files.Count > 0)
            {
                _context.File.AddRange(files);
                _context.SaveChanges();
                return files.Select(d => new UploadResult { ID = d.FileID, Name = d.FileID.ToString(), Length = d.FileContent.Length });
            }
            return new List<UploadResult>();
        }

        [HttpPost("file")]
        public UploadResult UploadFile(FormFileModel model)
        {


            var webSession = _authenticationService.LoginBySession(Request.Headers);


            if (model == null || model.File == null || model.File.Length <= 0)
                return null;

            using (var ms = new System.IO.MemoryStream())
            {
                model.File.CopyTo(ms);
                var fileBytes = ms.ToArray();
                File file = new File
                {
                    FileContent = fileBytes,
                    FileType = model.File.ContentType,
                    Created = DateTime.Now,
                    CreatedBy = webSession.DecodedToken.UserName
                };
                _context.File.Add(file);
                _context.SaveChanges();
                return new UploadResult { ID = file.FileID, Name = model.Description, Length = fileBytes.Length };
            }

        }

        [HttpPost("bytes")]
        public UploadResult UploadBytes([FromBody] BytesModel model)
        {


            var webSession = _authenticationService.LoginBySession(Request.Headers);


            if (model == null || model.Bytes == null || model.Bytes.Length <= 0)
                return null;

            File file = new File
            {
                FileContent = model.Bytes,
                FileType = model.MimeType,
                Created = DateTime.Now,
                CreatedBy = webSession.DecodedToken.UserName
            };
            _context.File.Add(file);
            _context.SaveChanges();
            return new UploadResult { ID = file.FileID, Name = file.FileID.ToString(), Length = file.FileContent.Length };


        }


    }


    public class Base64Model
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Base64 { get; set; }
        public string MimeType { get; set; }
    }

    public class BytesModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Bytes { get; set; }
        public string MimeType { get; set; }
    }

    public class FormFilesModel
    {
        public List<IFormFile> Files { get; set; }
    }

    public class FormFileModel
    {
        public IFormFile File { get; set; }
        public string Description { get; set; }
    }

    public class StreamModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UploadResult
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public long Length { get; set; }
    }

}