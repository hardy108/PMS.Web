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
    public class HarvestingWFLebihBasis : EntityFactoryWithWorkflow<THARVESTWFLEBIHBASIS, THARVESTWFLEBIHBASIS, GeneralFilter, PMSContextBase, WFContext>
    {

        AuthenticationServiceBase _authenticationService;
        public HarvestingWFLebihBasis(PMSContextBase context, WFContext wfContext, AuthenticationServiceBase authenticationService, AuthenticationServiceHO authenticationServiceHO, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext, authenticationService,authenticationServiceHO, taskQueue,emailSender,auditContext)
        {
            _serviceName = "WF Harvesting Lebih Basis";
            _wfDocumentType = "HVLB";
            _authenticationService = authenticationService;
            
        }

        public override THARVESTWFLEBIHBASIS CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            var record = base.CopyFromWebFormData(formData, newRecord);

            /*Custom Code - Start*/
            List<THARVESTWFLEBIHBASISDETAIL> _details = new List<THARVESTWFLEBIHBASISDETAIL>();
            _details.CopyFrom<THARVESTWFLEBIHBASISDETAIL>(formData, "THARVESTWFLEBIHBASISDETAIL");
            if (StandardUtility.IsEmptyList(_details))
                record.THARVESTWFLEBIHBASISDETAIL = new List<THARVESTWFLEBIHBASISDETAIL>();
            else
                record.THARVESTWFLEBIHBASISDETAIL = _details;
            
            _saveDetails = true;
            return record;
        }

        public override IEnumerable<THARVESTWFLEBIHBASIS> GetList(GeneralFilter filter)
        {
            
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                throw new Exception("Tanggal harus diisi");
            if (string.IsNullOrWhiteSpace(filter.UnitID))
                throw new Exception("Estate harus dipilih");

            var criteria = PredicateBuilder.True<THARVESTWFLEBIHBASIS>();
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
            return _context.THARVESTWFLEBIHBASIS.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        

        

        

        

        


        protected override THARVESTWFLEBIHBASIS SaveUpdateDetailsToDB(THARVESTWFLEBIHBASIS record, string userName)
        {
            

            var inserted =
                (from a in record.THARVESTWFLEBIHBASISDETAIL
                 join b in _context.THARVESTWFLEBIHBASISDETAIL.Where(d => d.ID.Equals(record.ID)) on a.EMPID equals b.EMPID into ab
                 from ableft in ab.DefaultIfEmpty()
                 where ableft == null
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(inserted))
                _context.THARVESTWFLEBIHBASISDETAIL.AddRange(inserted);

            var updated =
                (from a in record.THARVESTWFLEBIHBASISDETAIL
                 join b in _context.THARVESTWFLEBIHBASISDETAIL.Where(d => d.ID.Equals(record.ID)) on a.EMPID equals b.EMPID
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(updated))
                _context.THARVESTWFLEBIHBASISDETAIL.UpdateRange(updated);

            var deleted =
                (from a in _context.THARVESTWFLEBIHBASISDETAIL.Where(d => d.ID.Equals(record.ID))
                 join b in record.THARVESTWFLEBIHBASISDETAIL on a.EMPID equals b.EMPID into ab
                 from ableft in ab.DefaultIfEmpty()
                 where ableft == null
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(deleted))
                _context.THARVESTWFLEBIHBASISDETAIL.RemoveRange(deleted);
            return record;
        }

        protected override THARVESTWFLEBIHBASIS SaveInsertDetailsToDB(THARVESTWFLEBIHBASIS record, string userName)
        {

            if (!StandardUtility.IsEmptyList(record.THARVESTWFLEBIHBASISDETAIL))
            {
                _context.THARVESTWFLEBIHBASISDETAIL.AddRange(record.THARVESTWFLEBIHBASISDETAIL);
            }
            return record;
        }

        protected override bool DeleteDetailsFromDB(THARVESTWFLEBIHBASIS record, string userName)
        {
            var details = _context.THARVESTWFLEBIHBASISDETAIL.Where(d => d.ID.Equals(record.ID)).ToList();
            if (!StandardUtility.IsEmptyList(details))
                _context.THARVESTWFLEBIHBASISDETAIL.RemoveRange(details);
            return true;
        }

       

        protected override THARVESTWFLEBIHBASIS BeforeSave(THARVESTWFLEBIHBASIS record, string userName, bool newRecord)
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

        private void ValidateDetails(THARVESTWFLEBIHBASIS record, string userName)
        {
            //if (!StandardUtility.IsEmptyList(record.THARVESTWFLEBIHBASISDETAIL))
            //{

            //    int lineNo = 0;
            //    (
            //        from a in record.THARVESTWFLEBIHBASISDETAIL                    
            //        join c in _context.THARVESTWFLEBIHBASISDETAIL.Where(d => d.ID.Equals(record.ID)) on a.EMPID equals c.EMPID into ac
            //        from acLeft in ac.DefaultIfEmpty()
            //        orderby a.EMPID
            //        select new { THARVESTWFLEBIHBASISDETAIL = a, EXISTING = acLeft }

            //    ).ToList().ForEach(d => {
            //        lineNo++;

            //        if (d.EXISTING != null && d.EXISTING.APPROVED.HasValue && !d.EXISTING.APPROVED.Value)
            //        {
            //            if (d.THARVESTWFLEBIHBASISDETAIL.APPROVED.HasValue && d.THARVESTWFLEBIHBASISDETAIL.APPROVED.Value)
            //                throw new Exception($"Error Detail {d.THARVESTWFLEBIHBASISDETAIL.EMPID}: data sudah ditolak sebelumnya, tidak bisa disetujui lagi");
            //            d.THARVESTWFLEBIHBASISDETAIL.CopyFrom(d.EXISTING);
            //        }

            //        if (!d.THARVESTWFLEBIHBASISDETAIL.APPROVED.HasValue)
            //            d.THARVESTWFLEBIHBASISDETAIL.APPROVED = true;

            //        if (!d.THARVESTWFLEBIHBASISDETAIL.APPROVED.Value)
            //        {
            //            if (string.IsNullOrWhiteSpace(d.THARVESTWFLEBIHBASISDETAIL.REJECTIONREASON))
            //                throw new Exception($"Error Detail Line {lineNo}: alasan penolakan tidak boleh kosong");
            //            if (d.EXISTING != null && (!d.EXISTING.APPROVED.HasValue || d.EXISTING.APPROVED.Value) )
            //            {
            //                d.THARVESTWFLEBIHBASISDETAIL.REJECTEDBY = userName;
            //                d.THARVESTWFLEBIHBASISDETAIL.REJECTEDDATE = DateTime.Now;
            //            }
            //        }
                    
            //    });
            //}
        }

        public override THARVESTWFLEBIHBASIS NewRecord(string userName)
        {
            THARVESTWFLEBIHBASIS record = new THARVESTWFLEBIHBASIS
            {
                CREATED = GetServerTime().Date,
                WFDOCSTATUS = string.Empty,
                WFDOCSTATUSTEXT = "CREATED",
                DATE = DateTime.Today,
                STATUS = "P"
            };
            return record;
        }


        protected override Document WFGenerateDocument(THARVESTWFLEBIHBASIS record, string userName)
        {
            
            string wfFlag = string.Empty;

            string title = $"Pengajuan{(record.WFSEQUENCE > 1 ? " Ulang ke - " + record.WFSEQUENCE : string.Empty)} Lebih Basis {record.HARVESTCODE}";
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


        



        public override THARVESTWFLEBIHBASIS GetSingleByWorkflow(Document document, string userName)
        {

            
            var record = _context.THARVESTWFLEBIHBASIS
                .Include(d => d.THARVESTWFLEBIHBASISDETAIL)
                .FirstOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
            GetEmployeeDetails(record);
            return record;


            
        }

        private void GetEmployeeDetails(THARVESTWFLEBIHBASIS record)
        {
            if (!StandardUtility.IsEmptyList(record.THARVESTWFLEBIHBASISDETAIL))
            {
                var employeeIds = record.THARVESTWFLEBIHBASISDETAIL.Select(d => d.EMPID).Distinct().ToList();
                var employeeList = _context.MEMPLOYEE.Where(d => employeeIds.Contains(d.EMPID)).Select(d=> new { d.EMPID, d.EMPNAME,d.POSITIONID }).ToList();

                if (!StandardUtility.IsEmptyList(employeeList))
                {
                    var positionIds = employeeList.Select(d => d.POSITIONID).Distinct().ToList();
                    var positionList = _context.MPOSITION.Where(d => positionIds.Contains(d.POSITIONID)).Select(d => new { d.POSITIONID, d.POSITIONNAME }).ToList();

                    (
                        from a in record.THARVESTWFLEBIHBASISDETAIL
                        join b in employeeList on a.EMPID equals b.EMPID
                        join c in positionList on b.POSITIONID equals c.POSITIONID into bc
                        from bcLeft in bc.DefaultIfEmpty()
                        select new { Detail = a, EmployeeName = b.EMPNAME, PositionName=(bcLeft == null) ? string.Empty : bcLeft.POSITIONNAME }
                    ).ToList().ForEach(d => {
                        d.Detail.EMPNAME = d.EmployeeName;
                        d.Detail.EMPPOSITION = d.PositionName;
                    });

                }
            }
        }

        protected override THARVESTWFLEBIHBASIS GetSingleFromDB(params object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            var record = _context.THARVESTWFLEBIHBASIS
                .Include(d=>d.THARVESTWFLEBIHBASISDETAIL)                
                .SingleOrDefault(d => d.ID.Equals(Id));

            GetEmployeeDetails(record);
            return record;
            
        }

        
        protected override THARVESTWFLEBIHBASIS WFBeforeSendApproval(THARVESTWFLEBIHBASIS record, string userName, string actionCode, string approvalNote,bool newRecord)
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
                    if (!StandardUtility.IsEmptyList(record.THARVESTWFLEBIHBASISDETAIL))
                    {
                        record.THARVESTWFLEBIHBASISDETAIL.ToList().ForEach(d => {
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


        private void Approve(THARVESTWFLEBIHBASIS record, string userName, bool finalApprove)
        {
            if (StandardUtility.IsEmptyList(record.THARVESTWFLEBIHBASISDETAIL))
                throw new Exception("Tidak ada data detail yang diajukan");

            //ValidateDetails(record, userName);
            SaveUpdateDetailsToDB(record, userName);
            if (finalApprove)
                record.STATUS = "A";
            record.SYNCTOESTATE = false;
            record.UPDATED = DateTime.Now;
            record.UPDATEDBY = userName;
            _context.SaveChanges();

            
            
        }


        //Running On PMS Estate Server ==> Update To HO
        public void SyncByEstate(PMSContextEstate contextEstate,PMSContextHO contextHO)
        {

            //Check unsynced records

            DateTime now = GetServerTime();

            //Find Records to sync to estate server
           var unsyncedRecords = (from a in contextEstate.THARVESTWFLEBIHBASIS.AsNoTracking().Where(d =>  string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0" || d.STATUS == "P" )
                                   join b in contextEstate.THARVESTWFLEBIHBASISDETAIL.AsNoTracking() on a.ID equals b.ID
                                   select new { Header = a, Details = b })
                                  .ToList();


            if (!StandardUtility.IsEmptyList(unsyncedRecords))
            {

                var unsyncedDetails = unsyncedRecords.Select(d => d.Details).ToList();
                var unsyncedHeaders = unsyncedRecords.Select(d => d.Header).Distinct().ToList();

                var allIds = unsyncedRecords.Select(d => d.Header.ID).ToList();
                var existedOnHO = contextHO.THARVESTWFLEBIHBASIS.Where(d => allIds.Contains(d.ID)).ToList();
                var esistedOnHOIds = existedOnHO.Select(d => d.ID).ToList();
                var newIds = allIds.Where(d => !esistedOnHOIds.Contains(d));

                //only insert not existed record
                var newRecords = unsyncedRecords.Where(d => newIds.Contains(d.Header.ID)).ToList();

                //If not exists on HO : 1. Copy Record To HO                
                
                if (!StandardUtility.IsEmptyList(newRecords))
                {
                    contextHO.THARVESTWFLEBIHBASIS.AddRange(newRecords.Select(d => d.Header).Distinct().ToList());
                    contextHO.THARVESTWFLEBIHBASISDETAIL.AddRange(newRecords.Select(d => d.Details).ToList());
                    contextHO.SaveChanges();
                }

                var hoHeaderRecords =
                (
                    from a in contextHO.THARVESTWFLEBIHBASIS.AsNoTracking().Where(d => allIds.Contains(d.ID))
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
                    
                    var hoDetailRecords = (from a in contextHO.THARVESTWFLEBIHBASISDETAIL.AsNoTracking().Where(d => hoIDs.Contains(d.ID))
                                           join b in contextEstate.THARVESTWFLEBIHBASISDETAIL.AsNoTracking().Where(d => hoIDs.Contains(d.ID)) on new { a.ID,  a.EMPID } equals new { b.ID,  b.EMPID }
                                           where b.APPROVED != a.APPROVED
                                           select a).ToList();

                    contextEstate.THARVESTWFLEBIHBASIS.UpdateRange(hoHeaderRecords);
                    if (!StandardUtility.IsEmptyList(hoDetailRecords))
                        contextEstate.THARVESTWFLEBIHBASISDETAIL.UpdateRange(hoDetailRecords);
                    contextEstate.SaveChanges();

                    
                    //contextHO.THARVESTWFLEBIHBASIS.UpdateRange(hoHeaderRecords);
                    //contextHO.SaveChanges();
                }

            }

        }


        public void SubmitToWFByHO(PMSContextHO contextHO)
        {

            
            var listUnApproved = contextHO.THARVESTWFLEBIHBASIS.Include(d => d.THARVESTWFLEBIHBASISDETAIL).Where(d => (string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0")).ToList();
            if (!StandardUtility.IsEmptyList(listUnApproved))
            {
                listUnApproved.ForEach(d => {
                    try
                    {
                        WFSendApproval(d, d.CREATEDBY, "SUBM", "Submit for Approval", false);
                        _auditContext.SaveAuditTrail("ScheduledJob", "SubmitToWF Lebih Basis  " + d.ID, "Success Ask Approval");


                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                        _auditContext.SaveAuditTrail("ScheduledJob", "SubmitToWF Lebih Basis " + d.ID, "Error Ask Approval - " + errorMessage);                        
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
                    catch(Exception ex)
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
            var estateRecords = contextEstate.THARVESTWFLEBIHBASIS.AsNoTracking().Where(e => e.DATE >= startDate && e.DATE <= endDate).ToList();
            if (!StandardUtility.IsEmptyList(estateRecords))
            {
                var recordIds = estateRecords.Select(e => e.ID).ToList();
                var serverRecords = contextHO.THARVESTWFLEBIHBASIS.AsNoTracking().Where(e => recordIds.Contains(e.ID)).ToList();
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
                    var details = contextHO.THARVESTWFLEBIHBASISDETAIL.AsNoTracking().Where(e => unsyncIds.Contains(e.ID)).ToList();
                    contextEstate.THARVESTWFLEBIHBASIS.UpdateRange(unsyncRecords);
                    contextEstate.THARVESTWFLEBIHBASISDETAIL.UpdateRange(details);
                    contextEstate.SaveChanges();
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]{estateRecords.Count} records are re-syncronized");
                }
            }
            
        }

       
    }
}
