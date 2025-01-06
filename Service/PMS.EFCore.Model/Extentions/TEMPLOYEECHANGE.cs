using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class TEMPLOYEECHANGE
    {
        [NotMapped()]
        public string STATUSNAME 
        { 
            get 
            { 
                if (this.STATUS != null)                
                    return this.STATUS.STATUSNAME;                
                return string.Empty;
            }
        }
        [NotMapped()]
        public string TAXSTATUS
        {
            get
            {
                if (this.STATUS != null)
                    return this.STATUS.TAXSTATUS;
                return string.Empty;
            }
        }

        [NotMapped()]
        public string FAMILYSTATUS
        {
            get
            {
                if (this.STATUS != null)
                    return this.STATUS.FAMILYSTATUS;
                return string.Empty;
            }
        }

        [NotMapped()]
        public string NEWTAXSTATUS
        {
            get
            {
                if (this.NEWSTATUS != null)
                    return this.NEWSTATUS.TAXSTATUS;
                return string.Empty;
            }
        }

        [NotMapped()]
        public string NEWFAMILYSTATUS
        {
            get
            {
                if (this.NEWSTATUS != null)
                    return this.NEWSTATUS.FAMILYSTATUS;
                return string.Empty;
            }
        }

        [NotMapped()]
        public string POSITIONNAME
        {
            get
            {
                if (this.POSITION != null)
                    return this.POSITION.POSITIONNAME;
                return string.Empty;
            }
        }

        [NotMapped()]
        public string DIVNAME
        {
            get
            {
                return DIVID;
            }
        }


        [NotMapped()]
        public string UPDATED_IN_TEXT
        {
            get
            {
                return UPDATED.ToString("yyyy-MMM-dd HH:mm:ss");
            }
        }

        [NotMapped()]
        public string CREATED_IN_TEXT
        {
            get
            {
                return CREATED.ToString("yyyy-MMM-dd HH:mm:ss");
            }
        }


      
    }
}
