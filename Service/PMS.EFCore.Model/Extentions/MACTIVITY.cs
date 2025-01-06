using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class MACTIVITY
    {
        [NotMapped()]
        public string GAMATDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.GAMATDEBT; }
        }
        [NotMapped()]
        public string GAHKDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.GAHKDEBT; }
        }

        [NotMapped()]
        public string NMATDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.NMATDEBT; }
        }
        [NotMapped()]
        public string NHKDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.NHKDEBT; }
        }
        [NotMapped()]
        public string LCMATDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.LCMATDEBT; }
        }
        [NotMapped()]
        public string LCHKDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.LCHKDEBT; }
        }
        [NotMapped()]
        public string TBMMATDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.TBMMATDEBT; }
        }
        [NotMapped()]
        public string TBMHKDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.TBMHKDEBT; }
        }
        [NotMapped()]
        public string TMMATDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.TMMATDEBT; }
        }
        [NotMapped()]
        public string TMHKDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.TMHKDEBT; }
        }
        [NotMapped()]
        public string HKCRED
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.HKCRED; }
        }
        [NotMapped()]
        public string HKCREDASST
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.HKCREDASST; }
        }
        [NotMapped()]
        public string CEHKDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.CEHKDEBT; }
        }
        [NotMapped()]
        public string CEMATDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.CEMATDEBT; }
        }
        [NotMapped()]
        public string RAHKDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.RAHKDEBT; }
        }
        [NotMapped()]
        public string RAMATDEBT
        {
            get { return MACTIVITYACCOUNT == null ? null : MACTIVITYACCOUNT.RAMATDEBT; }
        }

        [NotMapped()]
        public IEnumerable<MMATERIAL> MATERIAL {get;set;}

    }

    
}
