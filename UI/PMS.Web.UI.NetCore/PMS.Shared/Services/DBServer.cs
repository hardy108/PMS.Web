using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Services
{
    public enum DBServerType
    {
        PMS = 0,        
        FingerPrint,
        WB,
        Workflow,
        FileStorage,
        Audit
    }
    public class DBServer
    {
        public string UNITCODE { get; set; }
        public string ALIAS { get; set; }

        public DBServerType DBTYPE { get; set; }

        public string SERVER { get; set; }
        public string DBNAME { get; set; }

        public string DBUSER { get; set; }

        public string DBPASSWORD { get; set; }

    }
}
