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
    public class HarvestingWFMaxBrondol : EntityFactoryWithWorkflow<THARVESTWFMAXBRONDOL, THARVESTWFMAXBRONDOL, GeneralFilter, PMSContextBase, WFContext>
    {

        AuthenticationServiceBase _authenticationService;
        
        public HarvestingWFMaxBrondol(PMSContextBase context, WFContext wfContext, AuthenticationServiceBase authenticationService, AuthenticationServiceHO authenticationServiceHO, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext, authenticationService,authenticationServiceHO, taskQueue,emailSender,auditContext)
        {
            _serviceName = "WF Harvesting Max Brondol";
            _wfDocumentType = "HVMAXBRONDOL";
            _authenticationService = authenticationService;
            
            
        }

        public override THARVESTWFMAXBRONDOL CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            var record = base.CopyFromWebFormData(formData, newRecord);

            /*Custom Code - Start*/
            List<THARVESTWFMAXBRONDOLDETAIL> _details = new List<THARVESTWFMAXBRONDOLDETAIL>();
            _details.CopyFrom<THARVESTWFMAXBRONDOLDETAIL>(formData, "THARVESTWFMAXBRONDOLDETAIL");
            if (StandardUtility.IsEmptyList(_details))
                record.THARVESTWFMAXBRONDOLDETAIL = new List<THARVESTWFMAXBRONDOLDETAIL>();
            else
                record.THARVESTWFMAXBRONDOLDETAIL = _details;
            
            _saveDetails = true;
            return record;
        }

        public override IEnumerable<THARVESTWFMAXBRONDOL> GetList(GeneralFilter filter)
        {
            
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                throw new Exception("Tanggal harus diisi");
            if (string.IsNullOrWhiteSpace(filter.UnitID))
                throw new Exception("Estate harus dipilih");

            var criteria = PredicateBuilder.True<THARVESTWFMAXBRONDOL>();
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
            return _context.THARVESTWFMAXBRONDOL.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        

        
        

        

       


        protected override THARVESTWFMAXBRONDOL SaveUpdateDetailsToDB(THARVESTWFMAXBRONDOL record, string userName)
        {
            

            var inserted =
                (from a in record.THARVESTWFMAXBRONDOLDETAIL
                 join b in _context.THARVESTWFMAXBRONDOLDETAIL.Where(d => d.ID.Equals(record.ID)) on a.BLOCKID equals b.BLOCKID into ab
                 from ableft in ab.DefaultIfEmpty()
                 where ableft == null
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(inserted))
                _context.THARVESTWFMAXBRONDOLDETAIL.AddRange(inserted);

            var updated =
                (from a in record.THARVESTWFMAXBRONDOLDETAIL
                 join b in _context.THARVESTWFMAXBRONDOLDETAIL.Where(d => d.ID.Equals(record.ID)) on a.BLOCKID equals b.BLOCKID
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(updated))
                _context.THARVESTWFMAXBRONDOLDETAIL.UpdateRange(updated);

            var deleted =
                (from a in _context.THARVESTWFMAXBRONDOLDETAIL.Where(d => d.ID.Equals(record.ID))
                 join b in record.THARVESTWFMAXBRONDOLDETAIL on a.BLOCKID equals b.BLOCKID into ab
                 from ableft in ab.DefaultIfEmpty()
                 where ableft == null
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(deleted))
                _context.THARVESTWFMAXBRONDOLDETAIL.RemoveRange(deleted);
            return record;
        }

        protected override THARVESTWFMAXBRONDOL SaveInsertDetailsToDB(THARVESTWFMAXBRONDOL record, string userName)
        {

            if (!StandardUtility.IsEmptyList(record.THARVESTWFMAXBRONDOLDETAIL))
            {
                _context.THARVESTWFMAXBRONDOLDETAIL.AddRange(record.THARVESTWFMAXBRONDOLDETAIL);
            }
            return record;
        }

        protected override bool DeleteDetailsFromDB(THARVESTWFMAXBRONDOL record, string userName)
        {
            var details = _context.THARVESTWFMAXBRONDOLDETAIL.Where(d => d.ID.Equals(record.ID)).ToList();
            if (!StandardUtility.IsEmptyList(details))
                _context.THARVESTWFMAXBRONDOLDETAIL.RemoveRange(details);
            return true;
        }

       

        protected override THARVESTWFMAXBRONDOL BeforeSave(THARVESTWFMAXBRONDOL record, string userName, bool newRecord)
        {

            if (string.IsNullOrWhiteSpace(record.UNITCODE))
                throw new Exception("Kode estate tidak boleh kosong");
            if (string.IsNullOrWhiteSpace(record.HARVESTCODE))
                throw new Exception("No dokumen buku panen tidak boleh kosong");
            if (record.WFSEQUENCE<=0)                
                throw new Exception("No dokumen pengajuan tidak boleh kosong");

            ValidateDetails(record,userName);

            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
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

        private void ValidateDetails(THARVESTWFMAXBRONDOL record, string userName)
        {
            //if (!StandardUtility.IsEmptyList(record.THARVESTWFMAXBRONDOLDETAIL))
            //{

            //    int lineNo = 0;
            //    (
            //        from a in record.THARVESTWFMAXBRONDOLDETAIL
            //        join c in _context.THARVESTWFMAXBRONDOLDETAIL.Where(d => d.ID.Equals(record.ID)) on a.BLOCKID equals c.BLOCKID into ac
            //        from acLeft in ac.DefaultIfEmpty()
            //        orderby a.BLOCKID
            //        select new { THARVESTWFMAXBRONDOLDETAIL = a, EXISTING = acLeft }

            //    ).ToList().ForEach(d => {
            //        lineNo++;

            //        if (d.EXISTING != null && d.EXISTING.APPROVED.HasValue && !d.EXISTING.APPROVED.Value)
            //        {
            //            if (d.THARVESTWFMAXBRONDOLDETAIL.APPROVED.HasValue && d.THARVESTWFMAXBRONDOLDETAIL.APPROVED.Value)
            //                throw new Exception($"Error Detail {d.THARVESTWFMAXBRONDOLDETAIL.BLOCKID}: data sudah ditolak sebelumnya, tidak bisa disetujui lagi");
            //            d.THARVESTWFMAXBRONDOLDETAIL.CopyFrom(d.EXISTING);
            //        }

            //        if (!d.THARVESTWFMAXBRONDOLDETAIL.APPROVED.HasValue)
            //            d.THARVESTWFMAXBRONDOLDETAIL.APPROVED = true;

            //        if (!d.THARVESTWFMAXBRONDOLDETAIL.APPROVED.Value)
            //        {
            //            if (string.IsNullOrWhiteSpace(d.THARVESTWFMAXBRONDOLDETAIL.REJECTIONREASON))
            //                throw new Exception($"Error Detail Line {lineNo}: alasan penolakan tidak boleh kosong");
            //            if (d.EXISTING != null && (!d.EXISTING.APPROVED.HasValue || d.EXISTING.APPROVED.Value) )
            //            {
            //                d.THARVESTWFMAXBRONDOLDETAIL.REJECTEDBY = userName;
            //                d.THARVESTWFMAXBRONDOLDETAIL.REJECTEDDATE = DateTime.Now;
            //            }
            //        }
                    
            //    });
            //}
        }

        public override THARVESTWFMAXBRONDOL NewRecord(string userName)
        {
            THARVESTWFMAXBRONDOL record = new THARVESTWFMAXBRONDOL
            {
                CREATED = GetServerTime().Date,
                WFDOCSTATUS = string.Empty,
                WFDOCSTATUSTEXT = "CREATED",
                DATE = DateTime.Today,
                STATUS = "P"
            };
            return record;
        }


        protected override Document WFGenerateDocument(THARVESTWFMAXBRONDOL record, string userName)
        {
            
            string wfFlag = string.Empty;

            string title = $"Pengajuan{(record.WFSEQUENCE > 1 ? " Ulang ke - " + record.WFSEQUENCE : string.Empty)} Max Brondol {record.HARVESTCODE}";
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


        



        public override THARVESTWFMAXBRONDOL GetSingleByWorkflow(Document document, string userName)
        {

            
            var record = _context.THARVESTWFMAXBRONDOL
                .Include(d => d.THARVESTWFMAXBRONDOLDETAIL)
                .FirstOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
            GetBlockDetails(record);
            return record;


            
        }

        private void GetBlockDetails(THARVESTWFMAXBRONDOL record)
        {
            if (!StandardUtility.IsEmptyList(record.THARVESTWFMAXBRONDOLDETAIL))
            {
                var blockIds = record.THARVESTWFMAXBRONDOLDETAIL.Select(d => d.BLOCKID).Distinct().ToList();
                var blockList = _context.VBLOCK.Where(d => blockIds.Contains(d.BLOCKID)).Select(d=> new { d.BLOCKID, d.LUASBLOCK,d.NAME }).ToList();

                if (!StandardUtility.IsEmptyList(blockList))
                {

                    (
                        from a in record.THARVESTWFMAXBRONDOLDETAIL
                        join b in blockList on a.BLOCKID equals b.BLOCKID                                                
                        select new { Detail = a, Block = b }
                    ).ToList().ForEach(d => {
                        d.Detail.BLOCKNAME = d.Block.NAME;
                        d.Detail.LUASBLOCK = d.Block.LUASBLOCK;
                    });

                }
            }
        }

        protected override THARVESTWFMAXBRONDOL GetSingleFromDB(params object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            var record = _context.THARVESTWFMAXBRONDOL
                .Include(d=>d.THARVESTWFMAXBRONDOLDETAIL)                
                .SingleOrDefault(d => d.ID.Equals(Id));

            GetBlockDetails(record);
            return record;
            
        }

        
        protected override THARVESTWFMAXBRONDOL WFBeforeSendApproval(THARVESTWFMAXBRONDOL record, string userName, string actionCode, string approvalNote,bool newRecord)
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
                    if (!StandardUtility.IsEmptyList(record.THARVESTWFMAXBRONDOLDETAIL))
                    {
                        record.THARVESTWFMAXBRONDOLDETAIL.ToList().ForEach(d => {
                            if (!d.APPROVED.HasValue || (d.APPROVED.HasValue && d.APPROVED.Value))
                            {
                                d.APPROVED = false;
                                d.REJECTEDBY = userName;
                                d.REJECTIONREASON = approvalNote;
                                d.REJECTEDDATE = DateTime.Now;
                            }
                        });
                        SaveUpdateDetailsToDB(record, userName);
                    }

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


        private void Approve(THARVESTWFMAXBRONDOL record, string userName, bool finalApprove)
        {
            if (StandardUtility.IsEmptyList(record.THARVESTWFMAXBRONDOLDETAIL))
                throw new Exception("Tidak ada data detail yang diajukan");

            //ValidateDetails(record, userName);
            SaveUpdateDetailsToDB(record, userName);
            if (finalApprove)
                record.STATUS = "A";
            record.SYNCTOESTATE = false;
            record.UPDATED = DateTime.Now;
            record.UPDATEDBY = userName;
            _context.SaveChanges();

            //if (!StandardUtility.IsEmptyList(record.THARVESTWFMAXBRONDOLDETAIL))
            //{
            //    if (StandardUtility.IsEmptyList(record.THARVESTWFMAXBRONDOLDETAIL.Where(d => d.APPROVED.HasValue && d.APPROVED.Value).ToList()))
            //        throw new Exception("Tidak ada data detail yang disetujui");
                



            //    if (finalApprove)
            //    {
                    
            //        //MUNITDBSERVER unitDBServer = _context.MUNITDBSERVER.Find(record.UNITCODE);
            //        //if (unitDBServer == null)
            //        //    throw new Exception("Invalid origin DB Server");

            //        //using (PMS.EFCore.Model.PMSContextBase contextEstate = new PMS.EFCore.Model.PMSContextBase(DBContextOption<PMS.EFCore.Model.PMSContextBase>.GetOptions(unitDBServer.SERVERNAME, unitDBServer.DBNAME, unitDBServer.DBUSER, unitDBServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
            //        //{

            //        //    TATTENDANCEPROBLEM estateRecord = new TATTENDANCEPROBLEM();
            //        //    record.CopyTo(estateRecord);
            //        //    contextEstate.TATTENDANCEPROBLEM.Add(estateRecord);
            //        //    contextEstate.TATTENDANCEPROBLEMEMPLOYEE.AddRange(estateRecord.TATTENDANCEPROBLEMEMPLOYEE);
            //        //    if (!StandardUtility.IsEmptyList(attendanceLogList))
            //        //    {
            //        //        contextEstate.TATTENDANCELOG.AddRange(attendanceLogList);
            //        //    }
            //        //    contextEstate.SaveChanges();
            //        //}

                    
            //    }

            //}
            
        }



        //Running On PMS Estate Server ==> Update To HO
        public void SyncByEstate(PMSContextEstate contextEstate, PMSContextHO contextHO)
        {

            DateTime now = GetServerTime();
            //Check unsynced records

            //Find Records to sync to estate server
            var unsyncedRecords = (from a in contextEstate.THARVESTWFMAXBRONDOL.AsNoTracking().Where(d => string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0" || d.STATUS == "P" )
                                   join b in contextEstate.THARVESTWFMAXBRONDOLDETAIL.AsNoTracking() on a.ID equals b.ID
                                   select new { Header = a, Details = b })
                                  .ToList();


            if (!StandardUtility.IsEmptyList(unsyncedRecords))
            {

                var unsyncedDetails = unsyncedRecords.Select(d => d.Details).ToList();
                var unsyncedHeaders = unsyncedRecords.Select(d => d.Header).Distinct().ToList();

                var allIds = unsyncedRecords.Select(d => d.Header.ID).ToList();
                var existedOnHO = contextHO.THARVESTWFMAXBRONDOL.Where(d => allIds.Contains(d.ID)).ToList();
                var esistedOnHOIds = existedOnHO.Select(d => d.ID).ToList();
                var newIds = allIds.Where(d => !esistedOnHOIds.Contains(d));

                //only insert not existed record
                var newRecords = unsyncedRecords.Where(d => newIds.Contains(d.Header.ID)).ToList();

                

                //If not exists on HO :
                //1. Copy Record To HO                
                if (!StandardUtility.IsEmptyList(newRecords))
                {
                    contextHO.THARVESTWFMAXBRONDOL.AddRange(newRecords.Select(d => d.Header).Distinct().ToList());
                    contextHO.THARVESTWFMAXBRONDOLDETAIL.AddRange(newRecords.Select(d => d.Details).ToList());
                    contextHO.SaveChanges();
                }

                var hoHeaderRecords =
               (
                   from a in contextHO.THARVESTWFMAXBRONDOL.AsNoTracking().Where(d => allIds.Contains(d.ID))
                   join b in unsyncedHeaders on a.ID equals b.ID
                   where a.WFDOCSTATUS != b.WFDOCSTATUS
                   select a
               ).ToList();

                if (!StandardUtility.IsEmptyList(hoHeaderRecords))
                {
                    hoHeaderRecords.ForEach(d => {
                        d.SYNCTOESTATE = true;
                        d.SYNCDATE = now;
                    });
                    var hoIDs = hoHeaderRecords.Select(d => d.ID).ToList();
                    

                    var hoDetailRecords = (from a in contextHO.THARVESTWFMAXBRONDOLDETAIL.AsNoTracking().Where(d => hoIDs.Contains(d.ID))
                                           join b in contextEstate.THARVESTWFMAXBRONDOLDETAIL.AsNoTracking().Where(d => hoIDs.Contains(d.ID)) on new { a.ID, a.BLOCKID } equals new { b.ID, b.BLOCKID }
                                           where b.APPROVED != a.APPROVED
                                           select a).ToList();

                    contextEstate.THARVESTWFMAXBRONDOL.UpdateRange(hoHeaderRecords);
                    if (!StandardUtility.IsEmptyList(hoDetailRecords))
                        contextEstate.THARVESTWFMAXBRONDOLDETAIL.UpdateRange(hoDetailRecords);
                    contextEstate.SaveChanges();

                    //contextHO.THARVESTWFMAXBRONDOL.UpdateRange(hoHeaderRecords);
                    //contextHO.SaveChanges();
                }
            }

        }


        public void SubmitToWFByHO(PMSContextHO contextHO)
        {

            var listUnApproved = contextHO.THARVESTWFMAXBRONDOL.Include(d => d.THARVESTWFMAXBRONDOLDETAIL).Where(d => (string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0")).ToList();
            if (!StandardUtility.IsEmptyList(listUnApproved))
            {
                listUnApproved.ForEach(d => {
                    try
                    {
                        WFSendApproval(d, d.CREATEDBY, "SUBM", "Submit for Approval", false);
                        _auditContext.SaveAuditTrail("ScheduledJob", "SubmitToWF Max Brondol " + d.ID, "Success Ask Approval");
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                        _auditContext.SaveAuditTrail("ScheduledJob", "SubmitToWF Max Brondol " + d.ID, "Error Ask Approval " + errorMessage);
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
            var estateRecords = contextEstate.THARVESTWFMAXBRONDOL.AsNoTracking().Where(e => e.DATE >= startDate && e.DATE <= endDate).ToList();
            if (!StandardUtility.IsEmptyList(estateRecords))
            {
                var recordIds = estateRecords.Select(e => e.ID).ToList();
                var serverRecords = contextHO.THARVESTWFMAXBRONDOL.AsNoTracking().Where(e => recordIds.Contains(e.ID)).ToList();
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
                    var details = contextHO.THARVESTWFMAXBRONDOLDETAIL.AsNoTracking().Where(e => unsyncIds.Contains(e.ID)).ToList();
                    contextEstate.THARVESTWFMAXBRONDOL.UpdateRange(unsyncRecords);
                    contextEstate.THARVESTWFMAXBRONDOLDETAIL.UpdateRange(details);
                    contextEstate.SaveChanges();
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]{estateRecords.Count} records are re-syncronized");
                }
            }

        }

    }
}
