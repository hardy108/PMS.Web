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
using PMS.EFCore.Services.Attendances;

namespace PMS.EFCore.Services.Logistic
{
    public class PaymentWFAdjustHK : EntityFactoryWithWorkflow<TPAYMENTWFADJUSTHK, TPAYMENTWFADJUSTHK, GeneralFilter, PMSContextBase, WFContext>
    {

        AuthenticationServiceBase _authenticationService;
        public PaymentWFAdjustHK(PMSContextBase context, WFContext wfContext, AuthenticationServiceBase authenticationService, AuthenticationServiceHO authenticationServiceHO, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext, authenticationService,authenticationServiceHO, taskQueue,emailSender,auditContext)
        {
            _serviceName = "WF Adjust HK";
            _wfDocumentType = "PAYADJHK";
            _authenticationService = authenticationService;
            
        }

        public override TPAYMENTWFADJUSTHK CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            var record = base.CopyFromWebFormData(formData, newRecord);

            /*Custom Code - Start*/
            List<TPAYMENTWFADJUSTHKDETAIL> _details = new List<TPAYMENTWFADJUSTHKDETAIL>();
            _details.CopyFrom<TPAYMENTWFADJUSTHKDETAIL>(formData, "TPAYMENTWFADJUSTHKDETAIL");
            if (StandardUtility.IsEmptyList(_details))
                record.TPAYMENTWFADJUSTHKDETAIL = new List<TPAYMENTWFADJUSTHKDETAIL>();
            else
                record.TPAYMENTWFADJUSTHKDETAIL = _details;
            
            _saveDetails = true;
            return record;
        }

        public override IEnumerable<TPAYMENTWFADJUSTHK> GetList(GeneralFilter filter)
        {
            
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                throw new Exception("Tanggal harus diisi");
            if (string.IsNullOrWhiteSpace(filter.UnitID))
                throw new Exception("Estate harus dipilih");

            var criteria = PredicateBuilder.True<TPAYMENTWFADJUSTHK>();
            criteria = criteria.And(d => d.CREATED.Value.Date >= filter.StartDate.Date && d.CREATED.Value.Date <= filter.EndDate.Date && d.UNITCODE.Equals(filter.UnitID));
            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));
            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(d =>
                    d.ID.ToUpper().Contains(filter.UpperCasedSearchTerm)                    
                    || (d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value.ToString().Equals(filter.Keyword)));
            }
            return _context.TPAYMENTWFADJUSTHK.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        

       

        protected override TPAYMENTWFADJUSTHK SaveUpdateDetailsToDB(TPAYMENTWFADJUSTHK record, string userName)
        {
            

            var inserted =
                (from a in record.TPAYMENTWFADJUSTHKDETAIL
                 join b in _context.TPAYMENTWFADJUSTHKDETAIL.Where(d => d.ID.Equals(record.ID)) on a.EMPID equals b.EMPID into ab
                 from ableft in ab.DefaultIfEmpty()
                 where ableft == null
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(inserted))
                _context.TPAYMENTWFADJUSTHKDETAIL.AddRange(inserted);

            var updated =
                (from a in record.TPAYMENTWFADJUSTHKDETAIL
                 join b in _context.TPAYMENTWFADJUSTHKDETAIL.Where(d => d.ID.Equals(record.ID)) on a.EMPID equals b.EMPID
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(updated))
                _context.TPAYMENTWFADJUSTHKDETAIL.UpdateRange(updated);

            var deleted =
                (from a in _context.TPAYMENTWFADJUSTHKDETAIL.Where(d => d.ID.Equals(record.ID))
                 join b in record.TPAYMENTWFADJUSTHKDETAIL on a.EMPID equals b.EMPID into ab
                 from ableft in ab.DefaultIfEmpty()
                 where ableft == null
                 select a
                ).ToList();
            if (!StandardUtility.IsEmptyList(deleted))
                _context.TPAYMENTWFADJUSTHKDETAIL.RemoveRange(deleted);
            return record;
        }

        protected override TPAYMENTWFADJUSTHK SaveInsertDetailsToDB(TPAYMENTWFADJUSTHK record, string userName)
        {

            if (!StandardUtility.IsEmptyList(record.TPAYMENTWFADJUSTHKDETAIL))
            {
                _context.TPAYMENTWFADJUSTHKDETAIL.AddRange(record.TPAYMENTWFADJUSTHKDETAIL);
            }
            return record;
        }

        protected override bool DeleteDetailsFromDB(TPAYMENTWFADJUSTHK record, string userName)
        {
            var details = _context.TPAYMENTWFADJUSTHKDETAIL.Where(d => d.ID.Equals(record.ID)).ToList();
            if (!StandardUtility.IsEmptyList(details))
                _context.TPAYMENTWFADJUSTHKDETAIL.RemoveRange(details);
            return true;
        }

       

        protected override TPAYMENTWFADJUSTHK BeforeSave(TPAYMENTWFADJUSTHK record, string userName, bool newRecord)
        {

            if (string.IsNullOrWhiteSpace(record.UNITCODE))
                throw new Exception("Kode estate tidak boleh kosong");
            if (string.IsNullOrWhiteSpace(record.PAYMENTDOCNO))
                throw new Exception("No dokumen tidak boleh kosong");

            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATED = currentDate;
                record.CREATEDBY = userName;
            }
            record.UPDATED = currentDate;
            
            record.UPDATEDBY = userName;
            

            //ValidateDetails(record,userName);

            return record;

        }

        

        public override TPAYMENTWFADJUSTHK NewRecord(string userName)
        {
            TPAYMENTWFADJUSTHK record = new TPAYMENTWFADJUSTHK
            {
                CREATED = GetServerTime().Date,
                WFDOCSTATUS = string.Empty,
                WFDOCSTATUSTEXT = "CREATED",
                DATE = DateTime.Today,
                STATUS = "P"
            };
            return record;
        }


        protected override Document WFGenerateDocument(TPAYMENTWFADJUSTHK record, string userName)
        {
            
            string wfFlag = string.Empty;

            string title = $"Pengajuan Adjust HK {record.PAYMENTDOCNO}";
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


        



        public override TPAYMENTWFADJUSTHK GetSingleByWorkflow(Document document, string userName)
        {

            
            var record = _context.TPAYMENTWFADJUSTHK
                .Include(d => d.TPAYMENTWFADJUSTHKDETAIL)
                .FirstOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
            GetEmployeeDetails(record);
            return record;


            
        }

        private void GetEmployeeDetails(TPAYMENTWFADJUSTHK record)
        {
            if (!StandardUtility.IsEmptyList(record.TPAYMENTWFADJUSTHKDETAIL))
            {
                var employeeIds = record.TPAYMENTWFADJUSTHKDETAIL.Select(d => d.EMPID).Distinct().ToList();
                var employeeList = _context.MEMPLOYEE.Where(d => employeeIds.Contains(d.EMPID)).Select(d=> new { d.EMPID, d.EMPNAME,d.POSITIONID }).ToList();

                if (!StandardUtility.IsEmptyList(employeeList))
                {
                    var positionIds = employeeList.Select(d => d.POSITIONID).Distinct().ToList();
                    var positionList = _context.MPOSITION.Where(d => positionIds.Contains(d.POSITIONID)).Select(d => new { d.POSITIONID, d.POSITIONNAME }).ToList();

                    (
                        from a in record.TPAYMENTWFADJUSTHKDETAIL
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

        protected override TPAYMENTWFADJUSTHK GetSingleFromDB(params object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            var record = _context.TPAYMENTWFADJUSTHK
                .Include(d=>d.TPAYMENTWFADJUSTHKDETAIL)                
                .SingleOrDefault(d => d.ID.Equals(Id));

            GetEmployeeDetails(record);
            return record;
            
        }

        
        protected override TPAYMENTWFADJUSTHK WFBeforeSendApproval(TPAYMENTWFADJUSTHK record, string userName, string actionCode, string approvalNote,bool newRecord)
        {
            actionCode = actionCode.ToUpper();
            switch(actionCode)
            {
                case "SUBM":
                    if (StandardUtility.IsEmptyList(record.TPAYMENTWFADJUSTHKDETAIL))
                        throw new Exception("Tidak ada data detail yang diajukan");
                    break;

                case "APRV":
                    Approve(record, userName,false);
                    break;
                case "RVSN":
                    //Nothing To DO
                    break;
                case "RJCT":
                    //Reject All Details
                    if (!StandardUtility.IsEmptyList(record.TPAYMENTWFADJUSTHKDETAIL))
                    {
                        record.TPAYMENTWFADJUSTHKDETAIL.ToList().ForEach(d => {
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


        private void Approve(TPAYMENTWFADJUSTHK record, string userName, bool finalApprove)
        {
            if (StandardUtility.IsEmptyList(record.TPAYMENTWFADJUSTHKDETAIL))
                throw new Exception("Tidak ada data detail yang diajukan");

            int approvedCount = record.TPAYMENTWFADJUSTHKDETAIL.Where(d => d.APPROVED.HasValue && d.APPROVED.Value).Count();
            if (approvedCount<=0)
                throw new Exception("Tidak ada data detail yang disetujui");

            SaveUpdateDetailsToDB(record, userName);
            if (finalApprove)
                record.STATUS = "A";
            record.SYNCTOESTATE = false;
            record.UPDATED = DateTime.Now;
            record.UPDATEDBY = userName;
            _context.SaveChanges();

            
            
        }


        //Running On PMS HO Server ==> Update To Estate
        //private bool ProcessAttendance(PMSContextEstate contextEstate,TPAYMENTWFADJUSTHK header, List<TPAYMENTWFADJUSTHKDETAIL> approvedDetails)
        //{
        //    try
        //    {
        //        _auditContext.SaveAuditTrail("ScheduledJob", header.UNITCODE, $"Start - Update Status WF Adjust HK : {header.ID}", DateTime.Now);
                
        //        if (!StandardUtility.IsEmptyList(approvedDetails))
        //        {
        //            var listHarvestCode = approvedDetails.Select(f => f.HARVESTCODE).Distinct().ToList();

        //            //Get Approved Only Harvest
        //            var listHarvest = contextEstate.THARVEST.Where(f => listHarvestCode.Contains(f.HARVESTCODE) && f.STATUS == "A").ToList();
        //            listHarvestCode = listHarvest.Select(f => f.HARVESTCODE).ToList();

        //            //Update Upload Status 0
        //            listHarvest.ForEach(f =>
        //            {
        //                f.UPLOAD = 0;
        //                f.UPLOADED = null;
        //            });

        //            if (!StandardUtility.IsEmptyList(listHarvest))
        //                contextEstate.THARVEST.UpdateRange(listHarvest);

        //            Attendance attendanceService = new Attendance(contextEstate, _authenticationService, _auditContext);
        //            List<TATTENDANCE> attendanceList = new List<TATTENDANCE>();

        //            approvedDetails.ForEach(f =>
        //            {
        //                var attendance = new TATTENDANCE
        //                {
        //                    EMPLOYEEID = f.EMPID,
        //                    PRESENT = true,
        //                    REMARK = "Adjustment HK",
        //                    HK = f.ADJUSTEDHK,
        //                    //AbsentId = "K",//-*Constant atau Enum
        //                    STATUS = header.STATUS,
        //                    REF = f.HARVESTCODE,
        //                    AUTO = true,
        //                    CREATEBY = header.ID,
        //                    CREATEDDATE = header.CREATED.HasValue ? header.CREATED.Value : DateTime.Now,
        //                    UPDATEBY = header.UPDATEDBY,
        //                    UPDATEDDATE = header.UPDATED.HasValue ? header.UPDATED.Value : DateTime.Now,
        //                };
        //                attendanceList.Add(attendance);
        //            });

                    


        //            if (!StandardUtility.IsEmptyList(attendanceList))
        //            {

        //                (
        //                    from a in attendanceList
        //                    join b in listHarvest on a.REF equals b.HARVESTCODE
        //                    join c in contextEstate.MEMPLOYEE on a.EMPLOYEEID equals c.EMPID
        //                    select new { a, b,c }
        //                ).ToList().ForEach(f =>
        //                {
        //                    f.a.DIVID = f.c.DIVID;
        //                    f.a.DATE = f.b.HARVESTDATE;
        //                });

                        

        //                attendanceService.SaveInsertFromAdjustmentHK(attendanceList);

                        

        //            }

        //        }
        //        contextEstate.SaveChanges();
        //        _auditContext.SaveAuditTrail("ScheduledJob", header.UNITCODE, $"Finish - Update Status WF Adjust HK : {header.ID}", DateTime.Now);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
        //        _auditContext.SaveAuditTrail("ScheduledJob", header.UNITCODE, $"Error Update Status WF Adjust HK : {header.ID} {errorMessage}", DateTime.Now);
        //    }

        //    return false;
        //}


       //Running On PMS Estate Server
        public void SyncByEstate(PMSContextEstate contextEstate, PMSContextHO contextHO)
        {

            DateTime now = GetServerTime();



            var unsyncedRecords = (from a in contextEstate.TPAYMENTWFADJUSTHK.AsNoTracking().Where(d => string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0" || d.STATUS == "P")
                                   join b in contextEstate.TPAYMENTWFADJUSTHKDETAIL.AsNoTracking() on a.ID equals b.ID
                                   select new { Header = a, Details = b })
                                  .ToList();


            if (!StandardUtility.IsEmptyList(unsyncedRecords))
            {

                var unsyncedDetails = unsyncedRecords.Select(d => d.Details).ToList();
                var unsyncedHeaders = unsyncedRecords.Select(d => d.Header).Distinct().ToList();

                var allIds = unsyncedRecords.Select(d => d.Header.ID).ToList();
                var existedOnHO = contextHO.TPAYMENTWFADJUSTHK.Where(d => allIds.Contains(d.ID)).ToList();
                var esistedOnHOIds = existedOnHO.Select(d => d.ID).ToList();
                var newIds = allIds.Where(d => !esistedOnHOIds.Contains(d));

                //only insert not existed record
                var newRecords = unsyncedRecords.Where(d => newIds.Contains(d.Header.ID)).ToList();
                if (!StandardUtility.IsEmptyList(newRecords))
                {
                    contextHO.TPAYMENTWFADJUSTHK.AddRange(newRecords.Select(d => d.Header).Distinct().ToList());
                    contextHO.TPAYMENTWFADJUSTHKDETAIL.AddRange(newRecords.Select(d => d.Details).ToList());
                    contextHO.SaveChanges();
                }


                var hoHeaderRecords =
                (
                    from a in contextHO.TPAYMENTWFADJUSTHK.AsNoTracking().Where(d => allIds.Contains(d.ID))
                    join b in unsyncedHeaders on a.ID equals b.ID
                    where a.WFDOCSTATUS != b.WFDOCSTATUS || a.STATUS != b.STATUS
                    select a
                ).ToList();
                
                if (!StandardUtility.IsEmptyList(hoHeaderRecords))
                {
                        hoHeaderRecords.ForEach(d =>
                        {
                            d.SYNCTOESTATE = true;
                            d.SYNCDATE = now;
                        });
                        var hoIDs = hoHeaderRecords.Select(d => d.ID).ToList();
                        var hoDetailRecord = contextHO.TPAYMENTWFADJUSTHKDETAIL.AsNoTracking().Where(d => hoIDs.Contains(d.ID)).ToList();
                        var estateDetailRecord = contextEstate.TPAYMENTWFADJUSTHKDETAIL.AsNoTracking().Where(d => hoIDs.Contains(d.ID)).ToList();
                        var updatedDetails = (from a in hoDetailRecord
                                             join b in estateDetailRecord on new { a.ID, a.HARVESTCODE, a.EMPID } equals new { b.ID, b.HARVESTCODE, b.EMPID }
                                              where b.APPROVED != a.APPROVED
                                              select a).ToList();

                        if (!StandardUtility.IsEmptyList(updatedDetails))                        
                            contextEstate.TPAYMENTWFADJUSTHKDETAIL.UpdateRange(updatedDetails);
                        
                        contextEstate.TPAYMENTWFADJUSTHK.UpdateRange(hoHeaderRecords);
                        contextEstate.SaveChanges();

                        //contextHO.TPAYMENTWFADJUSTHK.UpdateRange(hoHeaderRecords);
                        //contextHO.SaveChanges();
                    

                }

                

            }

        }

        public void ProcessAttendance(PMSContextEstate contextEstate) 
        {

            var activePeriods = contextEstate.MPERIOD.Where(d => d.ACTIVE1).Select(d => new { d.UNITCODE, STARTDATE = d.FROM1, ENDDATE = d.TO1 }).Union(
                    contextEstate.MPERIOD.Where(d => d.ACTIVE2).Select(d => new { d.UNITCODE, STARTDATE = d.FROM2, ENDDATE = d.TO2 })
                ).Distinct().ToList();


                var unprocessedDetailsCriteria = PredicateBuilder.True<TPAYMENTWFADJUSTHKDETAIL>();
            unprocessedDetailsCriteria = unprocessedDetailsCriteria.And(d =>
                d.APPROVED.HasValue && d.APPROVED.Value  //Approved
                && !(d.CANCEL.HasValue && d.CANCEL.Value)  //Havesting not canceled
                && (d.PROCESS == 0 || string.IsNullOrWhiteSpace(d.ATTDOCID))); //Unprocessed or Already Processed But Errors occured

            var unprocessedHeadersCriteria = PredicateBuilder.True<TPAYMENTWFADJUSTHK>();
            unprocessedHeadersCriteria = unprocessedHeadersCriteria.And(d =>
                d.STATUS == "A"  //Approved 
                && d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value > 0 && !string.IsNullOrWhiteSpace(d.WFDOCSTATUS)); //Approved By Workflow


            

            var approvedData =
            (
                from a in contextEstate.TPAYMENTWFADJUSTHK.Where(unprocessedHeadersCriteria)
                join b in contextEstate.TPAYMENTWFADJUSTHKDETAIL.Where(unprocessedDetailsCriteria) on a.ID equals b.ID
                join c in contextEstate.THARVEST.Where(e => e.STATUS == "A") on b.HARVESTCODE equals c.HARVESTCODE
                join e in contextEstate.MEMPLOYEE on b.EMPID equals e.EMPID 
                join f in activePeriods on e.UNITCODE equals f.UNITCODE 
                where c.HARVESTDATE >= f.STARTDATE && c.HARVESTDATE<=f.ENDDATE
                select new { b.ID, c.HARVESTCODE, DATE = c.HARVESTDATE, EMPLOYEEID = b.EMPID, b.ADJUSTEDHK, a.STATUS, e.DIVID, a.CREATED, a.CREATEDBY, a.UPDATED, a.UPDATEDBY,b.PROCESS,b.PROCESSSTATUS,b.ATTDOCID }
            ).ToList();



            var lastIDs = approvedData.GroupBy(e => new { e.HARVESTCODE, e.DATE, e.EMPLOYEEID })
                .Select(e => new { e.Key.HARVESTCODE, e.Key.DATE, e.Key.EMPLOYEEID, ID = e.Max(s => s.ID) }).ToList();

            var uniqueApprovedData = (from a in approvedData
                                        join b in lastIDs on new { a.ID, a.HARVESTCODE, a.EMPLOYEEID, a.DATE } equals new { b.ID, b.HARVESTCODE, b.EMPLOYEEID, b.DATE }
                                        select a).ToList();

            //var divIds = contextEstate.MDIVISI.Where(e => e.UNITCODE.Equals(d.UNITCODE)).Select(e=>e.DIVID).ToList();

            var employeeIDs = uniqueApprovedData.Select(e => e.EMPLOYEEID).Distinct().ToList();

            var activePeriodsByDivision = (from a in activePeriods
                                           join b in contextEstate.MDIVISI on a.UNITCODE equals b.UNITCODE
                                           select new { a.UNITCODE, b.DIVID, a.STARTDATE, a.ENDDATE }).ToList();


            var attendanceSummary = 
                            (
                                from a in contextEstate.TATTENDANCE.Where(e => e.STATUS == "A" && employeeIDs.Contains(e.EMPLOYEEID))
                                join b in activePeriodsByDivision on a.DIVID equals b.DIVID
                                join c in uniqueApprovedData on new { a.EMPLOYEEID, a.DATE } equals new { c.EMPLOYEEID, c.DATE }
                                where a.DATE>=b.STARTDATE && a.DATE<=b.ENDDATE
                                select a
                            )
                            .GroupBy(e => new { e.EMPLOYEEID, e.DATE })
                            .Select(e => new
                            {
                                e.Key.EMPLOYEEID,
                                e.Key.DATE,
                                HKTotal = e.Sum(s => s.HK),
                                HK = e.Sum(s => s.REMARK.StartsWith("Adjustment HK") ? 0 : s.HK),
                                AdjustmentHK = e.Sum(s => s.REMARK.StartsWith("Adjustment HK") ? s.HK : 0)
                            }).ToList();



            var unprocessedData =
            (
                from a in uniqueApprovedData
                join b in attendanceSummary on new { a.EMPLOYEEID, a.DATE } equals new { b.EMPLOYEEID, b.DATE } into ab
                from abLeft in ab.DefaultIfEmpty()
                where abLeft == null || (abLeft != null && abLeft.AdjustmentHK != a.ADJUSTEDHK)
                select a
            ).Distinct().ToList();

            

            if (!StandardUtility.IsEmptyList(unprocessedData))
            {

                
                List<TPAYMENTWFADJUSTHKDETAIL> processResult = new List<TPAYMENTWFADJUSTHKDETAIL>();
                Attendance attendanceService = new Attendance(contextEstate, _authenticationService, _auditContext);



                unprocessedData.ForEach(f => {
                    try
                    {
                        var attendance = new TATTENDANCE
                        {
                            EMPLOYEEID = f.EMPLOYEEID,
                            PRESENT = true,
                            REMARK = "Adjustment HK",
                            HK = f.ADJUSTEDHK,
                            //AbsentId = "K",//-*Constant atau Enum
                            STATUS = f.STATUS,
                            REF = f.HARVESTCODE,
                            AUTO = true,
                            DATE = f.DATE,
                            DIVID = f.DIVID,
                            CREATEBY = f.ID,
                            CREATEDDATE = f.CREATED.HasValue ? f.CREATED.Value : DateTime.Now,
                            UPDATEBY = f.UPDATEDBY,
                            UPDATEDDATE = f.UPDATED.HasValue ? f.UPDATED.Value : DateTime.Now
                        };
                        attendanceService.SaveInsert(attendance, f.ID);
                        processResult.Add(new TPAYMENTWFADJUSTHKDETAIL { ID = f.ID, HARVESTCODE = f.HARVESTCODE, EMPID = f.EMPLOYEEID, PROCESS = 1, PROCESSSTATUS = string.Empty, ATTDOCID = attendance.ID });
                            
                    }
                    //attendanceList.Add(attendance);
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                        processResult.Add(new TPAYMENTWFADJUSTHKDETAIL { ID = f.ID, HARVESTCODE = f.HARVESTCODE, EMPID = f.EMPLOYEEID, PROCESS = 2, PROCESSSTATUS = errorMessage, ATTDOCID = string.Empty });

                    }
                });


                var successHarvestID = processResult.Where(d => d.PROCESS == 1).Select(d => d.HARVESTCODE).Distinct().ToList();
                if (!StandardUtility.IsEmptyList(successHarvestID))
                {
                    var listHarvest = contextEstate.THARVEST.AsNoTracking().Where(e => e.STATUS == "A" & successHarvestID.Contains(e.HARVESTCODE)).ToList();
                    listHarvest.ForEach(e => {
                        e.UPLOAD = 0;
                        e.UPLOADED = null;
                    });
                    contextEstate.UpdateRange(listHarvest);
                    contextEstate.SaveChanges();
                }

                var unprocessedDataIds = unprocessedData.Select(d => d.ID).Distinct().ToList();

                var processResultUpdate =

                (
                    from a in contextEstate.TPAYMENTWFADJUSTHKDETAIL.AsNoTracking().Where(d=>unprocessedDataIds.Contains(d.ID)).ToList()
                    join b in processResult on new { a.ID, a.HARVESTCODE, a.EMPID } equals new { b.ID, b.HARVESTCODE, b.EMPID }
                    select new { TPAYMENTWFADJUSTHKDETAIL = a, ProcessResult = b }
                ).ToList();

                processResultUpdate.ForEach(d => {
                    d.TPAYMENTWFADJUSTHKDETAIL.PROCESS = d.ProcessResult.PROCESS;
                    d.TPAYMENTWFADJUSTHKDETAIL.PROCESSSTATUS = d.ProcessResult.PROCESSSTATUS;
                    d.TPAYMENTWFADJUSTHKDETAIL.ATTDOCID = d.ProcessResult.ATTDOCID;
                });
                contextEstate.UpdateRange(processResultUpdate.Select(d => d.TPAYMENTWFADJUSTHKDETAIL).ToList());
                contextEstate.SaveChanges();

                
            }


            
        }


        //Running On PMS Ho Server
        public void SubmitToWFByHO(PMSContextHO contextHO)
        {

            var listUnApproved = contextHO.TPAYMENTWFADJUSTHK.Include(d => d.TPAYMENTWFADJUSTHKDETAIL).Where(d => (string.IsNullOrWhiteSpace(d.WFDOCSTATUS) || d.WFDOCSTATUS == "0")).ToList();
            if (!StandardUtility.IsEmptyList(listUnApproved))
            {
                listUnApproved.ForEach(d => {
                    try
                    {
                        WFSendApproval(d, d.CREATEDBY, "SUBM", "Submit for Approval", false);
                        _auditContext.SaveAuditTrail("ScheduledJob", "SubmitToWF  Adjustment HK " + d.ID, "Success Ask Approval");
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                        _auditContext.SaveAuditTrail("ScheduledJob", "SubmitToWF Adjustment HK " + d.ID, "Error Ask Approval - " + errorMessage);                        
                    }
                });
            }

        }





        
        #region Tools

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
            var estateRecords = contextEstate.TPAYMENTWFADJUSTHK.AsNoTracking().Where(e => e.DATE >= startDate && e.DATE <= endDate).ToList();
            if (!StandardUtility.IsEmptyList(estateRecords))
            {
                var recordIds = estateRecords.Select(e => e.ID).ToList();
                var serverRecords = contextHO.TPAYMENTWFADJUSTHK.AsNoTracking().Where(e => recordIds.Contains(e.ID)).ToList();
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
                    var details = contextHO.TPAYMENTWFADJUSTHKDETAIL.AsNoTracking().Where(e => unsyncIds.Contains(e.ID)).ToList();
                    contextEstate.TPAYMENTWFADJUSTHK.UpdateRange(unsyncRecords);
                    contextEstate.TPAYMENTWFADJUSTHKDETAIL.UpdateRange(details);
                    contextEstate.SaveChanges();
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]{estateRecords.Count} records are re-syncronized");
                }
            }

        }



        public void ReprocessAttendance(PMSContextHO contextHO, List<string> unitCodes, DateTime startDate, DateTime endDate)
        {

            string processName = "Process Attendance";
            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Start Process");

            

            var criteria = PredicateBuilder.True<MUNITDBSERVER>();
            if (!StandardUtility.IsEmptyList(unitCodes))
            {
                criteria = criteria.And(d => unitCodes.Contains(d.UNITCODE));
            }
            contextHO.MUNITDBSERVER.AsNoTracking().Where(criteria).ToList().ForEach(d => {
                if (StandardUtility.NetWorkPing(d.SERVERNAME))
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Server {d.SERVERNAME} is ONLINE");
                    try
                    {
                        using (PMS.EFCore.Model.PMSContextEstate contextEstate = new PMS.EFCore.Model.PMSContextEstate(DBContextOption<PMS.EFCore.Model.PMSContextEstate>.GetOptions(d.SERVERNAME, d.DBNAME, d.DBUSER, d.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
                        {
                            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{d.UNITCODE}]Start Scanning Records");

                            ProcessAttendance(contextEstate);


                        }
                        Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{d.UNITCODE}]Finish Scanning Records");


                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                        Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Error [{d.UNITCODE}:{d.SERVERNAME}:{d.DBNAME}]{errorMessage}");
                        
                    }
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Server {d.SERVERNAME} is OFFLINE");
                }
            });

            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Finish Process");
        }


        public void CheckHKLebih1Dari1(PMSContextHO contextHO, List<string> unitCodes, DateTime startDate, DateTime endDate)
        {

            string processName = "Check HK Lebih Dari 1";
            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Start Scanning");



            var criteria = PredicateBuilder.True<MUNITDBSERVER>();
            if (!StandardUtility.IsEmptyList(unitCodes))
            {
                criteria = criteria.And(d => unitCodes.Contains(d.UNITCODE));
            }

            List<string> offLineServers = new List<string>();
            List<string> notOKServers = new List<string>();

            contextHO.MUNITDBSERVER.AsNoTracking().Where(criteria).Select(d => new { d.SERVERNAME, d.DBNAME, d.DBUSER, d.DBPASSWORD }).Distinct().ToList().ForEach(d => {
                if (StandardUtility.NetWorkPing(d.SERVERNAME))
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Server {d.SERVERNAME}:{d.DBNAME} is ONLINE");
                    if (!CheckHKLebihDari1(d.SERVERNAME, d.DBNAME, d.DBUSER, d.DBPASSWORD))
                        notOKServers.Add(d.SERVERNAME);

                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Server {d.SERVERNAME}:{d.DBNAME} is OFFLINE");
                    offLineServers.Add(d.SERVERNAME);
                }
            });

            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Finish Scanning");

            if (!StandardUtility.IsEmptyList(offLineServers))
            {
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Offline Servers : ");
                (from a in contextHO.MUNITDBSERVER.Where(d => offLineServers.Contains(d.SERVERNAME))
                 join b in contextHO.MUNIT on a.UNITCODE equals b.UNITCODE
                 select new { b.UNITCODE, b.ALIAS, a.SERVERNAME, a.DBNAME }).ToList().ForEach(d => {
                     Console.WriteLine(d);
                 });
            }

            if (!StandardUtility.IsEmptyList(notOKServers))
            {
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]NOT OK Servers : ");
                (from a in contextHO.MUNITDBSERVER.Where(d => notOKServers.Contains(d.SERVERNAME))
                 join b in contextHO.MUNIT on a.UNITCODE equals b.UNITCODE
                 select new { b.UNITCODE, b.ALIAS, a.SERVERNAME, a.DBNAME }).ToList().ForEach(d => {
                     Console.WriteLine(d);
                 });
            }
        }


        public class CheckCount
        {
            public int RCOUNT { get; set; }
        }

        private bool CheckHKLebihDari1(string serverName, string dbName, string dbUser, string dbPassword)
        {
            string processName = "Check HK Lebih Dari 1";

            CheckCount checkCount;
            bool result = false;

            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Start Scanning");

            
            string sql = "Select COUNT(*) RCOUNT " +
                "From TATTENDANCE A " +
                    "Inner Join( " +
                "Select EMPLOYEEID,[DATE] " +
                "From TATTENDANCE " +
                "Where[DATE] >= '2022-02-01' And STATUS = 'A' " +
                "Group By EMPLOYEEID,[DATE] " +
                "Having Sum(HK) > 1 " +
                ") B On A.EMPLOYEEID = B.EMPLOYEEID And A.DATE = B.DATE " +
                "Where A.[DATE] >= '2022-02-01' And A.STATUS = 'A'";
            try
            {
                using (PMS.EFCore.Model.PMSContextBase contextEstate = new PMS.EFCore.Model.PMSContextBase(DBContextOption<PMS.EFCore.Model.PMSContextBase>.GetOptions(serverName, dbName, dbUser, dbPassword, PMSConstants.ConnectionStringEncryptionKey)))
                {
                    contextEstate.ExecuteSqlText(sql).Exec(r =>
                    {
                        checkCount = r.ToList<CheckCount>().FirstOrDefault();
                        if (checkCount == null)
                        {
                            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]OK");
                            
                            result = true;
                        }
                        else
                        {
                            if (checkCount.RCOUNT == 0)
                            {
                                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]OK");
                                result = true;
                            }
                            else
                                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Not OK");
                        }
                    });
                }
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Finish Scanning");
            }
            catch (Exception ex)
            {
                string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Error {errorMessage}");
            }
            return result;
        }


        private void FixUnapprroveDetail(string serverName, string dbName, string dbUser, string dbPassword)
        {
            string processName = "Fix Unapproved Details";

            
            

            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Start Scanning");


            string sql = "Update A Set A.STATUS='P' From TPAYMENTWFADJUSTHK A Where STATUS='A' And Exists(Select * From TPAYMENTWFADJUSTHKDETAIL B Where APPROVED IS NULL And B.ID=A.ID )";
            try
            {
                using (PMS.EFCore.Model.PMSContextBase contextEstate = new PMS.EFCore.Model.PMSContextBase(DBContextOption<PMS.EFCore.Model.PMSContextBase>.GetOptions(serverName, dbName, dbUser, dbPassword, PMSConstants.ConnectionStringEncryptionKey)))
                {
                    contextEstate.ExecuteSqlText(sql).ExecNonQuery();
                }
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Finish Scanning");
            }
            catch (Exception ex)
            {
                string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Error {errorMessage}");
            }
            
        }

        public void FixUnapprroveDetail(PMSContextHO contextHO, List<string> unitCodes)
        {

            string processName = "Fix Unapproved Detail";
            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Start Scanning");



            var criteria = PredicateBuilder.True<MUNITDBSERVER>();
            if (!StandardUtility.IsEmptyList(unitCodes))
            {
                criteria = criteria.And(d => unitCodes.Contains(d.UNITCODE));
            }

            List<string> offLineServers = new List<string>();
            List<string> notOKServers = new List<string>();

            contextHO.MUNITDBSERVER.AsNoTracking().Where(criteria).Select(d => new { d.SERVERNAME, d.DBNAME, d.DBUSER, d.DBPASSWORD }).Distinct().ToList().ForEach(d => {
                if (StandardUtility.NetWorkPing(d.SERVERNAME))
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Server {d.SERVERNAME}:{d.DBNAME} is ONLINE");
                    FixUnapprroveDetail(d.SERVERNAME, d.DBNAME, d.DBUSER, d.DBPASSWORD);
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Server {d.SERVERNAME}:{d.DBNAME} is OFFLINE");
                    
                }
            });

            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Finish Scanning");

            if (!StandardUtility.IsEmptyList(offLineServers))
            {
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Offline Servers : ");
                (from a in contextHO.MUNITDBSERVER.Where(d => offLineServers.Contains(d.SERVERNAME))
                 join b in contextHO.MUNIT on a.UNITCODE equals b.UNITCODE
                 select new { b.UNITCODE, b.ALIAS, a.SERVERNAME, a.DBNAME }).ToList().ForEach(d => {
                     Console.WriteLine(d);
                 });
            }

            if (!StandardUtility.IsEmptyList(notOKServers))
            {
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]NOT OK Servers : ");
                (from a in contextHO.MUNITDBSERVER.Where(d => notOKServers.Contains(d.SERVERNAME))
                 join b in contextHO.MUNIT on a.UNITCODE equals b.UNITCODE
                 select new { b.UNITCODE, b.ALIAS, a.SERVERNAME, a.DBNAME }).ToList().ForEach(d => {
                     Console.WriteLine(d);
                 });
            }
        }


        private void FixProcessResultFlag(string serverName, string dbName, string dbUser, string dbPassword)
        {
            string processName = "Fix Unapproved Details";




            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Start Scanning");


            string sql = "Update A " +
                         "Set A.PROCESS = 1,A.ATTDOCID = B.ID " +
                         "From TPAYMENTWFADJUSTHKDETAIL A " +
                            "Inner Join TATTENDANCE B On A.ID = B.CREATEBY And A.EMPID = B.EMPLOYEEID And A.HARVESTCODE = B.REF And A.HARVESTDATE = B.[DATE] " +
                            "Inner Join TPAYMENTWFADJUSTHK C ON A.ID = C.ID " +
                        "Where B.STATUS = 'A' And B.REMARK = 'Adjustment HK' And C.Status = 'A' And A.Process <> 1";

            try
            {
                using (PMS.EFCore.Model.PMSContextBase contextEstate = new PMS.EFCore.Model.PMSContextBase(DBContextOption<PMS.EFCore.Model.PMSContextBase>.GetOptions(serverName, dbName, dbUser, dbPassword, PMSConstants.ConnectionStringEncryptionKey)))
                {
                    contextEstate.ExecuteSqlText(sql).ExecNonQuery();
                }
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Finish Scanning");
            }
            catch (Exception ex)
            {
                string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{serverName}:{dbName}]Error {errorMessage}");
            }

        }

        public void FixProcessResultFlag(PMSContextHO contextHO, List<string> unitCodes)
        {

            string processName = "Fix Process Result";
            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Start Scanning");



            var criteria = PredicateBuilder.True<MUNITDBSERVER>();
            if (!StandardUtility.IsEmptyList(unitCodes))
            {
                criteria = criteria.And(d => unitCodes.Contains(d.UNITCODE));
            }

            List<string> offLineServers = new List<string>();
            List<string> notOKServers = new List<string>();

            contextHO.MUNITDBSERVER.AsNoTracking().Where(criteria).Select(d => new { d.SERVERNAME, d.DBNAME, d.DBUSER, d.DBPASSWORD }).Distinct().ToList().ForEach(d => {
                if (StandardUtility.NetWorkPing(d.SERVERNAME))
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Server {d.SERVERNAME}:{d.DBNAME} is ONLINE");
                    FixProcessResultFlag(d.SERVERNAME, d.DBNAME, d.DBUSER, d.DBPASSWORD);
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Server {d.SERVERNAME}:{d.DBNAME} is OFFLINE");

                }
            });

            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Finish Scanning");

            if (!StandardUtility.IsEmptyList(offLineServers))
            {
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]Offline Servers : ");
                (from a in contextHO.MUNITDBSERVER.Where(d => offLineServers.Contains(d.SERVERNAME))
                 join b in contextHO.MUNIT on a.UNITCODE equals b.UNITCODE
                 select new { b.UNITCODE, b.ALIAS, a.SERVERNAME, a.DBNAME }).ToList().ForEach(d => {
                     Console.WriteLine(d);
                 });
            }

            if (!StandardUtility.IsEmptyList(notOKServers))
            {
                Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}]NOT OK Servers : ");
                (from a in contextHO.MUNITDBSERVER.Where(d => notOKServers.Contains(d.SERVERNAME))
                 join b in contextHO.MUNIT on a.UNITCODE equals b.UNITCODE
                 select new { b.UNITCODE, b.ALIAS, a.SERVERNAME, a.DBNAME }).ToList().ForEach(d => {
                     Console.WriteLine(d);
                 });
            }
        }

        #endregion


    }
}
