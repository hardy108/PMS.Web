using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Attendances;
using PMS.EFCore.Services.Logistic;
using PMS.EFCore.Services.Organization;
using PMS.EFCore.Services.Inventory;
using PMS.EFCore.Services.GL;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using PMS.EFCore.Services.Upkeep;

using PMS.Shared.Models;

using AM.EFCore.Services;

namespace PMS.EFCore.Services.Logistic
{
    public class Upkeep : EntityFactory<TUPKEEP,TUPKEEP, FilterUpkeep, PMSContextBase>
    {
        private Period _servicePeriodName;
        private Holiday _serviceHolidayName;
        private Activity _serviceActName;
        private Attendance _serviceAttName;
        private Divisi _serviceDivisiName;
        private PositionDetail _servicePosDetName;
        private RKH _serviceRKHName;
        private Contract _serviceContractName;
        private Employee _serviceEmpName;
        private ActivityAccount _serviceActAccName;
        private StoreLocation _serviceStorLocName;
        private JournalType _serviceJournaltypeName;
        private Block _serviceBlockName;
        private Stock _serviceStockName;
        private GoodIssue _serviceGIName;
        private Unit _serviceUnitName;
        private Material _serviceMatName;
        private Organizations _serviceOrgName;
        private List<TUPKEEPBLOCK> _upkeepBlock;
        private List<TUPKEEPEMPLOYEE> _upkeepEmployee;
        private List<TUPKEEPMATERIAL> _upkeepMaterial;
        private List<TUPKEEPCALC> _upkeepCalc;
        private TUPKEEPVENDOR _upkeepVendor;
        private AuthenticationServiceBase _authenticationService;
        public Upkeep(PMSContextBase context,AuthenticationServiceBase authenticationService,AuditContext auditContext) : base(context,auditContext)
        {
            _serviceName = "Upkeep";
            _authenticationService = authenticationService;
            _servicePeriodName = new Period(context,_authenticationService,auditContext);
            _serviceHolidayName = new Holiday(context,_authenticationService,auditContext);
            _serviceActName = new Activity(context, _authenticationService,auditContext);
            _serviceAttName = new Attendance(context, _authenticationService,auditContext);
            _serviceDivisiName = new Divisi(context, _authenticationService,auditContext);
            _servicePosDetName = new PositionDetail(context,_authenticationService,auditContext);
            _serviceRKHName = new RKH(context,_authenticationService,auditContext);
            _serviceContractName = new Contract(_context, _authenticationService,auditContext);
            _serviceEmpName = new Employee(context, _authenticationService,auditContext);
            _serviceActAccName = new ActivityAccount(context, _authenticationService,auditContext);
            _serviceStorLocName = new StoreLocation(context,_authenticationService,auditContext);
            _serviceJournaltypeName = new JournalType(context, _authenticationService,auditContext);
            _serviceBlockName = new Block(context,_authenticationService,auditContext);
            _serviceStockName = new Stock(context,_authenticationService,auditContext);
            _serviceGIName = new GoodIssue(context, _authenticationService,auditContext);
            _serviceUnitName = new Unit(context,_authenticationService,auditContext);
            _serviceOrgName = new Organizations(context, _authenticationService,auditContext);
            _serviceMatName = new Material(context, _authenticationService,auditContext);

            _upkeepBlock = new List<TUPKEEPBLOCK>();
            _upkeepCalc = new List<TUPKEEPCALC>();
            _upkeepEmployee = new List<TUPKEEPEMPLOYEE>();
            _upkeepMaterial = new List<TUPKEEPMATERIAL>();
            _upkeepVendor = new TUPKEEPVENDOR();
        }

        public override TUPKEEP NewRecord(string userName)
        {
            var record = new TUPKEEP();
            record.UPKEEPDATE = GetServerTime();
            record.TUPKEEPMATERIALSUM = new List<TUPKEEPMATERIALSUM>();
            record.STATUS = "P";
            return record;
        }

        private void ApproveValidate(TUPKEEP consumption,string userName)
        {
            if (consumption.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (consumption.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
            if (consumption.CONTRACTITEM != null && consumption.CONTRACTITEM.USED + consumption.TOTALOUTPUT > consumption.CONTRACTITEM.OUTPUT )
                throw new Exception("Hasil kerja melebihi jumlah yang ditentukan");

      
            var hol = _serviceHolidayName.GetList(new GeneralFilter { UnitID=consumption.DIV.UNITCODE, Date = consumption.UPKEEPDATE });

            this.Validate(consumption,userName);
            _servicePeriodName.CheckMaxPeriod(consumption.DIV.UNITCODE, consumption.UPKEEPDATE);

            bool allowAsistensi = HelperService.GetConfigValue(PMSConstants.CfgUpeepAllowAsistensi + consumption.DIV.UNITCODE, _context) == PMSConstants.CfgUpeepAllowAsistensiTrue;
            bool allowWork2Div = HelperService.GetConfigValue(PMSConstants.CfgAtendanceWork2Division + consumption.DIV.UNITCODE, _context) == PMSConstants.CfgAtendanceWork2DivisionTrue;

            var act = _serviceActName.GetSingle(consumption.ACTIVITYID);
            var chekcAttendance = HelperService.GetConfigValue(PMSConstants.CFG_UpkeepAttendanceCheck + consumption.DIV.UNITCODE, _context) == PMSConstants.CFG_UpkeepAttendanceCheckTrue;

            foreach (var employee in consumption.TUPKEEPEMPLOYEE)
            {
                if (chekcAttendance)
                {
                    var rfidType = string.Empty;
                    if (consumption.UPKEEPTYPE != 1) rfidType = act.RFID;

                    var isExist = _serviceAttName.CheckAttendance(employee.EMPLOYEE.UNITCODE, employee.EMPLOYEEID, consumption.UPKEEPDATE, "K", rfidType);
                    if (isExist == 0)
                        throw new Exception("Absensi karyawan " + employee.EMPLOYEEID + " tanggal " + consumption.UPKEEPDATE.ToString("dd/MM/yyyy") + " tidak valid.");
                }

                
                List<string> tkbmActivity = new List<string> { "HT0111", "HT0211", "HT0411", "HT0531", "HT0730", "HT0831", "HT0833", "HT0934", "HT1011", "HT1111", "HT1230" };
                var isTkbm = tkbmActivity.Contains(consumption.ACTIVITYID);

                if (consumption.UPKEEPTYPE == 1)
                    if (!(isTkbm && employee.EMPLOYEE.EMPTYPE.Contains("BHL")))
                    {
                        if (hol == null)
                        {
                            var currentAttendance = _serviceAttName.GetByEmployeeAndDate(employee.EMPLOYEEID, consumption.UPKEEPDATE);
                            currentAttendance.RemoveAll(a => a.STATUS != PMSConstants.TransactionStatusApproved);
                            var q = from itm in currentAttendance select itm.HK;
                            decimal currentHk = q.Sum();
                            if (currentHk == 0)
                                throw new Exception("Karyawan " + employee.EMPLOYEEID + " belum melakukan pekerjaan harian.");
                        }
                    }
            }

            if (consumption.UPKEEPTYPE != 1)
            {
                foreach (var employee in consumption.TUPKEEPEMPLOYEE)
                {
                    string result = string.Empty;
                    var list = _serviceAttName.GetByEmployeeAndDate(employee.EMPLOYEEID, consumption.UPKEEPDATE);
                    if (list.Count() > 0)
                    {
                        var c = from hk in list select hk.HK;
                        //if (c.Sum() + employee.Value > 1 //-*Parameter
                        //    && employee.Employee.TypeCode.Contains("SKU"))
                        //    result += "Absensi Karyawan SKU " + employee.EmployeeId + " Tidak boleh  lebih dari 1 HK per hari (Absensi Saat ini = " + c.Sum() + ")";
                        //if (c.Sum() + employee.Value > 2 //-*Parameter
                        //    && employee.Employee.TypeCode.Contains("BHL"))
                        //    result += "Absensi Karyawan BHL " + employee.EmployeeId + " Tidak boleh  lebih dari 2 HK per hari (Absensi Saat ini = " + c.Sum() + ")";
                        if (c.Sum() + employee.VALUE > 1)
                            result += "Absensi Karyawan " + employee.EMPLOYEEID + " Tidak boleh  lebih dari 1 HK per hari (Absensi Saat ini = " + c.Sum() + ")";
                    }

                    if (!string.IsNullOrEmpty(result))
                        throw new Exception(result);

                    if (!allowWork2Div)
                    {
                        var otherLoc = _serviceAttName.GetOtherLocation(consumption.UPKEEPDATE, employee.EMPLOYEEID, consumption.DIVID);
                        if (!string.IsNullOrEmpty(otherLoc) && otherLoc != consumption.DIVID)
                            throw new Exception("Karyawan " + employee.EMPLOYEEID + " sudah bekerja di divisi " + otherLoc);
                    }
                }

                if (!allowAsistensi)
                {
                    var mandorLoc = _serviceAttName.GetMandorOtherLocation(consumption.UPKEEPDATE, consumption.MANDORID, consumption.DIVID);
                    if (mandorLoc != null && mandorLoc != consumption.DIVID)
                        throw new Exception("Mandor " + consumption.MANDORID + " sudah bekerja di divisi " + mandorLoc);
                }
            }
        }

        private void CancelValidate(TUPKEEP consumption)
        {
            if (consumption.STATUS != PMSConstants.TransactionStatusApproved)
                throw new Exception("Data belum di approve.");
            if (consumption.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
            _servicePeriodName.CheckValidPeriod(consumption.DIV.UNITCODE, consumption.UPKEEPDATE);
        }

        private string FieldsValidation(TUPKEEP consumption)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(consumption.UPKEEPCODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(consumption.DIVID)) result += "Divisi tidak boleh kosong." + Environment.NewLine;
            if (consumption.UPKEEPDATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(consumption.MANDORID)) result += "Mandor tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(consumption.ACTIVITYID)) result += "Pekerjaan tidak boleh kosong." + Environment.NewLine;
            if (consumption.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;

            return result;
        }

        private void Validate(TUPKEEP consumption, string userName)
        {
            string result = this.FieldsValidation(consumption);
            if (!string.IsNullOrWhiteSpace(result))
                throw new Exception(result);
            if (StandardUtility.IsEmptyList(consumption.TUPKEEPBLOCK))            
                throw new Exception("Block / Batch tidak boleh kosong.");

            var hasEmp = !StandardUtility.IsEmptyList(consumption.TUPKEEPEMPLOYEE);
            if (string.IsNullOrWhiteSpace(consumption.CONTID) && !hasEmp)
                throw new Exception("Karyawan tidak boleh kosong.");

            var hasVend = (!string.IsNullOrWhiteSpace(consumption.CONTID) && consumption.TUPKEEPVENDOR.OUTPUT > 0);

            if (hasEmp && hasVend) throw new Exception("Hanya boleh isi salah satu dari keryawan atau vendor");
            if (!hasEmp && !hasVend) throw new Exception("Karyawan/Vendor harus diisi. ");

            var act = _serviceActName.GetSingle(consumption.ACTIVITYID);

            if (HelperService.GetConfigValue(PMSConstants.CFG_UpkeepAutoJournal + consumption.DIV.UNITCODE, _context) == PMSConstants.CFG_UpkeepAutoJournalTrue)
            {
                if (!StandardUtility.IsEmptyList(consumption.TUPKEEPMATERIAL))
                {
                    result = string.Empty;

                    var qtyUsed = consumption.TUPKEEPMATERIAL.GroupBy(d => d.MATERIALID).Select(d => new { MATERIALID = d.Key, TOTALQTY = d.Sum(s => s.QUANTITY) });

                    (from a in qtyUsed
                     join b in consumption.TUPKEEPMATERIAL on a.MATERIALID equals b.MATERIALID
                     where a.TOTALQTY > b.STOCK
                     select new { a.MATERIALID, a.TOTALQTY, b.STOCK }
                    ).ToList().ForEach(d => {
                        result += $"Jumlah item {d.MATERIALID} ({d.TOTALQTY}) melebihi stok({d.STOCK})\r\n";
                    });
                    if (!string.IsNullOrWhiteSpace(result))
                        throw new Exception(result);
                   
                }
            }

            _servicePeriodName.CheckValidPeriod(consumption.DIV.UNITCODE, consumption.UPKEEPDATE);

            

            if (!StandardUtility.IsEmptyList(consumption.TUPKEEPEMPLOYEE))
            {
                var empIds = consumption.TUPKEEPEMPLOYEE.Select(d => d.EMPLOYEEID).ToList();
                var employees = (from a in _context.MEMPLOYEE.Include(d=>d.POSITION).AsNoTracking().Where(d => empIds.Contains(d.EMPID))
                                 join b in _context.MPOSITIONDETAIL.AsNoTracking().Where(d => d.UNITID.Equals(consumption.DIV.UNITCODE)) on a.POSITIONID equals b.POSID into ab
                                 from abLeft in ab.DefaultIfEmpty()
                                 select new { EMPLOYEEID = a.EMPID, MEMPLOYEE = a, MPOSITIONDETAIL = abLeft }
                                ).ToList();
                var checkEmployee =
                    (
                        from a in consumption.TUPKEEPEMPLOYEE
                        join b in employees on a.EMPLOYEEID equals b.EMPLOYEEID
                        select new { TUPKEEPEMPLOYEE = a, MEMPLOYEE = b.MEMPLOYEE, MPOSITIONDETAIL  = b.MPOSITIONDETAIL}
                    ).ToList();

                result = string.Empty;
                if (consumption.UPKEEPTYPE == 1)
                {
                    checkEmployee.Where(d => d.MPOSITIONDETAIL == null || !d.MPOSITIONDETAIL.ALLOWBORONG).ToList().ForEach(d => {
                        result += $"Karyawan {d.MEMPLOYEE.EMPID}-{d.MEMPLOYEE.EMPNAME}, Jabatan {d.MEMPLOYEE.POSITION.POSITIONNAME} : Jabatan ini tidak berhak borongan.\r\n";
                    });
                }
                else
                {
                    checkEmployee.Where(d => d.MEMPLOYEE.EMPTYPE.ToUpper().Contains("SKU") && d.TUPKEEPEMPLOYEE.VALUE > 1).ToList().ForEach(d => {
                        result += $"Karyawan {d.MEMPLOYEE.EMPID}-{d.MEMPLOYEE.EMPNAME} : HK Karyawan harus <= 1 untuk SKUB dan SKUH bukan borongan.\r\n";
                    });
                }

                if (!string.IsNullOrWhiteSpace(result))
                    throw new Exception(result);
            }

            if (!string.IsNullOrWhiteSpace(consumption.CONTID))
            {
                var cont = _context.TCONTRACT.AsNoTracking().FirstOrDefault(d => d.ID.Equals(consumption.CONTID));
                if (consumption.UPKEEPDATE < cont.STARTDATE || consumption.UPKEEPDATE > cont.ENDDATE)
                    throw new Exception($"Dokumen untuk kontrak no {cont.ID} harus antara tanggal {cont.STARTDATE:'dd/MM/yyyy'}  dan {cont.ENDDATE:'dd/MM/yyyy'}");
                if (cont.CLOSE)
                    throw new Exception($"Kontrak no {cont.ID} sudah di close.");
            }
          

            if (consumption.UPKEEPTYPE == 0 && consumption.UPKEEPDATE >= new DateTime(2019, 5, 13))
            {
                var filrkh = new FilterRKH();
                filrkh.DivisionID = consumption.DIVID;
                filrkh.ActualDate = consumption.UPKEEPDATE;
                filrkh.ActivityID = consumption.ACTIVITYID;
                filrkh.UpkeepID = consumption.UPKEEPCODE;
                filrkh.PaymentType = consumption.UPKEEPTYPE;
                var dt = _serviceRKHName.GetActual(filrkh);
                Decimal plan = dt.PLAN;
                Decimal actual = dt.ACTUAL;

                if (consumption.TUPKEEPEMPLOYEE.Count + actual > plan)
                    throw new Exception("Karyawan melebihi RKH (" + plan.ToString("#0") + "). Sudah digunakan saat ini " + actual.ToString("#0"));
            }
        }

        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string upkeepCode = formDataCollection["UPKEEPCODE"];
            return Approve(upkeepCode, userName);
        }

        public bool Approve(string id, string userName)
        {
            var rfcResult = string.Empty;

            try
            {
                var consumption = _context.TUPKEEP
                    .Include(d=>d.DIV)
                    .Include(d=>d.ACTIVITY)
                    .Include(d=>d.TUPKEEPEMPLOYEE)
                    .Include(d=>d.TUPKEEPBLOCK)
                    .Include(d=>d.TUPKEEPMATERIAL)
                    .Include(d=>d.TUPKEEPCALC)
                    .Include(d=>d.TUPKEEPVENDOR)
                    .AsNoTracking()
                    .FirstOrDefault(d=>d.UPKEEPCODE.Equals(id));

                if (consumption == null)
                    throw new Exception("Data BKM tidak ditemukan");

                
                

                
                
                
                
                if (!string.IsNullOrWhiteSpace(consumption.CONTID))                
                    consumption.CONTRACTITEM = _context.TCONTRACTITEM.FirstOrDefault(a => a.CONTID == consumption.CONTID && a.ACTID == consumption.ACTIVITYID);
                    
                this.ApproveValidate(consumption, userName);

                consumption.UPDATEBY = userName;
                consumption.UPDATEDDATE = HelperService.GetServerDateTime(1, _context);
                consumption.STATUS = PMSConstants.TransactionStatusApproved;

                if (consumption.CONTRACTITEM != null)
                {
                    consumption.CONTRACTITEM.USED = consumption.CONTRACTITEM.USED + consumption.TOTALOUTPUT;
                    _context.TCONTRACTITEM.Update(consumption.CONTRACTITEM);
                    _context.SaveChanges();
                }

                if (consumption.UPKEEPTYPE == 0)
                {
                    foreach (var employee in consumption.TUPKEEPEMPLOYEE)
                    {
                        var attendance = new TATTENDANCE
                        {
                            DIVID = employee.EMPLOYEE.DIVID,
                            EMPLOYEEID = employee.EMPLOYEEID,
                            EMPLOYEE = employee.EMPLOYEE,
                            DIV = consumption.DIV,
                            DATE = consumption.UPKEEPDATE,
                            PRESENT = true,
                            REMARK = string.Empty,
                            HK = employee.VALUE,
                            //AbsentId = "K",//-*Constant atau Enum
                            STATUS = consumption.STATUS,
                            REF = consumption.UPKEEPCODE,
                            AUTO = true,
                            CREATEBY = consumption.UPDATEBY,
                            CREATEDDATE = consumption.UPDATEDDATE,
                            UPDATEBY = consumption.UPDATEBY,
                            UPDATEDDATE = consumption.UPDATEDDATE,
                        };
                        _serviceAttName.SaveInsert(attendance, userName);
                    }
                }

                if (HelperService.GetConfigValue(PMSConstants.CFG_UpkeepAutoJournal + consumption.DIV.UNITCODE, _context) == PMSConstants.CFG_UpkeepAutoJournalTrue)
                {
                    var account = _serviceActAccName.GetSingle(consumption.ACTIVITYID);

                    var q = consumption.TUPKEEPMATERIAL.Where(i => i.QUANTITY > 0);
                    if (q.Count() > 0)
                    {
                        var location = _context.MSTORELOCATION.Where(a => a.LOCID == consumption.DIVID).SingleOrDefault();
                        if (consumption.DIV == null)
                        {
                            var divisi = new MDIVISI();
                            divisi.CopyFrom(_serviceDivisiName.GetSingle(consumption.DIVID));
                            consumption.DIV = divisi;
                        }

                        var giType = _serviceJournaltypeName.GetByModul(PMSConstants.GL_Journal_GoodIssueModulCode);
                        var gi = new TGI
                        {
                            VOUCHERNO = giType[0].CODE + "9999",
                            DATE = consumption.UPKEEPDATE,
                            UNITCODE = consumption.DIV.UNITCODE,
                            LOCCODE = location.CODE,
                            NOTE = consumption.ACTIVITY.ACTIVITYNAME,
                            REF = consumption.UPKEEPCODE,
                            STATUS = PMSConstants.TransactionStatusProcess,
                            TGIITEM = new List<TGIITEM>(),
                        };

                        var consMat = _context.TUPKEEPMATERIAL.Where(a => a.UPKEEPCODE == consumption.UPKEEPCODE);
                        
                        foreach (var item in consMat.Where(i => i.QUANTITY > 0))
                        {
                            string matAccCode = string.Empty;
                            var block = _serviceBlockName.GetSingle(item.BLOCKID);
                            if (block.PHASE == 9) matAccCode = account.GAMATDEBT;
                            else if (block.PHASE == 8) matAccCode = account.LCMATDEBT;
                            else if (block.PHASE == 1) matAccCode = account.TBMMATDEBT;
                            else if (block.PHASE == 5) matAccCode = account.TMMATDEBT;
                            else if (block.PHASE == 0) matAccCode = account.NMATDEBT;

                            if (consumption.DIVID == "WS") matAccCode = account.RAMATDEBT;

                            var newItem = new TGIITEM
                            {
                                MATERIALID = item.MATERIALID,
                                ACCOUNTCODE = matAccCode,
                                QTY = item.QUANTITY,
                                NOTE = consumption.ACTIVITY.ACTIVITYNAME,
                                BLOCKID = item.BLOCKID,
                            };
                            gi.TGIITEM.Add(newItem);
                        }

                        foreach (var cm in consMat)
                        {
                            decimal[] stock = _serviceStockName.GetStock(location.CODE, cm.MATERIALID, consumption.UPKEEPDATE);
                            cm.STOCK = stock[0];
                        }

                        var giproc = _serviceGIName.SaveInsertOrUpdate(gi, consumption.UPDATEBY);
                        string giCode = giproc.NO;
                        _serviceGIName.Approve(giCode, false, consumption.UPDATEBY);

                        consumption.TUPKEEPMATERIAL.Clear();
                        consumption.TUPKEEPMATERIAL = consMat.ToList();
                    }
                }

                if (HelperService.GetConfigValue(PMSConstants.CfgUpeepAutoUpload + consumption.DIV.UNITCODE, _context)
                    == PMSConstants.CfgUpeepAutoUploadTrue)
                {
                    consumption.UPLOAD = 3;
                    consumption.UPLOADDATE = consumption.UPDATEDDATE;
                    var bkm = from a in _context.TUPKEEP //_context.sap_GetBkm(consumption.UPKEEPCODE);
                              join b in _context.MDIVISI on a.DIVID equals b.DIVID
                              join c in _context.MUNIT on b.UNITCODE equals c.UNITCODE
                              where a.UPKEEPCODE == consumption.UPKEEPCODE
                              select new
                              {
                                  UPKEEPCODE = a.UPKEEPCODE,
                                  isPlasma = (c.INTIID == null ? 0 : 1)
                              };
                    int isPlasma = StandardUtility.ToInt(bkm.SingleOrDefault().isPlasma);

                    //DataTable result;
                    //if (isPlasma == 0)
                    //    using (var services = PmsServicesFactory.GetPmsServices())
                    //        result = services.Upkeep_Approve(bkm, string.Empty);
                    //else
                    //    using (var services = PmsServicesFactory.GetPmsServices())
                    //        result = services.UpkeepPlasma_Approve(bkm, string.Empty);

                    //foreach (DataRow row in result.Rows)
                    //{
                    //    if (row["PSTG_STAT"].ToString() != "S")
                    //        rfcResult += row["PSTG_STAT"] + ": " + row["PSTG_MSG"] + " ";

                    //    if (isPlasma == 0)
                    //        HelperService.SetSapTran(consumption.UPKEEPCODE, row["DOC_NUMBER"].ToString(), _context);
                    //}

                    //if (!string.IsNullOrEmpty(rfcResult))
                    //    throw new Exception(rfcResult);
                }

                //this.SaveUpdate(consumption, userName);
                _context.TUPKEEP.Update(consumption);
                _context.SaveChanges();
                SaveAuditTrail(consumption, userName, "Approve Record");
                return true;
            }
            catch
            {
                //if (!string.IsNullOrEmpty(rfcResult))
                    //HelperService.SetSapResult(id, rfcResult, _context);

                throw;
                return false;
            }
        }

        public bool CancelApprove(IFormCollection formDataCollection, string userName)
        {
            string upkeepCode = formDataCollection["UPKEEPCODE"];
            string comment = formDataCollection["CANCELEDCOMMENT"];
            return Cancel(upkeepCode, comment, userName);
        }

        public bool Cancel(string id, string comment, string userName)
        {
            var rfcResult = string.Empty;

            try
            {
                var consumption = this.GetSingle(id);

                //if (consumption.DIV == null)
                //{
                //    var divisi = new MDIVISI();
                //    divisi.CopyFrom(_serviceDivisiName.GetSingle(consumption.DIVID));
                //    consumption.DIV = divisi;
                //}

                var act = _serviceActName.GetSingle(consumption.ACTIVITYID);

                //if (consumption.MANDOR == null)
                //{
                //    consumption.MANDOR = _serviceEmpName.GetSingle(consumption.MANDORID);
                //}

                TCONTRACTITEM currContract = null;
                decimal thisUsed = 0;
                if (!string.IsNullOrEmpty(consumption.CONTID))
                {
                    currContract = _context.TCONTRACTITEM.Where(a => a.CONTID == consumption.CONTID && a.ACTID == consumption.ACTIVITYID).SingleOrDefault();

                    var empQ = from i in consumption.TUPKEEPEMPLOYEE select i.OUTPUT;
                    decimal empOutput = empQ.Sum();
                    
                    decimal venOutput = consumption.TUPKEEPVENDOR.OUTPUT;

                    thisUsed = empOutput + venOutput;
                }

                this.CancelValidate(consumption);
                consumption.CANCELEDCOMMENT = comment;
                consumption.UPDATEDDATE = HelperService.GetServerDateTime(1, _context);
                consumption.STATUS = PMSConstants.TransactionStatusCanceled;
                consumption.UPLOAD = 0;
                consumption.UPLOADDATE = null;
                
                _context.TUPKEEP.Update(consumption);
                //_context.SaveChanges();

                var newConsumption = new TUPKEEP
                {
                    DIV = consumption.DIV,
                    DIVID = consumption.DIVID,
                    ACTIVITYID = consumption.ACTIVITYID,
                    //ActivityName = consumption.ActivityName,
                    //Uom = consumption.Uom,
                    //UomKonversi = consumption.UomKonversi,
                    UPKEEPTYPE = consumption.UPKEEPTYPE,
                    CONTID = consumption.CONTID,
                    CREATEBY = consumption.UPDATEBY,
                    CREATEDDATE = consumption.UPDATEDDATE,
                    UPDATEBY = consumption.UPDATEBY,
                    UPDATEDDATE = consumption.UPDATEDDATE,
                    UPKEEPDATE = consumption.UPKEEPDATE,
                    MANDORID = consumption.MANDORID,
                    REMARK = consumption.REMARK,
                    STATUS = PMSConstants.TransactionStatusProcess,
                    UPLOAD = 0,
                    UPLOADDATE = null,
                    UPKEEPCODE = this.GenereteNewDerivedNumber(consumption.UPKEEPCODE),
                    TUPKEEPBLOCK = new List<TUPKEEPBLOCK>(),
                    TUPKEEPMATERIAL = new List<TUPKEEPMATERIAL>(),
                    TUPKEEPEMPLOYEE = new List<TUPKEEPEMPLOYEE>(),
                    TUPKEEPVENDOR = new TUPKEEPVENDOR(),
                };

                foreach (var block in consumption.TUPKEEPBLOCK)
                {
                    var bl = new TUPKEEPBLOCK();
                    bl.CopyFrom(block);
                    bl.BLOCK = null;
                    bl.UPKEEP = null;
                    bl.UPKEEPCODE = newConsumption.UPKEEPCODE;
                    newConsumption.TUPKEEPBLOCK.Add(bl);
                }
                foreach (var mat in consumption.TUPKEEPMATERIAL)
                {
                    var mt = new TUPKEEPMATERIAL();
                    mt.CopyFrom(mat);
                    mt.MATERIAL = null;
                    mt.BLOCK = null;
                    mt.ACTIVITY = null;
                    mt.UPKEEP = null;
                    mt.UPKEEPCODE = newConsumption.UPKEEPCODE;
                    newConsumption.TUPKEEPMATERIAL.Add(mt);
                }
                foreach (var emp in consumption.TUPKEEPEMPLOYEE)
                {
                    var em = new TUPKEEPEMPLOYEE();
                    em.CopyFrom(emp);
                    em.EMPLOYEE = null;
                    em.UPKEEP = null;
                    newConsumption.TUPKEEPEMPLOYEE.Add(em);
                }
                if (consumption.TUPKEEPVENDOR != null)
                {
                    newConsumption.TUPKEEPVENDOR.UPKEEPCODE = newConsumption.UPKEEPCODE;
                }

                if (currContract != null)
                {
                    currContract.USED = currContract.USED - thisUsed;
                    _context.TCONTRACTITEM.Update(currContract);
                    //_context.SaveChanges();
                }

                _serviceAttName.DeleteByReferences(consumption.UPKEEPCODE);

                if (HelperService.GetConfigValue(PMSConstants.CFG_UpkeepAutoJournal + consumption.DIV.UNITCODE, _context) == PMSConstants.CFG_UpkeepAutoJournalTrue)
                {
                    var Fil = new FilterGoodIssue();
                    Fil.SearchTerm = consumption.UPKEEPCODE;
                    var gi = _serviceGIName.GetList(Fil).SingleOrDefault();
                    if (gi != null)
                    {
                        _serviceGIName.Reverse(gi.NO, consumption.UPDATEBY);
                    }
                    
                }

                this.SaveInsert(newConsumption, userName);

                if (HelperService.GetConfigValue(PMSConstants.CfgUpeepAutoUpload + consumption.DIV.UNITCODE, _context) == PMSConstants.CfgUpeepAutoUploadTrue)
                {
                    consumption.UPLOAD = 3;
                    consumption.UPLOADDATE = consumption.UPDATEDDATE;
                    var bkm = from a in _context.TUPKEEP //_context.sap_GetBkm(consumption.UPKEEPCODE);
                              join b in _context.MDIVISI on a.DIVID equals b.DIVID
                              join c in _context.MUNIT on b.UNITCODE equals c.UNITCODE
                              where a.UPKEEPCODE == consumption.UPKEEPCODE
                              select new
                              {
                                  UPKEEPCODE = a.UPKEEPCODE,
                                  isPlasma = (c.INTIID == null ? 0 : 1)
                              };
                    int isPlasma = StandardUtility.ToInt(bkm.SingleOrDefault().isPlasma);

                    //DataTable result;
                    //if (isPlasma == 0)
                    //    using (var services = PmsServicesFactory.GetPmsServices())
                    //        result = services.Upkeep_Cancel(consumption.Code, consumption.Date, consumption.Division.UnitCode, bkm, string.Empty);
                    //else
                    //    using (var services = PmsServicesFactory.GetPmsServices())
                    //        result = services.UpkeepPlasma_Cancel(consumption.Code, consumption.Date, consumption.Division.UnitCode, bkm, string.Empty);

                    //foreach (DataRow row in result.Rows)
                    //{
                    //    if (row["PSTG_STAT"].ToString() != "S")
                    //        rfcResult += row["PSTG_STAT"] + ": " + row["PSTG_MSG"] + " ";

                    //    if (isPlasma == 0)
                    //        HelperService.SetSapTran(consumption.Code, row["DOC_NUMBER"].ToString(), db);
                    //}

                    //if (!string.IsNullOrEmpty(rfcResult))
                    //    throw new Exception(rfcResult);
                }

                //return newConsumption.UPKEEPCODE;
                return true;
            }
            catch
            {
                //if (!string.IsNullOrEmpty(rfcResult))
                //    HelperService.SetSapResult(id, rfcResult, _context);

                throw;
                
            }
        }

        private string GenereteNewDerivedNumber(string code)
        {
            string newCode;
            string seq = code.Substring(code.Length - 5);

            if (seq.StartsWith("-"))
                newCode = code + PMSConstants.TransactionStatusApproved;
            else
            {
                string lastChar = seq.Substring(seq.Length - 1);
                string newChar = ((char)(Convert.ToInt32(lastChar.ToCharArray()[0]) + 1)).ToString();
                newCode = code.Substring(0, code.Length - 1) + newChar;
            }

            return newCode;
        }

        private string GenereteNewNumber(string divisionId, DateTime dateTime,string userName)
        {
            var org = _serviceOrgName.GetSingleV(divisionId);
            var estate = _serviceUnitName.GetSingle(org.LV2ID);
            var division = _serviceDivisiName.GetSingle(divisionId);
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.ConsumptionCodePrefix + divisionId, _context);
            return PMSConstants.ConsumptionCodePrefix + "-" + division.CODE + "-" + estate.UNITCODE
                + "-" + dateTime.ToString("yyyyMMdd") + "-" + lastNumber.ToString().PadLeft(4, '0');
        }

        private void CalculateConsumption(TUPKEEP consumption)
        {
            int decimalLimit = 3;
            const int decimalMaterialLimit = 3;
            if (consumption.UPKEEPTYPE == 1)
                decimalLimit = 0;

            var areaWorked = consumption.TUPKEEPBLOCK.Select(block => block.OUTPUTAREA);
            decimal totalAreaWorked = areaWorked.Sum();

            if (consumption.TUPKEEPCALC == null)
                consumption.TUPKEEPCALC = new List<TUPKEEPCALC>();

            consumption.TUPKEEPCALC.Clear();
            foreach (var employee in consumption.TUPKEEPEMPLOYEE)
            {
                decimal hkUsed = 0;
                int i = 1;
                foreach (var block in consumption.TUPKEEPBLOCK)
                {
                    decimal hkPerBlock;
                    if (i != consumption.TUPKEEPBLOCK.Count)
                    {
                        hkPerBlock = Decimal.Round(employee.VALUE * (block.OUTPUTAREA / totalAreaWorked), decimalLimit);
                        hkUsed += hkPerBlock;
                    }
                    else
                        hkPerBlock = employee.VALUE - hkUsed;

                    var calculation = new TUPKEEPCALC
                    {
                        UPKEEPCODE = employee.UPKEEPCODE,
                        EMPLOYEEID = employee.EMPLOYEEID,
                        BLOCKID = block.BLOCKID,
                        VALUE = hkPerBlock
                        //CALCID = "",
                        //CALCID = employee.UPKEEPCODE + '-' + employee.EMPLOYEEID + '-' + block.BLOCKID
                    };
                    consumption.TUPKEEPCALC.Add(calculation);

                    i++;
                }
            }

            if (consumption.TUPKEEPVENDOR != null)
                if (consumption.TUPKEEPVENDOR.VALUE > 0)
                {
                    decimal hkUsed = 0;
                    int i = 1;
                    foreach (var block in consumption.TUPKEEPBLOCK)
                    {
                        decimal hkPerBlock;
                        if (i != consumption.TUPKEEPBLOCK.Count)
                        {
                            hkPerBlock = Decimal.Round(consumption.TUPKEEPVENDOR.VALUE * (block.OUTPUTAREA / totalAreaWorked), decimalLimit);
                            hkUsed += hkPerBlock;
                        }
                        else
                            hkPerBlock = consumption.TUPKEEPVENDOR.VALUE - hkUsed;

                        var calculation = new TUPKEEPCALC
                        {
                            UPKEEPCODE = block.UPKEEPCODE,
                            EMPLOYEEID = null, //string.Empty,
                            BLOCKID = block.BLOCKID,
                            VALUE = hkPerBlock
                            //CALCID = "",
                            //CALCID = consumption.TUPKEEPVENDOR.UPKEEPCODE + '-' + '-' + block.BLOCKID
                        };
                        consumption.TUPKEEPCALC.Add(calculation);
                        
                        i++;
                    }
                }

            var newMaterials = new List<TUPKEEPMATERIAL>();
            foreach (var material in consumption.TUPKEEPMATERIAL)
            {
                decimal matUsed = 0;
                int i = 1;
                foreach (var block in consumption.TUPKEEPBLOCK)
                {
                    decimal matPerBlock;
                    if (i != consumption.TUPKEEPBLOCK.Count)
                    {
                        matPerBlock = Decimal.Round(material.QUANTITY * (block.OUTPUTAREA / totalAreaWorked), decimalMaterialLimit);
                        matUsed += matPerBlock;
                    }
                    else
                        matPerBlock = material.QUANTITY - matUsed;

                    var mat = new TUPKEEPMATERIAL
                    {
                        ACTIVITYID = consumption.ACTIVITYID,
                        BLOCKID = block.BLOCKID,
                        UPKEEPCODE = material.UPKEEPCODE,
                        MATERIALID = material.MATERIALID,
                        QUANTITY = matPerBlock,
                        BATCH = material.BATCH,
                    };
                    newMaterials.Add(mat);
                    i++;
                }
            }
            consumption.TUPKEEPMATERIAL.Clear();
            consumption.TUPKEEPMATERIAL = newMaterials;

            foreach (var block in consumption.TUPKEEPBLOCK)
            {
                var valuePerBlock = consumption.TUPKEEPCALC.Where(calc => calc.BLOCKID == block.BLOCKID).Select(calc => calc.VALUE);
                block.VALUE = valuePerBlock.Sum();
            }
        }

        private void InsertValidate(TUPKEEP consumption,string userName)
        {
            this.Validate(consumption, userName);

            var activity = _serviceActName.GetSingle(consumption.ACTIVITYID);
            if (activity.REQMAT && consumption.TUPKEEPMATERIAL.Count == 0) throw new Exception("Kegiatan ini membutuhkan bahan.");

            

            var consumptionExist = this.GetSingle(consumption.UPKEEPCODE);
            if (consumptionExist != null)
                throw new Exception("BKM dengan nomor " + consumption.UPKEEPCODE + " sudah ada.");
        }

        public override TUPKEEP CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TUPKEEP record = base.CopyFromWebFormData(formData, newRecord);

            /*Custom Code - Start*/
            List<TUPKEEPBLOCK> tupkeepBlock = new List<TUPKEEPBLOCK>();
            tupkeepBlock.CopyFrom<TUPKEEPBLOCK>(formData, "TUPKEEPBLOCK");
            record.TUPKEEPBLOCK = tupkeepBlock;

            List<TUPKEEPEMPLOYEE> tupkeepEmployee = new List<TUPKEEPEMPLOYEE>();
            tupkeepEmployee.CopyFrom<TUPKEEPEMPLOYEE>(formData, "TUPKEEPEMPLOYEE");
            record.TUPKEEPEMPLOYEE = tupkeepEmployee;

            List<TUPKEEPMATERIALSUM> tupkeepMaterialSum = new List<TUPKEEPMATERIALSUM>();
            tupkeepMaterialSum.CopyFrom<TUPKEEPMATERIALSUM>(formData, "TUPKEEPMATERIALSUM");
            record.TUPKEEPMATERIALSUM = tupkeepMaterialSum;

            TUPKEEPVENDOR tupkeepVendor = new TUPKEEPVENDOR();
            tupkeepVendor.CopyFrom(formData);
            if (!string.IsNullOrEmpty(record.CONTID))
                record.TUPKEEPVENDOR = tupkeepVendor;

            if(record.UPKEEPTYPE == 1)
            {
                record.ACTIVITYID = formData["ACTIVITYID[ACTID]"];
            }
            if (record.DIV == null)
            {
                var divisi = new MDIVISI();
                divisi.CopyFrom(_serviceDivisiName.GetSingle(record.DIVID));
                record.DIV = divisi;
            }

            return record;
        }

       

        protected override TUPKEEP AfterSave(TUPKEEP record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.ConsumptionCodePrefix + record.DIVID, _context);
            return record;
        }

        protected override TUPKEEP BeforeSave(TUPKEEP record, string userName, bool newRecord)
        {

            record.TUPKEEPMATERIAL.Clear();
            
            _upkeepBlock.Clear();
            foreach (var block in record.TUPKEEPBLOCK)
            {
                _upkeepBlock.Add(block);
            }
            record.TUPKEEPBLOCK.Clear();

            _upkeepEmployee.Clear();
            foreach (var emp in record.TUPKEEPEMPLOYEE)
            {
                _upkeepEmployee.Add(emp);
            }
            record.TUPKEEPEMPLOYEE.Clear();

            _upkeepMaterial.Clear();
            foreach (var mat in record.TUPKEEPMATERIAL)
            {
                _upkeepMaterial.Add(mat);
            }
            record.TUPKEEPMATERIAL.Clear();

            _upkeepCalc.Clear();
            foreach (var calc in record.TUPKEEPCALC)
            {
                _upkeepCalc.Add(calc);
            }
            record.TUPKEEPCALC.Clear();

            _upkeepVendor = record.TUPKEEPVENDOR;
            record.TUPKEEPVENDOR = null;
            record.DIV = null;
            record.ACTIVITY = null;
            record.MANDOR = null;
            

            if (record.REMARK == null)
                record.REMARK = "";
            if (record.CANCELEDCOMMENT == null)
                record.CANCELEDCOMMENT = "";


            DateTime now = GetServerTime();

            if (string.IsNullOrEmpty(record.UPKEEPCODE))
                record.UPKEEPCODE = this.GenereteNewNumber(record.DIVID, record.UPKEEPDATE, userName);

            if (newRecord)
            {
                record.STATUS = PMSConstants.TransactionStatusProcess;
                record.CREATEBY = userName;
                record.CREATEDDATE = now;
                this.InsertValidate(record, userName);
            }

            record.UPDATEBY = userName;
            record.UPDATEDDATE = now;



            CalculateConsumption(record);
            if (newRecord)
            {
                foreach (var calculation in record.TUPKEEPCALC)
                {
                    calculation.UPKEEPCODE = record.UPKEEPCODE;
                    this.InsertValidateCalc(calculation);
                }

                foreach (var block in record.TUPKEEPBLOCK)
                {
                    block.UPKEEPCODE = record.UPKEEPCODE;
                    this.InsertValidateBlock(block);
                    block.UPKEEP = null;
                }

                foreach (var material in record.TUPKEEPMATERIAL)
                {
                    material.UPKEEPCODE = record.UPKEEPCODE;
                    material.MATERIAL = _context.MMATERIAL.Where(a => a.MATERIALID.Equals(material.MATERIALID)).SingleOrDefault();
                    this.InsertValidateMat(material);
                    material.MATERIAL = null;
                    material.UPKEEP = null;
                }

                foreach (var employee in record.TUPKEEPEMPLOYEE)
                {
                    employee.UPKEEPCODE = record.UPKEEPCODE;
                    this.InsertValidateEmp(employee);
                    employee.EMPLOYEE = null;
                    employee.UPKEEP = null;
                }

                if (record.TUPKEEPVENDOR != null)
                {
                    record.TUPKEEPVENDOR.UPKEEPCODE = record.UPKEEPCODE;
                    this.InsertValidateVendor(record.TUPKEEPVENDOR);
                }
            }
            else
                UpdateValidate(record, userName);

            if (record.DIV == null)
            {
                MDIVISI divisi = new MDIVISI();
                divisi.CopyFrom(_serviceDivisiName.GetSingle(record.DIVID));
                record.DIV = divisi;
            }
            if (HelperService.GetConfigValue(PMSConstants.CFG_UpkeepAutoJournal + record.DIV.UNITCODE, _context) == PMSConstants.CFG_UpkeepAutoJournalTrue)
            {
                FilterCompany fil = new FilterCompany();
                fil.SearchTerm = record.DIVID;
                var location = _serviceStorLocName.GetList(fil).SingleOrDefault();
                foreach (var cm in record.TUPKEEPMATERIAL)
                {
                    decimal[] stock = _serviceStockName.GetStock(location.CODE, cm.MATERIALID, record.UPKEEPDATE);
                    cm.STOCK = stock[0];
                }
            }


            _saveDetails = _upkeepBlock.Any() || _upkeepEmployee.Any() || _upkeepMaterial.Any() || _upkeepCalc.Any();

            return record;
        }

        protected override TUPKEEP SaveInsertToDB(TUPKEEP record, string userName)
        {
            _context.TUPKEEP.Add(record);

            if (_upkeepBlock.Any())
                _context.TUPKEEPBLOCK.AddRange(_upkeepBlock);

            if (_upkeepMaterial.Any())
                _context.TUPKEEPMATERIAL.AddRange(_upkeepMaterial);

            if (_upkeepEmployee.Any())
                _context.TUPKEEPEMPLOYEE.AddRange(_upkeepEmployee);

            if (_upkeepCalc.Any())
                _context.TUPKEEPCALC.AddRange(_upkeepCalc);

            if (!string.IsNullOrEmpty(record.CONTID))
                _context.TUPKEEPVENDOR.Add(_upkeepVendor);

            _context.SaveChanges();

            return record;
        }

        //protected override TUPKEEP SaveInsertDetailsToDB(TUPKEEP consumption, string userName)
        //{
            
        //    return consumption;
        //}


        private void InsertValidateBlock(TUPKEEPBLOCK consumptionBlock)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(consumptionBlock.UPKEEPCODE)) result += "Kode harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(consumptionBlock.BLOCKID)) result += "Block harus diisi." + Environment.NewLine;
            if (consumptionBlock.OUTPUTAREA == 0) result += "Hasil Kerja harus diisi." + Environment.NewLine;
            if (consumptionBlock.VALUE == 0) result += "Perblok harus diisi." + Environment.NewLine;
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        private void InsertValidateMat(TUPKEEPMATERIAL consumptionMaterial)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(consumptionMaterial.UPKEEPCODE)) result += "Kode harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(consumptionMaterial.ACTIVITYID)) result += "Kegiatan harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(consumptionMaterial.BLOCKID)) result += "Blok harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(consumptionMaterial.MATERIALID)) result += "Bahan harus diisi." + Environment.NewLine;
            if (consumptionMaterial.QUANTITY == 0) result += "Jumlah harus diisi." + Environment.NewLine;

            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            //if (string.IsNullOrEmpty(consumptionMaterial.BATCHID) && consumptionMaterial.MATERIAL.REQBATCH)
            //    throw new Exception("Batch material harus diisi dahulu");

            //if (!string.IsNullOrEmpty(consumptionMaterial.BATCHID) && !consumptionMaterial.MATERIAL.REQBATCH)
            //    throw new Exception("Batch material tidak perlu diisi");
        }

        private void InsertValidateEmp(TUPKEEPEMPLOYEE consumptionEmployee)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(consumptionEmployee.UPKEEPCODE)) result += "Kode harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(consumptionEmployee.EMPLOYEEID)) result += "Karyawan harus diisi." + Environment.NewLine;
            if (consumptionEmployee.VALUE == 0) result += "Value Karyawan diisi." + Environment.NewLine;

            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        private void InsertValidateCalc(TUPKEEPCALC consumptionCalculation)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(consumptionCalculation.UPKEEPCODE)) result += "Kode harus diisi." + Environment.NewLine;
            //if (string.IsNullOrEmpty(consumptionCalculation.EmployeeId)) result += "Karyawan harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(consumptionCalculation.BLOCKID)) result += "Blok harus diisi." + Environment.NewLine;
            if (consumptionCalculation.VALUE == 0) result += "Nilai harus diisi." + Environment.NewLine;

            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        private void InsertValidateVendor(TUPKEEPVENDOR consumptionVendor)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(consumptionVendor.UPKEEPCODE)) result += "Kode harus diisi." + Environment.NewLine;

            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        private void UpdateValidate(TUPKEEP consumption,string userName)
        {
            if (consumption != null)
            {
                if (consumption.STATUS == PMSConstants.TransactionStatusApproved)
                    throw new Exception("Data sudah di approve.");
                if (consumption.STATUS == PMSConstants.TransactionStatusCanceled)
                    throw new Exception("Data sudah di cancel.");
            }

            var activity = _serviceActName.GetSingle(consumption.ACTIVITYID);
            if (activity.REQMAT && consumption.TUPKEEPMATERIAL.Count == 0) throw new Exception("Kegiatan ini membutuhkan bahan.");

            this.Validate(consumption,userName);
            foreach(var emp in consumption.TUPKEEPEMPLOYEE)
            {
                emp.EMPLOYEE = null;
            }
        }

        

        protected override TUPKEEP SaveUpdateDetailsToDB(TUPKEEP consumption, string userName)
        {
            
            _context.TUPKEEPBLOCK.RemoveRange(_context.TUPKEEPBLOCK.Where(a => a.UPKEEPCODE == consumption.UPKEEPCODE));
            if (_upkeepBlock.Any())
                _context.TUPKEEPBLOCK.AddRange(_upkeepBlock);

            _context.TUPKEEPMATERIAL.RemoveRange(_context.TUPKEEPMATERIAL.Where(a => a.UPKEEPCODE == consumption.UPKEEPCODE));
            if (_upkeepMaterial.Any())
                _context.TUPKEEPMATERIAL.AddRange(_upkeepMaterial);

            _context.TUPKEEPEMPLOYEE.RemoveRange(_context.TUPKEEPEMPLOYEE.Where(a => a.UPKEEPCODE == consumption.UPKEEPCODE));
            if (_upkeepEmployee.Any())
                _context.TUPKEEPEMPLOYEE.AddRange(_upkeepEmployee);

            _context.TUPKEEPCALC.RemoveRange(_context.TUPKEEPCALC.Where(a => a.UPKEEPCODE == consumption.UPKEEPCODE));
            if (_upkeepCalc.Any())
                _context.TUPKEEPCALC.AddRange(_upkeepCalc);

            if (_upkeepVendor != null)
            {
                _context.TUPKEEPVENDOR.RemoveRange(_context.TUPKEEPVENDOR.Where(a => a.UPKEEPCODE == consumption.UPKEEPCODE));
                _context.TUPKEEPVENDOR.Add(_upkeepVendor);
            }

            return consumption;
        }

        private void DeleteValidate(TUPKEEP consumption)
        {
            if (consumption.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (consumption.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
            _servicePeriodName.CheckValidPeriod(consumption.DIV.UNITCODE, consumption.UPKEEPDATE);
        }

        protected override TUPKEEP BeforeDelete(TUPKEEP consumption, string userName)
        {
            consumption = GetSingle(consumption.UPKEEPCODE);
            if (consumption.DIV == null)
            {
                var divisi = new MDIVISI();
                divisi.CopyFrom(_serviceDivisiName.GetSingle(consumption.DIVID));
                consumption.DIV = divisi;
            }

            this.DeleteValidate(consumption);

            foreach (var block in consumption.TUPKEEPBLOCK)
            {
                _upkeepBlock.Add(block);
            }
            consumption.TUPKEEPBLOCK.Clear();

            foreach (var emp in consumption.TUPKEEPEMPLOYEE)
            {
                _upkeepEmployee.Add(emp);
            }
            consumption.TUPKEEPEMPLOYEE.Clear();

            foreach (var mat in consumption.TUPKEEPMATERIAL)
            {
                _upkeepMaterial.Add(mat);
            }
            consumption.TUPKEEPMATERIAL.Clear();

            foreach (var calc in consumption.TUPKEEPCALC)
            {
                _upkeepCalc.Add(calc);
            }
            consumption.TUPKEEPCALC.Clear();

            _upkeepVendor = consumption.TUPKEEPVENDOR;
            consumption.TUPKEEPVENDOR = null;
            consumption.DIV = null;
            consumption.ACTIVITY = null;
            consumption.MANDOR = null;

            _saveDetails = _upkeepBlock.Any() || _upkeepEmployee.Any() || _upkeepMaterial.Any() || _upkeepCalc.Any();

            return consumption;
        }

        protected override bool DeleteFromDB(TUPKEEP consumption, string userName)
        {
            _context.TUPKEEP.Remove(consumption);

            if (_upkeepBlock.Any())
                _context.TUPKEEPBLOCK.RemoveRange(_upkeepBlock);

            if (_upkeepCalc.Any())
                _context.TUPKEEPCALC.RemoveRange(_upkeepCalc);

            if (_upkeepEmployee.Any())
                _context.TUPKEEPEMPLOYEE.RemoveRange(_upkeepEmployee);

            if (_upkeepMaterial.Any())
                _context.TUPKEEPMATERIAL.RemoveRange(_upkeepMaterial);

            if (!string.IsNullOrEmpty(consumption.CONTID) && _upkeepVendor != null)
                _context.TUPKEEPVENDOR.Remove(_upkeepVendor);

            _context.SaveChanges();

            return true;
        }

        protected override TUPKEEP GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            TUPKEEP record = _context.TUPKEEP
                                .Include(a => a.TUPKEEPBLOCK)
                                .Include(b => b.TUPKEEPCALC)
                                .Include(c => c.TUPKEEPEMPLOYEE)
                                .Include(d => d.TUPKEEPMATERIAL)
                                .Include(e => e.TUPKEEPVENDOR)
                                .Include(f => f.DIV)
                                .Include(g => g.ACTIVITY)
                                .Include(h => h.MANDOR)                                
                                .FirstOrDefault(d => d.UPKEEPCODE.Equals(Id));

            if (record!= null)
            {
                if (!StandardUtility.IsEmptyList(record.TUPKEEPBLOCK))
                {
                    var blocks = _context.VBLOCK.Where(d => record.TUPKEEPBLOCK.Select(s => s.BLOCKID).ToList().Contains(d.BLOCKID)).ToList();
                    (
                        from a in record.TUPKEEPBLOCK
                        join b in blocks on a.BLOCKID equals b.BLOCKID
                        select new { TUPKEEPBLOCK = a, MBLOCK = b }
                    ).ToList().ForEach(d => {
                     
                        d.TUPKEEPBLOCK.CURRENTPLANTED = d.MBLOCK.CURRENTPLANTED;
                        d.TUPKEEPBLOCK.THNTANAM = d.MBLOCK.THNTANAM;
                        d.TUPKEEPBLOCK.BLOCKCODE = d.MBLOCK.CODE;
                    });
                }
               
                if (!StandardUtility.IsEmptyList(record.TUPKEEPEMPLOYEE))
                {
                    var employees = _context.MEMPLOYEE.Where(d => record.TUPKEEPEMPLOYEE.Select(s => s.EMPLOYEEID).ToList().Contains(d.EMPID)).ToList();
                    (
                        from a in record.TUPKEEPEMPLOYEE
                        join b in employees on a.EMPLOYEEID equals b.EMPID
                        select new { TUPKEEPEMPLOYEE = a, MEMPLOYEE = b }
                    ).ToList().ForEach(d => {
                        
                        d.TUPKEEPEMPLOYEE.EMPNAME = d.MEMPLOYEE.EMPNAME;
                        d.TUPKEEPEMPLOYEE.EMPTYPE = d.MEMPLOYEE.EMPTYPE;
                    });
                }

                if (!StandardUtility.IsEmptyList(record.TUPKEEPMATERIAL))
                {
                    
                    record.TUPKEEPMATERIALSUM = record.TUPKEEPMATERIAL.GroupBy(d => new { d.UPKEEPCODE, d.MATERIALID, d.BATCHID })
                        .Select(d => new TUPKEEPMATERIALSUM
                        {
                            UPKEEPCODE = d.Key.UPKEEPCODE,
                            MATERIALID = d.Key.MATERIALID,
                            BATCHID = d.Key.BATCHID,
                            QUANTITY = d.Sum(s => s.QUANTITY),
                            STOCK = d.Average(s => s.STOCK)
                        }).ToList();
                    var materials = _context.MMATERIAL.Where(d => record.TUPKEEPMATERIAL.Select(s => s.MATERIALID).Distinct().ToList().Contains(d.MATERIALID)).ToList();

                    (
                        from a in record.TUPKEEPMATERIAL
                        join b in materials on a.MATERIALID equals b.MATERIALID
                        select new { TUPKEEPMATERIAL = a, MMATERIAL = b }
                    ).ToList().ForEach(d => {
                        d.TUPKEEPMATERIAL.MATERIAL = d.MMATERIAL;
                    });

                    (
                        from a in record.TUPKEEPMATERIALSUM
                        join b in materials on a.MATERIALID equals b.MATERIALID
                        select new { TUPKEEPMATERIALSUM = a, MMATERIAL = b }
                    ).ToList().ForEach(d => {
                        d.TUPKEEPMATERIALSUM.MATERIALNAME = d.MMATERIAL.MATERIALNAME;
                        d.TUPKEEPMATERIALSUM.UOM = d.MMATERIAL.UOM;
                        d.TUPKEEPMATERIALSUM.REQBATCH = d.MMATERIAL.REQBATCH;
                    });
                }

               
            }

            return record;
        }

        protected override TUPKEEP GetSingleFromDB(TUPKEEP record)
        {
            try
            {
                string Id = record.UPKEEPCODE;
                return this.GetSingleFromDB(Id);
            }
            catch
            {
                return null;
            }
            
        }

        public override IEnumerable<TUPKEEP> GetList(FilterUpkeep filter)
        {
            
            var criteria = PredicateBuilder.True<TUPKEEP>();
            DateTime dateNull = new DateTime();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.UPKEEPCODE.Equals(filter.Id));
                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.UPKEEPCODE));

                if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                    criteria = criteria.And(p => p.DIVID.Equals(filter.DivisionID));
                if (filter.DivisionIDs.Any())
                    criteria = criteria.And(d => filter.DivisionIDs.Contains(d.DIVID));

                if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(d => filter.RecordStatus.Equals(d.STATUS));

                //if (!string.IsNullOrWhiteSpace(filter.UpkeepType.ToString()))
                //    criteria = criteria.And(d => filter.UpkeepType.Equals(d.UPKEEPTYPE));

                if (filter.Date.Date != dateNull)
                    //criteria = criteria.And(d => d.UPKEEPDATE.Date == filter.Date);
                if (filter.StartDate.Date != dateNull || filter.EndDate.Date != dateNull)
                    criteria = criteria.And(d => d.UPKEEPDATE.Date >= filter.StartDate.Date && d.UPKEEPDATE.Date <= filter.EndDate.Date);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(d => d.UPKEEPCODE.Contains(filter.SearchTerm) || d.CANCELEDCOMMENT.Contains(filter.SearchTerm)
                                            || d.ACTIVITYID.Contains(filter.SearchTerm) || d.MANDORID.Contains(filter.SearchTerm)
                                            || d.CONTID.Contains(filter.SearchTerm));

                //if (!string.IsNullOrWhiteSpace(filter.MenuID))
                //    criteria = criteria.And(d => !string.IsNullOrWhiteSpace(d.CONTID));


                if (filter.MenuID == "HARIAN")
                    criteria = criteria.And(d => d.UPKEEPTYPE == 0);  // default harian
                if (filter.MenuID == "BORONGAN")
                    criteria = criteria.And(d => d.UPKEEPTYPE == 1); // default borongan
            }

            if(filter.PageSize <= 0)
                return _context.TUPKEEP.Where(criteria);
            return _context.TUPKEEP.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }


        
        public List<UPKEEPBLOCKWORKEDAREA> GetActualWorkedAreaOtherDocument(TUPKEEP record)
        {
            DateTime monthStart = new DateTime(record.UPKEEPDATE.Year, record.UPKEEPDATE.Month, 1);
            DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var upkeepCriteria = PredicateBuilder.True<TUPKEEP>();
            upkeepCriteria = upkeepCriteria.And(d => 
                    d.STATUS.ToUpper().Equals(PMSConstants.TransactionStatusApproved) 
                    && !d.UPKEEPCODE.Equals(record.UPKEEPCODE) 
                    && d.UPKEEPDATE >= monthStart && d.UPKEEPDATE <= monthEnd 
                    && d.ACTIVITYID.Equals(record.ACTIVITYID));

            var upkeepBlockCriteria = PredicateBuilder.True<TUPKEEPBLOCK>();
            upkeepBlockCriteria = upkeepBlockCriteria.And(d => record.TUPKEEPBLOCK.Select(s => d.BLOCKID).ToList().Contains(d.BLOCKID));

            return (
                from a in _context.TUPKEEP.Where(upkeepCriteria)
                join b in _context.TUPKEEPBLOCK.Where(upkeepBlockCriteria) on a.UPKEEPCODE equals b.UPKEEPCODE
                group b by b.BLOCKID into b1
                select new UPKEEPBLOCKWORKEDAREA { BLOCKID = b1.Key, WORKEDAREA = b1.Sum(s => s.OUTPUTAREA) }
            ).ToList();
        }
        public bool ValidateUpkeepBlockTotalOutput(TUPKEEP record,out string errorMessage)
        {
            errorMessage = string.Empty;
            var divisi = _context.VDIVISI.FirstOrDefault(d => d.DIVID.Equals(record.DIVID));
            if (divisi == null || !divisi.ACTIVE.Value) 
            {
                errorMessage = "Kode divisi tidak valid atau tidak aktif";
                return false;
            }
            if (!divisi.Seeding && !StandardUtility.IsEmptyList(record.TUPKEEPBLOCK ))            
            {
                var workedAreaOtherDocument = GetActualWorkedAreaOtherDocument(record);
                if (!StandardUtility.IsEmptyList(workedAreaOtherDocument))
                {
                    string result = string.Empty;
                    (
                          from a in record.TUPKEEPBLOCK.Where(d => d.UOM1.ToUpper() == "HA")
                          join b in workedAreaOtherDocument on a.BLOCKID equals b.BLOCKID
                          join c in _context.MBLOCK on b.BLOCKID equals c.BLOCKID
                          where b.WORKEDAREA + a.OUTPUTAREA > c.CURRENTPLANTED
                          select new {a.BLOCKID, a.BLOCKCODE, TOTALWORKEDAREA = b.WORKEDAREA + a.OUTPUTAREA, c.CURRENTPLANTED }
                     ).ToList().ForEach(d => {
                         result += $"\r\nBlok {d.BLOCKCODE}: Luas {d.CURRENTPLANTED:'#,#0.00'} HA, Total Hasil Kerja {d.TOTALWORKEDAREA:'#,#0.00'} HA ";
                     });
                     
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        errorMessage = "Hasil kerja blok-blok berikut melebihi luas blok: " + result;
                        return false;
                    }                       
                }
               
               
            }
            return true;
        }

    }
}
