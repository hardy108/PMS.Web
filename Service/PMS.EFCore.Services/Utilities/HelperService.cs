using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services.Entities;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Services.Utilities
{
    public static class HelperService
    {
        
        public static IEnumerable<MCONFIG> GetConfigAll(PMSContextBase context)
        {
            return context.MCONFIG.ToList();
        }

        

        public static string GetConfigValue(string name, PMSContextBase context)
        {
            return Utility.GetMConfigValue(name, context);
        }

        public static int GetCurrentDocumentNumber(string code, PMSContextBase context)
        {
            return Helper.Utility.GetCurrentDocumentNumber(code, context);
        }

        public static int GetLastDocumentNumber(string code, PMSContextBase context)
        {
            return Helper.Utility.GetLastDocumentNumber(code, context);
        }

        public static IEnumerable<SAPPAYROLLCOSTCTR> GetSapCostCenter(PMSContextBase context)
        {
            try
            {
                return context.SAPPAYROLLCOSTCTR.ToList();
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<SAPPAYROLLACCOUNT> GetSapJournalKey(PMSContextBase context)
        {
            try
            {
                return context.SAPPAYROLLACCOUNT.ToList();
            }
            catch
            {
                return null;
            } 
        }

        public static DateTime GetServerDateTime(long version, PMSContextBase context)
        {
            return Utility.GetServerTime(context);
        }

        public static void IncreaseRunningNumber(string code, PMSContextBase context)
        {

            Utility.IncreaseRunningNumber(code, context);
        }

        public static void SetConfigValue(string name, string value, PMSContextBase context)
        {
            Utility.SetMConfigValue(name, value, context);
        }

        public static void SetSapResult(string code, string note, PMSContextBase context)
        {
            try
            {
                context.Database.ExecuteSqlCommand($"Exec sap_SpbResult {code},{note}");
            }
            catch (Exception ex) { throw ex; }
        }

        public static void SetSapTran(string code, string no, PMSContextBase context)
        {
            try
            {
                context.Database.ExecuteSqlCommand($"Exec sap_SpbTran {code},{no}");
            }
            catch (Exception ex) { throw ex; }
        }

        public static void SetUpkeepUploadLog(string code, string note, PMSContextBase context)
        {
            try
            {
                context.Database.ExecuteSqlCommand($"Exec sap_UpkeepLog {code},{note}");
            }
            catch (Exception ex) { throw ex; }
        }

        public static void DHSUpdateMaster(string by, DateTime datetime, string note , PMSContextBase context)
        {
            try
            {
                context.Database.ExecuteSqlCommand($"Exec sp_DHSValues_UpdateMaster {by},{datetime},{note}");
            }
            catch (Exception ex) { throw ex; }
        }

    }


    


    
}
