using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ConnectionString
    {
        private Dictionary<string, string> _connectionString = new Dictionary<string, string>();
        public string FileStorage
        {
            get { return GetByName("FileStorage"); }
            set
            {
                try
                {
                    _connectionString.Add("FileStorage", value);
                }
                catch { _connectionString["FileStorage"]= value; }
            }
        }
        public string WF
        {
            get { return GetByName("WF"); }
            set
            {
                try
                {
                    _connectionString.Add("WF", value);
                }
                catch { _connectionString["WF"] = value; }
            }
        }

        public string AM
        {
            get { return GetByName("AM"); }
            set
            {
                try
                {
                    _connectionString.Add("AM", value);
                }
                catch { _connectionString["AM"] = value; }
            }
        }

        public string AMHO
        {
            get { return GetByName("AMHO"); }
            set
            {
                try
                {
                    _connectionString.Add("AMHO", value);
                }
                catch { _connectionString["AMHO"] = value; }
            }
        }
        public string PMS
        {
            get { return GetByName("PMS"); }
            set
            {
                try
                {
                    _connectionString.Add("PMS", value);
                }
                catch { _connectionString["PMS"] = value; }
            }
        }

        public string PMSHO
        {
            get { return GetByName("PMSHO"); }
            set
            {
                try
                {
                    _connectionString.Add("PMSHO", value);
                }
                catch { _connectionString["PMSHO"] = value; }
            }
        }

        public string GetByName(string name)
        {
            try { return _connectionString[name]; }
            catch { return string.Empty; }
        }
    }
}
