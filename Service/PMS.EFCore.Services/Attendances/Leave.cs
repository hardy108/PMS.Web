using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Organization;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;
using PMS.EFCore.Services.Approval;
using WF.EFCore.Data;
using PMS.Shared.Services;
using WF.EFCore.Models;
using PMS.Shared.Exceptions;

namespace PMS.EFCore.Services.Attendances
{
    public class Leave : EntityFactoryWithWorkflow<TLEAVE, TLEAVE, GeneralFilter, PMSContextBase, WFContext>
    {

        private AuthenticationServiceBase _authenticationService;
        
        public Leave(PMSContextBase context, WFContext wfContext, AuthenticationServiceBase authenticationService, AuthenticationServiceHO authenticationServiceHO, IBackgroundTaskQueue taskQueue, IEmailSender emailSender, AuditContext auditContext) : base(context, wfContext, authenticationService, authenticationServiceHO, taskQueue, emailSender, auditContext)
        {
            _serviceName = "Leave";
            _wfDocumentType = "LEAVE";
            _authenticationService = authenticationService;            
        }

        private string FieldsValidation(TLEAVE leave)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(leave.ID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(leave.UNITID)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(leave.EMPID)) result += "Karyawan tidak boleh kosong." + Environment.NewLine;
            if (leave.DATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(leave.TYPEID)) result += "Jenis cuti tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrWhiteSpace(leave.STATUS)) result += "Status tidak boleh kosong." + Environment.NewLine;
            if (leave.DATEFROM > leave.DATETO) result += "Tanggal salah." + Environment.NewLine;
            return result;
        }
        public override IEnumerable<TLEAVE> GetList(GeneralFilter filter)
        {
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                throw new Exception("Tanggal harus diisi");
            if (string.IsNullOrWhiteSpace(filter.UnitID))
                throw new Exception("Estate harus dipilih");

            var criteria = PredicateBuilder.True<TLEAVE>();
            var criteriaEmployee = PredicateBuilder.True<MEMPLOYEE>();
            criteria = criteria.And(d => d.DATE.Date >= filter.StartDate.Date && d.DATE.Date <= filter.EndDate.Date && d.UNITID.Equals(filter.UnitID));
            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));

            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(d =>
                    d.ID.ToUpper().Contains(filter.UpperCasedSearchTerm)
                    || d.EMPID.ToUpper().Contains(filter.UpperCasedSearchTerm)
                    || (d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value.ToString().Equals(filter.Keyword)));

                criteriaEmployee = criteriaEmployee.And(d => d.EMPNAME.ToUpper().Contains(filter.UpperCasedSearchTerm));
            }
            var list = (from a in _context.TLEAVE.Where(criteria)
                       join b in _context.MEMPLOYEE.Where(criteriaEmployee) on a.EMPID equals b.EMPID
                       join c in _context.MLEAVETYPE on a.TYPEID equals c.ID
                       select new { Leave =a, LeaveType =c, Employee = b }
                       ).GetPaged(filter.PageNo, filter.PageSize).Results;

            foreach(var record in list)
            {
                record.Leave.LEAVETYPENAME = record.LeaveType.NAME;
                record.Leave.ABSENTCODE = record.LeaveType.ABSENTCODE;
                record.Leave.EMPNAME = record.Employee.EMPNAME;
                record.Leave.DIVID = record.Employee.DIVID;
            }

            return list.Select(d => d.Leave).ToList();
        }

        

        protected override TLEAVE GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            var record = (from a in _context.TLEAVE.Where(d => d.ID.Equals(Id))
                          join b in _context.MEMPLOYEE on a.EMPID equals b.EMPID
                          join c in _context.MLEAVETYPE on a.TYPEID equals c.ID
                          select new { Leave = a, Employee = b, LeaveType = c }
            ).FirstOrDefault();

            if (record == null)
                throw new Exception("Data cuti tidak ditemukan");

            record.Leave.LEAVETYPENAME = record.LeaveType.NAME;
            record.Leave.ABSENTCODE = record.LeaveType.ABSENTCODE;
            record.Leave.EMPNAME = record.Employee.EMPNAME;
            record.Leave.DIVID = record.Employee.DIVID;
            return record.Leave;
        }


        protected override Document WFGenerateDocument(TLEAVE record, string userName)
        {

            string wfFlag = string.Empty;

            string title = $"Pengajuan Cuti {record.ID}";
            Document document = new Document
            {
                DocDate = DateTime.Today,
                Description = title,
                DocType = _wfDocumentType,
                UnitID = record.UNITID,
                DocOwner = record.CREATEBY,
                DocStatus = "",
                WFFlag = wfFlag,
                Title = title
            };
            return document;
        }


        public override TLEAVE GetSingleByWorkflow(Document document, string userName)
        {

            
            var record = (from a in _context.TLEAVE.Where(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == document.DocTransNo )
                          join b in _context.MEMPLOYEE on a.EMPID equals b.EMPID
                          join c in _context.MLEAVETYPE on a.TYPEID equals c.ID
                          select new { Leave = a, Employee = b, LeaveType = c }
            ).FirstOrDefault();

            if (record == null)
                throw new Exception("Data cuti tidak ditemukan");

            record.Leave.LEAVETYPENAME = record.LeaveType.NAME;
            record.Leave.ABSENTCODE = record.LeaveType.ABSENTCODE;
            record.Leave.EMPNAME = record.Employee.EMPNAME;

            return record.Leave;
        }

        protected override TLEAVE WFBeforeSendApproval(TLEAVE record, string userName, string actionCode, string approvalNote, bool newRecord)
        {
            actionCode = actionCode.ToUpper();
            switch (actionCode)
            {
                case "SUBM":
                case "APRV":     
                    
                case "RVSN":                    
                    break;
                case "RJCT":
                    //Reject All Details
                    record.STATUS = "C";                    
                    _context.SaveChanges();
                    break;
                case "FAPV":
                    record.STATUS = "A";
                    _context.SaveChanges();                    
                    break;
            }
            return record;
        }


        //Running On PMS Estate Server
        public void SyncByEstate(PMSContextEstate contextEstate, PMSContextHO contextHO)
        {

            //Check unsynced records

            DateTime now = GetServerTime();

            //Find Records to sync to estate server
            var unsyncedRecords = contextEstate.TLEAVE.AsNoTracking().Where(d => string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0" || d.STATUS == "P").ToList();


            if (!StandardUtility.IsEmptyList(unsyncedRecords))
            {

                var allIds = unsyncedRecords.Select(d => d.ID).ToList();
                var existedOnHO = contextHO.TLEAVE.Where(d => allIds.Contains(d.ID)).ToList();
                var esistedOnHOIds = existedOnHO.Select(d => d.ID).ToList();
                var newIds = allIds.Where(d => !esistedOnHOIds.Contains(d));

                //only insert not existed record
                var newRecords = unsyncedRecords.Where(d => newIds.Contains(d.ID)).ToList();
                if (!StandardUtility.IsEmptyList(newRecords))
                {
                    contextHO.TLEAVE.AddRange(newRecords);                    
                    contextHO.SaveChanges();
                }


                //Sync Record Status
                var hoRecords =
                (
                    from a in contextHO.TLEAVE.AsNoTracking().Where(d => allIds.Contains(d.ID))
                    join b in unsyncedRecords on a.ID equals b.ID
                    where a.WFDOCSTATUS != b.WFDOCSTATUS
                    select a
                ).ToList();

                if (!StandardUtility.IsEmptyList(hoRecords))
                {   
                    contextEstate.TLEAVE.UpdateRange(hoRecords);
                    contextEstate.SaveChanges();
                }
                
            }

        }

        public void ProcessAttendance(PMSContextEstate contextEstate)
        {
            var activePeriods = contextEstate.MPERIOD.Where(d => d.ACTIVE1).Select(d => new { d.UNITCODE, STARTDATE = d.FROM1, ENDDATE = d.TO1 })
                    .Union(
                        contextEstate.MPERIOD.Where(d => d.ACTIVE2).Select(d => new { d.UNITCODE, STARTDATE = d.FROM2, ENDDATE = d.TO2 })
                    ).Distinct().ToList();


            Attendance attendanceService = new Attendance(contextEstate, _authenticationService, _auditContext);
            var unprocessedCriteria = PredicateBuilder.True<TLEAVE>();
            unprocessedCriteria = unprocessedCriteria.And(d =>
                d.STATUS == "A"  //Approved
                && d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value > 0 && !string.IsNullOrWhiteSpace(d.WFDOCSTATUS) && //Approved By Workflow
                (d.PROCESS != 1 || string.IsNullOrWhiteSpace(d.ATTDOCID))); //Unprocessed or Already Processed But Errors occured

            (
                from a in contextEstate.TLEAVE.Where(unprocessedCriteria)
                join b in contextEstate.MEMPLOYEE on a.EMPID equals b.EMPID
                join c in contextEstate.MLEAVETYPE on a.TYPEID equals c.ID
                join d in activePeriods on a.UNITID equals d.UNITCODE
                select new { Leave = a, Employee = b, LeaveType = c }
            ).ToList().ForEach(d => {

                try
                {
                    List<TATTENDANCE> attendanceList = new List<TATTENDANCE>();
                    DateTime date = d.Leave.DATEFROM;
                    while (date <= d.Leave.DATETO)
                    {
                        var attendance = new TATTENDANCE
                        {
                            EMPLOYEEID = d.Leave.EMPID,
                            DIVID = d.Employee.DIVID,
                            PRESENT = false,
                            DATE = date,
                            REMARK = d.LeaveType.NAME,
                            HK = 0,
                            STATUS = d.Leave.STATUS,
                            REF = d.Leave.ID,
                            AUTO = true,
                            ABSENTCODE = d.LeaveType.ABSENTCODE,
                            CREATEBY = d.Leave.UPDATEBY,
                            CREATEDDATE = d.Leave.UPDATED,
                            UPDATEBY = d.Leave.UPDATEBY,
                            UPDATEDDATE = d.Leave.UPDATED
                        };
                        attendanceList.Add(attendance);
                    }

                    if (!StandardUtility.IsEmptyList(attendanceList))
                    {
                        d.Leave.ATTDOCID = attendanceService.SaveInsertFromAdjustmentHK(attendanceList);
                    }
                    d.Leave.PROCESS = 1;
                    d.Leave.PROCESSSTATUS = string.Empty;
                    contextEstate.SaveChanges();
                }
                catch (Exception ex)
                {
                    string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                    d.Leave.PROCESS = 2;
                    d.Leave.PROCESSSTATUS = errorMessage;
                    d.Leave.ATTDOCID = string.Empty;
                    contextEstate.SaveChanges();
                }

            });



        }

        //Running On PMS Ho Server
        public void SubmitToWFByHO(PMSContextHO contextHO)
        {

            var listUnApproved = contextHO.TLEAVE.Where(d => (string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0")).ToList();
            if (!StandardUtility.IsEmptyList(listUnApproved))
            {
                listUnApproved.ForEach(d => {
                    try
                    {
                        WFSendApproval(d, d.CREATEBY, "SUBM", "Submit for Approval", false);
                        _auditContext.SaveAuditTrail("ScheduledJob", $"SubmitToWF  {_serviceName} " + d.ID, "Success Ask Approval");
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                        _auditContext.SaveAuditTrail("ScheduledJob", $"SubmitToWF  {_serviceName} " + d.ID, "Error Ask Approval -  " + errorMessage);                        
                    }
                });
            }

        }
    }
}
