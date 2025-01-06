using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TPAYMENTWFADJUSTHKDETAIL
    {
        [NotMapped()]
        public string EMPNAME { get; set; }
        [NotMapped()]
        public string EMPPOSITION { get; set; }

        public string HARVESTDATETEXT { get { return HARVESTDATE.ToString("dd-MMM-yyyy"); } }
        

        private bool? _newApproved = null;
        [NotMapped()]
        public bool? NEWAPPROVED
        {
            get
            {
                if (!_newApproved.HasValue)
                {
                    if (!APPROVED.HasValue)
                        return false;

                    return APPROVED;

                }
                return _newApproved;
            }
            set
            {
                _newApproved = value;
            }
        }


        [NotMapped()]
        public bool REJECTED
        {   get 
            {
                return APPROVED.HasValue && !APPROVED.Value;
            } 
        }

        [NotMapped()]
        public string PROCESSRESULT
        {
            get
            {
                if (PROCESS == 0)
                    return "Belum Diproses";
                if (PROCESS == 1)
                    return $"Sudah Diproses, Transaksi Attendance {ATTDOCID}";
                if (PROCESS == 2)
                    return $"Error : {PROCESSSTATUS}";
                return "Tidak diketahui";
            }
        }



    }
}
