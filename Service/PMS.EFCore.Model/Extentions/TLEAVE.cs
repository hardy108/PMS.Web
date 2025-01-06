using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class TLEAVE
    {
        [NotMapped]
        public string EMPNAME { get; set; }

        [NotMapped]
        public string PROCESSRESULT         
        { 
            get 
            { 
                switch(PROCESS)
                {
                    case 0:
                        return "Unprocessed";
                    case 1:
                        return "Processed";
                    case 2:
                        return "Error";                        
                }
                return "Unknown";
            } 
        }

        [NotMapped]
        public string LEAVETYPENAME
        {
            get;
            set;
        }

        [NotMapped]
        public string ABSENTCODE
        {
            get;
            set;
        }

        [NotMapped]
        public string DIVID
        {
            get;
            set;
        }

        [NotMapped]
        public string DATE_IN_TEXT
        {
            get { return DATE.ToString("dd-MMM-yyyy"); }            
        }

        [NotMapped]
        public string DATEFROM_IN_TEXT
        {
            get { return DATEFROM.ToString("dd-MMM-yyyy"); }
        }

        [NotMapped]
        public string DATETO_IN_TEXT
        {
            get { return DATETO.ToString("dd-MMM-yyyy"); }
        }

        [NotMapped]
        public string LEAVEDATE_IN_TEXT
        {
            get { return $"{DATEFROM:'dd MMM yyyyy} - {DATETO:'dd MMM yyyyy}"; }
        }

    }
}


