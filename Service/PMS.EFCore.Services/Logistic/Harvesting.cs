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
using PMS.EFCore.Services.Location;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;
using Newtonsoft.Json;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.Payroll;
using PMS.EFCore.Services.Attendances;
using PMS.EFCore.Services.Organization;
using AM.EFCore.Services;
using PMS.Shared.Models;

namespace PMS.EFCore.Services.Logistic
{
    public class Harvesting : EntityFactory<THARVEST, THARVEST, FilterHarvest, PMSContextBase>
    {
        private Divisi _divisiService;
        private PremiPanen _premiPanenService;
        private Employee _employeeService;
        private TransaksiAbsensi _absensiService;
        private Block _blockService;
        private Period _periodService;
        private Activity _activityService;
        private Attendance _attendanceService;
        private AuthenticationServiceBase _authenticationService;

        private bool _newRecordAfterCancel = false;

        private MACTIVITY _activity;
        private VDIVISI _divisi;
        private bool _autoHK = false;
        private bool _calcKg = true;
        
        public Harvesting(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Harvesting";
            _authenticationService = authenticationService;
            _divisiService = new Divisi(_context,_authenticationService, auditContext);
            _premiPanenService = new PremiPanen(_context,_authenticationService,auditContext);
            _employeeService = new Employee(_context,_authenticationService,auditContext);
            _absensiService = new TransaksiAbsensi(_context,_authenticationService, auditContext);
            _blockService = new Block(_context,_authenticationService, auditContext);
            _activityService = new Activity(_context,_authenticationService,auditContext);
            _attendanceService = new Attendance(_context,_authenticationService, auditContext);
            _periodService = new Period(_context,_authenticationService, auditContext);

        }

        protected override THARVEST GetSingleFromDB(params  object[] keyValues)
        {
            string harvestCode = keyValues[0] as string;
            THARVEST record = _context.THARVEST               
                .Include(d => d.THARVESTBASE)                
                .SingleOrDefault(d => d.HARVESTCODE.Equals(harvestCode));
            if (record != null)
            {
                _context.Entry<THARVEST>(record).State = EntityState.Detached;
               

                var harvestEmployee =
                (
                    from a in _context.THARVESTEMPLOYEE.Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                    join b in _context.THARVESTCOLLECT
                        .Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                        .GroupBy(d => d.EMPLOYEEID)
                        .Select(d => new { EMPLOYEEID = d.Key, QTY = d.Sum(s => s.QTY), QTYKG = d.Sum(s => s.QTYKG) }) on a.EMPLOYEEID equals b.EMPLOYEEID into b1
                    from b2 in b1.DefaultIfEmpty()
                    join c in _context.THARVESTFINE
                        .Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                        .GroupBy(d => d.EMPLOYEEID)
                        .Select(d => new { EMPLOYEEID = d.Key, QTYFINE = d.Sum(s => s.QTY) }) on a.EMPLOYEEID equals c.EMPLOYEEID into c1
                    from c2 in c1.DefaultIfEmpty()
                    join d in _context.VEMPLOYEE on a.EMPLOYEEID equals d.EMPID
                    join e in _context.VEMPLOYEE on a.GEMPID equals e.EMPID into e1
                    from e2 in e1.DefaultIfEmpty()

                    select new
                    {
                        EMP = a,
                        QTY = (b2 == null) ? 0 : b2.QTY,
                        QTYKG = (b2 == null) ? 0 : b2.QTYKG,
                        QTYFINE = (c2 == null) ? 0 : c2.QTYFINE,
                        EMPNAME = d.EMPID + " - " + d.EMPNAME,
                        d.EMPTYPE,
                        d.UNITNAME,
                        GEMPNAME =  string.IsNullOrEmpty(a.GEMPID)? string.Empty : e2.EMPID + " - " + e2.EMPNAME
                        //e2 == null ? string.Empty : e.EMPID + " - " + e.EMPNAME

                    }
                 ).ToList();

                harvestEmployee.ForEach(d => 
                 {
                     d.EMP.QTY = d.QTY;
                     d.EMP.QTYKG = d.QTYKG;
                     d.EMP.QTYFINE = d.QTYFINE;
                     d.EMP.EMPNAME = d.EMPNAME;
                     d.EMP.EMPTYPE = d.EMPTYPE;
                     d.EMP.GEMPNAME = d.GEMPNAME;
                     d.EMP.UNITNAME = d.UNITNAME;
                 });

                record.THARVESTEMPLOYEE = harvestEmployee.Select(d => d.EMP).ToList();

                var hvtBlock =
                (
                    from a in _context.THARVESTBLOCK.Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                    join c in _context.THARVESTFINE
                        .Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                        .GroupBy(d => new { d.EMPLOYEEID, d.BLOCKID })
                        .Select(d => new { EMPLOYEEID = d.Key.EMPLOYEEID, BLOCKID = d.Key.BLOCKID, QTYFINE = d.Sum(s => s.QTY) }) on new { a.EMPLOYEEID, a.BLOCKID } equals new { c.EMPLOYEEID, c.BLOCKID } into c1
                    from c2 in c1.DefaultIfEmpty()
                    join d in _context.VBLOCK on a.BLOCKID equals d.BLOCKID
                    join e in _context.MEMPLOYEE on a.EMPLOYEEID equals e.EMPID

                    select new
                    {
                        BLOCK = a,
                        BLOCKCODE = d.CODE,
                        EMPNAME = e.EMPID + " - " + e.EMPNAME,
                        QTYFINE = c2 == null ? 0 : c2.QTYFINE
                    }
                ).ToList();

                hvtBlock.ForEach(d => {
                    d.BLOCK.BLOCKCODE = d.BLOCKCODE;
                    d.BLOCK.EMPNAME = d.EMPNAME;
                    d.BLOCK.QTYFINE = d.QTYFINE;
                });

                record.THARVESTBLOCK = hvtBlock.Select(d => d.BLOCK).ToList();


                record.VHARVESTBLOCK =
                (
                    from a in _context.THARVESTBLOCK
                        .Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                        .GroupBy(d => d.BLOCKID)
                        .Select(d => new { BLOCKID = d.Key, HARVESTAREA = d.Average(s => s.HARVESTAREA), KG = d.Average(s => s.KG) })
                    join b in _context.THARVESTCOLLECT
                        .Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                        .GroupBy(d => d.BLOCKID)
                        .Select(d => new { BLOCKID = d.Key, QTY = d.Sum(s => s.QTY) }) on a.BLOCKID equals b.BLOCKID into b1
                    from b2 in b1.DefaultIfEmpty()
                    join c in _context.THARVESTFINE
                        .Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                        .GroupBy(d => d.BLOCKID)
                        .Select(d => new { BLOCKID = d.Key, QTYFINE = d.Sum(s => s.QTY) }) on a.BLOCKID equals c.BLOCKID into c1
                    from c2 in c1.DefaultIfEmpty()
                    join d in _context.VBLOCK on a.BLOCKID equals d.BLOCKID

                    select new VHARVESTBLOCK
                    {
                        HARVESTCODE = record.HARVESTCODE,
                        BLOCKID = a.BLOCKID,
                        BLOCKCODE = d.CODE,
                        THNTANAM = d.THNTANAM,
                        LUASBLOCK = d.LUASBLOCK,
                        HARVESTAREA = a.HARVESTAREA,
                        KG = a.KG,
                        QTY = b2 == null ? 0 : b2.QTY,
                        QTYFINE = c2 == null ? 0 : c2.QTYFINE
                    }
                ).ToList();



               var harvestColl =
               (
                   from a in _context.THARVESTCOLLECT.Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                   join b in _context.MTPH on a.COLLPOINT equals b.TPHID into b1
                   from b2 in b1.DefaultIfEmpty()
                   join c in record.VHARVESTBLOCK on a.BLOCKID equals c.BLOCKID
                   join d in record.THARVESTEMPLOYEE on a.EMPLOYEEID equals d.EMPLOYEEID
                   select new { COLL = a, TPHCODE = (b2.TPHID == null) ? a.COLLPOINT : b2.CODE, EMPNAME = d.EMPNAME, BLOCKCODE = c.BLOCKCODE }
               ).ToList();

                harvestColl.ForEach(d => {
                    d.COLL.TPHCODE = d.TPHCODE;
                    d.COLL.EMPNAME = d.EMPNAME;
                    d.COLL.BLOCKCODE = d.BLOCKCODE;
                });

                record.THARVESTCOLLECT = harvestColl.Select(d => d.COLL).ToList();


               var harvestFine =
               (
                   from a in _context.THARVESTFINE.Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE))
                   join b in _context.MPENALTYTYPE on a.FINECODE equals b.PENALTYCODE
                   join c in record.VHARVESTBLOCK on a.BLOCKID equals c.BLOCKID
                   join d in record.THARVESTEMPLOYEE on a.EMPLOYEEID equals d.EMPLOYEEID
                   select new { FINE = a, FINENAME = b.DESCRIPTION, EMPNAME = d.EMPNAME, BLOCKCODE = c.BLOCKCODE }
               ).ToList();

                harvestFine.ForEach(d => {
                    d.FINE.FINENAME = d.FINENAME;
                    d.FINE.EMPNAME = d.EMPNAME;
                    d.FINE.BLOCKCODE = d.BLOCKCODE;
                });

                record.THARVESTFINE = harvestFine.Select(d => d.FINE).ToList();


            }

            return record;
        }

        

        public override IEnumerable<THARVEST> GetList(FilterHarvest filter)
        {
            var criteria = PredicateBuilder.True<THARVEST>();

            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                bool allDivision = true;
                List<string> authorizedDivisionIds = new List<string>();

                //Authorization check
                //Get Authorization By User Name

                authorizedDivisionIds = _authenticationService.GetAuthorizedDivisiByUserName(filter.UserName, string.Empty, string.Empty).Select(d => d.DIVID).ToList();
                if (!allDivision)
                    criteria = criteria.And(p => authorizedDivisionIds.Contains(p.DIVID));
            }

            //Single Unit ID
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
            {
                List<string> divisionIds = _divisiService.GetList(new GeneralFilter { UnitID = filter.UnitID }).Select(d => d.DIVID).ToList();
                criteria = criteria.And(d => divisionIds.Contains(d.DIVID));
            }

            //Multi Unit ID
            if (filter.UnitIDs.Any())
            {
                List<string> divisionIds = _divisiService.GetList(new GeneralFilter { UnitIDs = filter.UnitIDs }).Select(d => d.DIVID).ToList();
                criteria = criteria.And(d => divisionIds.Contains(d.DIVID));
            }

            //Single Division ID
            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(d => d.DIVID.Equals(d.DIVID));

            //Multi Division ID
            if (filter.DivisionIDs.Any())
                criteria = criteria.And(d => filter.DivisionIDs.Contains(d.DIVID));



            criteria = criteria.And(d =>
               (d.HARVESTDATE.Date >= filter.StartDate.Date && d.HARVESTDATE.Date <= filter.EndDate.Date));

            if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => !d.STATUS.Equals(PMSConstants.TransactionStatusDeleted) && !d.STATUS.Equals(PMSConstants.TransactionStatusCanceled));
            else
                criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));

            if (filter.HarvestType >= 0)
                criteria = criteria.And(d => d.HARVESTTYPE == filter.HarvestType);
            if (filter.HarvestTypes.Any())
                criteria = criteria.And(d => filter.HarvestTypes.Contains(d.HARVESTTYPE));

            if (filter.PaymentType >= 0)
                criteria = criteria.And(d => d.HARVESTPAYMENTTYPE == filter.PaymentType);

            if (filter.PaymentTypes.Any())
                criteria = criteria.And(d => filter.PaymentTypes.Contains(d.HARVESTPAYMENTTYPE.Value));

            if (filter.PageSize <= 0)
                return _context.THARVEST.Where(criteria).ToList();

            return _context.THARVEST.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

            
        }

        public override THARVEST CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            THARVEST record = base.CopyFromWebFormData(formData, newRecord);
            if (record != null)
            {
                record.InitEdit();             
                record.VHARVESTBLOCKEDIT.CopyFrom<VHARVESTBLOCK>(formData, "VHARVESTBLOCK");
                record.THARVESTEMPLOYEEEDIT.CopyFrom<THARVESTEMPLOYEE>(formData, "THARVESTEMPLOYEE");
                record.THARVESTCOLLECTEDIT.CopyFrom<THARVESTCOLLECT>(formData, "THARVESTCOLLECT");
                record.THARVESTFINEEDIT.CopyFrom<THARVESTFINE>(formData, "THARVESTFINE");
            }
            _calcKg = true;
            return record;
        }
        protected override THARVEST BeforeSave(THARVEST record, string userName, bool newRecord)
        {
            string result = string.Empty;
            if (!newRecord)
            {
                if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusApproved))
                    throw new Exception("Data sudah di approve.");
                if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusCanceled))
                    throw new Exception("Data sudah di cancel.");
                if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusDeleted))
                    throw new Exception("Data sudah di hapus.");
            }

            if (string.IsNullOrEmpty(record.HARVESTCODE)) result += "Nomor tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) result += "Kode Divisi tidak boleh kosong." + Environment.NewLine;
            if (record.HARVESTDATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.ACTIVITYID)) result += "Jenis Pekerjaan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.MANDORID)) result += "Nama Mandor tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.MANDOR1ID)) result += "Nama Mandor 1 tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.KRANIID)) result += "Nama Krani tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrWhiteSpace(record.STATUS)) result += "Status tidak boleh kosong." + Environment.NewLine;
            if (!string.IsNullOrWhiteSpace(result))
                throw new Exception(result);

            if (!_authenticationService.IsAuthorizedDivisi(userName, record.DIVID))
                throw new ExceptionNoDivisionAccess(record.DIVID);
            record.CANCELEDCOMMENT = StandardUtility.IsNull(record.CANCELEDCOMMENT,string.Empty);
            record.REMARK = StandardUtility.IsNull(record.REMARK, string.Empty);

            _autoHK = false;
            bool.TryParse(GetMConfigValue(PMSConstants.CFG_HarvestingEmployeeAutoHK + _divisi.UNITCODE), out _autoHK);


            VDIVISI divisi = _divisiService.GetSingle(record.DIVID);
            if (divisi == null)
                throw new Exception("Divisi tidak valid");

            _autoNumberPrefix = string.Empty;
            if (newRecord)
            {
                if (!_newRecordAfterCancel)
                {
                    _autoNumberPrefix = PMSConstants.HarvestingCodePrefix + divisi.DIVID;
                    int lastNumber = GetCurrentDocumentNumber();
                    record.HARVESTCODE = PMSConstants.HarvestingCodePrefix + "-" + divisi.CODE + "-" + divisi.UNITCODE + "-" + record.HARVESTDATE.ToString("yyyyMMdd") + "-" + lastNumber.ToString().PadLeft(4, '0');
                }
                else
                {
                    string seq = record.HARVESTCODE.Substring(record.HARVESTCODE.Length - 5);
                    if (seq.StartsWith("-"))
                        record.HARVESTCODE += "A";
                    else
                    {
                        string lastChar = seq.Substring(seq.Length - 1);
                        string newChar = ((char)(Convert.ToInt32(lastChar.ToCharArray()[0]) + 1)).ToString();
                        record.HARVESTCODE = record.HARVESTCODE.Substring(0, record.HARVESTCODE.Length - 1) + newChar;
                    }
                }
                record.STATUS = PMSConstants.TransactionStatusProcess;
                record.CREATED = GetServerTime();
                record.CREATEBY = userName;
            }
            record.UPDATED = GetServerTime();
            record.UPDATEBY = userName;
            


            return record;
        }

        

        

        protected override THARVEST BeforeDelete(THARVEST record, string userName)
        {
            if (!_authenticationService.IsAuthorizedDivisi(userName, record.DIVID))
                throw new ExceptionNoDivisionAccess(record.DIVID);
            if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusApproved))
                throw new Exception("Data sudah di approve.");
            if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusCanceled))
                throw new Exception("Data sudah di cancel.");
            if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusDeleted))
                throw new Exception("Data sudah di hapus.");

            _periodService.CheckValidPeriod(_existingRecord.DIV.UNITCODE, _existingRecord.HARVESTDATE.Date);
            return record;
        }

        protected override THARVEST SaveInsertToDB(THARVEST record, string userName)
        {
            if (_calcKg)
                CalculateKg(record);

            if (_autoHK)                
                record.THARVESTEMPLOYEEEDIT.ForEach(d => {
                    d.VALUECALC = 0;
                    d.VALUE = 0;
                    d.GVALUECALC = 0;
                    d.GVALUE = 0;
                    d.ALLOWQTY = false;
                });
            CalculateHarvesting(record);
            _saveDetails = record.THARVESTEMPLOYEEEDIT.Any() || record.THARVESTCOLLECTEDIT.Any() || record.THARVESTFINEEDIT.Any() || record.VHARVESTBLOCKEDIT.Any() || record.THARVESTBASEEDIT.Any();
            return base.SaveInsertToDB(record, userName);
        }

        protected override THARVEST SaveUpdateToDB(THARVEST record, string userName)
        {
            
            if (_autoHK)
                record.THARVESTEMPLOYEEEDIT.ForEach(d => {
                    d.VALUECALC = 0;
                    d.VALUE = 0;
                    d.GVALUECALC = 0;
                    d.GVALUE = 0;
                });
            CalculateKg(record);
            CalculateHarvesting(record);
            _saveDetails = record.THARVESTEMPLOYEEEDIT.Any() || record.THARVESTBLOCKEDIT.Any() || record.THARVESTCOLLECTEDIT.Any() || record.THARVESTFINEEDIT.Any() || record.THARVESTBASEEDIT.Any();            
            return base.SaveUpdateToDB(record, userName);
        }

        protected override THARVEST SaveUpdateDetailsToDB(THARVEST record, string userName)
        {
            _context.THARVESTFINE.RemoveRange(_context.THARVESTFINE.Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE)));
            _context.THARVESTCOLLECT.RemoveRange(_context.THARVESTCOLLECT.Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE)));
            _context.THARVESTBASE.RemoveRange(_context.THARVESTBASE.Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE)));
            _context.THARVESTBLOCK.RemoveRange(_context.THARVESTBLOCK.Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE)));
            _context.THARVESTEMPLOYEE.RemoveRange(_context.THARVESTEMPLOYEE.Where(d => d.HARVESTCODE.Equals(record.HARVESTCODE)));

            return SaveInsertDetailsToDB(record, userName);
        }

        protected override THARVEST SaveInsertDetailsToDB(THARVEST record, string userName)
        {
            record.THARVESTEMPLOYEEEDIT.ForEach(d => d.HARVESTCODE = record.HARVESTCODE);
            _context.THARVESTEMPLOYEE.AddRange(record.THARVESTEMPLOYEEEDIT);

            record.THARVESTBLOCKEDIT.ForEach(d => d.HARVESTCODE = record.HARVESTCODE);
            _context.THARVESTBLOCK.AddRange(record.THARVESTBLOCKEDIT);

            record.THARVESTCOLLECTEDIT.ForEach(d => d.HARVESTCODE = record.HARVESTCODE);
            _context.THARVESTCOLLECT.AddRange(record.THARVESTCOLLECTEDIT);

            record.THARVESTFINEEDIT.ForEach(d => d.HARVESTCODE = record.HARVESTCODE);
            _context.THARVESTFINE.AddRange(record.THARVESTFINEEDIT);

            record.THARVESTBASEEDIT.ForEach(d => d.HARVESTCODE = record.HARVESTCODE);
            _context.THARVESTBASE.AddRange(record.THARVESTBASEEDIT);
            return base.SaveInsertDetailsToDB(record, userName);
        }

        protected override bool DeleteFromDB(THARVEST record, string userName)
        {
            record.STATUS = PMSConstants.TransactionStatusDeleted;
            record.UPLOAD = 0;
            record.UPDATED = GetServerTime();
            record.UPDATEBY = userName;
            _context.Update(record);
            _context.THARVESTCOLLECT.RemoveRange(_existingRecord.THARVESTCOLLECT);
            _context.THARVESTFINE.RemoveRange(_existingRecord.THARVESTFINE);
            _context.THARVESTBLOCK.RemoveRange(_existingRecord.THARVESTBLOCK);
            _context.THARVESTEMPLOYEE.RemoveRange(_existingRecord.THARVESTEMPLOYEE);
            _context.THARVESTBASE.RemoveRange(_existingRecord.THARVESTBASE);
            if (_internalCommit)
                _context.SaveChanges();
            return true;
        }


        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string harvestCode = formDataCollection["HARVESTCODE"];
            return Approve(harvestCode, userName);
        }
        public bool Approve(string harvestCode, string userName)
        {
            if (string.IsNullOrWhiteSpace(harvestCode))
                throw new Exception("Harvest code tidak boleh kosong");
            THARVEST record = GetSingle(harvestCode);            
            return Approve(record, userName);

        }

        public bool CancelApprove(IFormCollection formDataCollection, string userName)
        {
            string harvestCode = formDataCollection["HARVESTCODE"];
            return CancelApprove(harvestCode, userName);
        }
        public bool CancelApprove(string harvestCode, string userName)
        {
            if (string.IsNullOrWhiteSpace(harvestCode))
                throw new Exception("Harvest code tidak boleh kosong");
            THARVEST record = GetSingle(harvestCode);
            return CancelApprove(record, userName);

        }

        private void Validate(THARVEST record,List<THARVESTEMPLOYEE> employees, List<THARVESTBLOCK> employeeBlocks, List<THARVESTCOLLECT> collections, List<THARVESTFINE> fines, string userName)
        {


            string errorMessage = string.Empty;
            if (string.IsNullOrEmpty(record.HARVESTCODE)) errorMessage += "Nomor tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) errorMessage += "Kode Divisi tidak boleh kosong." + Environment.NewLine;
            if (record.HARVESTDATE == new DateTime()) errorMessage += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.ACTIVITYID)) errorMessage += "Jenis Pekerjaan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.MANDORID)) errorMessage += "Nama Mandor tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.MANDOR1ID)) errorMessage += "Nama Mandor 1 tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.KRANIID)) errorMessage += "Nama Krani tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrWhiteSpace(record.STATUS)) errorMessage += "Status tidak boleh kosong." + Environment.NewLine;
            if (!string.IsNullOrWhiteSpace(errorMessage))
                throw new Exception(errorMessage);

            _divisi = _divisiService.GetSingle(record.DIVID);
            if (_divisi == null)
                throw new Exception("Divisi tidak valid");

            _periodService.CheckValidPeriod(_divisi.UNITCODE, record.HARVESTDATE.Date);

            _activity = _activityService.GetSingle(record.ACTIVITYID);
            if (_activity == null)
                throw new Exception("Kode kegiatan tidak valid");

            
            
            
            if (GetMConfigValue(PMSConstants.CFG_HarvestingEmployeeAllowEmptyHK + _divisi.UNITCODE) != PMSConstants.CFG_HarvestingEmployeeAllowEmptyHKTrue)
            {
                if (employees.Where(d=>d.VALUE<=0).Count()<=0)                
                    throw new Exception("HK harus > 0.");
            }

            if (employees == null || !employees.Any())
                throw new Exception("Karyawan tidak boleh kosong.");

            if (employeeBlocks == null || !employeeBlocks.Any())
                throw new Exception("Blok tidak boleh kosong.");

            if (collections == null || !collections.Any())
                throw new Exception("Panen tidak boleh kosong.");

            errorMessage = string.Empty;
            (
                from a in employees
                join b in collections on a.EMPLOYEEID equals b.EMPLOYEEID into ab
                from c in ab.DefaultIfEmpty()
                where c == null
                select a.EMPLOYEEID
            ).Distinct().ToList().ForEach(d => {
                errorMessage += "\r\n" + d;
            });
            if (!string.IsNullOrWhiteSpace(errorMessage))
                throw new Exception("Karyawan berikut belum tercatat melakukan panen :" + errorMessage);

            errorMessage = string.Empty;
            (
                from a in employeeBlocks
                join b in collections on a.BLOCKID equals b.BLOCKID into ab
                from c in ab.DefaultIfEmpty()
                where c == null
                select a.BLOCKID
            ).Distinct().ToList().ForEach(d => {
                errorMessage += "\r\n" + d;
            });

            
            if (!string.IsNullOrWhiteSpace(errorMessage))
                throw new Exception("Blok berikut belum tercatat dipanen :" + errorMessage);

            errorMessage = string.Empty;
            (
                from a in employeeBlocks
                join b in collections.GroupBy(d => new { d.EMPLOYEEID, d.BLOCKID }).Select(d => new { EMPLOYEEID = d.Key.EMPLOYEEID, BLOCKID = d.Key.BLOCKID, QTY = d.Sum(s => s.QTY) }) on new { a.EMPLOYEEID, a.BLOCKID } equals new { b.EMPLOYEEID, b.BLOCKID }
                join c in fines.GroupBy(d => new { d.EMPLOYEEID, d.BLOCKID }).Select(d => new { EMPLOYEEID = d.Key.EMPLOYEEID, BLOCKID = d.Key.BLOCKID, QTY = d.Sum(s => s.QTY) }) on new { a.EMPLOYEEID, a.BLOCKID } equals new { c.EMPLOYEEID, c.BLOCKID } into ab
                from d in ab.DefaultIfEmpty()
                where b.QTY<(d == null ? 0 : d.QTY)
                select new { a.EMPLOYEEID, a.BLOCKID, PANEN = b.QTY, DENDA = d == null ? 0 : d.QTY }
            ).ToList().ForEach(d => {
                errorMessage += "\r\n" + d.EMPLOYEEID + " - " + d.BLOCKID;
            });

            if (!string.IsNullOrWhiteSpace(errorMessage))
                throw new Exception("Panen berikut ini denda lebih besar dari total panen :" + errorMessage);

            errorMessage = string.Empty;
            if (record.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah)
            {
                employeeBlocks.Where(d => d.HARVESTAREA <= 0).Select(d => d.BLOCKID).Distinct().ToList().ForEach(d=> {
                    errorMessage += "\r\n" + d;
                });
            }
            if (!string.IsNullOrWhiteSpace(errorMessage))
                throw new Exception("Blok berikut luas panen 0 :" + errorMessage);
            
            errorMessage = string.Empty;

            (
                from a in employeeBlocks
                join b in _blockService.GetList(new FilterBlock { BlockIDs = employeeBlocks.Select(d => d.BLOCKID).Distinct().ToList() }) on a.BLOCKID equals b.BLOCKID
                select new { HARVESTBLOCK = a, MASTERBLOCK = b }
            ).ToList().ForEach(d=> {
                if (((d.MASTERBLOCK.PHASE == 1 && _activity.TBM == false) || _activity.TBM)
                         && ((d.MASTERBLOCK.PHASE == 5 && _activity.TM == false) || _activity.TM)
                         && ((d.MASTERBLOCK.PHASE == 9 && _activity.GA == false) || _activity.GA)
                         && ((d.MASTERBLOCK.PHASE == 8 && _activity.LC == false) || _activity.LC))
                {
                    errorMessage = "\r\n" + d.MASTERBLOCK.BLOCKID;
                }
            });
            if (!string.IsNullOrWhiteSpace(errorMessage))
                throw new Exception("Perikasa fase blok berikut:" + errorMessage);

            
        }


        public THARVEST BeforeApprove(THARVEST record,string userName)
        {
            if (!_authenticationService.IsAuthorizedDivisi(userName, record.DIVID))
                throw new ExceptionNoDivisionAccess(record.DIVID);

            _divisi = _divisiService.GetSingle(record.DIVID);
            string unitCode = _divisi.UNITCODE;
            Validate(record, record.THARVESTEMPLOYEE.ToList(), record.THARVESTBLOCK.ToList(), record.THARVESTCOLLECT.ToList(), record.THARVESTFINE.ToList(), userName);

            _periodService.CheckMaxPeriod(unitCode, record.HARVESTDATE.Date);
            

            bool basisByKg = false;
            if (GetMConfigValue(PMSConstants.CFG_HarvestingBasisByKg) == PMSConstants.CFG_HarvestingBasisByKgTrue)
                basisByKg = true;

            bool allowWork2Div = GetMConfigValue(PMSConstants.CfgAtendanceWork2Division + unitCode) == PMSConstants.CfgAtendanceWork2DivisionTrue;

            
            decimal percentMaxHasil = 0;
            decimal.TryParse(GetMConfigValue(PMSConstants.CFG_HarvestingResultMaxPercent + unitCode), out percentMaxHasil);
            

            
            decimal maxBrondol = 0;
            decimal.TryParse(GetMConfigValue(PMSConstants.CFG_HarvestingResultMaxBrondol + unitCode), out maxBrondol);
            
            
            decimal maxEmp = 0;
            decimal.TryParse(GetMConfigValue(PMSConstants.CfgHarvestingMaxEmployee + unitCode), out maxBrondol);
            
            if (maxEmp > 0 && record.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah)
            {
                int empQty = GetEmployeeCount(record.HARVESTCODE, record.HARVESTDATE, record.MANDORID);
                if (empQty > maxEmp) throw new Exception("Maksimal karyawan permandoran " + maxEmp + " orang.");
            }

            string errorMessage = string.Empty;
            List<string> employeeIds = record.THARVESTEMPLOYEE.Select(d => d.EMPLOYEEID).Distinct().ToList();

            if (GetMConfigValue(PMSConstants.CfgHarvestAttendanceCheck + unitCode) == PMSConstants.CfgHarvestAttendanceCheckTrue)
            {
                bool attValid = GetMConfigValue(PMSConstants.CfgHarvestAttendanceCheckValid + unitCode) == PMSConstants.CfgHarvestAttendanceCheckValidTrue;
                FilterAttendance filterAttendance = new FilterAttendance
                {
                    CardType = _activity.RFID,
                    RecordStatus = attValid ? "K" : string.Empty,
                    Ids = employeeIds,
                    StartDate = record.HARVESTDATE,
                    EndDate = record.HARVESTDATE
                };

                (
                    from a in record.THARVESTEMPLOYEE
                    join b in _attendanceService.CheckAttendances(filterAttendance) on a.EMPLOYEEID equals b.EMPID into ab
                    from c in ab.DefaultIfEmpty()
                    where c == null
                    select a
                ).ToList().ForEach(employee =>
                {
                    errorMessage = "\r\n" + employee.EMPLOYEEID;
                });
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    throw new Exception("Absensi pada tanggal " + record.HARVESTDATE_IN_TEXT + " untuk karyawan berikut tidak valid :" + errorMessage);
                
            }


            

            errorMessage = string.Empty;
            var attendanceHKList = _attendanceService.GetList(new GeneralFilter { StartDate = record.HARVESTDATE.Date, EndDate = record.HARVESTDATE.Date, Ids = employeeIds })
                                    .GroupBy(d => d.EMPLOYEEID)
                                    .Select(d => new { EMPLOYEEID = d.Key, HK = d.Sum(s => s.HK) })
                                    .ToList();

            (
                from a in record.THARVESTEMPLOYEE
                join b in attendanceHKList on a.EMPLOYEEID equals b.EMPLOYEEID into ab
                from c in ab.DefaultIfEmpty()
                where (c == null ? 0 : c.HK) + a.VALUE > 1
                select new { a.EMPLOYEEID, a.VALUE, HK = (c == null ? 0 : c.HK) }
            ).ToList().ForEach(d => {
                errorMessage += $"\r\n{d.EMPLOYEEID} {d.HK} HK";
            });
            if (!string.IsNullOrWhiteSpace(errorMessage))
                throw new Exception("Berikut ini karyawan lebih dari 1 HK " + errorMessage);

            if (!allowWork2Div)
            {
                errorMessage = string.Empty;
                record.THARVESTEMPLOYEE
                .Join(_attendanceService.GetOtherLocations(record.HARVESTDATE.Date, employeeIds, record.DIVID), a => a.EMPLOYEEID, b => b.EMPLOYEEID, (a, b) => new { a.EMPLOYEEID, b.DIVID })
                .ToList().ForEach(d => {
                    errorMessage = $"\r\nKaryawan {d.EMPLOYEEID} sudah bekerja di divisi {d.DIVID}";
                });

                if (!string.IsNullOrWhiteSpace(errorMessage))
                    throw new Exception("Berikut ini karyawan yang sudah berkerja di lokasi lain: " + errorMessage);
            }

            string msgEmployee = string.Empty;

            DataSet allowMaxBrondol = null;
            DataSet allowQty = null;
            var harvestEmployees =
            (
                from a in record.THARVESTEMPLOYEE
                join b in record.THARVESTBASE.GroupBy(d => d.EMPID).Select(d => new { EMPLOYEEID = d.Key, BASE1 = d.Sum(s => s.BASE1) }) on a.EMPLOYEEID equals b.EMPLOYEEID into bx
                from b1 in bx.DefaultIfEmpty()
                join c in record.THARVESTCOLLECT.GroupBy(d => d.EMPLOYEEID).Select(d => new { EMPLOYEEID = d.Key, QTY = d.Sum(s => s.QTY), QTYKG = d.Sum(s => s.QTYKG) }) on a.EMPLOYEEID equals c.EMPLOYEEID into cx
                from c1 in cx.DefaultIfEmpty()
                where !a.ALLOWQTY
                select new { a.EMPLOYEEID, MAXHASIL = percentMaxHasil * 0.01M * ((b1 == null) ? 0 : b1.BASE1), QTY = (c1 == null) ? 0 : c1.QTY, QTYKG = (c1 == null) ? 0 : c1.QTYKG }
            ).ToList();

            if (record.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah)
            {
                harvestEmployees.ForEach(d => {
                    decimal hasil = basisByKg ? d.QTYKG : d.QTY;

                    if (d.MAXHASIL > 0 && d.MAXHASIL < hasil)
                    {
                            //if (allowQty == null)
                            //    using (var services = PmsServicesFactory.GetIntraServices())
                            //        allowQty = services.Harvesting_GetApproval1(harvesting.DocumentId, harvesting.Code);

                            //var q = from i in allowQty.Tables[0].AsEnumerable() where i[0].ToString() == employee.EmployeeId select i;
                            //if (q.Count() == 0)
                            //    msgEmployee += "Hasil Panen Karyawan " + employee.EmployeeId + " (" +
                            //                  hasil.ToString("#0") +
                            //                  ") lebih besar dari batas maksimum (" + maxHasil.ToString("#0") +
                            //                  ")" + Environment.NewLine;
                    }
                });
            }
            else if (record.HARVESTTYPE == PMSConstants.HarvestTypeKutipBrondol)
            {
                harvestEmployees.ForEach(d => {
                    decimal hasil = d.QTYKG;
                    if (d.MAXHASIL > 0 && d.MAXHASIL < hasil)
                    {
                        //if (allowQty == null)
                        //    using (var services = PmsServicesFactory.GetIntraServices())
                        //        allowQty = services.Harvesting_GetApproval1(harvesting.DocumentId, harvesting.Code);

                        //var q = from i in allowQty.Tables[0].AsEnumerable() where i[0].ToString() == employee.EmployeeId select i;
                        //if (q.Count() == 0)
                        //    msgEmployee += "Hasil Panen Karyawan " + employee.EmployeeId + " (" +
                        //                   hasil.ToString("#0") +
                        //                   ") lebih besar dari batas maksimum (" + maxHasil.ToString("#0") +
                        //                   ")" + Environment.NewLine;
                    }
                    if (_activity.ACTIVITYID.Equals(PMSConstants.ActivityPungunBrondolanManual) || _activity.ACTIVITYID.Equals(PMSConstants.ActivityPungunBrondolanGardan))
                    {
                        if (maxBrondol > 0 && maxBrondol < hasil)
                        {
                            //if (allowQty == null)
                            //    using (var services = PmsServicesFactory.GetIntraServices())
                            //        allowQty = services.Harvesting_GetApproval1(harvesting.DocumentId, harvesting.Code);

                            //var q = from i in allowQty.Tables[0].AsEnumerable() where i[0].ToString() == employee.EmployeeId select i;
                            //if (q.Count() == 0)
                            //    msgEmployee += "Hasil Panen Karyawan " + employee.EmployeeId + " (" +
                            //                    hasil.ToString("#0") +
                            //                    ") lebih besar dari batas maksimum (" + maxBrondol.ToString("#0") +
                            //                    ")" + Environment.NewLine;
                        }
                    }   
                });
            }

           

            
            int maxAreaStartInt = 0;
            int.TryParse(GetMConfigValue(PMSConstants.CfgHarvestingMaxAreaDayStart + _divisi.UNITCODE), out maxAreaStartInt);
            
            int maxAreaEndInt = 0;
            int.TryParse(GetMConfigValue(PMSConstants.CfgHarvestingMaxAreaDayEnd + _divisi.UNITCODE), out maxAreaStartInt);

            var maxAreaStart = record.HARVESTDATE.Date.AddDays(maxAreaStartInt * -1);
            var maxAreaEnd = record.HARVESTDATE.Date.AddDays(maxAreaEndInt);

            var blockIds = record.THARVESTBLOCK.Select(d => d.BLOCKID).Distinct().ToList();
            var masterBlocks = _blockService.GetList(new FilterBlock { BlockIDs = blockIds });
            var totalharvestedArea = GetHarvestedArea(maxAreaStart, maxAreaEnd, record.HARVESTTYPE, blockIds,string.Empty, record.HARVESTCODE);
            var harvestedAreaByActivity = GetHarvestedArea(maxAreaStart, maxAreaEnd, record.HARVESTTYPE, blockIds, _activity.ACTIVITYID,string.Empty );


            errorMessage = string.Empty;
            (
                from a in record.THARVESTBLOCK.GroupBy(d => d.BLOCKID).Select(d => new { BLOCKID = d.Key, HARVESTAREA = d.Average(s => s.HARVESTAREA) })
                join b in masterBlocks on a.BLOCKID equals b.BLOCKID
                join c in totalharvestedArea on a.BLOCKID equals c.BLOCKID into cleft
                from c1 in cleft.DefaultIfEmpty()
                join d in harvestedAreaByActivity on a.BLOCKID equals d.BLOCKID into dleft
                from d1 in dleft.DefaultIfEmpty()
                where a.HARVESTAREA + (c1 == null ? 0 : c1.HARVESTAREA) > b.CURRENTPLANTED ||
                      a.HARVESTAREA + (d1 == null ? 0 : d1.HARVESTAREA)>b.CURRENTPLANTED
                select new { a.BLOCKID, a.HARVESTAREA, b.CURRENTPLANTED, TOTALHARVESTAREA = (c1 == null ? 0 : c1.HARVESTAREA),ACTIVITYHARVESTAREA = (d1 == null ? 0 : d1.HARVESTAREA) }
            ).ToList().ForEach(d => {
                if (d.HARVESTAREA + d.ACTIVITYHARVESTAREA > d.CURRENTPLANTED)
                    errorMessage += $"\r\nLuas panen blok {d.BLOCKID} tidak boleh lebih dari {d.CURRENTPLANTED} (luas panen saat ini = {d.ACTIVITYHARVESTAREA}).";
                else if ((maxAreaStartInt > 0 || maxAreaEndInt > 0) && d.TOTALHARVESTAREA + d.HARVESTAREA > d.CURRENTPLANTED)
                    errorMessage += $"\r\nLuas panen blok {d.BLOCKID}  antara tanggal {maxAreaStart.ToString("dd/MM/yyyy")} dan {maxAreaEnd.ToString("dd/MM/yyyy")} tidak boleh lebih dari {d.CURRENTPLANTED} (luas panen saat ini = {d.TOTALHARVESTAREA}).";
                
            });
            if (!string.IsNullOrWhiteSpace(errorMessage))
                throw new Exception("Berikut ini luas panen melebihi luas blok : " + errorMessage);
            
            
            if (record.HARVESTTYPE == PMSConstants.HarvestTypeKutipBrondol)
            {
                var stringBrondolMaxPercent = GetMConfigValue(PMSConstants.CfgHarvestingBrondolMaxPercent + _divisi.UNITCODE);
                decimal brondolMaxPercent = 0;
                decimal.TryParse(GetMConfigValue(PMSConstants.CfgHarvestingBrondolMaxPercent + _divisi.UNITCODE), out brondolMaxPercent);

                if (brondolMaxPercent > 0)
                {


                    //if (allowMaxBrondol == null)
                    //{
                    //    var docId = string.Empty;
                    //    var doc = PMSServices.DocumentApproval.Get(harvesting.Code, PMSConstants.WfJobHarvestMaxBrondolPercent, databases);
                    //    if (doc != null) docId = doc.DocumentId;

                    //    using (var services = PmsServicesFactory.GetIntraServices())
                    //        allowMaxBrondol = services.Harvesting_GetMaxBrondolApproval(docId, harvesting.Code);
                    //}

                    errorMessage = string.Empty;
                    (
                        from a in record.THARVESTBLOCK.GroupBy(d => d.BLOCKID).Select(d => new { BLOCKIID = d.Key, QTY = d.Sum(s => s.QTY), QTYKG = d.Sum(s => s.QTYKG) })
                        join b in _context.sp_HarvestingBlock_GetOtherQty(record.HARVESTCODE, record.HARVESTDATE.Date) on a.BLOCKIID equals b.BLOCKID into bleft
                        from b1 in bleft.DefaultIfEmpty()
                        select new { a.BLOCKIID, a.QTY, a.QTYKG, OTHQTY = b1 }
                    ).ToList().ForEach(d => {

                        decimal janjang0 = 0; decimal brondol0 = 0;
                        decimal janjang1 = 0; decimal brondol1 = 0;
                        decimal janjang2 = 0; decimal brondol2 = 0;
                        decimal qtyjanjang; decimal qtybrondol;

                        

                        if (record.HARVESTDATE.Day <= 2)
                        {
                            if (d.OTHQTY.JJG1 == 0)
                            {
                                qtyjanjang = d.OTHQTY.JJG0;
                                qtybrondol = d.OTHQTY.BRD0 + d.OTHQTY.BRD1;
                            }
                            else
                            {
                                qtyjanjang = janjang1 + janjang2;
                                qtybrondol = brondol1 + brondol2;
                            }
                        }
                        else
                        {
                            if (d.OTHQTY.JJG1 == 0)
                            {
                                qtyjanjang = d.OTHQTY.JJG2;
                                qtybrondol = d.OTHQTY.BRD2;
                            }
                            else
                            {
                                qtyjanjang = d.OTHQTY.JJG1 + d.OTHQTY.JJG2;
                                qtybrondol = d.OTHQTY.BRD1 + d.OTHQTY.BRD2;
                            }
                        }

                        decimal qty = d.QTY + qtybrondol;
                        decimal max = (qtyjanjang + qtybrondol + d.QTY) * brondolMaxPercent / 100;
                        if (qty > max)
                        {

                            //var m = from i in allowMaxBrondol.Tables[0].AsEnumerable()
                            //        where i[0].ToString() == itm.BlockId
                            //        select i;
                            //if (m.Count() == 0)
                            //{
                            //    errorMessage += "Jumlah hasil blok " + itm.BlockId
                            //                      + " (" + qty.ToString("#,#0.00") + ") melebihi batas maksimal (" +
                            //                      max.ToString("#,#0.00") + ")." + Environment.NewLine;
                            //}
                        }
                    });





                    if (!string.IsNullOrWhiteSpace(errorMessage))
                        throw new Exception(errorMessage);
                }
            }
            return record;
        }
        public bool Approve(THARVEST record, string userName)
        {
            if (record == null)
                throw new Exception("Buku panen tidak ditemukan");

            if (record.STATUS.Equals(PMSConstants.TransactionStatusDeleted))
                throw new Exception("Buku panen sudah dihapus");
            if (record.STATUS.Equals(PMSConstants.TransactionStatusApproved))
                throw new Exception("Buku panen sudah diappove");
            if (record.STATUS.Equals(PMSConstants.TransactionStatusCanceled))
                throw new Exception("Buku panen sudah dicancel");

            
            BeforeApprove(record, userName);
            record.STATUS = PMSConstants.TransactionStatusApproved;
            record.UPDATED = GetServerTime();
            record.UPDATEBY = userName;
            _context.SaveChanges();
            return true;
        }

        public bool CancelApprove(THARVEST record, string userName)
        {
            if (record == null)
                throw new Exception("Buku panen tidak ditemukan");

            if (!_authenticationService.IsAuthorizedDivisi(userName, record.DIVID))
                throw new ExceptionNoDivisionAccess(record.DIVID);

            if (record.STATUS.Equals(PMSConstants.TransactionStatusDeleted))
                throw new Exception("Buku panen sudah dihapus");
            if (!record.STATUS.Equals(PMSConstants.TransactionStatusApproved))
                throw new Exception("Buku panen belum diappove");
            if (record.STATUS.Equals(PMSConstants.TransactionStatusCanceled))
                throw new Exception("Buku panen sudah dicancel");
            _periodService.CheckValidPeriod(record.DIV.UNITCODE, record.HARVESTDATE.Date);
            record.STATUS = PMSConstants.TransactionStatusCanceled;
            record.UPDATED = GetServerTime();
            record.UPDATEBY = userName;
            record.UPLOAD = 0;

            //Copy Record
            THARVEST copyRecord = GetSingle(record);
            copyRecord.InitEdit();


            copyRecord.THARVESTEMPLOYEEEDIT.AddRange(record.THARVESTEMPLOYEE);
            copyRecord.THARVESTBLOCKEDIT.AddRange(record.THARVESTBLOCK);
            copyRecord.THARVESTCOLLECTEDIT.AddRange(record.THARVESTCOLLECT);
            copyRecord.THARVESTFINEEDIT.AddRange(record.THARVESTFINE);
            copyRecord.THARVESTBASEEDIT.AddRange(record.THARVESTBASE);
            copyRecord.STATUS = PMSConstants.TransactionStatusProcess;
            copyRecord.UPDATED = GetServerTime();
            copyRecord.UPDATEBY = userName;
            copyRecord.UPLOAD = 0;

            _attendanceService.DeleteByReferences(record.HARVESTCODE);
            _newRecordAfterCancel = true;
            SaveInsert(copyRecord, userName);

            //if (PMSServices.Helper.GetConfigValue(PMSConstants.CfgHarvestingAutoUpload + harvesting.Division.UnitCode, db)
            //       == PMSConstants.CfgHarvestingAutoUploadTrue)
            //{
            //    harvesting.Upload = 2;
            //    harvesting.Uploaded = harvesting.UpdatedDate;
            //    var skf = repository.GetHarvesting(harvesting.Code, db);
            //    int isPlasma = DataHelper.GetInteger(skf.Tables[2].Rows[0][0]);

            //    DataTable result;
            //    if (isPlasma == 0)
            //        using (var services = PmsServicesFactory.GetPmsServices())
            //            result = services.Harvesting_Cancel(harvesting.Code, harvesting.Division.UnitCode, harvesting.Date, skf, string.Empty);
            //    else
            //        using (var services = PmsServicesFactory.GetPmsServices())
            //            result = services.HarvestingPlasma_Cancel(harvesting.Code, harvesting.Division.UnitCode, harvesting.Date, skf, string.Empty);

            //    foreach (DataRow row in result.Rows)
            //    {
            //        if (row["PSTG_STAT"].ToString() != "S")
            //            rfcResult += row["PSTG_STAT"] + ": " + row["PSTG_MSG"] + " ";

            //        if (isPlasma == 0)
            //            PMSServices.Helper.SetSapTran(harvesting.Code, row["DOC_NUMBER"].ToString(), db);
            //    }

            //    if (!string.IsNullOrEmpty(rfcResult))
            //        throw new Exception(rfcResult);
            //}


            return true;
        }

        public class VHARVESTAREA
        { 
            public string BLOCKID { get; set; }            
            public decimal HARVESTAREA { get; set; }
        }
        public IEnumerable<VHARVESTAREA> GetHarvestedArea(DateTime startDate, DateTime endDate, int harvestType, List<string> blockIds, string activityId, string excludedHarvestCode)
        {
            bool allBlocks = (blockIds == null || !blockIds.Any());


            return 
                (
                    from h in _context.THARVEST
                    join hb in _context.THARVESTBLOCK on h.HARVESTCODE equals hb.HARVESTCODE
                    where h.HARVESTDATE.Date >= startDate && h.HARVESTDATE.Date <= endDate.Date
                        && (blockIds.Contains(hb.BLOCKID) || allBlocks)
                        && h.STATUS.Equals(PMSConstants.TransactionStatusApproved)
                        && h.HARVESTTYPE == harvestType
                        && !h.HARVESTCODE.Equals(excludedHarvestCode)
                        && (h.ACTIVITYID.Equals(activityId) || string.IsNullOrEmpty(activityId))
                    select new { h.HARVESTCODE, hb.BLOCKID, hb.HARVESTAREA }
                )
                .Distinct()
                .GroupBy(d => d.BLOCKID)
                .Select(d => new VHARVESTAREA { BLOCKID = d.Key, HARVESTAREA = d.Sum(s => s.HARVESTAREA) });
            
        }

        
        public void GenerateBP(string divid, DateTime date, string by,List<string> deletedBP, List<string> insertedBP)
        {
            //cek hvt sudah A, cancel dulu
            var approvedHvtList = GetList(new FilterHarvest { DivisionID = divid, StartDate = date.Date, EndDate = date.Date, HarvestType = -1, RecordStatus = PMSConstants.TransactionStatusApproved }).ToList();
            if (approvedHvtList.Any())
            {
                string errorMessage = "Buku panen berikut ini sudah approved dan harus dibatalkan dulu, yaitu sebagai berikut :\r\n";
                approvedHvtList.ForEach(d =>
                {
                    errorMessage += d.HARVESTCODE + "\r\n";
                });
                throw new Exception(errorMessage);
            }

            List<sp_Harvesting_GenerateBP_Result_Harvest> harvests = new List<sp_Harvesting_GenerateBP_Result_Harvest>();
            List<sp_Harvesting_GenerateBP_Result_HarvestEmployee> harvestEmployees = new List<sp_Harvesting_GenerateBP_Result_HarvestEmployee>();
            List<sp_Harvesting_GenerateBP_Result_HarvestBlock> harvestBlocks = new List<sp_Harvesting_GenerateBP_Result_HarvestBlock>();
            List<sp_Harvesting_GenerateBP_Result_HarvestCollect> harvestCollects = new List<sp_Harvesting_GenerateBP_Result_HarvestCollect>();
            List<sp_Harvesting_GenerateBP_Result_HarvestFine> harvestFines = new List<sp_Harvesting_GenerateBP_Result_HarvestFine>();

            _context.sp_Harvesting_GenerateBP(divid, date.Date,harvests,harvestEmployees,harvestBlocks,harvestCollects,harvestFines);


            _internalCommit = false;

            //Delete Existing Buku Panen Which Status Processed
            List<string> existingHarvestIds = new List<string>();
            existingHarvestIds = _context.THARVEST
                .Where(d => d.HARVESTDATE == date && d.DIVID.Equals(divid) && d.STATUS.Equals(PMSConstants.TransactionStatusProcess))
                .Select(d => d.HARVESTCODE).Distinct().ToList();

            _context.THARVESTCOLLECT.RemoveRange(_context.THARVESTCOLLECT.Where(d => existingHarvestIds.Contains(d.HARVESTCODE)));
            _context.THARVESTFINE.RemoveRange(_context.THARVESTFINE.Where(d => existingHarvestIds.Contains(d.HARVESTCODE)));
            _context.THARVESTBLOCK.RemoveRange(_context.THARVESTBLOCK.Where(d => existingHarvestIds.Contains(d.HARVESTCODE)));
            _context.THARVESTEMPLOYEE.RemoveRange(_context.THARVESTEMPLOYEE.Where(d => existingHarvestIds.Contains(d.HARVESTCODE)));
            _context.THARVESTBASE.RemoveRange(_context.THARVESTBASE.Where(d => existingHarvestIds.Contains(d.HARVESTCODE)));
            _context.THARVEST.RemoveRange(_context.THARVEST.Where(d => existingHarvestIds.Contains(d.HARVESTCODE)));


            harvests.ForEach(d => {
                //Harvest Header
                THARVEST hvt = new THARVEST();
                hvt.InitEdit();                
                hvt.DIVID = d.DIVID;
                hvt.HARVESTDATE = d.DATE;
                hvt.HARVESTPAYMENTTYPE = d.PAYMENT;
                hvt.ACTIVITYID = d.ACTID;
                hvt.MANDOR1ID = d.MANDOR1ID;
                hvt.MANDORID = d.MANDORID;
                hvt.KRANIID = d.KRANIID;
                hvt.CHECKERID = d.CHECKERID;

                
                harvestEmployees.Where(e => d.DATE == d.DATE && e.DIVID == d.DIVID
                    && e.PAYMENT == d.PAYMENT && e.ACTID == d.ACTID
                    && e.MANDOR1ID == d.MANDOR1ID && e.MANDORID == d.MANDORID
                    && e.KRANIID == e.KRANIID && e.CHECKERID == d.CHECKERID).ToList().ForEach(e => 
                    {
                        hvt.THARVESTEMPLOYEEEDIT.Add(new THARVESTEMPLOYEE { EMPLOYEEID = e.EMPLOYEEID }) ;
                        

                    });

                
                List<VHARVESTBLOCK> blocks = new List<VHARVESTBLOCK>();
                harvestBlocks.Where(e => d.DATE == d.DATE && e.DIVID == d.DIVID
                    && e.PAYMENT == d.PAYMENT && e.ACTID == d.ACTID
                    && e.MANDOR1ID == d.MANDOR1ID && e.MANDORID == d.MANDORID
                    && e.KRANIID == e.KRANIID && e.CHECKERID == d.CHECKERID).ToList().ForEach(e =>
                    {
                        hvt.VHARVESTBLOCKEDIT.Add(new VHARVESTBLOCK
                        {
                            BLOCKID = e.BLOCKID
                        }); 
                    });


                harvestCollects.Where(e => d.DATE == d.DATE && e.DIVID == d.DIVID
                    && e.PAYMENT == d.PAYMENT && e.ACTID == d.ACTID
                    && e.MANDOR1ID == d.MANDOR1ID && e.MANDORID == d.MANDORID
                    && e.KRANIID == e.KRANIID && e.CHECKERID == d.CHECKERID).ToList().ForEach(e => 
                    {
                        hvt.THARVESTCOLLECTEDIT.Add(new THARVESTCOLLECT 
                        { 
                            EMPLOYEEID = e.EMPLOYEEID,
                            BLOCKID = e.BLOCKID,
                            COLLPOINT = e.TPHID,
                            QTY = e.QTY,
                            QTYKG = e.QTYKG
                        });
                    });

                //Update Kg Per Block
                hvt.THARVESTCOLLECTEDIT.GroupBy(e => e.BLOCKID).Select(e => new { BLOCKID = e.Key, QTYKG = e.Sum(s => s.QTYKG) })
                .Join(hvt.VHARVESTBLOCKEDIT, a => a.BLOCKID, b => b.BLOCKID, (a, b) => new { VHARVESTBLOCKEDIT = b, a.QTYKG })
                .ToList().ForEach(e => { e.VHARVESTBLOCKEDIT.KG = e.QTYKG; });


                harvestFines.Where(e => d.DATE == d.DATE && e.DIVID == d.DIVID
                    && e.PAYMENT == d.PAYMENT && e.ACTID == d.ACTID
                    && e.MANDOR1ID == d.MANDOR1ID && e.MANDORID == d.MANDORID
                    && e.KRANIID == e.KRANIID && e.CHECKERID == d.CHECKERID).ToList().ForEach(e =>
                    {
                        hvt.THARVESTFINEEDIT.Add(new THARVESTFINE   
                        {
                            EMPLOYEEID = e.EMPLOYEEID,
                            BLOCKID = e.BLOCKID,
                            FINECODE = e.FINECODE,
                            QTY = e.PEN,
                            AUTO = e.AUTO
                        });
                    });

                _calcKg = false;
                SaveInsert(hvt, by);
                _calcKg = true;

            });

            deletedBP = new List<string>();
            deletedBP.AddRange(existingHarvestIds);

            insertedBP = new List<string>();
            insertedBP = GetList(new FilterHarvest { Date = date, DivisionID = divid, RecordStatus = PMSConstants.TransactionStatusProcess }).Select(d => d.HARVESTCODE).Distinct().ToList();

        }

     

        private class THARVESTBLOCKEMPPROPOTION
        {
            public string EMPLOYEEID { get; set; }
            public string BLOCKID { get; set; }
            public decimal KG { get; set; }
            public decimal QTY { get; set; }
        }

        private class THARVESTBLOCKEMPTPHPROPOTION
        {
            public string EMPLOYEEID { get; set; }
            public string BLOCKID { get; set; }
            public string TPHID { get; set; }
            public decimal KG { get; set; }
            public decimal QTY { get; set; }
        }

        /// <summary>
        /// Calculate Kg Proportion By Block to By Employee,TPH
        /// </summary>
        private void CalculateKg(THARVEST record)
        {
            var blockCollections = record.THARVESTCOLLECTEDIT
                .GroupBy(c => c.BLOCKID)                
                .Select(d => new { BLOCKID = d.Key, SUMQTYBLOCK = d.Sum(s => s.QTY) }).ToList();

            var employeeCollectionPerBlock = record.THARVESTCOLLECTEDIT
                .GroupBy(c => new { c.BLOCKID,c.EMPLOYEEID})
                .Select(d => new { BLOCKID = d.Key.BLOCKID ,EMPLOYEEID = d.Key.EMPLOYEEID,  SUMQTYBLOCKEMPLOYEE = d.Sum(s => s.QTY), COUNTX = d.Count() }).ToList();

            



            var employeeCollectionKg =
                (
                    from a in blockCollections
                    join b in employeeCollectionPerBlock on a.BLOCKID equals b.BLOCKID 
                    join c in record.VHARVESTBLOCKEDIT on a.BLOCKID  equals c.BLOCKID
                    select new THARVESTBLOCKEMPPROPOTION { BLOCKID = b.BLOCKID, EMPLOYEEID = b.EMPLOYEEID,
                            KG = Math.Round(c.KG * b.SUMQTYBLOCKEMPLOYEE/a.SUMQTYBLOCK,2),
                            QTY = b.SUMQTYBLOCKEMPLOYEE }
                ).ToList();

            //Check Sum, If Any Diffrence Add to the last
            var employeeCollectionKgCheckSum =
                (
                    from a in employeeCollectionKg
                    group a by a.BLOCKID into g
                    select new { BLOCKID = g.Key, KG = g.Sum(d => d.KG) } into c

                    join d in record.VHARVESTBLOCKEDIT on c.BLOCKID equals d.BLOCKID
                    where d.KG != c.KG
                    select new { BLOCKID = c.BLOCKID, KGDIFF = d.KG - c.KG }
                ).ToList();

            if (employeeCollectionKgCheckSum != null && employeeCollectionKgCheckSum.Any())
            {
                employeeCollectionKgCheckSum.ForEach(d => {
                    var empColKg = employeeCollectionKg.LastOrDefault(e => e.BLOCKID.Equals(d.BLOCKID));
                    empColKg.KG += d.KGDIFF;
                });
            }



            var harvestCollectionJoin =
                (
                    from a in record.THARVESTCOLLECTEDIT
                    join b in employeeCollectionKg on new { a.EMPLOYEEID, a.BLOCKID } equals new { b.EMPLOYEEID, b.BLOCKID }
                    select new THARVESTBLOCKEMPTPHPROPOTION { BLOCKID = b.BLOCKID, EMPLOYEEID = b.EMPLOYEEID, TPHID = a.COLLPOINT, KG = Math.Round(b.KG * a.QTY / b.QTY,2),QTY =a.QTY }
                ).ToList();


            //Check Sum, If Any Diffrence Add to the last
            var harvestCollectionJoinCheckSum =
                (
                    from a in harvestCollectionJoin
                    group a by new { a.EMPLOYEEID, a.BLOCKID } into g
                    select new { BLOCKID = g.Key.BLOCKID, EMPLOYEEID = g.Key.EMPLOYEEID,  KG = g.Sum(d => d.KG) } into c

                    join d in employeeCollectionKg on new { c.BLOCKID, c.EMPLOYEEID } equals new { d.BLOCKID, d.EMPLOYEEID }
                    where d.KG != c.KG
                    select new { BLOCKID = c.BLOCKID, EMPLOYEEID = c.EMPLOYEEID,  KGDIFF = d.KG - c.KG }
                ).ToList();

            if (harvestCollectionJoinCheckSum != null && harvestCollectionJoinCheckSum.Any())
            {
                harvestCollectionJoinCheckSum.ForEach(d => {
                    harvestCollectionJoin.LastOrDefault(e => e.BLOCKID.Equals(d.BLOCKID)).KG += d.KGDIFF;
                });
            }

            //Update QtyKG Proporsion

            harvestCollectionJoin.ForEach(d => {
                record.THARVESTCOLLECTEDIT.SingleOrDefault(e => e.BLOCKID.Equals(d.BLOCKID) && e.EMPLOYEEID.Equals(d.EMPLOYEEID) && e.COLLPOINT.Equals(d.TPHID)).QTYKG = d.KG;
            });
        }

        /// <summary>
        /// Generate THARVESTBLOCK
        /// </summary>
        private void CalculateHarvesting(THARVEST record)
        {

            var collections = record.THARVESTCOLLECTEDIT;
            var blocks = record.THARVESTBLOCKEDIT;
            var employees = record.THARVESTEMPLOYEEEDIT;
            var fines = record.THARVESTFINEEDIT;

            record.THARVESTCOLLECTEDIT =
                (
                    from a in record.THARVESTCOLLECTEDIT
                    join b in record.VHARVESTBLOCKEDIT on a.BLOCKID equals b.BLOCKID
                    join c in record.THARVESTEMPLOYEEEDIT on a.EMPLOYEEID equals c.EMPLOYEEID
                    select a
                ).ToList();



            record.THARVESTFINEEDIT = 
            (
                    from a in record.THARVESTFINEEDIT
                    join b in record.VHARVESTBLOCKEDIT on a.BLOCKID equals b.BLOCKID
                    join c in record.THARVESTEMPLOYEEEDIT on a.EMPLOYEEID equals c.EMPLOYEEID
                    select a
            ).ToList();

            var qtyCollectionByEmpBlock = record.THARVESTCOLLECTEDIT
                .GroupBy(d => new {d.EMPLOYEEID, d.BLOCKID })
                .Select(d => new {d.Key.EMPLOYEEID, d.Key.BLOCKID, QTY = d.Sum(s => s.QTY), QTYKG = d.Sum(s => s.QTYKG) });


            var qtyFineByEmpBlock = record.THARVESTFINEEDIT
                .GroupBy(d => new {d.EMPLOYEEID, d.BLOCKID })
                .Select(d => new {d.Key.EMPLOYEEID, d.Key.BLOCKID, QTY = d.Sum(s => s.QTY)});

           
            //Get Sum Qty By Emp,Block
            record.THARVESTBLOCKEDIT =
                (
                    from a in record.THARVESTCOLLECTEDIT
                                .GroupBy(d => new { d.EMPLOYEEID, d.BLOCKID })
                                .Select(d => new { d.Key.EMPLOYEEID, d.Key.BLOCKID, QTY = d.Sum(s => s.QTY), QTYKG = d.Sum(s => s.QTYKG) })
                    join b in record.THARVESTFINEEDIT
                                .GroupBy(d => new { d.EMPLOYEEID, d.BLOCKID })
                                .Select(d => new { d.Key.EMPLOYEEID, d.Key.BLOCKID, QTYFINE = d.Sum(s => s.QTY) })
                            on new { a.EMPLOYEEID, a.BLOCKID } equals new { b.EMPLOYEEID, b.BLOCKID } into b1
                    from b2 in b1.DefaultIfEmpty()
                    join c in record.THARVESTEMPLOYEEEDIT on a.EMPLOYEEID equals c.EMPLOYEEID
                    join d in record.VHARVESTBLOCKEDIT on a.BLOCKID equals d.BLOCKID
                    select new THARVESTBLOCK 
                    { 
                        HARVESTCODE = record.HARVESTCODE,
                        EMPLOYEEID = a.EMPLOYEEID,
                        BLOCKID = d.BLOCKID,
                        GERDANID = c.GEMPID,
                        QTY = a.QTY - (b2 == null? 0:b2.QTYFINE),
                        QTYKG = a.QTYKG,
                        HARVESTAREA = d.HARVESTAREA,
                        KG = d.KG,
                        EMPAREA = 0,
                        VALUE=0,
                        GVALUE=0
                    } 
                ).ToList();

            //Get HK Proportion
            (
                from a in record.THARVESTEMPLOYEEEDIT
                join b in record.THARVESTBLOCKEDIT on a.EMPLOYEEID equals b.EMPLOYEEID
                join c in record.THARVESTBLOCKEDIT.GroupBy(d => d.EMPLOYEEID).Select(d => new { EMPLOYEEID = d.Key, QTY = d.Sum(s => s.QTY) })
                    on a.EMPLOYEEID equals c.EMPLOYEEID
                select new { HVTBLOCK = b, 
                    HK = Math.Round(b.QTY / c.QTY * a.VALUE, 3), 
                    HKGerdan = Math.Round(b.QTY / c.QTY * a.GVALUE, 3),
                    EMPAREA = Math.Round(b.QTY / c.QTY * b.HARVESTAREA, 3)
                }
            ).ToList()
            .ForEach(d => {
                d.HVTBLOCK.VALUE = d.HK;
                d.HVTBLOCK.GVALUE = d.HKGerdan;
                d.HVTBLOCK.EMPAREA = d.EMPAREA;
            });

            record.THARVESTBLOCKEDIT.RemoveAll(d => d.QTY == 0 && d.VALUE == 0 && d.QTYKG == 0);

        }


        /// <summary>
        /// Extention class for combine MPREMIPANEN with BLOCKID
        /// </summary>
        private class MPREMIPANENEXT : MPREMIPANEN
        {
            public string BLOCKID { get; set; }
            public string PremiKey { get { return $"{BLOCKID}-{EMPLOYEETYPE}"; } }
            public MPREMIPANENEXT(MPREMIPANEN premiPanen,string blockId)
            {
                this.CopyFrom(premiPanen);
                BLOCKID = blockId;
            }
        }

        private void CalculateHK1(THARVEST record, List<THARVESTEMPLOYEE> employees, List<THARVESTBLOCK> employeeBlocks, List<THARVESTCOLLECT> collections, List<THARVESTBASE> bases, bool autoHk, bool basisByKg)
        {
            

            VDIVISI divisi = _divisiService.GetSingle(record.DIVID);
            var stringbase1TolerancePercent = GetMConfigValue(PMSConstants.CfgHarvestingResultBase1TolerancePercent + divisi.UNITCODE);
            decimal base1TolerancePercent = 0;

            decimal.TryParse(stringbase1TolerancePercent, out base1TolerancePercent);

            string stringPlanDatarArea = GetMConfigValue(PMSConstants.CfgHarvestingPlanStandartDatar + divisi.UNITCODE);
            decimal planDatarArea = 0;
            decimal.TryParse(stringPlanDatarArea, out planDatarArea);


            string stringPlanBukitArea = GetMConfigValue(PMSConstants.CfgHarvestingPlanStandartBerbukit + divisi.UNITCODE);
            decimal planBukitArea = 0;
            decimal.TryParse(stringPlanBukitArea, out planBukitArea);


            var results = new List<THARVESTRESULT1>();
            var gerdanResults = new List<THARVESTRESULT1>();

            var blocks = employeeBlocks.Select(d => new { d.HARVESTCODE, d.BLOCKID }).Distinct().ToList();
            var employeeMaster = _employeeService.GetList(new FilterEmployee { Ids = employees.Select(d => d.EMPLOYEEID).Distinct().ToList() })
                .Select(d => new { d.EMPID, d.EMPTYPE })
                .ToList();

            var filterPremi = new FilterPremiPanen
            {
                BlockIDs = employeeBlocks.Select(d => d.BLOCKID).Distinct().ToList(),
                EmployeeTypes = employeeMaster.Select(d => d.EMPTYPE).Distinct().ToList()
            };

            var premiPanens = _premiPanenService.GetListPremiPanenBlock(filterPremi)
                .Select(d => new MPREMIPANENEXT(d.PREMIPANEN, d.BLOCKID))
                .ToList();

            var collectionJoins =
                (
                    from a in collections
                                .GroupBy(d=>new { d.EMPLOYEEID,d.BLOCKID})
                                .Select(d=>new { d.Key.EMPLOYEEID, d.Key.BLOCKID, QTY = d.Sum(s => s.QTY), QTYKG = d.Sum(s => s.QTYKG) })
                    join b in employeeMaster on a.EMPLOYEEID equals b.EMPID                    
                    select new { a.EMPLOYEEID,a.BLOCKID,a.QTY,a.QTYKG, PremiKey = $"{a.BLOCKID}-{b.EMPTYPE}"}
                ).ToList();

            var collectionPremi =
                (
                    from a in collectionJoins
                    join e in employeeBlocks on new { a.BLOCKID,a.EMPLOYEEID} equals new {e.BLOCKID,e.EMPLOYEEID}
                    join b in premiPanens on  a.PremiKey equals b.PremiKey into ab
                    from c in ab.DefaultIfEmpty()
                    select new { a.EMPLOYEEID,a.BLOCKID, e.GERDANID, a.QTY,a.QTYKG, PREMIPANEN = c }
                ).ToList();


            
            //Check Premi Panen Yang Belum Disetting
            var collectionPremiNotSetting =
                collectionPremi
                .Where(d => d.PREMIPANEN == null)
                .Select(d => d.PREMIPANEN.PremiKey)
                .Distinct().ToList();

            string errorMessage = string.Empty;
            if (collectionPremiNotSetting.Any()) 
            {
                errorMessage = "Basis premi panen belum disetting untuk:";
                collectionPremiNotSetting.ForEach(d => {
                    errorMessage += $"\r\n{d}";
                });
                throw new Exception(errorMessage);
            }
            bool isFriday = (record.HARVESTDATE.DayOfWeek == DayOfWeek.Friday);
            collectionPremi.ForEach(collection =>
            {
                decimal dailyPercent = (isFriday ? collection.PREMIPANEN.FRIDAY1 * 0.01M : 1.00M);
                if (record.HARVESTTYPE == PMSConstants.PayTypeBorongan || record.HARVESTTYPE == PMSConstants.PayTypeKontanan)
                {
                    collection.PREMIPANEN.BASE1 = 0;
                    collection.PREMIPANEN.BASE2 = 0;
                    collection.PREMIPANEN.BASE3 = 0;

                    decimal hasilPanen = basisByKg ? collection.QTYKG : collection.QTY;
                    if (hasilPanen > 0)
                    {
                        bool withGerdan = !string.IsNullOrWhiteSpace(collection.GERDANID);
                        decimal base1 = 0,
                                base2 = 0,
                                base3 = 0,
                                gerdanBase1 = collection.PREMIPANEN.GBASE1 * dailyPercent,
                                gerdanBase2 = collection.PREMIPANEN.GBASE2 * dailyPercent,
                                gerdanBase3 = collection.PREMIPANEN.GBASE3 * dailyPercent,
                                pctBase1 = 0,
                                pctGerdanBase1 = 0;


                        if (withGerdan)
                        {
                            base1 = collection.PREMIPANEN.DBASE1 * dailyPercent;
                            base2 = collection.PREMIPANEN.DBASE2 * dailyPercent;
                            base3 = collection.PREMIPANEN.DBASE3 * dailyPercent;
                        }
                        else
                        {
                            base1 = collection.PREMIPANEN.BASE1 * dailyPercent;
                            base2 = collection.PREMIPANEN.BASE2 * dailyPercent;
                            base3 = collection.PREMIPANEN.BASE2 * dailyPercent;
                        }
                        pctBase1 = base1 == 0 ? 0 : hasilPanen / base1 * 100;
                        pctGerdanBase1 = gerdanBase1 == 0 ? 0 : hasilPanen / gerdanBase1 * 100;
                        var newResult = new THARVESTRESULT1
                        {
                            HARVESTDATE = record.HARVESTDATE,
                            EMPLOYEEID = collection.EMPLOYEEID,
                            BLOCKID = collection.BLOCKID,
                            ACTIVITYID = record.ACTIVITYID,
                            EMPLOYEETYPE = collection.PREMIPANEN.EMPLOYEETYPE,
                            HASILPANEN = hasilPanen,
                            BASE1 = base1,
                            BASE2 = base2,
                            BASE3 = base3,
                            PCTBASIS1 = pctBase1,                            
                            FRIDAY1 = collection.PREMIPANEN.FRIDAY1,
                            GEMPID = withGerdan ? collection.GERDANID : null                            
                        };
                        results.Add(newResult);
                        if (withGerdan)
                        {
                            var newGerdanResult = new THARVESTRESULT1();
                            newGerdanResult.CopyFrom(newResult);
                            newGerdanResult.BASE1 = gerdanBase1;
                            newGerdanResult.BASE2 = gerdanBase2;
                            newGerdanResult.BASE3 = gerdanBase3;
                            newGerdanResult.PCTBASIS1 = pctGerdanBase1;                            
                            gerdanResults.Add(newGerdanResult);
                        }

                    }
                }
            });

            

            var totalPercentBasisByEmployee =
                results
                .GroupBy(d =>d.EMPLOYEEID)
                .Select(d => new {EMPLOYEEID = d.Key, TOTALPCTBASIS1 = d.Sum(s => s.PCTBASIS1) });

            var totalPercentBasisGerndanByEmployee =
                gerdanResults
                .GroupBy(d => d.EMPLOYEEID )
                .Select(d => new { EMPLOYEEID = d.Key, TOTALPCTBASIS1 = d.Sum(s => s.PCTBASIS1) });

            bases = new List<THARVESTBASE>();
            results.Join(totalPercentBasisByEmployee, a => a.EMPLOYEEID, b => b.EMPLOYEEID, (a, b) => new { a,b.TOTALPCTBASIS1 }).ToList().ForEach(d=> {
                d.a.TOTALPCTBASIS1 = d.TOTALPCTBASIS1;
                d.a.NEWBASISPCT1 = d.TOTALPCTBASIS1 > 0 ? (d.a.PCTBASIS1 / d.TOTALPCTBASIS1) * 100 : 0;
                if (base1TolerancePercent > 0)
                {
                    decimal base1Tolerance = d.a.NEWBASIS1 - Math.Round(d.a.NEWBASIS1 * base1TolerancePercent / 100, 0);
                    if (d.a.HASILPANEN < d.a.NEWBASIS1 && d.a.HASILPANEN >= base1Tolerance) d.a.NEWBASIS1 = d.a.HASILPANEN;
                }
                bases.Add(new THARVESTBASE 
                    {
                        BLOCKID = d.a.BLOCKID,
                        EMPID = d.a.EMPLOYEEID,
                        BASE1 = d.a.NEWBASIS1
                    });

            });

            gerdanResults.Join(totalPercentBasisGerndanByEmployee, a => a.EMPLOYEEID, b => b.EMPLOYEEID, (a, b) => new { a, b.TOTALPCTBASIS1 }).ToList().ForEach(d => {
                d.a.TOTALPCTBASIS1 = d.TOTALPCTBASIS1;
                d.a.NEWBASISPCT1 = d.TOTALPCTBASIS1 > 0 ? (d.a.PCTBASIS1 / d.TOTALPCTBASIS1) * 100 : 0;
                if (base1TolerancePercent > 0)
                {
                    decimal base1Tolerance = d.a.NEWBASIS1 - Math.Round(d.a.NEWBASIS1 * base1TolerancePercent / 100, 0);
                    if (d.a.HASILPANEN < d.a.NEWBASIS1 && d.a.HASILPANEN >= base1Tolerance) d.a.NEWBASIS1 = d.a.HASILPANEN;
                }
            });

            
            if (autoHk)
            {
                var blockList = _blockService.GetList(new FilterBlock { BlockIDs = employeeBlocks.Select(d => d.BLOCKID).Distinct().ToList() }).ToList();

                var employeeWorkArea = employeeBlocks.Join(blockList, a => a.BLOCKID, b => b.BLOCKID, (a, b) =>
                            new
                            {
                                a.EMPLOYEEID,                                
                                BUKITAREA = b.TOPOGRAPI.Equals(PMSConstants.ArestaBlockTopografiBerbukit) ? a.EMPAREA : 0,
                                DATAAREA = !b.TOPOGRAPI.Equals(PMSConstants.ArestaBlockTopografiBerbukit) ? a.EMPAREA : 0
                            })
                            .GroupBy(d=>d.EMPLOYEEID)
                            .Select(d=>new { EMPLOYEEID = d.Key,DATARAREA = d.Sum(s=>s.DATAAREA), BUKITAREA = d.Sum(s=>s.BUKITAREA) })
                            .ToList();
                
                
                (   
                    from a in employees
                    join b in employeeWorkArea on a.EMPLOYEEID equals b.EMPLOYEEID
                    join c in employeeBlocks on a.EMPLOYEEID equals c.EMPLOYEEID
                    join d in results.GroupBy(d=>d.EMPLOYEEID).Select(d => 
                                new { 
                                    EMPLOYEEID = d.Key, 
                                    HKProporsi = d.Average(s=> record.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah ? s.HASILPANEN/s.NEWBASIS1 : 1)
                                }) on a.EMPLOYEEID equals d.EMPLOYEEID
                    join e in gerdanResults.GroupBy(d => d.EMPLOYEEID).Select(d =>
                                  new {
                                      EMPLOYEEID = d.Key,
                                      HKProporsiGerdan = d.Average(s => record.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah ? s.HASILPANEN / s.NEWBASIS1 : 1)
                                  }) on a.EMPLOYEEID equals e.EMPLOYEEID into ab
                    from f in ab.DefaultIfEmpty()
                    select new { EMPLOYEE = a,b.BUKITAREA,b.DATARAREA,EMPLOYEEBLOCK = c,d.HKProporsi,f.HKProporsiGerdan  }
                ).ToList().ForEach(d =>
                 {
                    var employee = d.EMPLOYEE;
                    var employeeBlock = d.EMPLOYEEBLOCK;

                    var att = _absensiService.GetSingle(employee.EMPLOYEEID, record.HARVESTDATE);
                    decimal workHour = 0; decimal workLimit = 24;
                    if (att != null)
                    {
                        workHour = att.WORKACTUAL.Hour;
                        workLimit = att.WORKLIMIT;
                    }

                     decimal haPlan = d.BUKITAREA, totalWorkArea = d.BUKITAREA + d.DATARAREA;
                     if (d.DATARAREA > d.BUKITAREA) haPlan = d.DATARAREA;


                     decimal hk = 0, hkGerdan = 0;
                     employee.VALUECALC = 0;
                     employee.VALUE = 0;
                     employee.GVALUECALC = 0;
                     employee.GVALUE = 0;

                     if (record.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian)
                     {
                         if (workHour >= workLimit && totalWorkArea >= haPlan && record.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah)
                             hk = 1;
                         else
                         {
                             hk = d.HKProporsi;
                             if (hk > 1)
                                 hk = 1;
                         }
                         employee.VALUECALC = Math.Round(hk, 3);
                         employee.VALUE = Math.Round(hk, 3);

                         if (!string.IsNullOrWhiteSpace(employee.GEMPID))
                         {
                             hkGerdan = d.HKProporsiGerdan;
                             if (hkGerdan > 1)
                                 hkGerdan = 1;
                         }
                         employee.GVALUECALC = hkGerdan;
                         employee.GVALUE =hkGerdan;


                     }
                    
                     
                 });
                
            }


            

            

            

            
                
        }


        public int GetEmployeeCount(string code, DateTime date, string mandorId)
        {
            return _context.THARVEST.Join(_context.THARVESTEMPLOYEE, a => a.HARVESTCODE, b => b.HARVESTCODE, (a, b) => new { HARVEST = a, EMPLOYEE = b })
                .Where(a => a.HARVEST.MANDORID.Equals(mandorId) && a.HARVEST.HARVESTDATE.Date == date.Date && (a.HARVEST.STATUS.Equals(PMSConstants.TransactionStatusApproved) || a.HARVEST.HARVESTCODE.Equals(code)))
                .Select(a => a.EMPLOYEE.EMPLOYEEID)
                .Distinct()
                .Count();
        }


        public THARVESTBLOCK GetHarvestingBlock(string code, string blockId, string employeeId)
        {
            return _context.THARVESTBLOCK.Find(code, blockId, employeeId);
        }

        public IEnumerable<sp_HarvestingBlockResult> GetHarvestingBlockResult(string unitId, string divisionCode, DateTime startDate, DateTime endDate)
        {
            return _context.sp_HarvestingBlock_Result(startDate, endDate, unitId, divisionCode);
        }


        public IEnumerable<VHARVESTBLOCK> GetGroupingByCode(string code)
        {
            return _context.THARVESTBLOCK.Where(d => d.HARVESTCODE.Equals(code))
                .GroupBy(d => d.BLOCKID)
                .Select(d => new VHARVESTBLOCK { HARVESTCODE = code, BLOCKID = d.Key, HARVESTAREA = d.Average(s => s.HARVESTAREA), KG = d.Average(s => s.KG) });
        }

        public IEnumerable<VEMPLOYEE> GetEmployeeCandidate(string unitId,string supervisorId, string employeeId, string keyword,DateTime date, string cardType)
        {
            var checkAttendance = GetMConfigValue(PMSConstants.CfgHarvestAttendanceCheck + unitId) == PMSConstants.CfgHarvestAttendanceCheckTrue;
            var employeeList = _employeeService.GetList(new FilterEmployee { Id = employeeId, SupervisorId = supervisorId, Keyword = keyword });

            if (!checkAttendance)
                return employeeList;

            var Ids = employeeList.Select(d => d.EMPID).Distinct().ToList();

            var employeePresents = _attendanceService.CheckAttendances(new FilterAttendance { Ids = Ids, StartDate = date, EndDate = date, CardType = cardType,RecordStatus = "K" }).Select(d=>d.EMPID).Distinct().ToList();

            return employeeList.Where(d => employeePresents.Contains(d.EMPID));
        }
        public override THARVEST NewRecord(string userName)
        {
            var record = new THARVEST();
            record.HARVESTDATE = GetServerTime().Date;
            record.STATUS = "P";
            record.CREATED = GetServerTime();
            record.UPDATED = GetServerTime();
            record.HARVESTPAYMENTTYPE = 0;
            record.HARVESTTYPE = 0;
            return record;
        }

        protected override THARVEST AfterSave(THARVEST record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                IncreaseRunningNumber();
                _autoNumberPrefix = string.Empty;
            }
            return record;
        }
    }
}
