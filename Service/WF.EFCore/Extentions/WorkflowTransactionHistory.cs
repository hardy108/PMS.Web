using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WF.EFCore.Models
{
    public partial class WorkflowTransactionHistory
    {
        [NotMapped()]
        public string ApprovalDateInText
        {
            get
            {
                if (!OutDate.HasValue)
                    return string.Empty;
                return OutDate.Value.ToString("dd-MMM-yyyy HH:mm:ss");

            }
        }

    }
}
