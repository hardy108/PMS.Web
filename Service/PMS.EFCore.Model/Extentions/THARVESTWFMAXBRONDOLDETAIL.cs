using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class THARVESTWFMAXBRONDOLDETAIL
    {
        [NotMapped()]
        public string BLOCKNAME { get; set; }
        [NotMapped()]
        public decimal LUASBLOCK { get; set; }



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
        {
            get
            {
                return APPROVED.HasValue && !APPROVED.Value;
            }
        }
    }
}
