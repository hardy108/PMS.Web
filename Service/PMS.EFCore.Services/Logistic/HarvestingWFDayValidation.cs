using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;

using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using System.Linq;

using PMS.EFCore.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WF.EFCore.Models;

using WF.EFCore.Data;

using PMS.EFCore.Helper;
using AM.EFCore.Services;
using PMS.EFCore.Services.Approval;
using WF.EFCore.Services;
using PMS.Shared.Services;
using System.Threading.Tasks;
using PMS.EFCore.Model;
using Remotion.Linq.Parsing.ExpressionVisitors.MemberBindings;
using PMS.EFCore.Services.Organization;
using PMS.Shared.Exceptions;

namespace PMS.EFCore.Services.Logistic
{
    public class HarvestingWFDayValidation : EntityFactoryWithWorkflow<THARVESTWFDAYVALIDATION, THARVESTWFDAYVALIDATION, GeneralFilter, PMSContextBase, WFContext>
    {

        AuthenticationServiceBase _authenticationService;
        
        public HarvestingWFDayValidation(PMSContextBase context,  WFContext wfContext, AuthenticationServiceBase authenticationService, AuthenticationServiceHO authenticationServiceHO, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext, authenticationService,authenticationServiceHO, taskQueue,emailSender,auditContext)
        {
            _serviceName = "WF Harvesting Day Validation";
            _wfDocumentType = "HVDAYVALID";
            _authenticationService = authenticationService;
            
        }

        

        public override IEnumerable<THARVESTWFDAYVALIDATION> GetList(GeneralFilter filter)
        {
            
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                throw new Exception("Tanggal harus diisi");
            if (string.IsNullOrWhiteSpace(filter.UnitID))
                throw new Exception("Estate harus dipilih");

            var criteria = PredicateBuilder.True<THARVESTWFDAYVALIDATION>();
            criteria = criteria.And(d => d.CREATED.Value.Date >= filter.StartDate.Date && d.CREATED.Value.Date <= filter.EndDate.Date && d.UNITCODE.Equals(filter.UnitID));
            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));
            
            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(d =>
                    d.ID.ToUpper().Contains(filter.UpperCasedSearchTerm)
                    || d.HARVESTCODE.ToUpper().Contains(filter.UpperCasedSearchTerm)
                    || (d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value.ToString().Equals(filter.Keyword)));
            }

            return _context.THARVESTWFDAYVALIDATION.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        

        
        



       

        protected override THARVESTWFDAYVALIDATION BeforeSave(THARVESTWFDAYVALIDATION record, string userName, bool newRecord)
        {

            if (string.IsNullOrWhiteSpace(record.UNITCODE))
                throw new Exception("Kode estate tidak boleh kosong");
            if (string.IsNullOrWhiteSpace(record.HARVESTCODE))
                throw new Exception("No dokumen buku panen tidak boleh kosong");
            if (record.WFSEQUENCE<=0)                
                throw new Exception("No dokumen pengajuan tidak boleh kosong");

            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            
            //if (record.ALLOWEDDAYS <= 0)
            //    throw new Exception("Jumlah hari yang diijinkan tidak valid");

            if (newRecord)
            {
                record.CREATED = currentDate;
                record.CREATEDBY = userName;
                record.ID = $"{record.HARVESTCODE}-{record.WFSEQUENCE:0000}";
            }
            record.UPDATED = currentDate;
            record.UPDATEDBY = userName;
            


            return record;

        }

        

        public override THARVESTWFDAYVALIDATION NewRecord(string userName)
        {
            THARVESTWFDAYVALIDATION record = new THARVESTWFDAYVALIDATION
            {
                CREATED = GetServerTime().Date,
                WFDOCSTATUS = string.Empty,
                WFDOCSTATUSTEXT = "CREATED",
                DATE = DateTime.Today,
                STATUS = "P"
            };
            return record;
        }


        protected override Document WFGenerateDocument(THARVESTWFDAYVALIDATION record, string userName)
        {
            
            string wfFlag = string.Empty;

            string title = $"Pengajuan{(record.WFSEQUENCE > 1 ? " Ulang ke - " + record.WFSEQUENCE : string.Empty)} limit {record.ALLOWEDDAYS} hari untuk {record.HARVESTCODE}";
            Document document = new Document
            {
                DocDate = DateTime.Today,
                Description = title,
                DocType = _wfDocumentType,
                UnitID = record.UNITCODE,
                DocOwner = record.CREATEDBY,
                DocStatus = "",
                WFFlag = wfFlag,
                Title = title
            };
            return document;
        }


        



        public override THARVESTWFDAYVALIDATION GetSingleByWorkflow(Document document, string userName)
        {

            
            var record = _context.THARVESTWFDAYVALIDATION                
                .FirstOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
            
            return record;


            
        }

        

        protected override THARVESTWFDAYVALIDATION GetSingleFromDB(params object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            var record = _context.THARVESTWFDAYVALIDATION                
                .SingleOrDefault(d => d.ID.Equals(Id));
            return record;
            
        }

        
        protected override THARVESTWFDAYVALIDATION WFBeforeSendApproval(THARVESTWFDAYVALIDATION record, string userName, string actionCode, string approvalNote,bool newRecord)
        {
            actionCode = actionCode.ToUpper();
            switch(actionCode)
            {
                case "SUBM":
                case "APRV":
                    Approve(record, userName,false);
                    break;
                case "RVSN":
                    //Nothing To DO
                    break;
                case "RJCT":
                    //Reject All Details
                    record.STATUS = "C";                    
                    record.SYNCTOESTATE = false;
                    record.UPDATED = DateTime.Now;
                    record.UPDATEDBY = userName;
                    _context.SaveChanges();
                    break;
                case "FAPV":
                    Approve(record, userName, true);
                    break;
            }
            return record;
        }


        private void Approve(THARVESTWFDAYVALIDATION record, string userName, bool finalApprove)
        {
            if (finalApprove)
                record.STATUS = "A";            
            record.SYNCTOESTATE = false;
            record.UPDATED = DateTime.Now;
            record.UPDATEDBY = userName;
            _context.SaveChanges();
        }


        
        //Running On PMS Estate Server ==> Update To HO
        public void SyncByEstate(PMSContextEstate contextEstate, PMSContextHO contextHO)
        {

            //Check unsynced records

            DateTime now = GetServerTime();
            //Find Records to sync to estate server
            var unsyncedRecords = contextEstate.THARVESTWFDAYVALIDATION.AsNoTracking().Where(d => string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0" || d.STATUS == "P").ToList();


            if (!StandardUtility.IsEmptyList(unsyncedRecords))
            {

                var allIds = unsyncedRecords.Select(d => d.ID).ToList();
                var existedOnHO = contextHO.THARVESTWFDAYVALIDATION.Where(d => allIds.Contains(d.ID)).ToList();
                var esistedOnHOIds = existedOnHO.Select(d => d.ID).ToList();
                var newIds = allIds.Where(d => !esistedOnHOIds.Contains(d));

                //only insert not existed record
                var newRecords = unsyncedRecords.Where(d => newIds.Contains(d.ID)).ToList();

                

                //If not exists on HO :
                //1. Copy Record To HO                                
                if (!StandardUtility.IsEmptyList(newRecords))
                {
                    contextHO.THARVESTWFDAYVALIDATION.AddRange(newRecords);
                    contextHO.SaveChanges();
                }

                var hoHeaderRecords =
               (
                   from a in contextHO.THARVESTWFDAYVALIDATION.AsNoTracking().Where(d => allIds.Contains(d.ID))
                   join b in unsyncedRecords on a.ID equals b.ID
                   where a.WFDOCSTATUS != b.WFDOCSTATUS
                   select a
               ).ToList();

                if (!StandardUtility.IsEmptyList(hoHeaderRecords))
                {
                    hoHeaderRecords.ForEach(d => {
                        d.SYNCTOESTATE = true;
                        d.SYNCDATE = now;
                    });
                    
                    contextEstate.THARVESTWFDAYVALIDATION.UpdateRange(hoHeaderRecords);                    
                    contextEstate.SaveChanges();

                    //contextHO.THARVESTWFDAYVALIDATION.UpdateRange(hoHeaderRecords);
                    //contextHO.SaveChanges();
                }

            }

        }

        public void SubmitToWFByHO(PMSContextHO contextHO)
        {
            var listUnApproved = contextHO.THARVESTWFDAYVALIDATION.Where(d => (string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0")).ToList();
            if (!StandardUtility.IsEmptyList(listUnApproved))
            {
                listUnApproved.ForEach(d => {
                    try
                    {
                        WFSendApproval(d, d.CREATEDBY, "SUBM", "Submit for Approval", false);                        
                        _auditContext.SaveAuditTrail("ScheduledJob", "SubmitToWF Limit Transaksi " + d.ID, "Success Ask Approval");
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);                        
                        _auditContext.SaveAuditTrail("ScheduledJob", "SubmitToWF Limit Transaksi " + d.ID, "Error Ask Approval " + errorMessage);
                    }
                });
            }
        }

        


        public void ResyncToEstate(PMSContextHO contextHO, List<string> unitCodes, DateTime startDate, DateTime endDate)
        {
            string processName = "Resync Data";
            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Start Process");

            var criteria = PredicateBuilder.True<MUNITDBSERVER>();
            if (!StandardUtility.IsEmptyList(unitCodes))
            {
                criteria = criteria.And(d => unitCodes.Contains(d.UNITCODE));
            }

            contextHO.MUNITDBSERVER.Where(criteria).Select(d => new MUNITDBSERVER { SERVERNAME = d.SERVERNAME, DBNAME = d.DBNAME, DBUSER = d.DBUSER, DBPASSWORD = d.DBPASSWORD }).Distinct().ToList().ForEach(d => {
                if (StandardUtility.NetWorkPing(d.SERVERNAME))
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{d.SERVERNAME}:{d.DBNAME}]Server is ONLINE");
                    try
                    {
                        using (PMS.EFCore.Model.PMSContextEstate contextEstate = new PMS.EFCore.Model.PMSContextEstate(DBContextOption<PMS.EFCore.Model.PMSContextEstate>.GetOptions(d.SERVERNAME, d.DBNAME, d.DBUSER, d.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
                        {
                            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{d.SERVERNAME}:{d.DBNAME}]Start Scanning");
                            ResyncFromHO(contextHO, contextEstate, startDate, endDate);
                            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{d.SERVERNAME}:{d.DBNAME}]Finish Scanning");
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                        Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{d.UNITCODE}]Error: {errorMessage}");
                    }
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{d.SERVERNAME}:{d.DBNAME}]Error: Server is OFFLINE");
                }
            });

        }

        public void ResyncFromHO(PMSContextHO contextHO, PMSContextEstate contextEstate, DateTime startDate, DateTime endDate)
        {
            string processName = "Resync From HO";            
            var estateRecords = contextEstate.THARVESTWFDAYVALIDATION.AsNoTracking().Where(e => e.DATE >= startDate && e.DATE <= endDate).ToList();
            if (!StandardUtility.IsEmptyList(estateRecords))
            {
                var recordIds = estateRecords.Select(e => e.ID).ToList();
                var serverRecords = contextHO.THARVESTWFDAYVALIDATION.AsNoTracking().Where(e => recordIds.Contains(e.ID)).ToList();
                var unsyncRecords =
                (
                    from a in estateRecords
                    join b in serverRecords on a.ID equals b.ID
                    where a.STATUS != b.STATUS
                    select b
                ).ToList();

                if (StandardUtility.IsEmptyList(unsyncRecords))
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]OK");
                }
                else
                {
                    var unsyncIds = unsyncRecords.Select(e => e.ID).ToList();                    
                    contextEstate.THARVESTWFDAYVALIDATION.UpdateRange(unsyncRecords);                    
                    contextEstate.SaveChanges();
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]{estateRecords.Count} records are re-syncronized");
                }
            }
        }

        
    }
}
