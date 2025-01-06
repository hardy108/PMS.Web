using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.EFCore.Model
{
    public partial class TUPKEEP
    {
        public decimal TOTALOUTPUT 
        { 
            get 
            {
                decimal result = 0;
                if (!StandardUtility.IsEmptyList(TUPKEEPEMPLOYEE))
                    result += TUPKEEPEMPLOYEE.Sum(d => d.OUTPUT);

                if (TUPKEEPVENDOR != null)
                    result += TUPKEEPVENDOR.OUTPUT;
                return result;
            } 
        }

        [NotMapped()]
        public TCONTRACTITEM CONTRACTITEM
        {
            get;
            set;
        }

        [NotMapped()]
        public List<TUPKEEPMATERIALSUM> TUPKEEPMATERIALSUM
        {
            get;set;
        }

        [NotMapped()]
        public List<string> BLOCKIDs
        {
            get { return TUPKEEPBLOCK.Select(d => d.BLOCKID).ToList(); }
        }

        [NotMapped()]
        public List<string> MATERIALIDs
        {
            get { return TUPKEEPMATERIAL.Select(d => d.MATERIALID).Distinct().ToList(); }
        }

        [NotMapped()]
        public List<string> EMPIDs
        {
            get { return TUPKEEPEMPLOYEE.Select(d => d.EMPLOYEEID).Distinct().ToList(); }
        }
    }
}
