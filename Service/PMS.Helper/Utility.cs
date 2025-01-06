using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Helper
{
    public static class Utility
    {

        

        
        

        #region Static Helper Functions
        public static int GetCurrentDocumentNumber(string code, DbContext context)
        {

            if (string.IsNullOrWhiteSpace(code))
                throw new Exception("Invalid auto number code");
            int result = 0;
            try
            {
                context.ExecuteSqlText($"Select Top 1 [NUMBER] From MRUNNUMBER Where [CODE]='{code}'")
                    .ExecScalar<int>(out result);
            }
            catch { }
            result++;
            return result;
        }

        public static int GetLastDocumentNumber(string code, DbContext context)
        {

            if (string.IsNullOrWhiteSpace(code))
                throw new Exception("Invalid auto number code");
            int result = 0;
            try
            {
                context.ExecuteSqlText($"Select Top 1 [NUMBER] From MRUNNUMBER Where [CODE]='{code}'")
                    .ExecScalar<int>(out result);
                
            }
            catch { }            
            return result;
        }

        public static void IncreaseRunningNumber(string code, DbContext context)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new Exception("Invalid auto number code");
            try
            {
                context.Database.ExecuteSqlCommand($"Exec sp_RunNo_Increase {code}");
            }
            catch (Exception ex) { throw ex; }
        }

        

        public static DateTime GetServerTime(DbContext context)
        {
            DateTime dateTime;
            try
            {   
                context.LoadStoredProc("sp_Helper_GetDateTime")
                        .AddParam("Version", 0)
                        .ExecScalar<DateTime>(out dateTime);
                return dateTime;
            }
            catch (Exception ex)
            {
                try
                {
                    context.ExecuteSqlText("Select GETDATE()")
                            .ExecScalar<DateTime>(out dateTime);
                    return dateTime;
                }
                catch { }
                
                
            }

            return DateTime.Now;
        }


        public static string GetMConfigValue(string name, DbContext context)
        {
            string result = string.Empty;
            context.ExecuteSqlText($"Select Top 1 [value] From MCONFIG Where [Name]='{name}'")
                .ExecScalar<string>(out result);
            return result;
        }

        public static void SetMConfigValue(string name, string value, DbContext context)
        {
            context.Database.ExecuteSqlCommand($"Exec sp_Config_SetValue {name},{value}");
        }
        #endregion
    }

    public enum CRUDMode
    {
        New = 0,
        Copy,
        Edit,
        Delete,
        View,
        Post,
        Unpost,
        Execute,
        Print,
        Approval,
        AskApproval
    }

    public enum ApprovalType
    {
        AskApproval = 0,
        Approve,
        Reject,
        Post,
        Unpost
    }



}
