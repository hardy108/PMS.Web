using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PMS.Shared.Models;
using PMS.Shared.Utilities;

namespace PMS.Web.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilitiesController : ControllerBase
    {
        private readonly AppSetting _appSetting;
        private readonly IHostingEnvironment _hostingEnvironment;

        public UtilitiesController(IOptions<AppSetting> options, IHostingEnvironment hostingEnvirontment)
        {
            _appSetting = options.Value;
            _hostingEnvironment = hostingEnvirontment;
        }

        [Route("appconfig")]
        public string GetAppConfig()
        {
            var config = Newtonsoft.Json.JsonConvert.SerializeObject(_appSetting);
            config = PMSEncryption.Encrypt(config, "mps");
            return config;
        }

        [Route("file")]
        public string GetJson([FromQuery]string path)
        {
            string file = _hostingEnvironment.ContentRootPath + "/" + path;
            // Open the file to read from.
            if (!System.IO.File.Exists(file))
                throw new Exception($"File {path} is not found");

            string readText = System.IO.File.ReadAllText(file);
            readText = PMSEncryption.Encrypt(readText, "mps");            
            return readText;
        }


    }
}