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

namespace PMS.EFCore.Services.Attendances
{
    public class AttendanceProblem:EntityFactoryWithWorkflow<TATTENDANCEPROBLEM, TATTENDANCEPROBLEM, GeneralFilter, PMSContextBase,WFContext>
    {

        AuthenticationServiceHO _authenticationService;
        PMS.EFCore.Services.Organization.Employee _employeeService;
        public AttendanceProblem(PMSContextBase context, WFContext wfContext, AuthenticationServiceHO authenticationService, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext, authenticationService,authenticationService, taskQueue,emailSender,auditContext)
        {
            _serviceName = "ATTPROBLEM";
            _wfDocumentType = "ATTPROBLEM";
            _authenticationService = authenticationService;
            _employeeService = new PMS.EFCore.Services.Organization.Employee(context, authenticationService, auditContext);
        }

        public override TATTENDANCEPROBLEM CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            var record = base.CopyFromWebFormData(formData, newRecord);

            /*Custom Code - Start*/
            List<TATTENDANCEPROBLEMEMPLOYEE> _details = new List<TATTENDANCEPROBLEMEMPLOYEE>();
            _details.CopyFrom<TATTENDANCEPROBLEMEMPLOYEE>(formData, "TATTENDANCEPROBLEMEMPLOYEE");
            if (StandardUtility.IsEmptyList(_details))
                record.TATTENDANCEPROBLEMEMPLOYEE = new List<TATTENDANCEPROBLEMEMPLOYEE>();
            else
                record.TATTENDANCEPROBLEMEMPLOYEE = _details;
            
            _saveDetails = true;
            return record;
        }

        public override IEnumerable<TATTENDANCEPROBLEM> GetList(GeneralFilter filter)
        {
            
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                throw new Exception("Tanggal harus diisi");
            if (string.IsNullOrWhiteSpace(filter.UnitID))
                throw new Exception("Estate harus dipilih");

            var criteria = PredicateBuilder.True<TATTENDANCEPROBLEM>();
            criteria = criteria.And(d => d.CREATED.Date >= filter.StartDate.Date && d.CREATED.Date <= filter.EndDate.Date && d.UNITCODE.Equals(filter.UnitID));            
            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);
            return _context.TATTENDANCEPROBLEM.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        

        protected override TATTENDANCEPROBLEM BeforeSave(TATTENDANCEPROBLEM record, string userName, bool newRecord)
        {
            /*Custom Code - Start*/
            /*Validation before save a new record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/

            if (string.IsNullOrWhiteSpace(record.UNITCODE))
                throw new Exception("Kode estate tidak boleh kosong");

            ValidateDetails(record, userName);

            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATED = currentDate;
                record.CREATEDBY = userName;
                record.ID = record.UNITCODE + "-" + currentDate.ToString("yyyyMM") + "-" + HelperService.GetCurrentDocumentNumber(_serviceName + record.UNITCODE, _context).ToString("0000");
            }
            record.UPDATED = currentDate;
            record.UPDATEDBY = userName;
            
            
            /*Custom Code - Here*/
            return record;
        }




        protected override TATTENDANCEPROBLEM AfterSave(TATTENDANCEPROBLEM record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(_serviceName + record.UNITCODE, _context);
            return record;
            
        }


        protected override TATTENDANCEPROBLEM SaveUpdateDetailsToDB(TATTENDANCEPROBLEM record, string userName)
        {
            

            var inserted =
                (from a in record.TATTENDANCEPROBLEMEMPLOYEE
                 join b in _context.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.ID.Equals(record.ID)) on a.LINENO equals b.LINENO into ab
                 from ableft in ab.DefaultIfEmpty()
                 where ableft == null
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(inserted))
                _context.TATTENDANCEPROBLEMEMPLOYEE.AddRange(inserted);

            var updated =
                (from a in record.TATTENDANCEPROBLEMEMPLOYEE
                 join b in _context.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.ID.Equals(record.ID)) on a.LINENO equals b.LINENO                 
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(updated))
                _context.TATTENDANCEPROBLEMEMPLOYEE.UpdateRange(updated);

            var deleted =
                (from a in _context.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.ID.Equals(record.ID))
                 join b in record.TATTENDANCEPROBLEMEMPLOYEE on a.LINENO equals b.LINENO into ab
                 from ableft in ab.DefaultIfEmpty()
                 where ableft == null
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(deleted))
                _context.TATTENDANCEPROBLEMEMPLOYEE.RemoveRange(deleted);
            return record;
        }

        protected override TATTENDANCEPROBLEM SaveInsertDetailsToDB(TATTENDANCEPROBLEM record, string userName)
        {

            if (!StandardUtility.IsEmptyList(record.TATTENDANCEPROBLEMEMPLOYEE))
            {
                _context.TATTENDANCEPROBLEMEMPLOYEE.AddRange(record.TATTENDANCEPROBLEMEMPLOYEE);
            }
            return record;
        }

        protected override bool DeleteDetailsFromDB(TATTENDANCEPROBLEM record, string userName)
        {
            var details = _context.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.ID.Equals(record.ID)).ToList();
            if (!StandardUtility.IsEmptyList(details))
                _context.TATTENDANCEPROBLEMEMPLOYEE.RemoveRange(details);
            return true;
        }

       

        

        private void ValidateDetails(TATTENDANCEPROBLEM record, string userName)
        {
            if (!StandardUtility.IsEmptyList(record.TATTENDANCEPROBLEMEMPLOYEE))
            {

                int lineNo = 0;
                (
                    from a in record.TATTENDANCEPROBLEMEMPLOYEE
                    join b in _context.MABSENCEREASON on a.REASONID equals b.ID into ab
                    from abLeft in ab.DefaultIfEmpty()
                    join c in _context.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.ID.Equals(record.ID)) on a.LINENO equals c.LINENO into ac
                    from acLeft in ac.DefaultIfEmpty()
                    orderby a.LINENO
                    select new { TATTENDANCEPROBLEMEMPLOYEE = a, MABSENCEREASON = abLeft, EXISTING = acLeft }

                ).ToList().ForEach(d => {
                    lineNo++;

                    if (d.EXISTING != null && d.EXISTING.APPROVED.HasValue && !d.EXISTING.APPROVED.Value)
                    {
                        if (d.TATTENDANCEPROBLEMEMPLOYEE.APPROVED.HasValue && d.TATTENDANCEPROBLEMEMPLOYEE.APPROVED.Value)
                            throw new Exception($"Error Detail Line {lineNo}: data sudah ditolak sebelumnya, tidak bisa disetujui lagi");
                        d.TATTENDANCEPROBLEMEMPLOYEE.CopyFrom(d.EXISTING);
                    }

                    if (!d.TATTENDANCEPROBLEMEMPLOYEE.APPROVED.HasValue)
                        d.TATTENDANCEPROBLEMEMPLOYEE.APPROVED = true;

                    if (!d.TATTENDANCEPROBLEMEMPLOYEE.APPROVED.Value)
                    {
                        if (string.IsNullOrWhiteSpace(d.TATTENDANCEPROBLEMEMPLOYEE.REJECTIONREASON))
                            throw new Exception($"Error Detail Line {lineNo}: alasan penolakan tidak boleh kosong");
                        if (d.EXISTING != null && (!d.EXISTING.APPROVED.HasValue || d.EXISTING.APPROVED.Value) )
                        {
                            d.TATTENDANCEPROBLEMEMPLOYEE.REJECTEDBY = userName;
                            d.TATTENDANCEPROBLEMEMPLOYEE.REJECTEDDATE = DateTime.Now;
                        }
                    }


                    if (d.MABSENCEREASON == null)
                        throw new Exception($"Error Detail Line {lineNo}: Alasan tidak valid");
                    if (!d.MABSENCEREASON.ACTIVE)
                        throw new Exception($"Error Detail Line {lineNo}: Alasan tidak aktif");
                    
                    if (d.MABSENCEREASON.ID == "OT" && string.IsNullOrWhiteSpace(d.TATTENDANCEPROBLEMEMPLOYEE.NOTES))
                        throw new Exception($"Error Detail Line {lineNo}: Alasan tidak boleh kosong");
                    if (d.MABSENCEREASON.ID != "OT")
                        d.TATTENDANCEPROBLEMEMPLOYEE.REASONTEXT = d.MABSENCEREASON.TEXT;
                    d.TATTENDANCEPROBLEMEMPLOYEE.FAILEDFINGER = d.MABSENCEREASON.FAILEDFINGER;
                    if (string.IsNullOrWhiteSpace(d.TATTENDANCEPROBLEMEMPLOYEE.FINGERDATE))
                        throw new Exception($"Error Detail Line {lineNo}: Tanggal Finger tidak boleh kosong");

                    if (d.TATTENDANCEPROBLEMEMPLOYEE.FAILEDFINGER)
                    {
                        if (string.IsNullOrWhiteSpace(d.TATTENDANCEPROBLEMEMPLOYEE.FINGERTIME))
                            throw new Exception($"Error Detail Line {lineNo}: Jam Finger tidak boleh kosong");

                    }
                    if (!d.TATTENDANCEPROBLEMEMPLOYEE.NEWTIME.HasValue)
                        throw new Exception($"Error Detail Line {lineNo}: Waktu Finger tidak valid");
                    d.TATTENDANCEPROBLEMEMPLOYEE.TIME = d.TATTENDANCEPROBLEMEMPLOYEE.NEWTIME;
                });

                //Cek Duplicate EMployee.FingerTime
                string errorDuplicateDetails = string.Empty;
                record.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.FAILEDFINGER)
                    .GroupBy(d => new { d.EMPID, d.EMPNAME, d.TIME })
                    .Select(d => new { d.Key.EMPID, d.Key.EMPNAME, d.Key.TIME, COUNTX = d.Count() })
                    .Where(d => d.COUNTX > 1)
                    .ToList().ForEach(d => {
                        errorDuplicateDetails += $"\r\nKaryawan {d.EMPID}-{d.EMPNAME} Jam Finger : {d.TIME:dd-MMM-yyyy HH:mm:ss}";
                    });
                
                if (!string.IsNullOrWhiteSpace(errorDuplicateDetails))                
                    throw new Exception($"Detail berikut ini duplikat: " + errorDuplicateDetails);


                var failedFingerDetails = record.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.FAILEDFINGER && !(d.APPROVED.HasValue && !d.APPROVED.Value))
                    .Select(d => new { d.EMPID,d.EMPNAME, d.TIME })
                    .Distinct().ToList();


                (from a in failedFingerDetails
                 join b in _context.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.FAILEDFINGER && !(d.APPROVED.HasValue && !d.APPROVED.Value) && !d.ID.Equals(record.ID)) on new { a.EMPID, a.TIME } equals new { b.EMPID, b.TIME }
                 select new { a, b.ID }).ToList().ForEach(d =>
                 {
                     errorDuplicateDetails += $"\r\nKaryawan {d.a.EMPID}-{d.a.EMPNAME} Jam Finger : {d.a.TIME:dd-MMM-yyyy HH:mm:ss} - ID Dokumen {d.ID}";
                 });

                if (!string.IsNullOrWhiteSpace(errorDuplicateDetails))
                    throw new Exception($"Detail berikut ini duplikat: " + errorDuplicateDetails);






            }
        }

        public override TATTENDANCEPROBLEM NewRecord(string userName)
        {
            TATTENDANCEPROBLEM record = new TATTENDANCEPROBLEM
            {
                CREATED = GetServerTime().Date,
                WFDOCSTATUS = string.Empty,
                WFDOCSTATUSTEXT = "CREATED",
                DATE = DateTime.Today
            };
            return record;
        }


        protected override Document WFGenerateDocument(TATTENDANCEPROBLEM record, string userName)
        {
            
            string wfFlag = string.Empty;

            Document document = new Document
            {
                DocDate = DateTime.Today,
                Description = "Attendance Problem of " + record.ID,
                DocType = _wfDocumentType,
                UnitID = record.UNITCODE,
                DocOwner = userName,
                DocStatus = "",
                WFFlag = wfFlag,
                Title = "Attendance Problem of " + record.ID
            };
            return document;
        }


        



        public override TATTENDANCEPROBLEM GetSingleByWorkflow(Document document, string userName)
        {

            
            var record = _context.TATTENDANCEPROBLEM                
                .Include(d => d.TATTENDANCEPROBLEMEMPLOYEE)
                .FirstOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
            GetEmployeeDetails(record);
            return record;


            
        }

        private void GetEmployeeDetails(TATTENDANCEPROBLEM record)
        {
            if (!StandardUtility.IsEmptyList(record.TATTENDANCEPROBLEMEMPLOYEE))
            {
                var employeeIds = record.TATTENDANCEPROBLEMEMPLOYEE.Select(d => d.EMPID).Distinct().ToList();
                var employeeList = _context.MEMPLOYEE.Where(d => employeeIds.Contains(d.EMPID)).Select(d=> new { d.EMPID, d.EMPNAME,d.POSITIONID }).ToList();

                if (!StandardUtility.IsEmptyList(employeeList))
                {
                    var positionIds = employeeList.Select(d => d.POSITIONID).Distinct().ToList();
                    var positionList = _context.MPOSITION.Where(d => positionIds.Contains(d.POSITIONID)).Select(d => new { d.POSITIONID, d.POSITIONNAME }).ToList();

                    (
                        from a in record.TATTENDANCEPROBLEMEMPLOYEE
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

        protected override TATTENDANCEPROBLEM GetSingleFromDB(params object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            var record = _context.TATTENDANCEPROBLEM
                .Include(d=>d.TATTENDANCEPROBLEMEMPLOYEE)                
                .SingleOrDefault(d => d.ID.Equals(Id));

            GetEmployeeDetails(record);
            return record;
            
        }


        
        private void UpdateAttendance(TATTENDANCEPROBLEM record)
        {
            MUNITDBSERVER unitDBServer = _context.MUNITDBSERVER.Find(record.UNITCODE);
            if (unitDBServer == null)
                throw new Exception("Invalid origin DB Server");

            using (PMS.EFCore.Model.PMSContextBase contextEstate = new PMS.EFCore.Model.PMSContextBase(DBContextOption<PMS.EFCore.Model.PMSContextBase>.GetOptions(unitDBServer.SERVERNAME, unitDBServer.DBNAME, unitDBServer.DBUSER, unitDBServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
            {
                //employeeService.UpdateFromEmployeeChange(record, userName, contextEstate);
            }
        }


        
        protected override TATTENDANCEPROBLEM WFBeforeSendApproval(TATTENDANCEPROBLEM record, string userName, string actionCode, string approvalNote,bool newRecord)
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
                    if (!StandardUtility.IsEmptyList(record.TATTENDANCEPROBLEMEMPLOYEE))
                    {
                        record.TATTENDANCEPROBLEMEMPLOYEE.ToList().ForEach(d => {
                            if (!d.APPROVED.HasValue || (d.APPROVED.HasValue && d.APPROVED.Value))
                            {
                                d.APPROVED = false;
                                d.REJECTEDBY = userName;
                                d.REJECTIONREASON = approvalNote;
                                d.REJECTEDDATE = DateTime.Now;
                            }
                        });

                        SaveUpdateDetailsToDB(record, userName);
                        _context.SaveChanges();
                    }
                    break;
                case "FAPV":
                    Approve(record, userName, true);
                    break;
            }
            return record;
        }


   


        private void Approve(TATTENDANCEPROBLEM record, string userName, bool finalApprove)
        {
            if (!StandardUtility.IsEmptyList(record.TATTENDANCEPROBLEMEMPLOYEE))
            {
                if (StandardUtility.IsEmptyList(record.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.APPROVED.HasValue && d.APPROVED.Value).ToList()))
                    throw new Exception("Tidak ada data detail yang disetujui");
                ValidateDetails(record, userName);
                SaveUpdateDetailsToDB(record, userName);
                _context.SaveChanges();


                if (finalApprove)
                {
                    var attendanceLogList =
                        (from a in record.TATTENDANCEPROBLEMEMPLOYEE.Where(d => d.APPROVED.HasValue && d.APPROVED.Value)
                         join b in _context.MEMPLOYEE on a.EMPID equals b.EMPID
                         join c in _context.MABSENCEREASON.Where(d=>d.FAILEDFINGER) on a.REASONID equals c.ID 
                         select new TATTENDANCELOG
                         {
                             EMPID = a.EMPID,
                             PIN = b.PINID,
                             DATETIME = a.TIME.Value,
                             VERIFY = 0,
                             INOUTMODE = 0,
                             WORKCODE = 0,
                             DEVICE = a.ID,
                             UPDATED = DateTime.Now
                             
                         }
                        ).ToList();

                    MUNITDBSERVER unitDBServer = _context.MUNITDBSERVER.Find(record.UNITCODE);
                    if (unitDBServer == null)
                        throw new Exception("Invalid origin DB Server");

                    using (PMS.EFCore.Model.PMSContextBase contextEstate = new PMS.EFCore.Model.PMSContextBase(DBContextOption<PMS.EFCore.Model.PMSContextBase>.GetOptions(unitDBServer.SERVERNAME, unitDBServer.DBNAME, unitDBServer.DBUSER, unitDBServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
                    {

                        TATTENDANCEPROBLEM estateRecord = new TATTENDANCEPROBLEM();
                        record.CopyTo(estateRecord);
                        contextEstate.TATTENDANCEPROBLEM.Add(estateRecord);
                        contextEstate.TATTENDANCEPROBLEMEMPLOYEE.AddRange(estateRecord.TATTENDANCEPROBLEMEMPLOYEE);
                        if (!StandardUtility.IsEmptyList(attendanceLogList))
                        {
                            contextEstate.TATTENDANCELOG.AddRange(attendanceLogList);
                        }
                        contextEstate.SaveChanges();
                    }

                    
                }

            }
            
        }
    }
}
