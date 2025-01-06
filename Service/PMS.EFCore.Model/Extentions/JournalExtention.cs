using System;
using System.Collections.Generic;
using System.Text;
using PMS.EFCore.Model;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using PMS.Shared;
using PMS.Shared.Utilities;

namespace PMS.EFCore.Model
{
    public static class JournalExtention
    {
        public static decimal Debits(this TJOURNAL journal)
        {
            if (journal.TJOURNALITEM == null)
                return 0;
            decimal Debits = 0;
            journal.TJOURNALITEM.ToList().ForEach(d => Debits += (d.AMOUNT > 0 ? d.AMOUNT : 0));
            return Debits;
        }

        public static decimal Credits(this TJOURNAL journal)
        {
            if (journal.TJOURNALITEM == null)
                return 0;
            decimal Credits = 0;
            journal.TJOURNALITEM.ToList().ForEach(d => Credits += (d.AMOUNT < 0 ? d.AMOUNT : 0));
            return Credits;
        }

        public static decimal Amount(this TJOURNAL journal)
        {
            if (journal.TJOURNALITEM == null)
                return 0;
            decimal Amount = 0;
            journal.TJOURNALITEM.ToList().ForEach(d => Amount += d.AMOUNT);
            return Amount;
        }


    }


    public partial class TJOURNAL
    {
        [NotMapped()]
        public decimal TOTALDEBIT 
        {
            get 
            {
                if (TJOURNALITEM == null || !TJOURNALITEM.Any())
                    return 0;
                decimal total = 0;
                TJOURNALITEM.ToList().ForEach(d => { total += d.DEBIT; });
                return total;
            }
        }

        [NotMapped()]
        public decimal TOTALCREDIT 
        {
            get
            {
                if (TJOURNALITEM == null || !TJOURNALITEM.Any())
                    return 0;
                decimal total = 0;
                TJOURNALITEM.ToList().ForEach(d => { total += d.CREDIT; });
                return total;
            }
        }

        [NotMapped()]
        public decimal NETDEBIT { get { return TOTALDEBIT - TOTALCREDIT; } }

        [NotMapped]
        public string DATE_IN_TEXT { get { return DATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        
        [NotMapped]
        public string STATUS_IN_TEXT
        {
            get
            {
                return StandardUtility.GetRecordStatusDescription(STATUS);
            }
        }
    }

    public partial class TJOURNALITEM
    { 
        [NotMapped]
        public string ACCOUNTNAME { get; set; }
        [NotMapped]
        public string BLOCKCODE { get; set; }

        [NotMapped]
        public decimal DEBIT { get { return (AMOUNT > 0) ? AMOUNT : 0; } }

        [NotMapped]
        public decimal CREDIT { get { return (AMOUNT < 0) ? -AMOUNT : 0; } }


    }


}
