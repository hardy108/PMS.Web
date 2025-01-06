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

namespace PMS.EFCore.Services.Attendances
{
    public class Attendance : EntityFactory<TATTENDANCE,TATTENDANCE,GeneralFilter,PMSContextBase>
    {

        private Period _serviceperiod ;
        private Employee _serviceemployee;
        private AuthenticationServiceBase _authenticationService;
        public Attendance(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Attendance";
            _authenticationService = authenticationService;
            _serviceperiod = new Period(_context,_authenticationService,auditContext);
            _serviceemployee = new Employee(_context,_authenticationService,auditContext);
        }


        private string FieldsValidation(TATTENDANCE attendance, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(attendance.ID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(attendance.DIVID)) result += "Divisi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(attendance.EMPLOYEEID)) result += "Karyawan tidak boleh kosong." + Environment.NewLine;
            if (attendance.DATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            //if (attendance.HK == 0) result += "HK tidak boleh kosong." + Environment.NewLine;
            if (attendance.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;
            return result;
        }

        protected override TATTENDANCE GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            return _context.TATTENDANCE
                .Include(d => d.EMPLOYEE)
                .Include(d => d.DIV)
                .SingleOrDefault(d => d.ID.Equals(Id));
        }

        public override TATTENDANCE CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TATTENDANCE record = base.CopyFromWebFormData(formData, newRecord);//If Required Replace with your custom code

            if (record.EMPLOYEE_FOR_VALIDATION == null)
            {
                var employee = (from a in _context.MEMPLOYEE
                                where a.EMPID.Equals(record.EMPLOYEEID)
                                select a).FirstOrDefault();

                if (employee != null)
                {
                    record.EMPLOYEE_FOR_VALIDATION = new MEMPLOYEE();
                    record.EMPLOYEE_FOR_VALIDATION.CopyFrom(employee);
                }

            }

            if (string.IsNullOrEmpty(record.UNITCODE))
            {
                string unitCode = _context.MDIVISI.Where(s => s.DIVID.Equals(record.DIVID)).Select(d => d.UNITCODE).FirstOrDefault();
                if (string.IsNullOrEmpty(unitCode))
                    throw new Exception("Unit/Divisi tidak valid - pilih karyawan terlebih dahulu");

                record.UNITCODE = unitCode;
            }


            if (record.REF == null)
                record.REF = "";
            if (record.REMARK == null)
                record.REMARK = "";

            return record;
        }

        protected override TATTENDANCE BeforeDelete(TATTENDANCE record, string userName)
        {
            if (string.IsNullOrEmpty(record.UNITCODE))
            {
                string unitCode = _context.MDIVISI.Where(s => s.DIVID.Equals(record.DIVID)).Select(d => d.UNITCODE).FirstOrDefault();
                if (string.IsNullOrEmpty(unitCode))
                    throw new Exception("Unit/Divisi tidak valid");

                record.UNITCODE = unitCode;
            }

            _serviceperiod.CheckValidPeriod(record.UNITCODE, record.DATE);
            if (record.AUTO) throw new Exception("Data harus dihapus melalui " + record.REF);

            
            
            return record; // base.BeforeDelete(record, userName);
        }

        protected override TATTENDANCE BeforeSave(TATTENDANCE record, string userName,bool newRecord)
        {
            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATEDDATE = currentDate;
                record.CREATEBY = userName;
                record.ID = PMSConstants.AttendanceIdPrefix + record.DIVID + record.UPDATEDDATE.ToString("yyyyMMdd") + HelperService.GetLastDocumentNumber(PMSConstants.AttendanceIdPrefix + record.DIVID, _context).ToString("0000");
                record.STATUS = PMSConstants.TransactionStatusApproved;
            }
            record.UPDATEDDATE = currentDate;
            record.UPDATEBY = userName;


            if (record.EMPLOYEE_FOR_VALIDATION == null)
            {
                var employee = (from a in _context.MEMPLOYEE
                                where a.EMPID.Equals(record.EMPLOYEEID) && a.STATUS == "A"
                                select a).FirstOrDefault();
                if (employee != null)
                {
                    record.EMPLOYEE_FOR_VALIDATION = new MEMPLOYEE();
                    record.EMPLOYEE_FOR_VALIDATION.CopyFrom(employee);
                }
            }

            if (record.EMPLOYEE_FOR_VALIDATION == null)            
                throw new Exception("Karyawan tidak ditemukan atau tidak aktif");


            if (string.IsNullOrWhiteSpace(record.DIVID))
            {
                record.DIVID = record.EMPLOYEE_FOR_VALIDATION.DIVID;
                record.UNITCODE = record.EMPLOYEE_FOR_VALIDATION.UNITCODE;
            }

            if (string.IsNullOrEmpty(record.UNITCODE))
            {
                string unitCode = _context.MDIVISI.Where(s => s.DIVID.Equals(record.DIVID)).Select(d => d.UNITCODE).FirstOrDefault();
                if (string.IsNullOrEmpty(unitCode))
                    throw new Exception("Unit/Divisi tidak valid");

                record.UNITCODE = unitCode;
            }

            string result = this.FieldsValidation(record, userName);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            this.Validate(record, userName);


            //DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            //if (newRecord)
            //{
            //    record.CREATEDDATE = currentDate;
            //    record.CREATEBY = userName;
            //    record.ID = PMSConstants.AttendanceIdPrefix + record.DIVID + record.UPDATEDDATE.ToString("yyyyMMdd") + HelperService.GetLastDocumentNumber(PMSConstants.AttendanceIdPrefix + record.DIVID, _context).ToString("0000");
            //    record.STATUS = PMSConstants.TransactionStatusApproved;
            //}
            //record.UPDATEDDATE = currentDate;
            //record.UPDATEBY = userName;

            return record;
        }

       

       
        private void Validate(TATTENDANCE attendance, string userName)
        {
            if (!attendance.PRESENT && string.IsNullOrEmpty(attendance.ABSENTCODE))
                throw new Exception("Kode absen tidak boleh kosong karena pegawai tidak hadir.");

            if (attendance.PRESENT && !string.IsNullOrEmpty(attendance.ABSENTCODE))
                throw new Exception("Kode absen tidak boleh diisi karena pegawai hadir.");

            if (attendance.PRESENT && attendance.HK <= 0)
                throw new Exception("HK tidak boleh kosong.");

            if (!string.IsNullOrEmpty(attendance.ABSENTCODE))
            {
                var absentType = _context.MABSENTTYPE.SingleOrDefault(d => d.ABSENTCODE.Equals(attendance.ABSENTCODE));

                if (!string.IsNullOrEmpty(absentType.ABSENSEX))
                    if (absentType.ABSENSEX != attendance.EMPLOYEE_FOR_VALIDATION.EMPSEX)
                        throw new Exception("Alasan absen bukan untuk jenis kelamin yang bersangkutan");
            }
            
            

            
            _serviceperiod.CheckValidPeriod(attendance.UNITCODE, attendance.DATE);
            //_serviceperiod.CheckMaxPeriod(attendance.UNITCODE, attendance.DATE);

            // Remark by Abi -- 31/05/2022
            //if (attendance.EMPLOYEE_FOR_VALIDATION.EMPTYPE.ToUpper().StartsWith("SKU"))
            //{
            //    var holiday = _context.TCALENDAR.SingleOrDefault(d => d.UNITCODE == attendance.UNITCODE && d.DTDATE == attendance.DATE);
            //    if (holiday != null)
            //        if (holiday.STATUS == PMSConstants.TransactionStatusApproved)
            //            throw new Exception("SKU tidak boleh kerja hari Minggu/hari Besar.");

            //    //if (attendance.HK > 1)//-*Parameter
            //    //    throw new Exception("Pegawai bersangkutan adalah pegawai SKU. HK tidak boleh lebih dari 1.");
            //    //
            //}


            //Add by Abi -- 31/05/2022
            if (attendance.EMPLOYEE_FOR_VALIDATION.EMPTYPE.ToUpper().StartsWith("SKU"))
            {
                bool isHoliday = false;

                var empSchedule = _context.MSCHEDULEEMPLOYEE.SingleOrDefault(d => d.UNITID == attendance.UNITCODE && d.DATE == attendance.DATE && d.EMPLOYEEID.Equals(attendance.EMPLOYEEID));
                //PMSServices.ScheduleEmployee.Get(attendance.EmployeeId, attendance.Date, databases);
                if (empSchedule != null)
                {
                    if (empSchedule.STATUS == PMSConstants.TransactionStatusApproved)
                        if (empSchedule.HOLIDAY)
                            isHoliday = true;
                }
                else
                {
                   var holiday = _context.TCALENDAR.SingleOrDefault(d => d.UNITCODE == attendance.UNITCODE && d.DTDATE == attendance.DATE);
                    if (holiday != null)
                        if (holiday.STATUS == PMSConstants.TransactionStatusApproved)
                            isHoliday = true;
                }

                if (isHoliday)
                    throw new Exception("SKU tidak boleh kerja hari Libur.");
            }


            if (attendance.HK > 1)//-*Parameter
                throw new Exception(attendance.DATE.ToString("dd-MMM-yyyy") + " HK tidak boleh lebih dari 1.");

            var currentAttendance = GetByEmployeeAndDate(attendance.EMPLOYEEID, attendance.DATE);
            currentAttendance.RemoveAll(a => a.ID == attendance.ID || a.STATUS != PMSConstants.TransactionStatusApproved);
            var q = from itm in currentAttendance select itm.HK;

            decimal currentHk = q.Sum();
            //if (employee.TypeCode.ToUpper().StartsWith("SKU") && currentHk + attendance.HK > 1)//-*Parameter
            //    throw new Exception("Pegawai bersangkutan adalah pegawai SKU. HK tidak boleh lebih dari 1 dalam 1 hari.");
            //if (employee.TypeCode.ToUpper().StartsWith("BHL") && currentHk + attendance.HK > 2)//-*Parameter
            //    throw new Exception("Pegawai bersangkutan adalah pegawai BHL. HK tidak boleh lebih dari 2 dalam 1 hari.");
            if (currentHk + attendance.HK > 1)//-*Parameter
                throw new Exception(attendance.EMPLOYEEID + " " + attendance.DATE.ToString("dd-MMM-yyyy") + " HK tidak boleh lebih dari 1 dalam 1 hari.");

            if (!attendance.AUTO)
            {
                if (HelperService.GetConfigValue(PMSConstants.CfgAttendanceForbidDiv + attendance.EMPLOYEE_FOR_VALIDATION.UNITCODE,_context) == PMSConstants.CfgAttendanceForbidDivTrue)
                {
                    var div = _context.VDIVISI.SingleOrDefault(d => d.DIVID == attendance.DIVID);
                    if (div.CODE != "20" && div.CODE != "30" && div.CODE != "40"
                        && (attendance.ABSENTCODE == "K" || attendance.ABSENTCODE == ""))
                        throw new Exception("Absensi hanya untuk karyawan GA.");
                }

                if (attendance.PRESENT)
                {
                    
                    if (HelperService.GetConfigValue(PMSConstants.CfgAttendanceCheckFinger + attendance.UNITCODE, _context) == PMSConstants.CfgAttendanceCheckFingerTrue)
                    {
                        var isExist = CheckAttendance(attendance.UNITCODE, attendance.EMPLOYEEID, attendance.DATE, "K", string.Empty);
                        if (isExist == 0)
                            throw new Exception("Absensi karyawan " + attendance.EMPLOYEEID + " tanggal " + attendance.DATE.ToString("dd/MM/yyyy") + " tidak valid.");
                    }
                }
            }

            //string result = this.FieldsValidation(attendance);
            //if (!string.IsNullOrEmpty(result))
            //    throw new Exception(result);
        }

        protected override TATTENDANCE AfterSave(TATTENDANCE record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.AttendanceIdPrefix + record.DIVID, _context);
            return record;
        }

        

        protected override bool DeleteFromDB(TATTENDANCE record, string userName)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);
            record.STATUS = PMSConstants.TransactionStatusDeleted;
            record.UPDATEDDATE = now;
            _context.TATTENDANCE.Update(record);
            _context.SaveChanges();
            SaveAuditTrail(record, userName, "Delete Status Record");
            return true;
        }

      
        public int CheckAttendance(string unitCode, string empId, DateTime date, string status, string cardType)
        {
            int result = _context.TATTENDANCETRX.Where(d => d.UNITID == unitCode && d.EMPID == empId && d.DATE == date
            && (d.ATTSTATUS == status || status == string.Empty)
            && ((d.CARDIN == cardType && d.CARDOUT == cardType) || cardType == string.Empty)
            && (d.STATUS == status || status == string.Empty)
            ).Count();

            if (result<=0)
            {
                result =
                (
                    from a in _context.TATTENDANCEPROBLEM
                    join b in _context.TATTENDANCEPROBLEMEMPLOYEE on a.ID equals b.ID
                    join c in _context.MABSENCEREASON on b.REASONID equals c.ID
                    where a.WFDOCSTATUS.Equals("9999") && (b.APPROVED.HasValue && b.APPROVED.Value) && !c.FAILEDFINGER && b.EMPID.Equals(empId) && b.TIME.HasValue && b.TIME.Value.Date == date.Date
                    select b.EMPID
                ).Count();
            }
            return result;
        }

        public IEnumerable<TATTENDANCETRX> CheckAttendances(FilterAttendance filter)
        {
            var criteria = PredicateBuilder.True<TATTENDANCETRX>();
            criteria = criteria.And(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved));
            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => d.ATTSTATUS.Equals(filter.RecordStatus));
            

            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(d => d.UNITID == filter.UnitID);
            if (filter.UnitIDs.Any())
                criteria = criteria.And(d => filter.UnitIDs.Contains(d.UNITID));

            
            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(d => d.EMPID == filter.Id);
            if (filter.Ids.Any())
                criteria = criteria.And(d => filter.Ids.Contains(d.EMPID));

            criteria = criteria.And(d => d.DATE.Date >= filter.StartDate.Date && d.DATE.Date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.CardType))
                criteria = criteria.And(d => d.CARDIN.Equals(filter.CardType) && d.CARDOUT.Equals(filter.CardType));

            if (filter.PageSize <= 0)
                return _context.TATTENDANCETRX.Where(criteria);
            return _context.TATTENDANCETRX.Include(e => e.EMPLOYEE).Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public void DeleteByReferences(string references)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);

            var tatt = _context.TATTENDANCE.Where(d => d.REF == references).ToList();
            foreach(var att in tatt)
            {
                att.STATUS = PMSConstants.TransactionStatusDeleted;
                att.UPDATEDDATE = now;
            }
            
            _context.TATTENDANCE.UpdateRange(tatt);
            //Security.Audit.Insert(userName, _serviceName, now, $"Delete {references}", _context);
            _context.SaveChanges();

            foreach (var att in tatt)
            {
                SaveAuditTrail(att, att.UPDATEBY, "Delete Status Record");
            }
        }

        public void DeleteByReferencesNik(string references, string employeeId)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);

            var tatt = _context.TATTENDANCE.Where(d => d.REF == references && d.EMPLOYEEID == employeeId).ToList();
            foreach (var att in tatt)
            {
                att.STATUS = PMSConstants.TransactionStatusDeleted;
                att.UPDATEDDATE = now;
            }

            _context.TATTENDANCE.UpdateRange(tatt);
            //Security.Audit.Insert(userName, _serviceName, now, $"Delete {references}", _context);
            _context.SaveChanges();
            foreach (var att in tatt)
            {
                SaveAuditTrail(att, att.UPDATEBY, "Delete Status Record");
            }
        }

        public List<TATTENDANCE> GetByEmployeeAndDate(string employeeId, DateTime dateTime)
        {
            return _context.TATTENDANCE.Where(d => d.EMPLOYEEID == employeeId && d.DATE == dateTime && d.STATUS != PMSConstants.TransactionStatusDeleted).ToList();

        }

        public List<MEMPLOYEE> GetEmployeeCandidate(string unitCode, string divisionId, string employeeCode, string employeeName)
        {
            string divId = string.Empty;
            if (!string.IsNullOrEmpty(divisionId)) divId = divisionId;
            return _context.MEMPLOYEE.Where(a => a.UNITCODE.Contains(unitCode) && a.DIVID.Contains(divisionId) && 
                    a.EMPCODE.Contains(employeeCode) && a.EMPNAME.Contains(employeeName)).ToList();
        }

        public List<sp_Attendance_GetGroupingByUnitAndDate_Result> GetGroupingByUnitAndDate(string unitCode, DateTime startDate, DateTime endDate, bool useFingerprint)
        {
            return _context.sp_Attendance_GetGroupingByUnitAndDate(unitCode, startDate, endDate, useFingerprint).ToList();
        }

        public List<sp_Attendance_GetGroupingByDocType_result> GetGroupingByDocType(string unitId, DateTime startDate, DateTime endDate, bool useFinger)
        {
            return _context.sp_Attendance_GetGroupingByDocType(unitId, startDate, endDate, useFinger).ToList();
        }

        public decimal GetHarvestHkByEmployeeAndDate(string employeeId, DateTime date)
        {
            var sql = _context.TATTENDANCE.Join(_context.THARVEST, a => a.REF, b => b.HARVESTCODE, (a, b) =>
                        new { HK = a.HK, HARVESTCODE = b.HARVESTCODE, STATUS = a.STATUS, AUTO = a.AUTO, DATE = a.DATE, EMPID = a.EMPLOYEEID })
                        .Where(a => a.DATE == date && a.STATUS == PMSConstants.TransactionStatusApproved && a.AUTO == true && a.EMPID == employeeId && a.HARVESTCODE != null)
                        .GroupBy(o => o.HK)
                        .Select(b => new { TOTHK = b.Sum(i => i.HK) }).SingleOrDefault();
            return sql.TOTHK;
        }

        public override IEnumerable<TATTENDANCE> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<TATTENDANCE>();
            DateTime dateNull = new DateTime();

            try
            {
                if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(d => !d.STATUS.Equals(PMSConstants.TransactionStatusDeleted));
                else
                    criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(d => d.EMPLOYEE.UNITCODE == filter.UnitID);
                if (filter.UnitIDs.Any())
                    criteria = criteria.And(d => filter.UnitIDs.Contains(d.EMPLOYEE.UNITCODE));

                if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                    criteria = criteria.And(d => d.DIVID == filter.DivisionID);
                if (filter.DivisionIDs.Any())
                    criteria = criteria.And(d => filter.DivisionIDs.Contains(d.EMPLOYEE.DIVID));

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(d => d.EMPLOYEEID == filter.Id);
                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.EMPLOYEE.EMPID));

                if (filter.StartDate.Date != dateNull || filter.EndDate.Date != dateNull)
                    criteria = criteria.And(d => d.DATE.Date >= filter.StartDate.Date && d.DATE.Date <= filter.EndDate.Date);

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    criteria = criteria.And(d => d.EMPLOYEEID.Contains(filter.Keyword) || d.ID.Contains(filter.Keyword) );

                if (criteria.Body.ToString().ToUpper() == "TRUE")
                    criteria = criteria.And(d => d.DATE == GetServerTime());

                if (filter.PageSize <= 0)
                    return _context.TATTENDANCE.Include(e => e.EMPLOYEE).Where(criteria).ToList();
                return _context.TATTENDANCE.Include(e => e.EMPLOYEE).Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }

        public string GetMandorOtherLocation(DateTime date, string employeeId, string divId)
        {
            var q = (from a in _context.THARVEST.Where(a => a.HARVESTDATE == date && a.DIVID != divId && a.STATUS == PMSConstants.TransactionStatusApproved &&
                    (a.MANDOR1ID == employeeId || a.MANDORID == employeeId || a.KRANIID == employeeId  )) 
                    select a.DIVID)
                    .Union( from b in _context.TUPKEEP.Where(c => c.DIVID != divId && c.STATUS == PMSConstants.TransactionStatusApproved && c.UPKEEPDATE == date && 
                            c.MANDORID == employeeId) select b.DIVID ).SingleOrDefault();
            //string div = null;
            //if(!string.IsNullOrEmpty(q.DIV.ToString()))
            //    div = q.DIV.ToString();
            return q;
        }

        public List<sp_Attendance_GetNaturaSunday_Result> GetNaturaSunday(string unitId, DateTime startDate, DateTime endDate, bool useFinger)
        {
            return _context.sp_Attendance_GetNaturaSunday(unitId, startDate, endDate, useFinger).ToList();
        }

        public string GetOtherLocation(DateTime date, string employeeId, string divId)
        {
            var q = (from a in _context.TATTENDANCE.Where(b => b.DATE == date && b.EMPLOYEEID == employeeId && b.STATUS == PMSConstants.TransactionStatusApproved && b.AUTO == true)
                    join c in _context.THARVEST.Join(_context.MORGANIZATION, i => i.DIVID, x => x.ID, 
                                (i, x) => new { HARVESTCODE = i.HARVESTCODE, DIVID = i.DIVID } ).Where(y => y.DIVID != divId) on a.REF equals c.HARVESTCODE
                    select c.DIVID)
                    .Union(
                    from a in _context.TATTENDANCE.Where(b => b.DATE == date && b.EMPLOYEEID == employeeId && b.STATUS == PMSConstants.TransactionStatusApproved && b.AUTO == true)
                    join c in _context.TUPKEEP.Join(_context.MORGANIZATION, i => i.DIVID, x => x.ID,
                                (i, x) => new { UPKEEPCODE = i.UPKEEPCODE, DIVID = i.DIVID }).Where(y => y.DIVID != divId) on a.REF equals c.UPKEEPCODE
                    select c.DIVID
                    );
            return q.SingleOrDefault();
        }

        public class VWORKINGLOCATION
        {
            public string EMPLOYEEID { get; set; }
            public string DIVID { get; set; }
        }
        public List<VWORKINGLOCATION> GetOtherLocations(DateTime date,  List<string> employeeIds, string excludedDivId)
        {
            return
            (
                from a in _context.TATTENDANCE.Where(b => b.DATE == date && employeeIds.Contains(b.EMPLOYEEID) && b.STATUS == PMSConstants.TransactionStatusApproved && b.AUTO == true)
                join c in _context.THARVEST.Join(_context.MORGANIZATION, i => i.DIVID, x => x.ID,
                            (i, x) => new { HARVESTCODE = i.HARVESTCODE, DIVID = i.DIVID }).Where(y => y.DIVID != excludedDivId) on a.REF equals c.HARVESTCODE
                select new { a.EMPLOYEEID, c.DIVID }
            ).Union
            (
                from a in _context.TATTENDANCE.Where(b => b.DATE == date && employeeIds.Contains(b.EMPLOYEEID) && b.STATUS == PMSConstants.TransactionStatusApproved && b.AUTO == true)
                join c in _context.TUPKEEP.Join(_context.MORGANIZATION, i => i.DIVID, x => x.ID,
                            (i, x) => new { UPKEEPCODE = i.UPKEEPCODE, DIVID = i.DIVID }).Where(y => y.DIVID != excludedDivId) on a.REF equals c.UPKEEPCODE
                select new { a.EMPLOYEEID, c.DIVID }
            )
            .GroupBy(d => d.EMPLOYEEID)
            .Select(d => new VWORKINGLOCATION { EMPLOYEEID = d.Key, DIVID = d.Max(s => s.DIVID) })
            .ToList();
        }

        public List<TATTENDANCE> GetPresent(string unitCode, DateTime startDate, DateTime endDate)
        {
            return _context.TATTENDANCE.Include(a => a.DIV).Where(b => b.DATE >= startDate && b.DATE <= endDate && b.UNITCODE == unitCode).ToList();
        }

        public override TATTENDANCE NewRecord(string userName)
        {
            var record = new TATTENDANCE();
            record.DATE = GetServerTime();
            return record;
        }



        public string SaveInsertFromAdjustmentHK(IEnumerable<TATTENDANCE> records)
        {
            string result = string.Empty;
            
            if (!StandardUtility.IsEmptyList(records))
            {
                _internalCommit = false;
                foreach(var record in records)
                {
                    
                    SaveInsert(record,string.IsNullOrWhiteSpace(record.UPDATEBY)?record.CREATEBY:record.UPDATEBY);
                    result += record.ID + ",";
                }
                _internalCommit = true;
            }
            if (!string.IsNullOrWhiteSpace(result))
                result = result.Substring(0, result.Length - 1);

            return result;
        }
    }
}
