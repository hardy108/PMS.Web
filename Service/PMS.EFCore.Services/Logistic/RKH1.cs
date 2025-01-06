using System;
using System.Collections;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Helper;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.Attendances;
using PMS.EFCore.Services.Organization;
using PMS.EFCore.Services.Logistic;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Logistic
{
    public class RKH1 : EntityFactory<RKHHEADER1, RKHHEADER1, FilterRKH1, PMSContextBase>
    {
        private Period _periodService;
        private Activity _activityService;
        private Block _blockService;
        private Employee _employeeService;
        private AuthenticationServiceBase _authenticationService;

        private List<RKHMANDOR1> _newRKHMANDOR1s = new List<RKHMANDOR1>();
        private List<RKHBLOCK1> _newRKHBLOCK1s = new List<RKHBLOCK1>();
        private List<RKHEMPLOYEE1> _newRKHEMPLOYEE1s = new List<RKHEMPLOYEE1>();
        private List<RKHMATERIAL1> _newRKHMATERIAL1s = new List<RKHMATERIAL1>();

        private RKH1 RKH1_old;

        public RKH1(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "RKHHEADER1";
            _authenticationService = authenticationService;
            _periodService = new Period(context, _authenticationService, auditContext);

            _activityService = new Activity(context, _authenticationService, auditContext);
            _blockService = new Block(context, _authenticationService, auditContext);
            _employeeService = new Employee(context, _authenticationService, auditContext);

        }

        protected override RKHHEADER1 GetSingleFromDB(params object[] keyValues)
        {
            RKHHEADER1 record =
            _context.RKHHEADER1
            .Include(d => d.RKHMANDOR1)
            .Include(d => d.RKHBLOCK1)
            .Include(d => d.RKHEMPLOYEE1)
            .Include(d => d.RKHMATERIAL1)
            .SingleOrDefault(d => d.RKH_CODE.Equals(keyValues[0]));

            if (record != null)
            {
                List<string>
                            mandorIds = record.RKHMANDOR1.Select(d => d.MANDORID).Distinct().ToList(),
                            blockIds = record.RKHBLOCK1.Select(d => d.BLOCKID).Distinct().ToList(),
                            empIds = record.RKHEMPLOYEE1.Select(d => d.EMPID).Distinct().ToList(),
                            materialIds = record.RKHMATERIAL1.Select(d => d.MATERIALID).Distinct().ToList();



                if (mandorIds.Any())
                {
                    var activities = _context.MACTIVITY.Where(d => record.RKHMANDOR1.Select(s => s.ACTID).Distinct().ToList().Contains(d.ACTIVITYID)).ToList();
                    var memployees = _context.MEMPLOYEE.Where(d => record.RKHMANDOR1.Select(s => s.MANDORID).Distinct().ToList().Contains(d.EMPID)).ToList();

                    (
                    from a in record.RKHMANDOR1
                    join b in activities on a.ACTID equals b.ACTIVITYID
                    join c in memployees on a.MANDORID equals c.EMPID
                    select new { RKHMANDOR1 = a, MACTIVITY = b , MEMPLOYEE = c }
                    ).ToList().ForEach(d => {
                        d.RKHMANDOR1.ACTID = d.MACTIVITY.ACTIVITYID;
                        d.RKHMANDOR1.ACTIVITY = d.MACTIVITY;
                        d.RKHMANDOR1.MANDORID = d.RKHMANDOR1.MANDORID;
                        d.RKHMANDOR1.MANDOR = d.MEMPLOYEE;
                    });

                }
            }              

            return record;

        }

        public RKHHEADER1 AddDetail (RKHHEADER1 record)
        {
            record.DIV = _context.MDIVISI.Where(d => d.DIVID.Equals(record.RKH_DIVID)).SingleOrDefault();

            //RKHMANDOR
            foreach (var mandor in record.RKHMANDOR1)
            {
                mandor.MANDOR = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(mandor.MANDORID)).SingleOrDefault();
            }
            //RKHBLOCK
            foreach (var block in record.RKHBLOCK1)
            {
                block.MBLOCK = _context.MBLOCK.Where(d => d.BLOCKID.Equals(block.BLOCKID)).SingleOrDefault();
            }
            //RKHEMPLOYEE
            foreach (var employee in record.RKHEMPLOYEE1)
            {
                employee.MEMPLOYEE  = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(employee.EMPID)).SingleOrDefault();
            }
            //RKHMATERIAL
            foreach (var material in record.RKHMATERIAL1)
            {
                material.MATERIAL = _context.MMATERIAL.Where(d => d.MATERIALID.Equals(material.MATERIALID)).SingleOrDefault();
            }

            return record;
        }

        public RKHHEADER1 RemoveDetail(RKHHEADER1 record)
        {
            record.DIV = null;

            //RKHMANDOR1
            foreach (var mandor in record.RKHMANDOR1)
            {
                mandor.MANDOR = null;
            }
            //RKHBLOCK1
            foreach (var block in record.RKHBLOCK1)
            {
                block.MBLOCK = null;
            }
            //RKHEMPLOYEE1
            foreach (var employee in record.RKHEMPLOYEE1)
            {
                employee.MEMPLOYEE = null;
            }
            //RKHMATERIAL1
            foreach (var material in record.RKHMATERIAL1)
            {
                material.MATERIAL = null;
            }

            return record;

        }

        public override RKHHEADER1 CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            RKHHEADER1 record = base.CopyFromWebFormData(formData, newRecord);
            record.RKHMANDOR1.Clear();
            _newRKHMANDOR1s = new List<RKHMANDOR1>();
            _newRKHMANDOR1s.CopyFrom<RKHMANDOR1>(formData, "RKHMANDOR1");
            _newRKHMANDOR1s.ForEach(d =>
            { record.RKHMANDOR1.Add(d); }
            );

            record.RKHBLOCK1.Clear();
            _newRKHBLOCK1s = new List<RKHBLOCK1>();
            _newRKHBLOCK1s.CopyFrom<RKHBLOCK1>(formData, "RKHBLOCK1");
            _newRKHBLOCK1s.ForEach(d =>
            { record.RKHBLOCK1.Add(d); }
            );

            record.RKHEMPLOYEE1.Clear();
            _newRKHEMPLOYEE1s = new List<RKHEMPLOYEE1>();
            _newRKHEMPLOYEE1s.CopyFrom<RKHEMPLOYEE1>(formData, "RKHEMPLOYEE1");
            _newRKHEMPLOYEE1s.ForEach(d =>
            { record.RKHEMPLOYEE1.Add(d); }
            );

            record.RKHMATERIAL1.Clear();
            _newRKHMATERIAL1s = new List<RKHMATERIAL1>();
            _newRKHMATERIAL1s.CopyFrom<RKHMATERIAL1>(formData, "RKHMATERIAL1");
            _newRKHMATERIAL1s.ForEach(d =>
            { record.RKHMATERIAL1.Add(d); }
            );

            return record;
        }

        private string FieldsValidation(RKHHEADER1 RKH1)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(RKH1.RKH_CODE)) result += "Kode RKH tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(RKH1.RKH_DIVID)) result += "Kode Divisi tidak boleh kosong." + Environment.NewLine;
            if (RKH1.RKH_DATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (RKH1.RKH_ACTDATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (RKH1.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;

            //RKH Mandor
            foreach (var item in RKH1.RKHMANDOR1)
            {
                if (string.IsNullOrEmpty(item.RKH_MANDOR_CODE)) result += "Kode Mandor tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.RKH_CODE)) result += "Kode RKH tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.MANDORID)) result += "Mandor tidak boleh kosong." + Environment.NewLine;
            }

            //RKH Block
            foreach (var item in RKH1.RKHBLOCK1)
            {
                if (string.IsNullOrEmpty(item.RKH_BLOCK_CODE)) result += "Kode Block tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.RKH_MANDOR_CODE)) result += "Kode Mandor tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.BLOCKID)) result += "Blok tidak boleh kosong." + Environment.NewLine;
                if (item.OUTPUT <= 0) result += "Output tidak boleh kosong." + Environment.NewLine;
            }

            //RKH Employee
            foreach (var item in RKH1.RKHEMPLOYEE1)
            {
                if (string.IsNullOrEmpty(item.RKH_EMPLOYEE_CODE)) result += "Kode Block tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.RKH_MANDOR_CODE)) result += "Kode Mandor tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.EMPID)) result += "Karyawan tidak boleh kosong." + Environment.NewLine;
            }

            //RKH Material
            foreach (var item in RKH1.RKHMATERIAL1)
            {
                if (string.IsNullOrEmpty(item.RKH_MATERIAL_CODE)) result += "Kode Block tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.RKH_MANDOR_CODE)) result += "Kode Mandor tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.MATERIALID)) result += "Material tidak boleh kosong." + Environment.NewLine;
                if (item.QTY <= 0) result += "Qty tidak boleh kosong." + Environment.NewLine;
            }

            return result;
        }

        private void Validate(RKHHEADER1 RKH1)
        {
            _periodService.CheckValidPeriod(RKH1.DIV.UNITCODE, RKH1.RKH_DATE);
        }

        private void InsertValidate(RKHHEADER1 RKH1)
        {
            this.Validate(RKH1);

            RKHHEADER1 RKH1Exist = _context.RKHHEADER1.Where(d => d.RKH_CODE.Equals(RKH1.RKH_CODE)).SingleOrDefault();
            if (RKH1Exist != null)
                throw new Exception("RKH dengan nomor " + RKH1.RKH_CODE + " sudah ada.");

            RKHHEADER1 RKH1ActDate =
                _context.RKHHEADER1.Where(d => d.STATUS.Equals("A") && d.STATUS.Equals("P") && d.RKH_DATE == RKH1.RKH_DATE && d.RKH_DIVID == RKH1.RKH_DIVID).SingleOrDefault();
            if (RKH1ActDate != null)
                throw new Exception("RKH dengan nomor " + RKH1.RKH_CODE + " sudah ada.");
        }

        private void UpdateValidate(RKHHEADER1 RKH1)
        {
            this.Validate(RKH1);

            var currData = GetSingle(RKH1.RKH_CODE);
            if (currData != null)
            {
                if (currData.STATUS == "A")
                    throw new Exception("Data sudah di approve.");
                if (currData.STATUS == "C")
                    throw new Exception("Data sudah di cancel.");
            }
        }

        private void DeleteValidate(RKHHEADER1 RKH1)
        {
            if (RKH1.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (RKH1.STATUS == "C")
                throw new Exception("Data sudah di cancel.");
            _periodService.CheckValidPeriod(RKH1.DIV.UNITCODE, RKH1.RKH_DATE);
        }

        private void ApproveValidate(RKHHEADER1 RKH1)
        {
            if (RKH1.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (RKH1.STATUS == "C")
                throw new Exception("Data sudah di cancel.");

            this.Validate(RKH1);
            _periodService.CheckMaxPeriod(RKH1.DIV.UNITCODE, RKH1.RKH_DATE);
        }

        private void CancelValidate(RKHHEADER1 RKH1)
        {
            if (RKH1.STATUS != "A")
                throw new Exception("Data belum di approve.");
            if (RKH1.STATUS == "C")
                throw new Exception("Data sudah di cancel.");
            _periodService.CheckValidPeriod(RKH1.DIV.UNITCODE, RKH1.RKH_DATE);
        }

        public override IEnumerable<RKHHEADER1> GetList(FilterRKH1 filter)
        {
            var criteria = PredicateBuilder.True<RKHHEADER1>();

            criteria = criteria.And(d => !d.STATUS.Equals("D"));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p =>
                    p.RKH_CODE.ToLower().Contains(filter.LowerCasedSearchTerm)
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(p => p.RKH_DIVID.StartsWith(filter.UnitID));

            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(p => p.RKH_DIVID.Equals(filter.DivisionID));

            if (!string.IsNullOrWhiteSpace(filter.RKHCode))
                criteria = criteria.And(p => p.RKH_CODE.Equals(filter.RKHCode));

            if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                criteria = criteria.And(p => p.RKH_DATE.Date >= filter.StartDate.Date && p.RKH_DATE.Date <= filter.EndDate.Date);

            if (filter.PageSize <= 0)
                return _context.RKHHEADER1.Where(criteria).ToList();

            return _context.RKHHEADER1.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public override RKHHEADER1 NewRecord(string userName)
        {
            RKHHEADER1 record = new RKHHEADER1();
            record.RKH_DATE = GetServerTime().Date;
            record.RKH_ACTDATE = GetServerTime().Date;
            record.STATUS = "";
            return record;
        }

        public string GenereteNewNumber(string divisionId, DateTime dateTime)
        {
            VDIVISI vdivisi = _context.VDIVISI.Find(divisionId);

            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.RkhACodePrefix + divisionId, _context);
            return PMSConstants.LoadingCodePrefix + "-" + vdivisi.CODE + "-" + vdivisi.UNITCODE
                + "-" + dateTime.ToString("yyyyMMdd") + "-" + lastNumber.ToString().PadLeft(4, '0');
        }

        protected override RKHHEADER1 BeforeSave(RKHHEADER1 record, string userName, bool newRecord)
        {
            return base.BeforeSave(record, userName, newRecord);
        }

        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["RKH_CODE"];
            var rkh1 = GetSingle(no);
            this.ApproveValidate(rkh1);

            rkh1.UPDATEBY = userName;
            rkh1.UPDATED = GetServerTime();
            rkh1.STATUS = "A";

            _context.Entry(rkh1).State = EntityState.Modified;
            _context.SaveChanges();

            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Approve {rkh1.RKH_CODE}", _context);


            return true;
        }

        public bool CancelApprove(IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["LOADINGCODE"];
            string canceledcomment = formDataCollection["CANCELEDCOMMENT"];
            var rfcResult = string.Empty;
            try
            {
                var rkh1 = GetSingle(no);
                this.CancelValidate(rkh1);

                var newRKH1 = new RKHHEADER1
                {

                    RKH_CODE = this.GenereteNewDerivedNumber(rkh1.RKH_CODE),
                    DIV = rkh1.DIV,
                    RKH_DIVID = rkh1.RKH_DIVID,
                    RKH_DATE = rkh1.RKH_DATE,
                    RKH_ACTDATE = rkh1.RKH_ACTDATE,
                    NOTE = rkh1.NOTE,
                    STATUS = "P",
                    CREATEBY = rkh1.UPDATEBY,
                    CREATED = rkh1.UPDATED,
                    UPDATEBY = rkh1.UPDATEBY,
                    UPDATED = rkh1.UPDATED,
                    RKHMANDOR1 = rkh1.RKHMANDOR1,
                    RKHBLOCK1 = rkh1.RKHBLOCK1,
                    RKHEMPLOYEE1 = rkh1.RKHEMPLOYEE1,
                    RKHMATERIAL1 = rkh1.RKHMATERIAL1,
                };


                var local = _context.Set<RKHHEADER1>()
                .Local
                .SingleOrDefault(entry => entry.RKH_CODE.Equals(newRKH1.RKH_CODE) || entry.Equals(rkh1.RKH_CODE));
                if (local != null) { _context.Entry(local).State = EntityState.Detached; }

                SaveInsert(newRKH1, userName);

                RKHHEADER1 rkh1cancel = _context.RKHHEADER1.Where(d => d.RKH_CODE.Equals(no)).SingleOrDefault();
                rkh1cancel.CANCELEDCOMMENT = canceledcomment;
                rkh1cancel.UPDATEBY = userName;
                rkh1cancel.UPDATED = GetServerTime();
                rkh1cancel.STATUS = "C";

                _context.Entry(rkh1cancel).State = EntityState.Modified;
                _context.SaveChanges();
                Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Cancel {rkh1cancel.RKH_CODE}", _context);

            }
            catch
            { throw; }

            return true;
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

    }

}
