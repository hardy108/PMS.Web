using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{

    public class sp_sap_Payment_Journal_Result
    {
        public string Line_Item { get; set; }
        public string Doc_Date { get; set; }
        public string Doc_Type { get; set; }
        public string Company_Code { get; set; }
        public string Posting_Date { get; set; }
        public string Period { get; set; }
        public string Currency { get; set; }
        public string Reference { get; set; }
        public string Doc_Header_Text { get; set; }
        public string Posting_Key_1 { get; set; }
        public string Account_1 { get; set; }
        public string Special_GL_1 { get; set; }
        public decimal? Amount_1 { get; set; }
        public string Tax_Code_1 { get; set; }
        public string Business_Area_1 { get; set; }
        public string Internal_Order_1 { get; set; }
        public string Cost_Center { get; set; }
        public string Profit_Center { get; set; }
        public string Assignment_1 { get; set; }
        public string Text_1 { get; set; }
        public string Posting_Key_2 { get; set; }
        public string Account_2 { get; set; }
        public string Special_GL_2 { get; set; }
        public decimal? Amount_2 { get; set; }
        public string Tax_Code_2 { get; set; }
        public string Business_Area_2 { get; set; }
        public string Internal_Order_2 { get; set; }
        public string Cost_Center_2 { get; set; }
        public string Profit_Center_2 { get; set; }
        public string Assignment_2 { get; set; }
        public string Text_2 { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_sap_Payment_Journal_Result> sp_sap_Payment_Journal1(string unitCode, DateTime From, DateTime To)
        {
            IEnumerable<sp_sap_Payment_Journal_Result> rows = null;
            this.LoadStoredProc("sap_Payment_Journal1")
                .AddParam("UnitCode", unitCode)
                .AddParam("From", From)
                .AddParam("To", To)
                .Exec(r => rows = r.ToList<sp_sap_Payment_Journal_Result>());
            return rows;
        }
    }
}
