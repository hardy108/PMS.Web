using Newtonsoft.Json;
using PMS.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PMS.Shared.Services
{
    public class FilterServices
    {


        FilterJson _filter = new FilterJson();
        public FilterServices(string filterFile)
        {
            _filter = new FilterJson();
            try
            {
                using (StreamReader stream = System.IO.File.OpenText(filterFile))
                {
                    using (JsonTextReader reader = new JsonTextReader(stream))
                    {
                        var x = Newtonsoft.Json.JsonSerializer.Create();
                        _filter.AddRange(x.Deserialize<IEnumerable<FilterJsonRow>>(reader));


                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public FilterJson AllRows
        {
            get { return _filter; }
        }
                
    }
}
