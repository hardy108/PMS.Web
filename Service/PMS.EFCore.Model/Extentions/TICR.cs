using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Linq;


namespace PMS.EFCore.Model
{
    public partial class TICR
    {
        [NotMapped()]
        public List<string> APPIDs
        {
            get { return TICRAPP.Select(d => d.APPID).ToList(); }
           
        }


        [NotMapped()]
        public string CREATEDDATE_IN_TEXT
        {
            get { return CREATED.ToString("dd-MMM-yyyy"); }            
        }
    }
}
