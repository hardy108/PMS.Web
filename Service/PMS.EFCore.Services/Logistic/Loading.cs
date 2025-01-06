using System;
using System.Collections;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.Shared.Utilities;
using PMS.EFCore.Helper;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.Attendances;
using PMS.EFCore.Services.Organization;
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Logistic
{
    public class Loading:EntityFactory<TLOADING,TLOADING, FilterLoading, PMSContextBase>
    {
        private Period _periodService;
        private Activity _activityService;
        private Block _blockService;
        private Employee _employeeService;

        private Attendance _attendanceService;
        private Holiday _holidayService;
        private HarvestingBlockResult _HarvestingBlockResultService;
        private AuthenticationServiceBase _authenticationService;

        private List<TLOADINGBLOCK> _newTLoadingBlocks = new List<TLOADINGBLOCK>();
        private List<TLOADINGEMPLOYEE> _newTLoadingEmployees = new List<TLOADINGEMPLOYEE>();
        private List<TLOADINGCOLLECT> _newTLoadingCollects = new List<TLOADINGCOLLECT>();
        private List<TLOADINGDRIVER> _newTLoadingDrivers = new List<TLOADINGDRIVER>();

        private TLOADING LoadingSPB_old;

        public Loading(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Loading";
            _authenticationService = authenticationService;
            _periodService = new Period(context,_authenticationService,auditContext);

            _activityService = new Activity(context,_authenticationService,auditContext);
            _blockService = new Block(context,_authenticationService,auditContext);
            _employeeService = new Employee(context,_authenticationService,auditContext);

            _attendanceService = new Attendance(context,_authenticationService, auditContext);
            _holidayService = new Holiday(context,_authenticationService, auditContext);
            _HarvestingBlockResultService = new HarvestingBlockResult(context,_authenticationService,auditContext);
        }

        protected override TLOADING GetSingleFromDB(params  object[] keyValues)
        {
            TLOADING record =
            _context.TLOADING
            .Include(d => d.TLOADINGEMPLOYEE)
            .Include(d => d.TLOADINGBLOCK)
            .Include(d => d.TLOADINGCOLLECT)
            .Include(d => d.TLOADINGDRIVER)
            .SingleOrDefault(d => d.LOADINGCODE.Equals(keyValues[0]));

            if(record != null)
            {
                List<string>
                            blockIds = record.TLOADINGBLOCK.Select(d => d.BLOCKID).Distinct().ToList(),
                            empIds = record.TLOADINGEMPLOYEE.Select(d => d.EMPLOYEEID).Distinct().ToList();


                if (blockIds.Any())
                {
                    var blockItem =
                        record.TLOADINGBLOCK.GroupBy(x => new { x.LOADINGCODE, x.BLOCKID, x.KG })
                        .Select(b => new TLOADINGBLOCK
                        {
                            LOADINGCODE = b.Key.LOADINGCODE,
                            BLOCKID = b.Key.BLOCKID,
                            KG = b.Key.KG
                        }).ToList();

                    if (blockItem.Any())
                        record.VLOADINGBLOCKREF = _blockService.GetList(new GeneralFilter { Ids = blockIds })
                                .Select(d => new VLOADINGBLOCK
                                {
                                    THNTANAM = d.THNTANAM,
                                    BLOCKID = d.BLOCKID,
                                    LUASBLOCK = d.LUASBLOCK,
                                    KG = blockItem.Where(c => c.BLOCKID.Equals(d.BLOCKID)).Select(a => a.KG).SingleOrDefault()
                                })
                               .ToList();

                }
                if (empIds.Any())
                    record.VLOADINGEMPLOYEEREF = _employeeService.GetList(new FilterEmployee { Ids = empIds })
                        .Select(d => new VLOADINGEMPLOYEE
                        {
                            EMPID = d.EMPID,
                            EMPCODE = d.EMPCODE,
                            EMPNAME = $"{d.EMPCODE} - {d.EMPNAME}",
                            EMPTYPE = d.EMPTYPE
                        })
                        .ToList();

                AddDetail(record);
            }

            return record;
        }

        public TLOADING AddDetail (TLOADING record)
        {
            record.DIV = _context.MDIVISI.Where(d => d.DIVID.Equals(record.DIVID)).SingleOrDefault();
            record.MACTIVITY = _context.MACTIVITY.Where(d => d.ACTIVITYID.Equals(record.ACTIVITYID)).SingleOrDefault();

            //TLOADINGBLOCK
            foreach (var block in record.TLOADINGBLOCK)
            {
                block.BLOCK = _context.MBLOCK.Where(d => d.BLOCKID.Equals(block.BLOCKID)).SingleOrDefault();
            }
            //TLOADINGEMPLOYEE
            foreach (var emp in record.TLOADINGEMPLOYEE)
            {
                emp.EMPLOYEE = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(emp.EMPLOYEEID)).SingleOrDefault();
            }
            //TLOADINGCOLLECT
            foreach (var coll in record.TLOADINGCOLLECT)
            {
                coll.BLOCK = _context.MBLOCK.Where(d => d.BLOCKID.Equals(coll.BLOCKID)).SingleOrDefault();
            }
            //TLOADINGDRIVER
            foreach (var emp in record.TLOADINGDRIVER)
            {
                emp.DRIVER = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(emp.DRIVERID)).SingleOrDefault();
            }
            return record;   
        }

        public TLOADING RemoveDetail (TLOADING record)
        {
            record.DIV = null;
            record.MACTIVITY = null;

            //TLOADINGBLOCK
            foreach (var block in record.TLOADINGBLOCK)
            {
                block.BLOCK = null;
            }
            //TLOADINGEMPLOYEE
            foreach (var emp in record.TLOADINGEMPLOYEE)
            {
                emp.EMPLOYEE = null;
            }
            //TLOADINGCOLLECT
            foreach (var coll in record.TLOADINGCOLLECT)
            {
                coll.BLOCK = null;
            }
            //TLOADINGDRIVER
            foreach (var emp in record.TLOADINGDRIVER)
            {
                emp.DRIVER = null;
            }

            return record;

        }

        public override TLOADING CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TLOADING record = base.CopyFromWebFormData(formData, newRecord);
            record.TLOADINGBLOCK.Clear();
            _newTLoadingBlocks = new List<TLOADINGBLOCK>();
            _newTLoadingBlocks.CopyFrom<TLOADINGBLOCK>(formData, "TLOADINGBLOCK");
            _newTLoadingBlocks.ForEach(d =>
            { record.TLOADINGBLOCK.Add(d); }
            );

            record.TLOADINGEMPLOYEE.Clear();
            _newTLoadingEmployees = new List<TLOADINGEMPLOYEE>();
            _newTLoadingEmployees.CopyFrom<TLOADINGEMPLOYEE>(formData, "TLOADINGEMPLOYEE");
            _newTLoadingEmployees.ForEach(d =>
            { record.TLOADINGEMPLOYEE.Add(d); }
            );

            record.TLOADINGCOLLECT.Clear();
            _newTLoadingCollects = new List<TLOADINGCOLLECT>();
            _newTLoadingCollects.CopyFrom<TLOADINGCOLLECT>(formData, "TLOADINGCOLLECT");
            _newTLoadingCollects.ForEach(d =>
            { record.TLOADINGCOLLECT.Add(d); }
            );

            record.TLOADINGDRIVER.Clear();
            _newTLoadingDrivers = new List<TLOADINGDRIVER>();
            _newTLoadingDrivers.CopyFrom<TLOADINGDRIVER>(formData, "TLOADINGDRIVER");
            _newTLoadingDrivers.ForEach(d =>
            { record.TLOADINGDRIVER.Add(d); }
            );

            return record;
        }

        private string FieldsValidation(TLOADING Loading)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(Loading.LOADINGCODE)) result += "Nomor tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(Loading.DIVID)) result += "Kode Divisi tidak boleh kosong." + Environment.NewLine;
            if (Loading.LOADINGDATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(Loading.PRODUCTID.ToString()) || Loading.PRODUCTID.ToString() == "0") result += "Jenis Produk tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(Loading.KRANIID)) result += "Nama Krani tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(Loading.ACTIVITYID)) result += "Jenis Pekerjaan tidak boleh kosong." + Environment.NewLine;
            if (Loading.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;

            //Loading Block
            foreach (var item in Loading.TLOADINGBLOCK)
            {
                if (string.IsNullOrEmpty(item.LOADINGCODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.BLOCKID)) result += "Block tidak boleh kosong." + Environment.NewLine;
            }

            //Loading Employee
            foreach (var item in Loading.TLOADINGEMPLOYEE)
            {
                if (string.IsNullOrEmpty(item.LOADINGCODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.EMPLOYEEID)) result += "Karyawan tidak boleh kosong." + Environment.NewLine;

            }

            //Loading Collect
            foreach (var item in Loading.TLOADINGCOLLECT)
            {
                if (string.IsNullOrEmpty(item.LOADINGCODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.EMPLOYEEID)) result += "Karyawan tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.BLOCKID)) result += "Block tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.COLLPOINT)) result += "No TPH tidak boleh kosong." + Environment.NewLine;
                if (item.QTY <= 0) result += "Jumlah panen harus > 0." + Environment.NewLine;
            }

            //Loading Driver
            foreach (var item in Loading.TLOADINGDRIVER)
            {
                if (string.IsNullOrEmpty(item.LOADINGCODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;
            }

            return result;
        }

        private void Validate(TLOADING Loading)
        {
            string result = this.FieldsValidation(Loading);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            if (Loading.TLOADINGCOLLECT == null && Loading.TLOADINGDRIVER == null)
                throw new Exception("Tidak boleh hasil muat kosong dan operator tidak ada");

            if (Loading.SPBDATATYPE == 0)
            {
                if (Loading.PRODUCTID.ToString() == string.Empty)
                    throw new Exception("Product tidak boleh kosong.");

                if (Loading.TLOADINGBLOCK.Count == 0)
                    throw new Exception("Blok tidak boleh kosong.");               
            }

            if (Loading.DESTINATIONID == string.Empty)
                throw new Exception("Lokasi Tujuan tidak boleh kosong.");


            var blocks = from block in Loading.TLOADINGBLOCK
                         where ((block.BLOCK.PHASE == 1 && Loading.MACTIVITY.TBM == false) || Loading.MACTIVITY.TBM)
                         && ((block.BLOCK.PHASE == 5 && Loading.MACTIVITY.TM == false) || Loading.MACTIVITY.TM)
                         && ((block.BLOCK.PHASE == 9 && Loading.MACTIVITY.GA == false) || Loading.MACTIVITY.GA)
                         && ((block.BLOCK.PHASE == 8 && Loading.MACTIVITY.LC == false) || Loading.MACTIVITY.LC)
                         select block;

            if (blocks.Count() > 0 && blocks != null)
                throw new Exception("periksa kembali phase blok dan kegiatan.");

            if (Loading.LOADINGTYPE == 0)
            {
                bool Isnumber = true;
                string errorMessage = " Buku Muat Pekerjaan " + Loading.ACTIVITYID + " (JJG) tidak bisa input QTY pecahan desimal (koma) , yaitu sebagai berikut :\r\n";
                Loading.TLOADINGCOLLECT.ToList().ForEach(d =>
                {
                    if (!int.TryParse(Convert.ToString(d.QTY), out int number))
                    {
                        Isnumber = false;
                        errorMessage += "[ "+Convert.ToString(d.QTY) + " ] " + "\r\n";
                    }
                });
                if(Isnumber==false)
                throw new Exception(errorMessage);
            }
            _periodService.CheckValidPeriod(Loading.DIV.UNITCODE, Loading.LOADINGDATE);
        }

        private void InsertValidate(TLOADING Loading)
        {
            this.Validate(Loading);

            TLOADING LoadingExist = _context.TLOADING.Where(d => d.LOADINGCODE.Equals(Loading.LOADINGCODE)).SingleOrDefault();
            if (LoadingExist != null)
                throw new Exception("Buku Pemuat dengan nomor " + Loading.LOADINGCODE + " sudah ada.");

            int isExist = CheckActivity(Loading.DIVID, Loading.ACTIVITYID, Loading.NOSPB, Loading.LOADINGDATE.Date);
            if (isExist > 0)
                throw new Exception(Loading.MACTIVITY.ACTIVITYNAME + " Dan No.SPB " + Loading.NOSPB + " sudah dipakai.");
        }

        private void UpdateValidate(TLOADING Loading)
        {
            this.Validate(Loading);

            var currHarvest = GetSingle(Loading.LOADINGCODE);
            if (currHarvest != null)
            {
                if (currHarvest.STATUS == "A")
                    throw new Exception("Data sudah di approve.");
                if (currHarvest.STATUS == "C")
                    throw new Exception("Data sudah di cancel.");
            }
        }

        private void DeleteValidate(TLOADING Loading)
        {
            if (Loading.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (Loading.STATUS == "C")
                throw new Exception("Data sudah di cancel.");
            _periodService.CheckValidPeriod(Loading.DIV.UNITCODE, Loading.LOADINGDATE);
        }

        private void ValidateProcess(string UnitCode, DateTime date)
        {
            if (GetApprovedData(UnitCode, date, date) >= 1)
            {
                throw new Exception("Proses Hitung Tidak Bisa, Ada BKP Yang Diapprove");
            }
        }

        private void ApproveValidate(TLOADING Loading)
        {
            if (Loading.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (Loading.STATUS == "C")
                throw new Exception("Data sudah di cancel.");

            if (Loading.SPBDATATYPE == 1)
            {
                if (Loading.NOSPB == null || Loading.NOSPB == string.Empty)
                    throw new Exception("Pilih Salah Satu SPB.");
            }

            this.Validate(Loading);
            _periodService.CheckMaxPeriod(Loading.DIV.UNITCODE, Loading.LOADINGDATE);

            bool basisByKg = false;
            if (HelperService.GetConfigValue(PMSConstants.CFG_LoadingBasisByKg, _context) ==
                PMSConstants.CFG_LoadingBasisByKgTrue)
                basisByKg = true;

            var stringMaxHasil = HelperService.GetConfigValue(PMSConstants.CFG_LoadingResultMaxPercent + Loading.DIV.UNITCODE, _context);
            decimal percentMaxHasil = 0;
            if (!string.IsNullOrEmpty(stringMaxHasil))
                percentMaxHasil = Convert.ToDecimal(stringMaxHasil);

            var stringMaxBrondol = HelperService.GetConfigValue(PMSConstants.CFG_LoadingResultMaxBrondol + Loading.DIV.UNITCODE, _context);
            decimal maxBrondol = 0;
            if (!string.IsNullOrEmpty(stringMaxBrondol))
                maxBrondol = Convert.ToDecimal(stringMaxBrondol);

            if (HelperService.GetConfigValue(PMSConstants.CfgLoadingAttendanceCheck + Loading.DIV.UNITCODE, _context) == PMSConstants.CfgLoadingAttendanceCheckTrue)
            {
                var attValid = HelperService.GetConfigValue(PMSConstants.CfgLoadingAttendanceCheckValid + Loading.DIV.UNITCODE, _context) == PMSConstants.CfgLoadingAttendanceCheckValidTrue;
                foreach (var employee in Loading.TLOADINGEMPLOYEE)
                {
                    int isExist = _attendanceService.CheckAttendance(employee.EMPLOYEE.UNITCODE, employee.EMPLOYEEID,
                        Loading.LOADINGDATE, attValid ? "K" : string.Empty, Loading.MACTIVITY.RFID);
                    if (isExist == 0)
                        throw new Exception("Absensi karyawan " + employee.EMPLOYEEID + " tanggal " +
                                            Loading.LOADINGDATE.ToString("dd/MM/yyyy") + " tidak valid.");
                }

                foreach (var employee in Loading.TLOADINGDRIVER)
                {
                    int isExist = _attendanceService.CheckAttendance(employee.DRIVER.UNITCODE, employee.DRIVERID,
                        Loading.LOADINGDATE, attValid ? "K" : string.Empty, Loading.MACTIVITY.RFID);
                    if (isExist == 0)
                        throw new Exception("Absensi Driver/Operator " + employee.DRIVERID + " tanggal " +
                                            Loading.LOADINGDATE.ToString("dd/MM/yyyy") + " tidak valid.");
                }
                _attendanceService.CloseConnection();
            }

            string msgEmployee = string.Empty;

            foreach (var employee in Loading.TLOADINGEMPLOYEE)
            {
                string result = string.Empty;
                var list = _attendanceService.GetByEmployeeAndDate(employee.EMPLOYEEID, Loading.LOADINGDATE.Date);
                if (list != null)
                {
                    var c = from hk in list select hk.HK;
                    if (c.Sum() + employee.VALUE > 1)
                        result += "Absensi Karyawan " + employee.EMPLOYEEID + " Tidak boleh  lebih dari 1 HK per hari (Absensi Saat ini = " + c.Sum() + ")";
                }

                if (!string.IsNullOrEmpty(result))
                    throw new Exception(result);

                bool allowWork2Div = HelperService.GetConfigValue(PMSConstants.CfgAtendanceWork2Division + Loading.DIV.UNITCODE, _context) == PMSConstants.CfgAtendanceWork2DivisionTrue;
                if (!allowWork2Div)
                {
                    var otherLoc = _attendanceService.GetOtherLocation(Loading.LOADINGDATE, employee.EMPLOYEEID, Loading.DIVID);
                    if (!string.IsNullOrEmpty(otherLoc) && otherLoc != Loading.DIVID)
                        throw new Exception("Karyawan " + employee.EMPLOYEEID + " sudah bekerja di divisi " + otherLoc);
                }

            }

            foreach (var employee in Loading.TLOADINGDRIVER)
            {
                string result = string.Empty;
                var list = _attendanceService.GetByEmployeeAndDate(employee.DRIVERID, Loading.LOADINGDATE.Date);
                if (list != null)
                {
                    var c = from hk in list select hk.HK;
                    if (c.Sum() + employee.VALUE > 1)
                        result += "Absensi Driver/Operator " + employee.DRIVERID + " Tidak boleh  lebih dari 1 HK per hari (Absensi Saat ini = " + c.Sum() + ")";
                }

                if (!string.IsNullOrEmpty(result))
                    throw new Exception(result);

                bool allowWork2Div = HelperService.GetConfigValue(PMSConstants.CfgAtendanceWork2Division + Loading.DIV.UNITCODE, _context) == PMSConstants.CfgAtendanceWork2DivisionTrue;
                if (!allowWork2Div)
                {
                    var otherLoc = _attendanceService.GetOtherLocation(Loading.LOADINGDATE, employee.DRIVERID, Loading.DIVID);
                    if (!string.IsNullOrEmpty(otherLoc) && otherLoc != Loading.DIVID)
                        throw new Exception("Driver/Operator " + employee.DRIVERID + " sudah bekerja di divisi " + otherLoc);
                }
            }

            if (!string.IsNullOrEmpty(msgEmployee))
                throw new Exception(msgEmployee);

        }

        private void CancelValidate(TLOADING Loading)
        {
            if (Loading.STATUS != "A")
                throw new Exception("Data belum di approve.");
            if (Loading.STATUS == "C")
                throw new Exception("Data sudah di cancel.");
            _periodService.CheckValidPeriod(Loading.DIV.UNITCODE, Loading.LOADINGDATE);
        }

        public int CheckActivity(string divId, string activityID, string NoSPB, DateTime date)
        {
            return _context.TLOADING.Where(d => d.STATUS.Equals("A") && d.STATUS.Equals("P") && d.LOADINGDATE.Date == date.Date && d.DIVID.Equals(divId) && d.ACTIVITYID.Equals(activityID) && d.NOSPB.Equals(NoSPB)).Count();
        }

        public int GetApprovedData(string unitCode, DateTime startDate, DateTime endDate)
        {
            return
            (
            from a in _context.TLOADING
            join b in _context.MDIVISI on a.DIVID equals b.DIVID
            into temp
            from t in temp.DefaultIfEmpty()
            where a.STATUS == "A" && t.DIVID.Equals(unitCode) &&
            (a.LOADINGDATE.Date >= startDate.Date && a.LOADINGDATE.Date <= endDate.Date)
            select a
            ).Count();           
        }

        public override IEnumerable<TLOADING> GetList(FilterLoading filter)
        {
        
            var criteria = PredicateBuilder.True<TLOADING>();

                criteria = criteria.And(d => !d.STATUS.Equals("D"));

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.LOADINGCODE.ToLower().Contains(filter.LowerCasedSearchTerm) 
                    );
                }

            

            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.DIVID.StartsWith(filter.UnitID));

                if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                    criteria = criteria.And(p => p.DIVID.Equals(filter.DivisionID));

                if (!string.IsNullOrWhiteSpace(filter.LoadingCode))
                    criteria = criteria.And(p => p.LOADINGCODE.Equals(filter.LoadingCode));

                if (filter.LoadingType != null)
                    criteria = criteria.And(p => p.LOADINGTYPE.Equals(filter.LoadingType));

                if (filter.SPBDataType != null)
                    criteria = criteria.And(p => p.SPBDATATYPE.Equals(filter.SPBDataType));

                if (!string.IsNullOrWhiteSpace(filter.NoSPB))
                    criteria = criteria.And(p => p.NOSPB.Contains(filter.NoSPB));

                if (!string.IsNullOrWhiteSpace(filter.VehicleId))
                    criteria = criteria.And(p => p.VEHICLEID.Contains(filter.VehicleId));

                if (!string.IsNullOrWhiteSpace(filter.VehicleTypeId))
                    criteria = criteria.And(p => p.VEHICLETYPEID.Contains(filter.VehicleTypeId));

                if (filter.LoadingPaymentType != null)
                    criteria = criteria.And(p => p.LOADINGPAYMENTTYPE.Equals(filter.LoadingPaymentType));

                if (!string.IsNullOrWhiteSpace(filter.ActivityId))
                    criteria = criteria.And(p => p.ACTIVITYID.Contains(filter.ActivityId));

                if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.LOADINGDATE.Date >= filter.StartDate.Date && p.LOADINGDATE.Date <= filter.EndDate.Date);

                if (filter.PageSize <= 0)
                    return _context.TLOADING.Where(criteria).ToList();

                return _context.TLOADING.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        private void CalculateLoading(TLOADING Loading)
        {
            var newCollection = new List<TLOADINGCOLLECT>();
            foreach (var collection in Loading.TLOADINGCOLLECT)
            {
                var b = from blk in Loading.TLOADINGBLOCK where blk.BLOCKID == collection.BLOCKID select blk;
                var e = from emp in Loading.TLOADINGEMPLOYEE where emp.EMPLOYEEID == collection.EMPLOYEEID select emp;
                if (b.Count() > 0 && e.Count() > 0)
                    newCollection.Add(collection);
            }

            var newBlock = new List<TLOADINGBLOCK>();
            foreach (var employee in Loading.TLOADINGEMPLOYEE)
            {
                foreach (var block in Loading.TLOADINGBLOCK)
                {
                    var c = from coll in Loading.TLOADINGCOLLECT
                            where coll.EMPLOYEEID == employee.EMPLOYEEID && coll.BLOCKID == block.BLOCKID
                            select coll.QTY;

                    var k = from coll in Loading.TLOADINGCOLLECT
                            where coll.EMPLOYEEID == employee.EMPLOYEEID && coll.BLOCKID == block.BLOCKID
                            select coll.QTYKG;

                    var blk = new TLOADINGBLOCK
                    {
                        EMPLOYEEID = employee.EMPLOYEEID,
                        BLOCK = block.BLOCK,
                        BLOCKID = block.BLOCKID,
                        KG = block.KG,
                        QTY = decimal.Round(c.Sum(), 4),
                        QTYKG = k.Sum()
                    };
                    newBlock.Add(blk);
                }
            }

            foreach (var employee in Loading.TLOADINGEMPLOYEE)
            {
                decimal hkUsed = 0;
                int i = 1;
                var empBlock = from emp in newBlock where emp.EMPLOYEEID == employee.EMPLOYEEID select emp;
                foreach (var block in empBlock.ToList())
                {
                    decimal hkPerBlock;
                    if (i != empBlock.Count())
                    {
                        var q = from blk in newBlock where blk.EMPLOYEEID == block.EMPLOYEEID select blk.QTY;
                        decimal qty = q.Sum();

                        hkPerBlock = Decimal.Round(employee.VALUE * (block.QTY / qty), 3);
                        hkUsed += hkPerBlock;
                    }
                    else
                        hkPerBlock = employee.VALUE - hkUsed;

                    block.VALUE = hkPerBlock;
                    i++;
                }
            }

            //Operator Only
            if (Loading.TLOADINGEMPLOYEE.Count == 0)
            {
                foreach (var block in Loading.TLOADINGBLOCK)
                {
                    var blk = new TLOADINGBLOCK
                    {
                        BLOCK = block.BLOCK,
                        KG = block.KG,
                        QTY = block.KG,
                        QTYKG = block.KG
                    };
                    newBlock.Add(blk);
                }
            }

            newBlock.RemoveAll(b => b.QTY == 0 && b.VALUE == 0 && b.QTYKG == 0);
            Loading.TLOADINGBLOCK.Clear();
            foreach(TLOADINGBLOCK item in newBlock)
            {
                Loading.TLOADINGBLOCK.Add(item);
            }

            Loading.TLOADINGCOLLECT.Clear();
            foreach (TLOADINGCOLLECT item in newCollection)
            {
                Loading.TLOADINGCOLLECT.Add(item);
            }

        }

        private void CalculateSPB(TLOADING Loading)
        {

            var q = from emp in Loading.TLOADINGEMPLOYEE select emp;
            decimal empcount = q.Count();

            Loading.TLOADINGCOLLECT = new List<TLOADINGCOLLECT>();
            foreach (var LoadingBlock in Loading.TLOADINGBLOCK)
            {

                foreach (var LoadingEmployee in Loading.TLOADINGEMPLOYEE)
                {

                    Loading.TLOADINGCOLLECT.Add(new TLOADINGCOLLECT
                    {
                        EMPLOYEEID = LoadingEmployee.EMPLOYEEID,
                        BLOCKID = LoadingBlock.BLOCKID,
                        QTYKG = LoadingBlock.KG / empcount,
                        QTY = LoadingBlock.KG / empcount,
                        COLLPOINT = "-",
                    });

                }
            }

        }

        private void CalculateKg(TLOADING Loading)
        {
            foreach (var hvBlock in Loading.TLOADINGBLOCK)
            {
                var q = from coll in Loading.TLOADINGCOLLECT
                        where coll.BLOCKID == hvBlock.BLOCKID
                        select coll.QTY;
                decimal qty = q.Sum();

                var empColl = (from coll in Loading.TLOADINGCOLLECT
                               where coll.BLOCKID == hvBlock.BLOCKID
                               group coll by new { coll.EMPLOYEEID }
                                   into grp
                               select new
                               {
                                   grp.Key.EMPLOYEEID,
                                   Quantity = grp.Sum(r => r.QTY),
                               }).ToList();

                decimal kgUsed = 0;
                int i = 1;
                foreach (var hvColl in empColl)
                {
                    var r = from coll in Loading.TLOADINGCOLLECT
                            where coll.BLOCKID == hvBlock.BLOCKID
                            && coll.EMPLOYEEID == hvColl.EMPLOYEEID
                            select coll.QTY;
                    decimal qtyEmp = r.Sum();

                    decimal kgEmp;
                    if (i != empColl.Count())
                    {
                        kgEmp = Decimal.Round(hvBlock.KG * (hvColl.Quantity / qty), 2);
                        kgUsed += kgEmp;
                    }
                    else
                        kgEmp = hvBlock.KG - kgUsed;

                    var hvtColls = (from coll in Loading.TLOADINGCOLLECT
                                    where coll.BLOCKID == hvBlock.BLOCKID
                                    && coll.EMPLOYEEID == hvColl.EMPLOYEEID
                                    select coll).ToList();

                    decimal kgEmpUsed = 0;
                    int j = 1;
                    foreach (var coll in hvtColls)
                    {
                        decimal kgTph;
                        if (j != hvtColls.Count())
                        {
                            kgTph = Decimal.Round(kgEmp * (coll.QTY / qtyEmp), 2);
                            kgEmpUsed += kgTph;
                        }
                        else
                            kgTph = kgEmp - kgEmpUsed;

                        coll.QTYKG = kgTph;
                        j++;
                    }

                    i++;
                }
            }
        }

        public void CalculateProcessHK(IEnumerable<TLOADINGRESULT> LoadingResultlist, string UnitCode, DateTime date, string userName)
        {

            DateTime startDate; DateTime endDate;
            startDate = date;
            endDate = date;


            try
            {
                this.ValidateProcess(UnitCode, date);

                _periodService.CheckValidPeriod(UnitCode, date.Date);
                //PMSServices.LoadingResult.DeleteByUnitDate(UnitCode, date);

                var data = GetList(new FilterLoading { UnitID = UnitCode, Date = date, SPBDataType = -1, RecordStatus = "P" });
                foreach (var Row in data)
                {
                    var loading = GetSingle(Row.LOADINGCODE);                   
                    SaveUpdate(loading, userName);
                }

                var EmpHK = (from coll in LoadingResultlist
                             group coll by new { coll.EMPLOYEEID }
                                 into grp
                             select new
                             {
                                 grp.Key.EMPLOYEEID,
                                 Date = grp.Select(r => r.LOADINGDATE),
                                 HK = grp.Sum(r => r.HASILPANEN) / grp.Sum(r => r.NEWBASIS1) > 1 ? 1 : grp.Sum(r => r.HASILPANEN) / grp.Sum(r => r.NEWBASIS1),
                             }).ToList();

                var LoadingEmployee1 =  (
                                        from a in _context.TLOADING
                                        join b in _context.TLOADINGEMPLOYEE on a.LOADINGCODE equals b.LOADINGCODE
                                        into temp
                                        from t in temp.DefaultIfEmpty()
                                        where a.STATUS == "P" && a.DIVID.StartsWith(UnitCode) && a.LOADINGDATE.Date == date
                                        select t
                                        ).ToList();

                foreach (var emp in LoadingEmployee1)
                {
                    var x =
                        from x1 in EmpHK
                        where x1.EMPLOYEEID == emp.EMPLOYEEID
                        select x1.HK;
                    decimal hk = x.Sum();
                    if (hk > 1) hk = 1;

                    var y =
                    from y1 in LoadingEmployee1
                    where y1.EMPLOYEEID == emp.EMPLOYEEID
                    select y1.EMPLOYEEID;
                    decimal empcount = y.Count();

                    emp.VALUE = Math.Round(hk / empcount, 3);
                    emp.VALUECALC = Math.Round(hk / empcount, 3);

                }

                foreach (var Row in data)
                {
                    var loading = GetSingle(Row.LOADINGCODE);

                    foreach (var x2 in loading.TLOADINGEMPLOYEE)
                    {
                        foreach (var A2 in LoadingEmployee1)
                        {
                            if (x2.LOADINGCODE == A2.LOADINGCODE && x2.EMPLOYEEID == A2.EMPLOYEEID)
                                x2.VALUE = A2.VALUE;
                            x2.VALUECALC = A2.VALUECALC;
                        }

                        if (!string.IsNullOrEmpty(x2.LOADINGCODE))
                        {
                            //Update LoadingEmployee
                            _context.Entry(x2).State = EntityState.Modified;
                            _context.SaveChanges();
                        }
                    }

                }


                foreach (var i in LoadingResultlist)
                {
                    var loadingresultlistNew = new TLOADINGRESULT
                    {
                        LOADINGDATE = i.LOADINGDATE,
                        EMPLOYEEID = i.EMPLOYEEID,
                        BLOCKID = i.BLOCKID,
                        BASISGROUP = i.BASISGROUP,
                        DIVID = i.DIVID,
                        KRANIID = i.KRANIID,
                        LOADINGTYPE = i.LOADINGTYPE,
                        VEHICLEID = i.VEHICLEID,
                        VEHICLETYPEID = i.VEHICLETYPEID,
                        EMPLOYEETYPE = i.EMPLOYEETYPE,
                        BASE1 = i.BASE1,
                        BASE2 = i.BASE2,
                        BASE3 = i.BASE3,
                        FRIDAY1 = i.FRIDAY1,
                        FRIDAY2 = i.FRIDAY2,
                        FRIDAY3 = i.FRIDAY3,
                        PREMI1 = i.PREMI1,
                        PREMI2 = i.PREMI2,
                        PREMI3 = i.PREMI3,
                        EXCEED1 = i.EXCEED1,
                        EXCEED2 = i.EXCEED2,
                        EXCEED3 = i.EXCEED3,
                        QTYJJG = i.QTYJJG,
                        QTYKG = i.QTYKG,
                        HASILPANEN = i.HASILPANEN,
                        TOTALPANEN = i.TOTALPANEN,
                        PCTBASIS1 = i.PCTBASIS1,
                        PCTBASIS2 = i.PCTBASIS2,
                        PCTBASIS3 = i.PCTBASIS3,
                        TOTALPCTBASIS1 = i.TOTALPCTBASIS1,
                        TOTALPCTBASIS2 = i.TOTALPCTBASIS2,
                        TOTALPCTBASIS3 = i.TOTALPCTBASIS3,

                        NEWBASISPCT1 = i.NEWBASISPCT1,
                        NEWBASIS1 = i.NEWBASIS1,
                        NEWPREMISIAP1 = i.NEWPREMISIAP1,
                        NEWBASISPCT2 = i.NEWBASISPCT2,
                        NEWBASIS2 = i.NEWBASIS2,
                        NEWPREMISIAP2 = i.NEWPREMISIAP2,
                        NEWBASISPCT3 = i.NEWBASISPCT3,
                        NEWBASIS3 = i.NEWBASIS3,
                        NEWPREMISIAP3 = i.NEWPREMISIAP3,

                        NEWHASIL1 = i.NEWHASIL1,
                        NEWPREMILEBIH1 = i.NEWPREMILEBIH1,
                        NEWINCENTIVE1 = i.NEWINCENTIVE1,
                        NEWHASIL2 = i.NEWHASIL2,
                        NEWPREMILEBIH2 = i.NEWPREMILEBIH2,
                        NEWINCENTIVE2 = i.NEWINCENTIVE2,
                        NEWHASIL3 = i.NEWHASIL3,
                        NEWPREMILEBIH3 = i.NEWPREMILEBIH3,
                        NEWINCENTIVE3 = i.NEWINCENTIVE3,
                        NEWHASIL4 = i.NEWHASIL4,
                        NEWPREMILEBIH4 = i.NEWPREMILEBIH4,
                        NEWINCENTIVE4 = i.NEWINCENTIVE4,
                        NEWHASIL5 = i.NEWHASIL5,
                        NEWPREMILEBIH5 = i.NEWPREMILEBIH5,
                        NEWINCENTIVE5 = i.NEWINCENTIVE5,
                        NEWHASIL6 = i.NEWHASIL6,
                        NEWPREMILEBIH6 = i.NEWPREMILEBIH6,
                        NEWINCENTIVE6 = i.NEWINCENTIVE6,

                        ACTIVITYID = i.ACTIVITYID,
                        UNITCODE = i.UNITCODE,
                        PAYMENTNO = i.PAYMENTNO,

                    };
                    //PMSServices.LoadingResult.Insert(loadingresultlistNew);
                }

            }
            catch
            {
                throw;
            }

        }

        public string GenereteNewNumber(string divisionId, DateTime dateTime)
        {
            VDIVISI vdivisi = _context.VDIVISI.Find(divisionId);

            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.LoadingCodePrefix + divisionId, _context);
            return PMSConstants.LoadingCodePrefix + "-" + vdivisi.CODE  + "-" + vdivisi.UNITCODE
                + "-" + dateTime.ToString("yyyyMMdd") + "-" + lastNumber.ToString().PadLeft(4, '0');
        }

        private string GenereteNewDerivedNumber(string code)
        {
            string newCode;
            string seq = code.Substring(code.Length - 5);

            if (seq.StartsWith("-"))
                newCode = code + "A";
            else
            {
                string lastChar = seq.Substring(seq.Length - 1);
                string newChar = ((char)(Convert.ToInt32(lastChar.ToCharArray()[0]) + 1)).ToString();
                newCode = code.Substring(0, code.Length - 1) + newChar;
            }

            return newCode;
        }

        protected override TLOADING BeforeSave(TLOADING record, string userName, bool newRecord)
        {
            if (record.SPBDATATYPE == 0)
                record.NOSPB = string.Empty;

            //if (record.ACTIVITYID.Any())
            //{
            //    var act = _activityService.GetList(new FilterActivity { Id = record.ACTIVITYID }).SingleOrDefault();
            //    record.LOADINGTYPE = act.HVTYPE;
            //}

            string docId = record.LOADINGCODE;
            LoadingSPB_old = new TLOADING();
            if (!newRecord)            
                LoadingSPB_old = GetSingle(record.LOADINGCODE);
            
            AddDetail(record);

            var originalBlocks = new List<TLOADINGBLOCK>();
            originalBlocks.AddRange(record.TLOADINGBLOCK);

            var originalEmployees = new List<TLOADINGEMPLOYEE>();
            originalEmployees.AddRange(record.TLOADINGEMPLOYEE);

            var originalCollection = new List<TLOADINGCOLLECT>();
            originalCollection.AddRange(record.TLOADINGCOLLECT);

            var originalDrivers = new List<TLOADINGDRIVER>();
            originalDrivers.AddRange(record.TLOADINGDRIVER);
            record.REMARK = StandardUtility.IsNull(record.REMARK, string.Empty);
            record.CANCELEDCOMMENT = StandardUtility.IsNull(record.CANCELEDCOMMENT, string.Empty);

            try
            {
                

                bool basisByKg = false;
                if (HelperService.GetConfigValue(PMSConstants.CFG_LoadingBasisByKg, _context)
                    == PMSConstants.CFG_LoadingBasisByKgTrue)
                    basisByKg = true;

                //SPBDataType.Otomatis
                if (record.SPBDATATYPE == 1)
                {
                    this.CalculateSPB(record);
                }
                else
                {
                    this.CalculateKg(record);
                }
                this.CalculateLoading(record);

                if (string.IsNullOrEmpty(record.LOADINGCODE))
                {
                    record.LOADINGCODE = this.GenereteNewNumber(record.DIVID, record.LOADINGDATE);
                    HelperService.IncreaseRunningNumber(PMSConstants.LoadingCodePrefix + record.DIVID, _context);
                }

                foreach (var item in record.TLOADINGBLOCK)
                { item.LOADINGCODE = record.LOADINGCODE; }

                foreach (var item in record.TLOADINGEMPLOYEE)
                { item.LOADINGCODE = record.LOADINGCODE; }

                foreach (var item in record.TLOADINGCOLLECT)
                { item.LOADINGCODE = record.LOADINGCODE; }

                foreach (var item in record.TLOADINGDRIVER)
                { item.LOADINGCODE = record.LOADINGCODE; }

                DateTime now = GetServerTime();
                if (newRecord)
                {
                    record.STATUS = "P";
                    this.InsertValidate(record);
                    record.CREATEBY = userName;
                    record.CREATED = now;
                }
                else
                    UpdateValidate(record);

                record.UPDATEBY = userName;
                record.UPDATED = now;
                

                _saveDetails = record.TLOADINGBLOCK.Any();
                RemoveDetail(record);

                return record;

            }
            catch
            {
                record.LOADINGCODE = string.Empty;

                record.TLOADINGBLOCK.Clear();
                foreach (var item in originalBlocks)
                { record.TLOADINGBLOCK.Add(item); }

                record.TLOADINGEMPLOYEE.Clear();
                foreach (var item in originalEmployees)
                { record.TLOADINGEMPLOYEE.Add(item); }

                record.TLOADINGDRIVER.Clear();
                foreach (var item in originalDrivers)
                { record.TLOADINGDRIVER.Add(item); }


                throw;
            }



            return record;
        }

        

       
        

        protected override TLOADING AfterSave(TLOADING record, string userName,bool newRecord)
        {
            //Update SPB Old Process

            List<THARVESTBLOCKRESULT> resultsOld =
            (
            from p in _context.THARVESTBLOCKRESULT
           .Where(d => d.NOSPB.Equals(LoadingSPB_old.NOSPB) && d.BLOCKID.StartsWith(LoadingSPB_old.DIVID))
            select p
            ).ToList();
            foreach (THARVESTBLOCKRESULT p in resultsOld)
            {
                p.STATUS = "P";
                Security.Audit.Insert(userName, "Update HarvestingBlockResult", GetServerTime(), $"After Save By Loading {p.ID}", _context);
            }
            _context.SaveChanges();

           

            List<THARVESTBLOCKRESULT> results =
            (
            from p in _context.THARVESTBLOCKRESULT
            .Where(d => d.NOSPB.Equals(record.NOSPB) && d.BLOCKID.StartsWith(record.DIVID))
            select p
            ).ToList();
            foreach (THARVESTBLOCKRESULT p in results)
            {
                p.STATUS = "A";
                Security.Audit.Insert(userName, "Update HarvestingBlockResult", GetServerTime(), $"After Save By Loading {p.ID}", _context);
            }
            _context.SaveChanges();

            return record;
        }

        protected override TLOADING BeforeDelete(TLOADING record, string userName)
        {
            try
            {
                var Loading = GetSingle(record.LOADINGCODE);
                this.DeleteValidate(Loading);
                _saveDetails = record.TLOADINGBLOCK.Any();
            }
            catch
            {
                throw;
            }
                return record;
        }

        protected override TLOADING AfterDelete(TLOADING record, string userName)
        {
            try
            {
                List<THARVESTBLOCKRESULT> results =
                (
                from p in
               _context.THARVESTBLOCKRESULT
               .Where(d => d.NOSPB.Equals(record.NOSPB) && d.BLOCKID.StartsWith(record.DIVID))
                select p
                ).ToList();
                foreach (THARVESTBLOCKRESULT p in results)
                {
                    p.STATUS = "P";
                    Security.Audit.Insert(userName, "Update HarvestingBlockResult", GetServerTime(), $"After Save By Loading {p.ID}", _context);
                }
                _context.SaveChanges();
            }
            catch
            { throw; }
            return record;
        }

        protected override TLOADING SaveInsertDetailsToDB(TLOADING record, string userName)
        {
            _context.TLOADINGBLOCK.AddRange(record.TLOADINGBLOCK);
            _context.TLOADINGEMPLOYEE.AddRange(record.TLOADINGEMPLOYEE);
            _context.TLOADINGCOLLECT.AddRange(record.TLOADINGCOLLECT);
            _context.TLOADINGDRIVER.AddRange(record.TLOADINGDRIVER);

            return record;
        }

        protected override TLOADING SaveUpdateDetailsToDB(TLOADING record, string userName)
        {
            DeleteDetailsFromDB(record, userName);
            return SaveInsertDetailsToDB(record, userName);
        }

        protected override bool DeleteDetailsFromDB(TLOADING record, string userName)
        {

            _context.TLOADINGBLOCK.RemoveRange(_context.TLOADINGBLOCK.Where(d => d.LOADINGCODE.Equals(record.LOADINGCODE)));
            _context.TLOADINGEMPLOYEE.RemoveRange(_context.TLOADINGEMPLOYEE.Where(d => d.LOADINGCODE.Equals(record.LOADINGCODE)));
            _context.TLOADINGCOLLECT.RemoveRange(_context.TLOADINGCOLLECT.Where(d => d.LOADINGCODE.Equals(record.LOADINGCODE)));
            _context.TLOADINGDRIVER.RemoveRange(_context.TLOADINGDRIVER.Where(d => d.LOADINGCODE.Equals(record.LOADINGCODE)));

            _context.SaveChanges();

            return true; ;
        }

        public bool Approve (IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["LOADINGCODE"];
            var loading = GetSingle(no);
            this.ApproveValidate(loading);

            loading.UPDATEBY = userName;
            loading.UPDATED = GetServerTime();
            loading.STATUS = "A";

            var asistensi = new List<TLOADINGASIS>();

             //For Pemuat
            foreach (var employee in loading.TLOADINGEMPLOYEE)
            {
                if (employee.VALUE > 0)
                {
                    var attendance = new TATTENDANCE
                    {
                        DIVID = employee.EMPLOYEE.DIVID,
                        EMPLOYEEID = employee.EMPLOYEEID,
                        DATE = loading.LOADINGDATE,
                        REMARK = string.Empty,
                        HK = employee.VALUE,
                        PRESENT = true,
                        ABSENTCODE = "K",
                        //-*Constant
                        STATUS = loading.STATUS,
                        REF = loading.LOADINGCODE,
                        AUTO = true,
                        CREATEBY = loading.CREATEBY,
                        CREATEDDATE = loading.UPDATED,
                        UPDATEBY = loading.UPDATEBY,
                        UPDATEDDATE = loading.UPDATED,
                    };
                    _attendanceService.SaveInsertOrUpdate(attendance, userName);
                }

                var empUnit = employee.EMPLOYEE.UNITCODE;
                if (empUnit != loading.DIV.UNITCODE)
                {
                    var q = from i in asistensi where i.UNITID == empUnit select i;
                    if (q.Count() == 0) asistensi.Add(new TLOADINGASIS {UNITID = empUnit });
                }
            }

             //For Operator/Driver
            foreach (var employee in loading.TLOADINGDRIVER)
            {
                if (employee.VALUE > 0)
                {
                    var attendance = new TATTENDANCE
                    {
                        DIVID = employee.DRIVER.DIVID,
                        EMPLOYEEID = employee.DRIVERID,
                        DATE = loading.LOADINGDATE,
                        REMARK = string.Empty,
                        HK = Convert.ToDecimal(employee.VALUE),
                        PRESENT = true,
                        ABSENTCODE = "K",
                        //-*Constant
                        STATUS = loading.STATUS,
                        REF = loading.LOADINGCODE,
                        AUTO = true,
                        CREATEBY = loading.UPDATEBY,
                        CREATEDDATE = loading.UPDATED,
                        UPDATEBY = loading.UPDATEBY,
                        UPDATEDDATE = loading.UPDATED,
                    };
                    _attendanceService.SaveInsertOrUpdate(attendance, userName);
                }

                var empUnit = employee.DRIVER.UNITCODE;
                if (empUnit != loading.DIV.UNITCODE && asistensi.Count == 0)
                {
                    var q = from i in asistensi where i.UNITID == empUnit select i;
                    if (q.Count() == 0) asistensi.Add(new TLOADINGASIS { UNITID = empUnit });
                }
            }

            _context.Entry(loading).State = EntityState.Modified;
            _context.SaveChanges();

            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Approve {loading.LOADINGCODE}", _context);


            return true;
        }

        public bool CancelApprove(IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["LOADINGCODE"];
            string canceledcomment = formDataCollection["CANCELEDCOMMENT"];
            var rfcResult = string.Empty;
            try
            {
                var loading = GetSingle(no);
                this.CancelValidate(loading);

                var newLoading = new TLOADING
                {
                    DIV = loading.DIV,
                    DIVID = loading.DIVID,
                    PRODUCTID = loading.PRODUCTID,
                    LOADINGDATE = loading.LOADINGDATE,
                    ACTIVITYID = loading.ACTIVITYID,
                    LOADINGPAYMENTTYPE = loading.LOADINGPAYMENTTYPE,
                    VEHICLEID = loading.VEHICLEID,
                    VEHICLETYPEID = loading.VEHICLETYPEID,
                    DESTINATIONID = loading.DESTINATIONID,
                    NOSPB = loading.NOSPB,
                    SPBDATATYPE = loading.SPBDATATYPE,
                    DRIVERID = loading.DRIVERID,
                    ACTIVITYDRIVERID = loading.ACTIVITYDRIVERID,
                    KRANIID = loading.KRANIID,
                    REMARK = loading.REMARK,
                    STATUS = "P",
                    LOADINGTYPE = loading.LOADINGTYPE,
                    LOADINGCODE = this.GenereteNewDerivedNumber(loading.LOADINGCODE),
                    CREATEBY = loading.UPDATEBY,
                    CREATED = loading.UPDATED,
                    UPDATEBY = loading.UPDATEBY,
                    UPDATED = loading.UPDATED,
                    UPLOAD = 0,
                    TLOADINGEMPLOYEE = loading.TLOADINGEMPLOYEE,
                    //TLOADINGBLOCK = loading.TLOADINGBLOCK,
                    TLOADINGBLOCK = LoadingBlockGetGroupingByCode(loading.LOADINGCODE),
                    TLOADINGCOLLECT = loading.TLOADINGCOLLECT,
                    TLOADINGDRIVER = loading.TLOADINGDRIVER,                   
                };

                newLoading.MACTIVITY = loading.MACTIVITY;

                _attendanceService.DeleteByReferences(loading.LOADINGCODE);

                var local = _context.Set<TLOADING>()
                .Local
                .SingleOrDefault(entry => entry.LOADINGCODE.Equals(newLoading.LOADINGCODE) || entry.Equals(loading.LOADINGCODE));
                if (local != null) { _context.Entry(local).State = EntityState.Detached; }

                SaveInsert(newLoading, userName);

                TLOADING loadingcancel = _context.TLOADING.Where(d => d.LOADINGCODE.Equals(no)).SingleOrDefault();
                loadingcancel.CANCELEDCOMMENT = canceledcomment;
                loadingcancel.UPDATEBY = userName;
                loadingcancel.UPDATED = GetServerTime();
                loadingcancel.STATUS = "C";
                //loadingcancel.UPLOAD = 0;

                _context.Entry(loadingcancel).State = EntityState.Modified;
                _context.SaveChanges();
                Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Cancel {loadingcancel.LOADINGCODE}", _context);

            }
            catch
            { throw;}

            return true;
        }

        public List<TLOADINGBLOCK> LoadingBlockGetGroupingByCode(string loadingCode)
        {
            List<TLOADINGBLOCK> hrvests = new List<TLOADINGBLOCK>();

            var data = (
                       from p in _context.TLOADINGBLOCK
                       group p by new { p.LOADINGCODE, p.BLOCKID,p.KG }
                       into grp
                       select new
                       {
                           grp.Key.LOADINGCODE, grp.Key.BLOCKID,
                           EMPLOYEEID = 0, VALUE=0, QTY=0, QTYKG=0, grp.Key.KG
                       })
                       .Where(d=> d.LOADINGCODE.Equals(loadingCode))
                       .ToList();

            TLOADINGBLOCK loadingBlock;
            foreach(var item in data)
            {
                loadingBlock = new TLOADINGBLOCK();
                loadingBlock.CopyFrom(item);
                hrvests.Add(loadingBlock);
                _context.Entry(loadingBlock).State = EntityState.Detached;
            }
            return hrvests;

        }

        public override TLOADING NewRecord(string userName)
        {
            TLOADING record = new TLOADING();
            record.LOADINGDATE = GetServerTime().Date;
            record.STATUS = "";
            return record;
        }

    }
}
