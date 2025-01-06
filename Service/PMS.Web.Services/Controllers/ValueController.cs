using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace PMS.Web.Services.Controllers
{
    
    [ApiController]
    public class ValueController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        [Route("api/values")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost]
        [Route("api/values/testdictionary")]
        public ActionResult<IEnumerable<string>> TestDictionary([FromBody]dynamic dynamic)
        {
            string value1 = dynamic.Value1.Value,
                   value2 = dynamic.Value2.Value;
            return new string[] { value1.ToString(), value2.ToString() };
        }

        [HttpGet]
        [Route("api/values/testfilterget")]
        public ActionResult<IEnumerable<string>> TestFilterGet()
        {
            var query = Request.Query;
            string value1 = query["Value1"],
                   value2 = query["Value2"];
            return new string[] { value1.ToString(), value2.ToString() };
        }

    }
}