using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class THARVESTWFLEBIHBASISDETAIL
    {
        [NotMapped()]
        public string EMPNAME { get; set; }
        [NotMapped()]
        public string EMPPOSITION { get; set; }

        

        private bool? _newApproved = null;
        [NotMapped()]
        public bool? NEWAPPROVED
        {
            get
            {
                if (!_newApproved.HasValue)
                {
                    if (!APPROVED.HasValue)
                        return true;

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

    }
}
