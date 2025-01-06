using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TATTENDANCEPROBLEMEMPLOYEE
    {
        [NotMapped()]
        public string EMPNAME { get; set; }
        [NotMapped()]
        public string EMPPOSITION { get; set; }

        private string _fingerDate = string.Empty, _fingerTime = string.Empty;
        private DateTime? _newTime = null;

        [NotMapped()]
        public string FINGERDATE
        {
            get
            {
                
                if (TIME.HasValue)                    
                    return TIME.Value.ToString("dd-MMM-yyyy");
                if (_newTime.HasValue)
                    return _newTime.Value.ToString("dd-MMM-yyyy");
                return string.Empty;

            }
            set
            {
                _fingerDate = value;
                SetNewTime();
            }
        }
        [NotMapped()]
        public string FINGERTIME
        {
            get
            {
                string sTime = "";
                if (TIME.HasValue)
                    sTime = TIME.Value.ToString("HH:mm:ss");
                else if (_newTime.HasValue)
                    sTime = _newTime.Value.ToString("HH:mm:ss");
                if (!FAILEDFINGER && sTime == "00:00:00")
                    sTime = string.Empty;
                return sTime;
            }
            set
            {
                _fingerTime = value;
                SetNewTime();
            }
        }

        [NotMapped()]
        public DateTime? NEWTIME
        {
            get { return _newTime; }
        }


        private bool? _newApproved = null;
        [NotMapped()]
        public bool? NEWAPPROVED
        {
            get
            {
                if (!_newApproved.HasValue || (APPROVED.HasValue && !APPROVED.Value))
                    return APPROVED;

                return _newApproved;
            }
            set
            {
                _newApproved = value;
            }
        }



        private void SetNewTime()
        {
            if (string.IsNullOrWhiteSpace(_fingerDate))            
                _newTime = null;
            else
            {
                DateTime time = new DateTime();
                string sdateTime = _fingerDate;
                if (!string.IsNullOrWhiteSpace(_fingerTime))
                    sdateTime += " " + _fingerTime;
                
                if (DateTime.TryParse(sdateTime, out time))
                    _newTime = time;
                else
                    _newTime = null;
            }
        }

        [NotMapped()]
        public string MANUALREASONTEXT
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(REASONID) && REASONID.ToUpper() == "OT")
                    return REASONTEXT;
                return string.Empty;
            }
        }



    }
}
