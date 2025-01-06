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
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Attendances
{
    public class TransaksiAbsensi : EntityFactory<TATTENDANCETRX, TATTENDANCETRX,GeneralFilter, PMSContextBase>
    {
        private Period _serviceperiod;
        private Employee _serviceEmp;
        private AuthenticationServiceBase _authenticationService;
        public TransaksiAbsensi(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "TransaksiAbsensi";
            _authenticationService = authenticationService;
            _serviceperiod = new Period(_context,_authenticationService,auditContext);
            _serviceEmp = new Employee(_context,_authenticationService,auditContext);
        }

        private string FieldsValidation(TATTENDANCETRX absensi)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(absensi.EMPID)) result += "Pin tidak boleh kosong." + Environment.NewLine;
            if (absensi.DATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(absensi.ATTSTATUS)) result += "Status absensi tidak boleh kosong." + Environment.NewLine;
            if (absensi.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;
            return result;
        }

        protected override TATTENDANCETRX GetSingleFromDB(params  object[] keyValues)
        {
            string unitId = null;
            string empId = null;
            DateTime date = new DateTime();

            if (keyValues.Count() == 1)
            {
                string Id = keyValues[0].ToString();
                string[] key = Id.Split('_');
                unitId = key[0];
                empId = key[1];
                date = DateTime.ParseExact(key[2], "yyyyMMdd", CultureInfo.InvariantCulture);
            } 
            else
            {
                unitId = keyValues[0].ToString();
                empId = keyValues[1].ToString();
                date = DateTime.Parse(keyValues[2].ToString());
            }
            
            return _context.TATTENDANCETRX.Include(b => b.EMPLOYEE).Where(a => a.UNITID.Equals(unitId) && a.EMPID.Equals(empId) && a.DATE.Date.Equals(date.Date)).SingleOrDefault(); //base.GetSingle(keyValues);
        }

        

        protected override TATTENDANCETRX BeforeSave(TATTENDANCETRX record, string userName, bool newRecord)
        {
            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.STATUS = PMSConstants.TransactionStatusApproved;
            }
            record.UPDATEDBY = userName;
            record.UPDATED = currentDate;
            this.FieldsValidation(record);

            var emp = _serviceEmp.GetSingle(record.EMPID);
            _serviceperiod.CheckValidPeriod(emp.UNITCODE, record.DATE);

            return record;
        }

        protected override TATTENDANCETRX BeforeDelete(TATTENDANCETRX record, string userName)
        {
            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            record.STATUS = PMSConstants.TransactionStatusDeleted;
            record.UPDATEDBY = userName;
            record.UPDATED = currentDate;
            record.EMPLOYEE = null;

            var emp = _serviceEmp.GetSingle(record.EMPID);
            _serviceperiod.CheckValidPeriod(emp.UNITCODE, record.DATE);

            return record;
        }

        protected override bool DeleteFromDB(TATTENDANCETRX record, string userName)
        {
            _context.TATTENDANCETRX.Update(record);
            _context.SaveChanges();

            return true;
        }

        public void Calculate1(string unitId, DateTime date, string by)
        {
            string inCutOffValue = HelperService.GetConfigValue(PMSConstants.CFG_AtendanceInCutOff + unitId, _context);
            var inCutOff = new DateTime();
            if (!string.IsNullOrEmpty(inCutOffValue)) inCutOff = date.AddHours(StandardUtility.ToDouble(inCutOffValue));

            string inLimitValue = HelperService.GetConfigValue(PMSConstants.CFG_AtendanceInLimit + unitId, _context);
            var inLimit = new DateTime();
            if (!string.IsNullOrEmpty(inLimitValue)) inLimit = date.AddHours(StandardUtility.ToDouble(inLimitValue));

            string outLimitValue = HelperService.GetConfigValue(PMSConstants.CFG_AtendanceOutLimit + unitId, _context);
            string outLimitFridayValue = HelperService.GetConfigValue(PMSConstants.CFG_AtendanceOutLimitFriday + unitId, _context);

            bool useCardValidate = HelperService.GetConfigValue(PMSConstants.CFG_AtendanceUseCardValidate + unitId, _context) == PMSConstants.CFG_AtendanceUseCardValidateTrue;

            var outLimit = new DateTime();
            if (date.DayOfWeek == DayOfWeek.Friday)
            {
                if (!string.IsNullOrEmpty(outLimitFridayValue)) outLimit = date.AddHours(StandardUtility.ToDouble(outLimitFridayValue));
            }
            else
            {
                if (!string.IsNullOrEmpty(outLimitValue)) outLimit = date.AddHours(StandardUtility.ToDouble(outLimitValue));
            }

            string quotaValue = HelperService.GetConfigValue(PMSConstants.CFG_AtendanceCardQuota + unitId, _context);
            decimal quota = 0;
            if (!string.IsNullOrEmpty(quotaValue)) quota = StandardUtility.ToDecimal(quotaValue);

            var list = new List<TATTENDANCETRX>();
            var cards = new List<string[]>();
            var dataLog = _context.TATTENDANCELOG
                            .Join(_context.MEMPLOYEE, a => a.EMPID, e => e.EMPID, (a, e) =>
                            new { PIN = a.PIN, EMPID = a.EMPID, DATETIME = a.DATETIME, DEVICE = a.DEVICE, UNITCODE = e.UNITCODE })
                            .Where(a => a.DATETIME >= date && a.DATETIME <= date).ToList();
            //repository.GetDataLogger(unitId, "*", date, date, string.Empty);
            //from a in _context.TATTENDANCELOG
            //          join b in _context.MEMPLOYEE on a.EMPID equals b.EMPID into c
            //          from b in c.DefaultIfEmpty()
            //          where a.DATETIME >= date && a.DATETIME <= date
            //          select ( new { PIN = a.PIN, EMPID = a.EMPID, DATETIME = a.DATETIME, DEVICE = a.DEVICE, UNITCODE = b.UNITCODE })
            //          ;

            int rowCounter = 0;
            int deviceCounter = 0;
            string currentDevice = string.Empty;
            foreach (var row in dataLog)
            {
                var pin = row.PIN.ToString();
                var employeeId = row.EMPID.ToString();
                var employeeUnit = row.UNITCODE.ToString();
                var deviceName = row.DEVICE.ToString();
                var currDate = StandardUtility.ToDateTime(row.DATETIME);
                var checkIn = StandardUtility.ToDateTime(row.DATETIME);
                var check = currDate.Add(new TimeSpan(0, checkIn.Hour, checkIn.Minute, checkIn.Second));

                if (currentDevice != deviceName)
                {
                    currentDevice = deviceName;
                    deviceCounter = 0;
                }

                var flag = "OUT";
                if (check <= inCutOff) flag = "IN";

                if (pin.Length != 6 && pin.Length != 7)
                {
                    var checkStatus = string.Empty;
                    var cardFlag = string.Empty;
                    if (useCardValidate)
                    {
                        if (deviceCounter > 0)
                        {
                            var cardPin = dataLog[rowCounter - 1].PIN.ToString();
                            if (cardPin.Length == 6 || cardPin.Length == 7)
                            {
                                if (cardPin.Length == 6) cardFlag = PMSConstants.AttendanceCardTypeUpkeep;
                                else if (cardPin.Length == 7) cardFlag = PMSConstants.AttendanceCardTypeHarvesting;

                                var r = from i in cards where i[1] == flag select i;
                                if (r.Count() < quota)
                                {
                                    var q = from i in cards where i[0] == cardPin && i[1] == flag select i;
                                    if (q.Count() == 0)
                                    {
                                        var value = new string[2];
                                        value[0] = cardPin;
                                        value[1] = flag;
                                        cards.Add(value);

                                    }
                                    else
                                        checkStatus = "CU";
                                }
                                else
                                    checkStatus = "NQ";
                            }
                            else
                                checkStatus = "NC";
                        }
                        else
                            checkStatus = "NC";
                    }

                    if (!string.IsNullOrEmpty(employeeId) && employeeUnit == unitId)
                    {
                        var attend = new TATTENDANCETRX();
                        var q = from i in list
                                where i.EMPID == employeeId && i.DATE == currDate
                                select i;

                        if (q.Count() == 0)
                        {
                            attend.UNITID = unitId;
                            attend.PIN = pin;
                            attend.EMPID = employeeId;
                            attend.DATE = currDate;
                            attend.AUTO = true;
                            list.Add(attend);
                        }
                        else
                            attend = q.SingleOrDefault();

                        if (attend != null)
                        {
                            if (flag == "IN")
                            {
                                if (attend.CHECKIN == null || !string.IsNullOrEmpty(attend.INSTATUS))
                                {
                                    attend.INSTATUS = checkStatus;
                                    attend.CHECKIN = check;
                                    attend.CARDIN = cardFlag;
                                }
                            }
                            else
                            {
                                if (attend.CHECKOUT == null || !string.IsNullOrEmpty(attend.OUTSTATUS))
                                {
                                    attend.OUTSTATUS = checkStatus;
                                    attend.CHECKOUT = check;
                                    attend.CARDOUT = cardFlag;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(checkStatus))
                                    {
                                        attend.OUTSTATUS = checkStatus;
                                        attend.CHECKOUT = check;
                                        attend.CARDOUT = cardFlag;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(attend.INSTATUS) || !string.IsNullOrEmpty(attend.OUTSTATUS))
                                attend.ATTSTATUS = "M";
                            else if (attend.CHECKIN == null || attend.CHECKOUT == null)
                                attend.ATTSTATUS = "M";
                            else if (attend.CHECKIN > inLimit)
                                attend.ATTSTATUS = "L";
                            else if (attend.CHECKOUT < outLimit)
                                attend.ATTSTATUS = "E";
                            else if (attend.CHECKIN <= inLimit && attend.CHECKOUT >= outLimit)
                                attend.ATTSTATUS = "K";
                            else
                                attend.ATTSTATUS = "M";
                        }
                    }
                }

                rowCounter++;
                deviceCounter++;
            }

            this.InsertAll(list, unitId, date, by);
        }

        public void Calculate(string unitId, DateTime date, string by)
        {
            string checkResult = string.Empty;
            var checkData = _context.TATTENDANCETRX.Join(_context.TATTENDANCE, a => a.EMPID, b => b.EMPLOYEEID,
                            (a, b) => new
                            {
                                EMPID = a.EMPID,
                                DATE = a.DATE,
                                DOC = b.REF,
                                UNITID = a.UNITID,
                                AUTO = b.AUTO,
                                TSTATUS = a.STATUS,
                                STATUS = b.STATUS,
                                ABSENTCODE = b.ABSENTCODE
                            })
                            .Where(a => a.DATE.Equals(date) && a.UNITID.Equals(unitId) && a.AUTO.Equals("1") && a.STATUS.Equals(PMSConstants.TransactionStatusApproved)
                            && a.TSTATUS.Equals(PMSConstants.TransactionStatusApproved) && a.DOC != "" && a.ABSENTCODE.Equals(null) )
                            .ToList();                          
                            //repository.CheckUsed(unitId, date);
            checkResult = checkData.Aggregate(checkResult, (current, row) => current + (row.EMPID + " - " + row.DOC + Environment.NewLine));
            //checkData.Rows.Cast<DataRow>().Aggregate(checkResult, (current, row) => current + (row["EMPID"] + " - " + row["DOC"] + Environment.NewLine));
            if (!string.IsNullOrEmpty(checkResult))
                throw new Exception("Data karyawan berikut sudah digunakan :" + Environment.NewLine + checkResult);

            var dataLog = _context.TATTENDANCELOG
                            .Join(_context.MEMPLOYEE, a => a.EMPID, e => e.EMPID, (a, e) =>
                            new { PIN = a.PIN, EMPID = a.EMPID, DATETIME = a.DATETIME, DEVICE = a.DEVICE, UNITCODE = e.UNITCODE, CHECKIN = a.DATETIME })
                            .Where(a => a.DATETIME >= date && a.DATETIME <= date.AddDays(2)).ToList();
            //repository.GetDataLogger(unitId, "*", date, date.AddDays(2), string.Empty);
            var list = _context.sp_TransaksiAbsensi_GetEmployee(unitId, date);
                //repository.GetFromEmployee(unitId, date);
            var cards = new List<string[]>();

            int rowCounter = 0;
            int deviceCounter = 0;
            string currentDevice = string.Empty;
            foreach (var row in dataLog)
            {
                var pin = row.PIN.ToString();
                //var employeeId = row["EMPID"].ToString();
                var deviceName = row.DEVICE.ToString();
                var currDate = StandardUtility.ToDateTime(row.DATETIME);
                var checkIn = StandardUtility.ToDateTime(row.CHECKIN);
                var check = currDate.Add(new TimeSpan(0, checkIn.Hour, checkIn.Minute, checkIn.Second));

                if (currentDevice != deviceName)
                {
                    currentDevice = deviceName;
                    deviceCounter = 0;
                }

                if (pin.Length != 6 && pin.Length != 7)
                {
                    var att = from i in list
                              where i.PIN.ToString().Trim() == pin.Trim()
                              && i.INSTART <= check
                              && i.OUTEND >= check
                              select i;
                    if (att.Count() > 0)
                    {
                        sp_TransaksiAbsensi_GetEmployee_Result attTrx;
                        if (att.Count() > 1)
                            throw new Exception("Pin/jadwal " + pin + " double.");

                        attTrx = att.SingleOrDefault();
                        //attTrx = att.SingleOrDefault();
                        attTrx.PIN = Convert.ToInt32(pin);

                        var flag = string.Empty;
                        if (checkIn >= attTrx.INSTART && checkIn <= attTrx.INEND) flag = "IN";
                        else if (checkIn >= attTrx.OUTSTART && checkIn <= attTrx.OUTEND) flag = "OUT";

                        var checkStatus = string.Empty;
                        var cardFlag = string.Empty;
                        if (attTrx.CARDVALID == true)
                        {
                            if (deviceCounter > 0)
                            {
                                var cardPin = dataLog[rowCounter - 1].PIN.ToString();
                                if (cardPin.Length == 6 || cardPin.Length == 7)
                                {
                                    if (cardPin.Length == 6) cardFlag = PMSConstants.AttendanceCardTypeUpkeep;
                                    else if (cardPin.Length == 7) cardFlag = PMSConstants.AttendanceCardTypeHarvesting;

                                    var r = from i in cards where i[1] == flag select i;
                                    if (r.Count() < attTrx.CARDQUOTA)
                                    {
                                        var q = from i in cards where i[0] == cardPin && i[1] == flag select i;
                                        if (q.Count() == 0)
                                        {
                                            var value = new string[2];
                                            value[0] = cardPin;
                                            value[1] = flag;
                                            cards.Add(value);

                                        }
                                        else
                                            checkStatus = "CU";
                                    }
                                    else
                                        checkStatus = "NQ";
                                }
                                else
                                    checkStatus = "NC";
                            }
                            else
                                checkStatus = "NC";
                        }

                        if (flag == "IN")
                        {
                            if (attTrx.CHECKIN == null || !string.IsNullOrEmpty(attTrx.INSTATUS))
                            {
                                attTrx.INSTATUS = checkStatus;
                                attTrx.CHECKIN = check;
                                attTrx.CARDIN = cardFlag;
                            }
                        }
                        else if (flag == "OUT")
                        {
                            if (attTrx.CHECKOUT == null || !string.IsNullOrEmpty(attTrx.OUTSTATUS))
                            {
                                attTrx.OUTSTATUS = checkStatus;
                                attTrx.CHECKOUT = check;
                                attTrx.CARDOUT = cardFlag;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(checkStatus))
                                {
                                    attTrx.OUTSTATUS = checkStatus;
                                    attTrx.CHECKOUT = check;
                                    attTrx.CARDOUT = cardFlag;
                                }
                            }
                        }
                    }
                }

                rowCounter++;
                deviceCounter++;
            }

            var listattTrx = new List<TATTENDANCETRX>();
            foreach (var attTrx in list)
            {
                if (attTrx.HOLIDAY > 0)
                {
                    attTrx.ATTSTATUS = "H";
                    //if (attTrx.EmployeeType.StartsWith("BHL"))
                    if (attTrx.CHECKIN != null || attTrx.CHECKOUT != null)
                        attTrx.ATTSTATUS = "K";
                }
                else if (!string.IsNullOrEmpty(attTrx.INSTATUS) || !string.IsNullOrEmpty(attTrx.OUTSTATUS))
                    attTrx.ATTSTATUS = "M";
                else if (attTrx.CHECKIN == null || attTrx.CHECKOUT == null)
                    attTrx.ATTSTATUS = "M";
                else if (attTrx.CHECKIN > attTrx.INTIME)
                    attTrx.ATTSTATUS = "L";
                else if (attTrx.CHECKOUT < attTrx.OUTTIME)
                    attTrx.ATTSTATUS = "E";
                else
                    attTrx.ATTSTATUS = "K";

                if (attTrx.CHECKIN != null && attTrx.CHECKOUT != null)
                {
                    TimeSpan workHour = ((DateTime)attTrx.CHECKOUT - (DateTime)attTrx.CHECKIN);
                    TimeSpan breakHour = (attTrx.BREAKEND - attTrx.BREAKSTART).Value;
                    attTrx.WORKACTUAL = new DateTime(1900, 1, 1) + workHour - breakHour;
                }

                //---------------------------------------------------------------------------------------------------
                //Hitung HK, Hour Limit
                //---------------------------------------------------------------------------------------------------

                var workHourLimit = attTrx.WORKLIMIT;
                if (attTrx.HOLIDAY > 0 && attTrx.EMPTYPE.StartsWith("SKU"))
                    attTrx.WORKLIMIT = 0;

                if (attTrx.WORKLIMIT > 0 && attTrx.WORKACTUAL != new DateTime(1900, 1, 1, 0, 0, 0))
                    attTrx.HKPAID = 1;

                //---------------------------------------------------------------------------------------------------
                //Hitung Overtime
                //---------------------------------------------------------------------------------------------------

                var workLimit = attTrx.WORKLIMIT * 60;//Jadwal Kerja (Menit)
                var workActual = (attTrx.WORKACTUAL - new DateTime(1900, 1, 1, 0, 0, 0)).Value.TotalMinutes;//Kerja Actual (Menit)
                if (workActual > workLimit)
                    attTrx.OVERTIME = (int)((StandardUtility.ToDecimal(workActual) - StandardUtility.ToDecimal(workLimit))
                        / StandardUtility.ToDecimal(60));//Lembur (Jam)

                if (attTrx.OVERTIME > attTrx.SPL) attTrx.OVERTIME = Convert.ToInt32(attTrx.SPL);

                if (attTrx.OVERTIME > 8) attTrx.OVERTIME = attTrx.OVERTIME - 1;
                else if (attTrx.OVERTIME > 4) attTrx.OVERTIME = Convert.ToInt32(attTrx.OVERTIME - StandardUtility.ToDecimal(0.5));

                if (attTrx.OVERTIME > 0)
                {
                    if (attTrx.HOLIDAY > 0)
                    {
                        if (attTrx.EMPTYPE.StartsWith("SKU"))
                        {
                            decimal otTime = attTrx.OVERTIME;

                            if (otTime > 0)
                            {
                                if (otTime > workHourLimit)
                                {
                                    attTrx.OT200 = Convert.ToInt32(workHourLimit);
                                    otTime -= Convert.ToInt32(workHourLimit);
                                }
                                else
                                {
                                    attTrx.OT200 = Convert.ToInt32(otTime);
                                    otTime = 0;
                                }
                            }

                            if (otTime > 0)
                            {
                                if (otTime > 1)
                                {
                                    attTrx.OT300 = 1;
                                    otTime -= 1;
                                }
                                else
                                {
                                    attTrx.OT300 = Convert.ToInt32(otTime);
                                    otTime = 0;
                                }
                            }

                            if (otTime > 0)
                            {
                                attTrx.OT400 = Convert.ToInt32(otTime);
                            }
                        }

                        if (attTrx.EMPTYPE.StartsWith("BHL"))
                        {
                            decimal otTime = attTrx.OVERTIME;

                            if (otTime > 0)
                            {
                                if (otTime > 1)
                                {
                                    attTrx.OT200 = 1;
                                    otTime -= 1;
                                }
                                else
                                {
                                    attTrx.OT200 = Convert.ToInt32(otTime);
                                    otTime = 0;
                                }
                            }

                            if (otTime > 0)
                            {
                                if (otTime > 1)
                                {
                                    attTrx.OT300 = 1;
                                    otTime -= 1;
                                }
                                else
                                {
                                    attTrx.OT300 = Convert.ToInt32(otTime);
                                    otTime = 0;
                                }
                            }

                            if (otTime > 0)
                            {
                                attTrx.OT400 = Convert.ToInt32(otTime);
                            }
                        }
                    }
                    else
                    {
                        if (attTrx.OVERTIME > 1)
                        {
                            attTrx.OT150 = 1;
                            attTrx.OT200 = attTrx.OVERTIME - 1;
                        }
                        else
                        {
                            attTrx.OT150 = attTrx.OVERTIME;
                        }
                    }
                }

                var newattTrx = new TATTENDANCETRX();
                newattTrx.CopyFrom(attTrx);
                listattTrx.Add(newattTrx);
            }

            this.InsertAll(listattTrx, unitId, date, by);
        }

        public List<TATTENDANCELOG> GetDataLogger(string unitId, string divisionId, DateTime from, DateTime to, string device)
        {
            if (string.IsNullOrEmpty(unitId)) unitId = "*";
            if (string.IsNullOrEmpty(divisionId)) divisionId = "*";
            return _context.TATTENDANCELOG.Include(a => a.EMPLOYEE).Where(b => b.DATETIME >= from && b.DATETIME <= to
                    && b.DEVICE.Contains(device)).ToList();
        }

        public List<MATTMACHINE> GetMachine()
        {
            return _context.MATTMACHINE.ToList();
        }

        public override IEnumerable<TATTENDANCETRX> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<TATTENDANCETRX>();
            DateTime dateNull = new DateTime();

            try
            {
                criteria = criteria.And(d => d.STATUS != PMSConstants.TransactionStatusDeleted);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(d => d.EMPID == filter.Id);
                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.EMPLOYEE.EMPID));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(d => d.UNITID == filter.UnitID);
                if (filter.UnitIDs.Any())
                    criteria = criteria.And(d => filter.UnitIDs.Contains(d.EMPLOYEE.UNITCODE));

                //if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                //    criteria = criteria.And(d => d.EMPLOYEE.DIVID == filter.DivisionID);
                //if (filter.DivisionIDs.Any())
                //    criteria = criteria.And(d => filter.DivisionIDs.Contains(d.EMPLOYEE.DIVID));

                if (filter.StartDate.Date != dateNull || filter.EndDate.Date != dateNull)
                    criteria = criteria.And(d => d.DATE.Date >= filter.StartDate.Date && d.DATE.Date <= filter.EndDate.Date);

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    criteria = criteria.And(d => d.EMPID.Contains(filter.Keyword) || d.ID.Contains(filter.Keyword) || d.UNITID.Contains(filter.Keyword));

                if (criteria.Body.ToString().ToUpper() == "TRUE")
                    criteria = criteria.And(d => d.DATE == GetServerTime());

                if (filter.PageSize <= 0)
                    return _context.TATTENDANCETRX.Include(e => e.EMPLOYEE).Where(criteria).ToList();
                return _context.TATTENDANCETRX.Include(e => e.EMPLOYEE).Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }

        public void insertLog(TATTENDANCELOG record)
        {
            try
            {
                var emp = _context.MEMPLOYEE.Where(a => a.EMPID.Equals(record.EMPID) && a.PINID.Equals(record.PIN));
                if (emp.Count() == 0) record.EMPID = "X";

                _context.TATTENDANCELOG.Add(record);
                _context.SaveChanges();
            }
            catch
            {
                throw new Exception("Gagal insert Log.");
            }
        }

        public override TATTENDANCETRX NewRecord(string userName)
        {
            var record = new TATTENDANCETRX();
            record.DATE = GetServerTime();
            return record;
        }

        private void InsertAll(List<TATTENDANCETRX> absensis, string unitId, DateTime date, string by)
        {
            _serviceperiod.CheckValidPeriod(unitId, date);

            foreach (var absensi in absensis)
                this.SaveInsert(absensi, by);
        }

    }
}
