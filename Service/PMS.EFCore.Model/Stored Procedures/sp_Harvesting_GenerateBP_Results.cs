using PMS.EFCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    
    public class sp_Harvesting_GenerateBP_Result_Harvest
    {
        public string DIVID { get; set; }
        public DateTime DATE { get; set; }        
        public short? PAYMENT { get; set; }
        public string ACTID { get; set; }
        public string MANDOR1ID { get; set; }
        public string MANDORID { get; set; }
        public string KRANIID { get; set; }        
        public string CHECKERID { get; set; }
    }
    public class sp_Harvesting_GenerateBP_Result_HarvestEmployee:sp_Harvesting_GenerateBP_Result_Harvest
    {
        public string EMPLOYEEID { get; set; }
    }

    public class sp_Harvesting_GenerateBP_Result_HarvestBlock : sp_Harvesting_GenerateBP_Result_Harvest
    {
        public string BLOCKID { get; set; }
    }

    public class sp_Harvesting_GenerateBP_Result_HarvestCollect : sp_Harvesting_GenerateBP_Result_Harvest
    {
        public string TPHID { get; set; }
        public string BLOCKID { get; set; }
        public string EMPLOYEEID { get; set; }
        public decimal QTY { get; set; }
        public decimal QTYKG { get; set; }
    }

    public class sp_Harvesting_GenerateBP_Result_HarvestFine : sp_Harvesting_GenerateBP_Result_Harvest
    {
        public string FINECODE { get; set; }
        public string BLOCKID { get; set; }
        public string EMPLOYEEID { get; set; }
        public decimal PEN { get; set; }
        public bool AUTO { get; set; }
    }


    public partial class PMSContextBase
    {
        public void sp_Harvesting_GenerateBP(string divId, DateTime date,
                List<sp_Harvesting_GenerateBP_Result_Harvest> harvests,
                List<sp_Harvesting_GenerateBP_Result_HarvestEmployee> harvestEmployees,
                List<sp_Harvesting_GenerateBP_Result_HarvestBlock> harvestBlocks,
                List<sp_Harvesting_GenerateBP_Result_HarvestCollect> harvestCollects, 
                List<sp_Harvesting_GenerateBP_Result_HarvestFine> harvestFines)
        {
            
            this.LoadStoredProc("sp_Harvesting_GenerateBP")
                .AddParam("Divid", divId)
                .AddParam("Date", date)
                .ExecMultiResults(r => {

                    harvests = new List<sp_Harvesting_GenerateBP_Result_Harvest>();
                    harvests.AddRange(r.ReadToList<sp_Harvesting_GenerateBP_Result_Harvest>());

                    harvestEmployees = new List<sp_Harvesting_GenerateBP_Result_HarvestEmployee>();
                    if (r.NextResult())
                        harvestEmployees.AddRange(r.ReadToList<sp_Harvesting_GenerateBP_Result_HarvestEmployee>());

                    harvestBlocks = new List<sp_Harvesting_GenerateBP_Result_HarvestBlock>();
                    if (r.NextResult())
                        harvestBlocks.AddRange( r.ReadToList<sp_Harvesting_GenerateBP_Result_HarvestBlock>());

                    harvestCollects = new List<sp_Harvesting_GenerateBP_Result_HarvestCollect>();
                    if (r.NextResult())
                        harvestCollects.AddRange(r.ReadToList<sp_Harvesting_GenerateBP_Result_HarvestCollect>());

                    
                    if (r.NextResult())
                    {
                        string errorMessage = string.Empty;
                        List<object> checkDoket  = r.ReadToValues(0);
                        checkDoket.ForEach(d=> { errorMessage += $"{d} ada di dua atau lebih SPB\r\n";});
                        if (!string.IsNullOrWhiteSpace(errorMessage))
                        {
                            throw new Exception(errorMessage);
                        }
                    }

                    harvestFines = new List<sp_Harvesting_GenerateBP_Result_HarvestFine>();

                    if (r.NextResult())
                        harvestFines.AddRange(r.ReadToList<sp_Harvesting_GenerateBP_Result_HarvestFine>());

                    
                });
            
        }
    }
}
