using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using PMS.EFCore.Helper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.Organization;
using AM.EFCore.Services;
using PMS.Shared.Models;

namespace PMS.EFCore.Services.Logistic
{
    public class RKH : EntityFactory<TRKH, TRKH, GeneralFilter, PMSContextBase>
    {
        private Period _periodService;
        private Divisi _divisiService;
        private Block _blockService;
        private Activity _activityService;
        private Material _materialService;
        private Employee _employeeService;
        private Harvesting _harvestingService;
        private AuthenticationServiceBase _authenticationService;

        List<TRKHACTUAL> _rkhActuals = new List<TRKHACTUAL>();
        List<TRKHTAKSASI> _rkhTaksasis = new List<TRKHTAKSASI>();
        List<TRKHMATERIAL> _rkhMaterials = new List<TRKHMATERIAL>();
        List<TRKHHERBISIDA> _rkhHerbisidas = new List<TRKHHERBISIDA>();
        List<TRKHDETAIL> _rkhDetails = new List<TRKHDETAIL>();
        TRKHESTPANEN _estimasiPanen = new TRKHESTPANEN();
        TRKHHASILKERJA _hasilKerja = new TRKHHASILKERJA();

        public RKH(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "RKH";
            _authenticationService = authenticationService;
            _periodService = new Period(context,_authenticationService,auditContext);
            _divisiService = new Divisi(context,_authenticationService,auditContext);
            _blockService = new Block(context,_authenticationService, auditContext);
            _activityService = new Activity(context,_authenticationService,auditContext);
            _materialService = new Material(context,_authenticationService, auditContext);
            _employeeService = new Employee(context,_authenticationService,auditContext);
            _harvestingService = new Harvesting(context,_authenticationService,auditContext);            
        }

        protected override TRKH GetSingleFromDB(params  object[] keyValues)
        {
            TRKH record = _context.TRKH
                .Include(d => d.DIV)
                .Include(d => d.TRKHDETAIL)
                .Include(d => d.TRKHTAKSASI)
                .Include(d => d.TRKHESTPANEN)
                .Include(d => d.TRKHHASILKERJA)
                .Include(d => d.TRKHMATERIAL)
                .Include(d => d.TRKHHERBISIDA)
                .Include(d => d.TRKHACTUAL)
                .FirstOrDefault(d => d.ID.Equals(keyValues[0]));



            if (record != null)
            {
                List<string> activityIds = record.TRKHDETAIL.Select(d => d.ACTID).Distinct().ToList(),
                             empIds = record.TRKHDETAIL.Select(d => d.SPVID).Distinct().ToList(),
                             blockIds = record.TRKHDETAIL.Select(d => d.BLOCKID)
                                        .Union(record.TRKHTAKSASI.Select(d => d.BLOCKID))
                                    .ToList(),
                             materialIds = record.TRKHMATERIAL.Select(d => d.MATID)
                                    .Union(record.TRKHHERBISIDA.Select(d => d.MATID))
                                    .ToList();
                if (activityIds.Any())
                    record.VRKHACTIVITYREF = _activityService.GetList(new FilterActivity { Ids = activityIds })
                                        .Select(d => new VRKHACTIVITY {
                                            ACTID = d.ACTIVITYID,
                                            ACTNAME = $"{d.ACTIVITYID} - {d.ACTIVITYNAME}",
                                            UOM = d.UOM1
                                        })
                                        .ToList();

                if (materialIds.Any())
                    record.VRKHMATERIALREF = _materialService.GetList(new FilterMaterial { Ids = materialIds })
                                    .Select(d => new VRKHMATERIAL
                                    {

                                        MATID = d.MATERIALID,
                                        MATNAME = $"{d.MATERIALID} - {d.MATERIALNAME}",
                                        UOM = d.UOM
                                    })
                                    .ToList();

                if (empIds.Any())
                    record.VRKHMANDORREF = _employeeService.GetList(new FilterEmployee { Ids = empIds })
                                    .Select(d => new VRKHMANDOR
                                    {

                                        SPVID = d.EMPID,
                                        SPVNAME = $"{d.EMPID} - {d.EMPNAME}"
                                    })
                                    .ToList();
                if (blockIds.Any())
                    record.VRKHBLOCKREF = _blockService.GetList(new GeneralFilter { Ids = blockIds })
                                .Select(d => new VRKHBLOCK
                                {

                                    BLOCKID = d.BLOCKID,
                                    BLOCKCODE = d.CODE,
                                    LUASBLOCK = d.LUASBLOCK,
                                    SPH = d.SPH,
                                    THNTANAM = d.THNTANAM,
                                    TOPOGRAFI = d.TOPOGRAPI
                                })
                                .ToList();

            }



            return record;
        }



        public sp_Rkh_PlanActual_Result GetActual(FilterRKH filter)
        {
            return _context.sp_Rkh_PlanActual(filter.DivisionID, filter.ActualDate, filter.PaymentType, filter.ActivityID, filter.UpkeepID).FirstOrDefault();
        }


        public override IEnumerable<TRKH> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TRKH>();

            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                bool allDivision = true;
                List<string> authorizedDivisionIds = new List<string>();

                //Authorization check
                //Get Authorization By User Name

                _authenticationService.GetAuthorizedLocationIdByUserName(filter.UserName, out allDivision);

                if (!allDivision)
                {
                    authorizedDivisionIds = _authenticationService.GetAuthorizedDivisi(filter.UserName, string.Empty, string.Empty)
                        .Select(d => d.DIVID)
                        .ToList();

                    criteria = criteria.And(p => authorizedDivisionIds.Contains(p.DIVID));
                }
            }

            criteria = criteria.And(d => !d.STATUS.Equals("D"));
            criteria = criteria.And(d =>
               (d.ACTDATE.Date >= filter.StartDate.Date && d.ACTDATE.Date <= filter.EndDate.Date) ||
               (d.DATE.Date >= filter.StartDate.Date && d.DATE.Date <= filter.EndDate.Date));

            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(d => d.DIVID.Substring(0, 4).Equals(filter.UnitID));
            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(d => d.DIVID.Equals(filter.DivisionID));
            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));
            if (filter.PageSize <= 0)
                return _context.TRKH.Where(criteria);
            return _context.TRKH.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public override TRKH NewRecord(string userName)
        {
            TRKH record = new TRKH();
            record.DATE = GetServerTime().Date;
            record.ACTDATE = GetServerTime().Date;
            record.STATUS = "";

            return record;
        }
        protected override TRKH BeforeSave(TRKH record, string userName, bool newRecord)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.ID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) result += "Divisi tidak boleh kosong." + Environment.NewLine;

            VDIVISI divisi = _divisiService.GetSingle(record.DIVID);
            if (divisi == null)
                result += "Divisi tidak valid." + Environment.NewLine;


            if (record.DATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (record.ACTDATE == new DateTime()) result += "Tanggal aktual tidak boleh kosong." + Environment.NewLine;
            if (record.STATUS.Equals(PMSConstants.TransactionStatusNone)) result += "Status tidak boleh kosong." + Environment.NewLine;

            if (divisi != null)
            {
                try
                {
                    _periodService.CheckValidPeriod(divisi.UNITCODE, record.DATE);
                    _periodService.CheckValidPeriod(divisi.UNITCODE, record.ACTDATE);
                }
                catch (Exception ex)
                {
                    result += "Error cek periode: " + ex.Message + ".\r\n";
                }

            }

            if (!string.IsNullOrWhiteSpace(result))
                throw new Exception(result);

            if (!_authenticationService.IsAuthorizedDivisi(userName, record.DIVID))
                throw new ExceptionNoDivisionAccess(record.DIVID);
            DateTime now = GetServerTime();
            if (newRecord)
            {
                if (_context.TRKH.Where(d => d.DIVID.Equals(record.DIVID) && d.ACTDATE.Date == record.ACTDATE.Date).FirstOrDefault() != null)
                    throw new Exception("RKH untuk tanggal " + record.ACTDATE.ToString("dd/MM/yyyy") + " sudah ada.");

                _autoNumberPrefix = PMSConstants.RkhCodePrefix + divisi.UNITCODE;

                int lastNumber = GetCurrentDocumentNumber();
                record.ID = PMSConstants.RkhCodePrefix + divisi.UNITCODE + record.DATE.ToString("yyyyMMdd") + lastNumber.ToString().PadLeft(4, '0');
                record.STATUS = PMSConstants.TransactionStatusProcess;

                
            }
            else
            {
                if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusApproved))
                    throw new Exception("Data sudah di approve.");
                if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusCanceled))
                    throw new Exception("Data sudah di cancel.");
                if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusDeleted))
                    throw new Exception("Data sudah di hapus.");

            }

            record.UPDATED = now;
            return record;
        }


        public override TRKH CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TRKH record = base.CopyFromWebFormData(formData, newRecord);
            _rkhActuals = new List<TRKHACTUAL>();
            _rkhTaksasis = new List<TRKHTAKSASI>();
            _rkhMaterials = new List<TRKHMATERIAL>();
            _rkhHerbisidas = new List<TRKHHERBISIDA>();
            _rkhDetails = new List<TRKHDETAIL>();

            _rkhTaksasis = CopyTaksasiFromWebFormData(formData);            
            _rkhMaterials = CopyMaterialFromWebFormData(formData);
            _rkhHerbisidas = CopyHerbisidaFromWebFormData(formData);
            _estimasiPanen = CopyEstimasiPanenFromWebFormData(formData);
            _hasilKerja = CopyHasilFromWebFormData(formData);
            _rkhActuals = CopyActualFromWebFormData(formData);
            _rkhDetails = CopyDetailFromWebFormData(formData);
            
            return record;
        }

        private List<TRKHHERBISIDA> CopyHerbisidaFromWebFormData(IFormCollection formData)
        {
            List<TRKHHERBISIDA> result = new List<TRKHHERBISIDA>();
            result.CopyFrom<TRKHHERBISIDA>(formData, "TRKHHERBISIDA");
            return result;
        }

        private List<TRKHTAKSASI> CopyTaksasiFromWebFormData(IFormCollection formData)
        {
            List<TRKHTAKSASI> result = new List<TRKHTAKSASI>();
            result.CopyFrom<TRKHTAKSASI>(formData, "TRKHTAKSASI");
            return result;
        }

        private List<TRKHMATERIAL> CopyMaterialFromWebFormData(IFormCollection formData)
        {
            List<TRKHMATERIAL> result = new List<TRKHMATERIAL>();
            result.CopyFrom<TRKHMATERIAL>(formData, "TRKHMATERIAL");
            return result;
        }



        private List<TRKHDETAIL> CopyDetailFromWebFormData(IFormCollection formData)
        {
            List<TRKHDETAIL> result = new List<TRKHDETAIL>();
            result.CopyFrom<TRKHDETAIL>(formData, "TRKHDETAIL");
            return result;
        }

        private List<TRKHACTUAL> CopyActualFromWebFormData(IFormCollection formData)
        {
            List<TRKHACTUAL> result = new List<TRKHACTUAL>();
            result.CopyFrom<TRKHACTUAL>(formData, "TRKHACTUAL");
            return result;
        }

        private TRKHESTPANEN CopyEstimasiPanenFromWebFormData(IFormCollection formData)
        {
            string rkhId = formData["Id"];
            TRKHESTPANEN result = _context.TRKHESTPANEN.Find(rkhId);
            if (result == null)
                result = new TRKHESTPANEN();
            result.CopyFrom(formData);
            return result;
        }

        private TRKHHASILKERJA CopyHasilFromWebFormData(IFormCollection formData)
        {
            string rkhId = formData["Id"];
            TRKHHASILKERJA result = _context.TRKHHASILKERJA.Find(rkhId);
            if (result == null)
                result = new TRKHHASILKERJA();
            result.CopyFrom(formData);
            return result;
        }


       

        
        protected override TRKH AfterSave(TRKH record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                IncreaseRunningNumber();
                _autoNumberPrefix = string.Empty;
            }
            return record;
        }

        protected override TRKH BeforeDelete(TRKH record, string userName)
        {
            

            if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusApproved))
                throw new Exception("Data sudah di approve.");
            if (_existingRecord.STATUS.Equals(PMSConstants.TransactionStatusCanceled))
                throw new Exception("Data sudah di cancel.");
            if (!_authenticationService.IsAuthorizedDivisi(userName, record.DIVID))
                throw new ExceptionNoDivisionAccess(record.DIVID);
            VDIVISI divisi = _divisiService.GetSingle(record.DIVID);
            _periodService.CheckValidPeriod(divisi.UNITCODE, record.DATE);
            return record;
        }


        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string rkhId = formDataCollection["ID"];
            return Approve(rkhId, userName);
        }
        public bool Approve(string rkhId, string userName)
        {
            if (string.IsNullOrWhiteSpace(rkhId))
                throw new Exception("Id tidak boleh kosong");
            TRKH record = GetSingle(rkhId);
            return Approve(record, userName);

        }

        public bool Approve(TRKH record, string userName)
        {
            if (record == null)
                throw new Exception("RKH tidak ditemukan");

            if (record.STATUS.Equals(PMSConstants.TransactionStatusDeleted))
                throw new Exception("RKH sudah dihapus");
            if (record.STATUS.Equals(PMSConstants.TransactionStatusApproved))
                throw new Exception("RKH sudah diappove");
            if (record.STATUS.Equals(PMSConstants.TransactionStatusCanceled))
                throw new Exception("RKH sudah dicancel");
            if (!_authenticationService.IsAuthorizedDivisi(userName, record.DIVID))
                throw new ExceptionNoDivisionAccess(record.DIVID);
            record.STATUS = PMSConstants.TransactionStatusApproved;
            record.UPDATED = GetServerTime();
            _context.SaveChanges();
            return true;
        }
        public bool CancelApprove(IFormCollection formDataCollection, string userName)
        {
            string rkhId = formDataCollection["ID"];
            return CancelApprove(rkhId, userName);
        }
        public bool CancelApprove(string rkhId, string userName)
        {
            if (string.IsNullOrWhiteSpace(rkhId))
                throw new Exception("Id tidak boleh kosong");
            TRKH record = GetSingle(rkhId);
            return CancelApprove(record, userName);

        }
        public bool CancelApprove(TRKH record, string userName)
        {
            if (record == null)
                throw new Exception("RKH tidak ditemukan");
            if (record.STATUS.Equals(PMSConstants.TransactionStatusDeleted))
                throw new Exception("RKH sudah dihapus");
            if (!record.STATUS.Equals(PMSConstants.TransactionStatusApproved))
                throw new Exception("RKH belum diappove");
            if (!_authenticationService.IsAuthorizedDivisi(userName, record.DIVID))
                throw new ExceptionNoDivisionAccess(record.DIVID);
            record.STATUS = PMSConstants.TransactionStatusCanceled;
            record.UPDATED = GetServerTime();
            _context.SaveChanges();
            return true;
        }
        public void SaveActuals(TRKH record, List<TRKHDETAIL> details, List<TRKHACTUAL> actuals, string userName, bool externalCommit)
        {
            List<string> actspv = details.Select(d => d.ACTID + "***" + d.SPVID).Distinct().ToList();
            List<TRKHACTUAL> validActuals = actuals.Where(d => actspv.Contains((d.ACTID + "***" + d.SPVID))).ToList();
            _context.TRKHACTUAL.RemoveRange(_context.TRKHACTUAL.Where(d => d.RKHID.Equals(record.ID)));
            if (validActuals != null && validActuals.Any())
            {
                validActuals.ForEach(d => { d.RKHID = record.ID; });
                _context.TRKHACTUAL.AddRange(validActuals);
            }
            if (!externalCommit)
                _context.SaveChanges();

        }
        public void SaveActuals(TRKH record, List<TRKHDETAIL> details, string userName, bool externalCommit)
        {
            List<TRKHACTUAL> validActuals = new List<TRKHACTUAL>();
            details.Select(d => new { d.ACTID, d.SPVID }).Distinct().ToList().ForEach(d => {
                validActuals.Add(new TRKHACTUAL
                {
                    RKHID = record.ID,
                    ACTID = d.ACTID,
                    SPVID = d.SPVID
                });
            });

            _context.TRKHACTUAL.RemoveRange(_context.TRKHACTUAL.Where(d => d.RKHID.Equals(record.ID)));
            if (validActuals != null && validActuals.Any())
                _context.TRKHACTUAL.AddRange(validActuals);
            if (!externalCommit)
                _context.SaveChanges();

        }

        public void SaveEstimasiPanen(string rkhId, TRKHESTPANEN estimasiPanen, string userName, bool externalCommit)
        {
            estimasiPanen.RKHID = rkhId;
            if (_context.TRKHESTPANEN.Find(estimasiPanen.RKHID) == null)
                _context.TRKHESTPANEN.Add(estimasiPanen);
            else
                _context.TRKHESTPANEN.Update(estimasiPanen);
            if (!externalCommit)
                _context.SaveChanges();
        }

        private void ValidateBlocks(TRKH record, List<TRKHDETAIL> details)
        {
            List<string> blockIds = details.Select(d => d.BLOCKID).Distinct().ToList();
            List<string> activityIds = details.Select(d => d.ACTID).Distinct().ToList();
            List<VBLOCK> blocks = _blockService.GetList(new FilterBlock { BlockIDs = blockIds }).ToList();
            List<MACTIVITY> activities = _activityService.GetList(new FilterActivity { Ids = activityIds }).ToList();





            var errorBlocks = (
                    from a in details
                    join e in activities on a.ACTID equals e.ACTIVITYID into ae
                    from d in ae.DefaultIfEmpty()

                    join b in blocks on a.BLOCKID equals b.BLOCKID into ab
                    from c in ab.DefaultIfEmpty()

                    where (c == null || d == null || (d.UOM1.Equals("HA") && a.VOL > c.LUASBLOCK))

                    select new {
                        a.BLOCKID,
                        a.ACTID,
                        a.VOL,
                        c.LUASBLOCK,
                        InvalidBlock = (c == null) ? true : false,
                        InvalidActivity = (d == null) ? true : false
                    })
                        .Distinct()
                        .ToList();


            if (errorBlocks == null || !errorBlocks.Any())
                return;

            string errorMessage = string.Empty;
            errorBlocks.ForEach(d => {
                if (d.InvalidBlock)
                    errorMessage += $"Block {d.BLOCKID} tidak valid\r\n";
                else if (d.InvalidActivity)
                    errorMessage += $"Activity {d.ACTID} tidak valid\r\n";
                else
                    errorMessage += "Volume blok " + d.BLOCKID + " (" + d.VOL.ToString("#,#0.00") + ") lebih besar dari luas blok (" + d.LUASBLOCK.ToString("#,#0.00") + "). Kegiatan - " + d.ACTID + "\r\n";
            });
            throw new Exception(errorMessage);
        }

        private void ValidateActivities(TRKH record, List<TRKHDETAIL> details)
        {
            var checkActivities = (
                from a in details
                join b in _context.MACTIVITY on a.ACTID equals b.ACTIVITYID
                where (!b.HV && !b.GA && !b.RA)
                select new { a.ACTID, a.EMPCOUNT }).Distinct().ToList();

            if (checkActivities == null || !checkActivities.Any())
                return;

            VDIVISI divisi = _divisiService.GetSingle(record.DIVID);

            int upkeepCount = 0;
            checkActivities.ForEach(d => { upkeepCount += d.EMPCOUNT; });
            int actualQuota = _context.GetRKHQuota(divisi.UNITCODE, record.ACTDATE.Date, (int)record.PAYMENTTYPE, record.ID);

            int quota = 0;
            var sQuota = (HelperService.GetConfigValue(PMSConstants.CfgAtendanceCardQuotaUpkeep + divisi.UNITCODE, _context));
            int.TryParse(sQuota, out quota);
            if (upkeepCount + actualQuota > quota) throw new Exception("Jumlah karyawan melebihi quota ( " + quota.ToString("#0") + " ), sudah terpakai " + actualQuota.ToString("#0") + ".");

        }



        private void ValidateTaksasiPanen(TRKH record, List<TRKHTAKSASI> taksasiPanens, TRKHESTPANEN estimasiPanen)
        {   
            if (taksasiPanens != null && taksasiPanens.Any())
            {


                decimal totalKg = 0, totalBasis = 0, totalLebihBasis = 0, totalOutputPemanen = 0, totalEstTkPanen = 0, totalAncakTk = 0;
                DateTime _3daysBefore = record.ACTDATE.AddDays(-3).Date;


                (
                    from a in taksasiPanens
                    join b in _harvestingService.GetHarvestedArea(_3daysBefore, _3daysBefore, PMSConstants.HarvestTypePotongBuah,
                                taksasiPanens.Select(d => d.BLOCKID).Distinct().ToList(),string.Empty, string.Empty)
                    on a.BLOCKID equals b.BLOCKID into ab
                    from c in ab.DefaultIfEmpty()
                    select new { TAKSASI = a, HARVESTAREA = (c == null ? 0 : c.HARVESTAREA) }
                ).ToList().ForEach(a => {

                    var item = a.TAKSASI;
                    if (item.WORKAREA > item.BLOCKAREA)
                        throw new Exception("Luas kerja blok " + item.BLOCKID + " (" + item.WORKAREA.ToString("#,#0.00") + ") lebih besar dari luas blok (" + item.BLOCKAREA.ToString("#,#0.00") + ").");

                    if (item.TOPOGRAFI == PMSConstants.ArestaBlockTopografiBerbukit && item.AKP <= 20M && item.ANCAKTK > 3.5M)
                        throw new Exception("Area berbukit dengan akp <= 20%, ancak/TK tidak boleh lebih dari 3,5 Ha");
                    else if (item.TOPOGRAFI == PMSConstants.ArestaBlockTopografiDatar && item.AKP <= 20M && item.ANCAKTK > 5M)
                        throw new Exception("Area datar dengan akp <= 20%, ancak/TK tidak boleh lebih dari 5 Ha");

                    
                    if (a.HARVESTAREA + item.WORKAREA > item.BLOCKAREA)
                        throw new Exception("Luas panen blok " + item.BLOCKID
                            + " antara tanggal " + _3daysBefore.ToString("dd/MM/yyyy")
                            + " dan " + record.ACTDATE.ToString("dd/MM/yyyy") + " tidak boleh lebih dari "
                            + item.BLOCKAREA + " (luas panen saat ini = " + a.HARVESTAREA + ").");


                    item.JANJANG = item.WORKAREA * (item.SPH / 100M) * item.AKP;
                    item.KG = item.JANJANG * item.BJR;
                    item.OUTPUTPANEN = item.BASIS + item.LEBIHBASIS;
                    if (item.OUTPUTPANEN > 0)
                        item.ESTBUTUHPANEN = Math.Round(item.KG / item.OUTPUTPANEN);
                    else
                        item.ESTBUTUHPANEN = 0;

                    if (item.ESTBUTUHPANEN > 0)
                        item.ANCAKTK = item.WORKAREA / item.ESTBUTUHPANEN;
                    else
                        item.ANCAKTK = 0;


                    totalKg += item.KG;
                    totalBasis += item.BASIS;
                    totalLebihBasis += item.LEBIHBASIS;
                    totalOutputPemanen += item.OUTPUTPANEN;
                    totalEstTkPanen += item.ESTBUTUHPANEN;
                    totalAncakTk += item.ANCAKTK;

                });


                
                    
                
                estimasiPanen.BASIS = totalBasis / taksasiPanens.Count();
                estimasiPanen.LEBIH = totalLebihBasis / taksasiPanens.Count();
                estimasiPanen.ANCAKPANEN = totalAncakTk / taksasiPanens.Count();
                if (estimasiPanen.OUTPUTANGKUT > 0)
                    estimasiPanen.RITASI = totalKg / estimasiPanen.OUTPUTANGKUT;
                else
                    estimasiPanen.RITASI = 0;
                if (estimasiPanen.ESTBRONDOLPCT > 0)
                    estimasiPanen.ESTBRONDOLKG = totalKg * 0.01M * estimasiPanen.ESTBRONDOLPCT;
                else
                    estimasiPanen.ESTBRONDOLKG = 0;
                if (estimasiPanen.OUTPUTKUTIP > 0)
                    estimasiPanen.ESTTKBRONDOL = estimasiPanen.ESTBRONDOLKG / estimasiPanen.OUTPUTKUTIP;
                else
                    estimasiPanen.ESTTKBRONDOL = 0;

                if (estimasiPanen.ESTTKBRONDOL > 0 && totalEstTkPanen > 0)
                    estimasiPanen.RATIO = totalEstTkPanen / estimasiPanen.ESTTKBRONDOL;
                else
                    estimasiPanen.RATIO = 0;
            }
        }

        public void SaveTaksasiPanen(TRKH record, List<TRKHTAKSASI> taksasiPanens, TRKHESTPANEN estimasiPanen, string userName, bool externalCommit)
        {
            taksasiPanens.ForEach(d => { d.RKHID = record.ID; });
            ValidateTaksasiPanen(record, taksasiPanens, estimasiPanen);
            _context.TRKHTAKSASI.RemoveRange(_context.TRKHTAKSASI.Where(d => d.RKHID.Equals(record.ID)));
            if (taksasiPanens != null && taksasiPanens.Any())
                _context.TRKHTAKSASI.AddRange(taksasiPanens);
            if (!externalCommit)
                _context.SaveChanges();
        }



        public void SaveHasilKerja(string rkhId, TRKHHASILKERJA hasil, string userName, bool externalCommit)
        {
            hasil.RKHID = rkhId;
            if (hasil.PRODKG <= 0 || hasil.BRONDOLKG<=0) hasil.BRONDOLPCT = 0;
            else hasil.BRONDOLPCT = hasil.BRONDOLKG / hasil.PRODKG * 100M;

            if (hasil.PANENTK <= 0 || hasil.OUTPUTPANEN<=0) hasil.OUTPUTPANEN = 0;
            else hasil.OUTPUTPANEN = hasil.PRODKG / hasil.PANENTK;

            if (hasil.PUPUKTK <= 0 || hasil.OUTPUTPUPUK<=0) hasil.OUTPUTPUPUK = 0;
            else hasil.OUTPUTPUPUK = hasil.PUPUKKG / hasil.PUPUKTK;

            if (_context.TRKHHASILKERJA.Find(hasil.RKHID) == null)
                _context.TRKHHASILKERJA.Add(hasil);
            else
                _context.TRKHHASILKERJA.Update(hasil);
            if (!externalCommit)
                _context.SaveChanges();
        }

        public void SaveDetails(TRKH record, List<TRKHDETAIL> details, string userName, bool externalCommit)
        {

            ValidateActivities(record, details);            
            ValidateBlocks(record, details);
            details.ForEach(d => { d.RKHID = record.ID; });
            _context.TRKHDETAIL.RemoveRange(_context.TRKHDETAIL.Where(d => d.RKHID.Equals(record.ID)));
            if (details != null && details.Any())
                _context.TRKHDETAIL.AddRange(details);
            if (!externalCommit)
                _context.SaveChanges();
        }

        public void SaveMaterials(string rkhId, List<TRKHDETAIL> details, List<TRKHMATERIAL> materials, string userName, bool externalCommit)
        {
            List<string> activityIds = details.Select(d => d.ACTID).Distinct().ToList();

            List<TRKHMATERIAL> validMaterials = materials.Where(d => activityIds.Contains((d.ACTID))).ToList();
            validMaterials.ForEach(d => { d.RKHID = rkhId; });
            _context.TRKHMATERIAL.RemoveRange(_context.TRKHMATERIAL.Where(d => d.RKHID.Equals(rkhId)));
            if (validMaterials != null && validMaterials.Any())
                _context.TRKHMATERIAL.AddRange(validMaterials);
            if (!externalCommit)
                _context.SaveChanges();
        }

        public void SaveMaterials(IFormCollection formDataCollection, string userName, bool externalCommit)
        {
            string rkhId = formDataCollection["Id"];
            if (string.IsNullOrWhiteSpace(rkhId))
                throw new Exception("Id tidak boleh kosong");
            List<TRKHMATERIAL> materials = CopyMaterialFromWebFormData(formDataCollection);
            List<TRKHDETAIL> details = CopyDetailFromWebFormData(formDataCollection);
            SaveMaterials(rkhId, details, materials, userName, externalCommit);
        }

        public void SaveHerbisidas(IFormCollection formDataCollection, string userName, bool externalCommit)
        {
            string rkhId = formDataCollection["Id"];
            if (string.IsNullOrWhiteSpace(rkhId))
                throw new Exception("Id tidak boleh kosong");
            List<TRKHHERBISIDA> herbisidas = CopyHerbisidaFromWebFormData(formDataCollection);            
            SaveHerbisidas(rkhId, herbisidas, userName, externalCommit);
        }

        public void SaveHerbisidas(string rkhId, List<TRKHHERBISIDA> herbisidas, string userName, bool externalCommit)
        {
            herbisidas.ForEach(d => { d.RKHID = rkhId; });
            _context.TRKHHERBISIDA.RemoveRange(_context.TRKHHERBISIDA.Where(d => d.RKHID.Equals(rkhId)));
            if (herbisidas != null && herbisidas.Any())
                _context.TRKHHERBISIDA.AddRange(herbisidas);
            if (!externalCommit)
                _context.SaveChanges();
        }

        protected override bool DeleteFromDB(TRKH record, string userName)
        {
            record.STATUS = PMSConstants.TransactionStatusDeleted;
            record.UPDATED = GetServerTime();
            CommitAllChanges();
            return true;
        }


        protected override TRKH SaveInsertToDB(TRKH record, string userName)
        {
            _internalCommit = false;
            base.SaveInsertToDB(record, userName);
            SaveDetails(record,_rkhDetails, userName, !_internalCommit);
            SaveActuals(record, _rkhDetails,_rkhActuals,  userName, !_internalCommit);
            SaveHasilKerja(record.ID, _hasilKerja, userName, !_internalCommit);            
            SaveTaksasiPanen(record, _rkhTaksasis, _estimasiPanen, userName, !_internalCommit);
            SaveEstimasiPanen(record.ID, _estimasiPanen, userName, !_internalCommit);
            SaveMaterials(record.ID,_rkhDetails, _rkhMaterials, userName, !_internalCommit);
            SaveHerbisidas(record.ID, _rkhHerbisidas, userName, !_internalCommit);
            CommitAllChanges();
            return GetSingle(record.ID);
        }

        protected override TRKH SaveUpdateToDB(TRKH record, string userName)
        {
            _internalCommit = false;
            SaveDetails(record, _rkhDetails, userName, !_internalCommit);
            SaveActuals(record, _rkhDetails, _rkhActuals, userName, !_internalCommit);
            SaveHasilKerja(record.ID, _hasilKerja, userName, !_internalCommit);
            SaveTaksasiPanen(record, _rkhTaksasis, _estimasiPanen, userName, !_internalCommit);
            SaveEstimasiPanen(record.ID, _estimasiPanen, userName, !_internalCommit);
            SaveMaterials(record.ID, _rkhDetails, _rkhMaterials, userName, !_internalCommit);
            SaveHerbisidas(record.ID, _rkhHerbisidas, userName, !_internalCommit);
            base.SaveUpdateToDB(record, userName);
            CommitAllChanges();
            return GetSingle(record.ID);
        }
        

        
    }
}
