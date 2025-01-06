using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class AppSetting
    {
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        public string ConfigEncrytionKey
        {
            get 
            {
                string key = GetSettingByName("ConfigEncrytionKey");
                if (string.IsNullOrWhiteSpace(key))
                    key = "mps";
                return key;
            }
            set
            {
                try
                {
                    _settings.Add("ConfigEncrytionKey", value);
                }
                catch { _settings["ConfigEncrytionKey"] = value; }
            }
        }

        public string FileStorageFolder
        {
            get { return GetSettingByName("FileStorageFolder"); }
            set
            {
                try
                {
                    _settings.Add("FileStorageFolder", value);
                }
                catch { _settings["FileStorageFolder"] = value; }
            }
        }
        public string MenuJsonFile
        {
            get { return GetSettingByName("MenuJsonFile"); }
            set
            {
                try
                {
                    _settings.Add("MenuJsonFile", value);
                }
                catch { _settings["MenuJsonFile"] = value; }
            }
        }
        public string ReportJsonFile
        {
            get { return GetSettingByName("ReportJsonFile"); }
            set
            {
                try
                {
                    _settings.Add("ReportJsonFile", value);
                }
                catch { _settings["ReportJsonFile"] = value; }
            }
        }
        public string ReportGroupJsonFile
        {
            get { return GetSettingByName("ReportGroupJsonFile"); }
            set
            {
                try
                {
                    _settings.Add("ReportGroupJsonFile", value);
                }
                catch { _settings["ReportGroupJsonFile"] = value; }
            }
        }
        public string LOVJsonFile
        {
            get { return GetSettingByName("LOVJsonFile"); }
            set
            {
                try
                {
                    _settings.Add("LOVJsonFile", value);
                }
                catch { _settings["LOVJsonFile"] = value; }
            }
        }
        public string ListFieldsFolder
        {
            get { return GetSettingByName("ListFieldsFolder"); }
            set
            {
                try
                {
                    _settings.Add("ListFieldsFolder", value);
                }
                catch { _settings["ListFieldsFolder"] = value; }
            }
        }
        public string FilterFolder
        {
            get { return GetSettingByName("FilterFolder"); }
            set
            {
                try
                {
                    _settings.Add("FilterFolder", value);
                }
                catch { _settings["FilterFolder"] = value; }
            }
        }

        public string DHSPhotoFolder
        {
            get { return GetSettingByName("DHSPhotoFolder"); }
            set
            {
                try
                {
                    _settings.Add("DHSPhotoFolder", value);
                }
                catch { _settings["DHSPhotoFolder"] = value; }
            }
        }

        public int MaxDataListRows
        {
            get
            {
                int i = 0;
                int.TryParse(GetSettingByName("MaxDataListRows"), out i);
                return i;

            }
            set
            {
                try
                {
                    _settings.Add("MaxDataListRows", value.ToString());
                }
                catch { _settings["MaxDataListRows"] = value.ToString(); }
            }
        }

        public string WebApiUrl
        {
            get
            {
                return GetSettingByName("WebApiUrl");

            }
            set
            {
                try
                {
                    _settings.Add("WebApiUrl", value);
                }
                catch { _settings["WebApiUrl"] = value; }
            }
        }

        public string DHSHost
        {
            get
            {
                return GetSettingByName("DHSHost");

            }
            set
            {
                try
                {
                    _settings.Add("DHSHost", value);
                }
                catch { _settings["DHSHost"] = value; }
            }
        }

        public string LoginPage
        {
            get
            {
                return GetSettingByName("LoginPage");

            }
            set
            {
                try
                {
                    _settings.Add("LoginPage", value);
                }
                catch { _settings["LoginPage"] = value; }
            }
        }

        public string ChangePasswordPage
        {
            get
            {
                return GetSettingByName("ChangePasswordPage");

            }
            set
            {
                try
                {
                    _settings.Add("ChangePasswordPage", value);
                }
                catch { _settings["ChangePasswordPage"] = value; }
            }
        }

        public string HomePage
        {
            get
            {
                return GetSettingByName("HomePage");

            }
            set
            {
                try
                {
                    _settings.Add("HomePage", value);
                }
                catch { _settings["HomePage"] = value; }
            }
        }

        public string GoogleMapLink
        {
            get
            {
                return GetSettingByName("GoogleMapLink");

            }
            set
            {
                try
                {
                    _settings.Add("GoogleMapLink", value);
                }
                catch { _settings["GoogleMapLink"] = value; }
            }
        }


        public int ListPageSize
        {
            get
            {
                int i = 0;
                int.TryParse(GetSettingByName("ListPageSize"), out i);
                return i;

            }
            set
            {
                try
                {
                    _settings.Add("ListPageSize", value.ToString());
                }
                catch { _settings["ListPageSize"] = value.ToString(); }
            }
        }

        public int MaxAttachmentFileSize
        {
            get
            {
                int i = 0;
                int.TryParse(GetSettingByName("MaxAttachmentFileSize"), out i);
                return i;

            }
            set
            {
                try
                {
                    _settings.Add("MaxAttachmentFileSize", value.ToString());
                }
                catch { _settings["MaxAttachmentFileSize"] = value.ToString(); }
            }
        }

        public string TokenSigningKey 
        {
            get
            {
                return GetSettingByName("TokenSigningKey");

            }
            set
            {
                try
                {
                    _settings.Add("TokenSigningKey", value);
                }
                catch { _settings["TokenSigningKey"] = value; }
            }
        }

        public string TokenIssuer
        {
            get
            {
                return GetSettingByName("TokenIssuer");

            }
            set
            {
                try
                {
                    _settings.Add("TokenIssuer", value);
                }
                catch { _settings["TokenIssuer"] = value; }
            }
        }


        public string ValidIssuer
        {
            get
            {
                return GetSettingByName("ValidIssuer");

            }
            set
            {
                try
                {
                    _settings.Add("ValidIssuer", value);
                }
                catch { _settings["ValidIssuer"] = value; }
            }
        }


        public string SessionAudience
        {
            get
            {
                return GetSettingByName("SessionAudience");

            }
            set
            {
                try
                {
                    _settings.Add("SessionAudience", value);
                }
                catch { _settings["SessionAudience"] = value; }
            }
        }

        public string AccessAudience
        {
            get
            {
                return GetSettingByName("AccessAudience");

            }
            set
            {
                try
                {
                    _settings.Add("AccessAudience", value);
                }
                catch { _settings["AccessAudience"] = value; }
            }
        }

        public double ResetPasswordMaxDays
        {
            get
            {
                double i = 0;
                double.TryParse(GetSettingByName("ResetPasswordMaxDays"), out i);
                return i;

            }
            set
            {
                try
                {
                    _settings.Add("ResetPasswordMaxDays", value.ToString());
                }
                catch { _settings["ResetPasswordMaxDays"] = value.ToString(); }
            }
        }

        public double IdleTimeMaxMinutes
        {
            get
            {
                double i = 0;
                double.TryParse(GetSettingByName("IdleTimeMaxMinutes"), out i);
                return i;

            }
            set
            {
                try
                {
                    _settings.Add("IdleTimeMaxMinutes", value.ToString());
                }
                catch { _settings["IdleTimeMaxMinutes"] = value.ToString(); }
            }
        }

        public double MaxAccessTokenLifeTimeInMinutes
        {
            get
            {
                double i = 0;
                double.TryParse(GetSettingByName("MaxAccessTokenLifeTimeInMinutes"), out i);
                return i;

            }
            set
            {
                try
                {
                    _settings.Add("MaxAccessTokenLifeTimeInMinutes", value.ToString());
                }
                catch { _settings["MaxAccessTokenLifeTimeInMinutes"] = value.ToString(); }
            }
        }

        public string DbServerJsonFile
        {
            get { return GetSettingByName("DbServerJsonFile"); }
            set
            {
                try
                {
                    _settings.Add("DbServerJsonFile", value);
                }
                catch { _settings["DbServerJsonFile"] = value; }
            }
        }

        public string JwtKey
        {
            get { return GetSettingByName("JwtKey"); }
            set
            {
                try
                {
                    _settings.Add("JwtKey", value);
                }
                catch { _settings["JwtKey"] = value; }
            }
        }

        public string SmtpHost
        {
            get { return GetSettingByName("SmtpHost"); }
            set
            {
                try
                {
                    _settings.Add("SmtpHost", value);
                }
                catch { _settings["SmtpHost"] = value; }
            }
        }

        public int SmtpPort
        {
            get 
            {
                int i = 0;
                int.TryParse(GetSettingByName("SmtpPort"), out i);
                return i;
            }
            set
            {
                try
                {
                    _settings.Add("SmtpPort", value.ToString());
                }
                catch { _settings["SmtpPort"] = value.ToString(); }
            }
        }

        public string SmtpUser
        {
            get { return GetSettingByName("SmtpUser"); }
            set
            {
                try
                {
                    _settings.Add("SmtpUser", value);
                }
                catch { _settings["SmtpUser"] = value; }
            }
        }

        public string SmtpPassword
        {
            get { return GetSettingByName("SmtpPassword"); }
            set
            {
                try
                {
                    _settings.Add("SmtpPassword", value);
                }
                catch { _settings["SmtpPassword"] = value; }
            }
        }

        public bool SmtpUseSsl
        {
            get 
            {
                bool useSsl = false;
                bool.TryParse(GetSettingByName("SmtpUseSsl"), out useSsl);
                return useSsl;
            }
            set
            {
                try
                {
                    _settings.Add("SmtpUseSsl", value?"true":"false");
                }
                catch { _settings["SmtpUseSsl"] = value ? "true" : "false"; }
            }
        }

        public string SmtpSenderAddress
        {
            get { return GetSettingByName("SmtpSenderAddress"); }
            set
            {
                try
                {
                    _settings.Add("SmtpSenderAddress", value);
                }
                catch { _settings["SmtpSenderAddress"] = value; }
            }
        }

        public string SmtpSenderName
        {
            get { return GetSettingByName("SmtpSenderName"); }
            set
            {
                try
                {
                    _settings.Add("SmtpSenderName", value);
                }
                catch { _settings["SmtpSenderName"] = value; }
            }
        }

        public string UIResetPasswordTokenProcessor
        {
            get { return GetSettingByName("UIResetPasswordTokenProcessor"); }
            set
            {
                try
                {
                    _settings.Add("UIResetPasswordTokenProcessor", value);
                }
                catch { _settings["UIResetPasswordTokenProcessor"] = value; }
            }
        }


        public string UIResetPasswordPage
        {
            get { return GetSettingByName("UIResetPasswordPage"); }
            set
            {
                try
                {
                    _settings.Add("UIResetPasswordPage", value);
                }
                catch { _settings["UIResetPasswordPage"] = value; }
            }
        }

        public string UIResetPasswordMessage
        {
            get { return GetSettingByName("UIResetPasswordMessage"); }
            set
            {
                try
                {
                    _settings.Add("UIResetPasswordMessage", value);
                }
                catch { _settings["UIResetPasswordMessage"] = value; }
            }
        }

        public string SapAppServerHost
        {
            get { return GetSettingByName("SapAppServerHost"); }
            set
            {
                try
                {
                    _settings.Add("SapAppServerHost", value);
                }
                catch { _settings["SapAppServerHost"] = value; }
            }
        }

        public string SapSystemNumber
        {
            get { return GetSettingByName("SapSystemNumber"); }
            set
            {
                try
                {
                    _settings.Add("SapSystemNumber", value);
                }
                catch { _settings["SapSystemNumber"] = value; }
            }
        }

        public string SapClient
        {
            get { return GetSettingByName("SapClient"); }
            set
            {
                try
                {
                    _settings.Add("SapClient", value);
                }
                catch { _settings["SapClient"] = value; }
            }
        }

        public string SapUser
        {
            get { return GetSettingByName("SapUser"); }
            set
            {
                try
                {
                    _settings.Add("SapUser", value);
                }
                catch { _settings["SapUser"] = value; }
            }
        }

        public string SapPassword
        {
            get { return GetSettingByName("SapPassword"); }
            set
            {
                try
                {
                    _settings.Add("SapPassword", value);
                }
                catch { _settings["SapPassword"] = value; }
            }
        }

        public string SapLanguage
        {
            get { return GetSettingByName("SapLanguage"); }
            set
            {
                try
                {
                    _settings.Add("SapLanguage", value);
                }
                catch { _settings["SapLanguage"] = value; }
            }
        }

        public bool NonActiveEmployeeOnMaster
        {
            get
            {
                bool result = false;
                bool.TryParse(GetSettingByName("NonActiveEmployeeOnMaster"), out result);
                return result;
                
            }
            set
            {
                try
                {

                    _settings.Add("NonActiveEmployeeOnMaster", value?"true":"false");
                }
                catch { _settings["NonActiveEmployeeOnMaster"] = "false"; }
            }
        }


        

        public string GetSettingByName(string name)
        {
            try { return _settings[name]; }
            catch { return string.Empty; }
        }
    }
}
