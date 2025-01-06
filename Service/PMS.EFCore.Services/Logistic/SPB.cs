using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
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
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Logistic
{
    public class SPB : EntityFactory<TSPB,TSPB,FilterSPB, PMSContextBase>
    {
        private Period _servicePeriod;
        private Block _serviceBlock;
        private Divisi _divisiService;

        private List<TSPBDETAIL> _newTSPBDetails = new List<TSPBDETAIL>();
        private List<TKARTUTIMBANG> _newKartuTimbangs = new List<TKARTUTIMBANG>();
        private AuthenticationServiceBase _authenticationService;
        public SPB(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "SPB";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(_context,_authenticationService, auditContext);
            _serviceBlock = new Block(_context,_authenticationService, auditContext);
            _divisiService = new Divisi(context,_authenticationService,auditContext);
        }

        private string FieldsValidation(TSPB spb)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(spb.SPBNO)) result += "No tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(spb.MILLCODE)) result += "Mill tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(spb.DRIVERNAME)) result += "Driver tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(spb.OPERATORNAME)) result += "Operator tidak boleh kosong." + Environment.NewLine;
            if (spb.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;

            //SPB Item
            foreach(var item in spb.TSPBDETAIL)
            {
                if (string.IsNullOrEmpty(item.SPBNO)) result += "No tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.BLOCKID)) result += "Blok tidak boleh kosong." + Environment.NewLine;
            }

            //Kartu Timbang
            foreach (var item in spb.TKARTUTIMBANG)
            {
                if (string.IsNullOrEmpty(item.SPBNO)) result += "No tidak boleh kosong." + Environment.NewLine;
                if (item.CHECKDATE == new DateTime()) result += "Tanggal kartu timbang tidak boleh kosong." + Environment.NewLine;
                if (item.WEIGHT == 0) result += "Berat tidak boleh kosong." + Environment.NewLine;
            }

            return result;

        }

        private void Validate(TSPB spb)
        {
            string result = this.FieldsValidation(spb);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckValidPeriod(spb.DIV.UNITCODE, spb.SPBDATE.Date);

            TKARTUTIMBANG timbang1 = new TKARTUTIMBANG();
            TKARTUTIMBANG timbang2 = new TKARTUTIMBANG();

            foreach (var detail in spb.TKARTUTIMBANG)
            {
                if (detail.SRNO.Equals(1)) timbang1 = detail;
                else if (detail.SRNO.Equals(2)) timbang2 = detail;
            }

            if (timbang1.WEIGHT < timbang2.WEIGHT)
                throw new Exception("Berat Timbang 1 harus lebih besar dari Berat Timbang 2.");

            if (timbang1.CHECKDATE == timbang2.CHECKDATE)
                throw new Exception("Tangga & jam Timbang 1 tidak boleh sama dengan Timbang 2.");

            if (spb.SPBNO.Contains(" "))
                throw new Exception("Kode tidak boleh mengandung spasi.");

            if (spb.TSPBDETAIL.Count == 0)
                throw new Exception("SPB Detail tidak boleh kosong.");
        }

        private void InsertValidate(TSPB spb)
        {
            //this.Validate(spb);
            var spbExist = GetSingle(spb.SPBNO);
            if (spbExist != null)
                throw new Exception("Surat Pengantar Buah dengan kode: " + spb.SPBNO + " sudah ada. Silakan gunakan kode yang lain.");
        }

        private void UpdateValidate(TSPB spb)
        {
            if (spb.STATUS == "A")
                throw new Exception("Data sudah di approve.");

            //this.Validate(spb);
        }

        private void ApproveValidate(TSPB spb)
        {
            if (spb.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (spb.STATUS == "C")
                throw new Exception("Data sudah di cancel.");
            this.Validate(spb);
        }

        private void CancelValidate(TSPB spb)
        {
            if (spb.STATUS != "A")
                throw new Exception("Data belum di approve.");
            _servicePeriod.CheckValidPeriod(spb.DIV.UNITCODE, spb.SPBDATE.Date);
        }

        private void DeleteValidate(TSPB spb)
        {
            if (spb.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (spb.STATUS == "C")
                throw new Exception("Data sudah di cancel.");
            _servicePeriod.CheckValidPeriod(spb.DIV.UNITCODE, spb.SPBDATE.Date);
        }

        protected override TSPB GetSingleFromDB(params  object[] keyValues)
        {
            return _context.TSPB
            .Include(d => d.TSPBDETAIL)
            .Include(d => d.TKARTUTIMBANG)
            .Include(d => d.DIV)
            .SingleOrDefault(d => d.SPBNO.Equals(keyValues[0]));
        }

        public override TSPB CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TSPB record = base.CopyFromWebFormData(formData, newRecord);
            record.TSPBDETAIL.Clear();
            record.TKARTUTIMBANG.Clear();

            _newTSPBDetails = new List<TSPBDETAIL>();
            _newTSPBDetails.CopyFrom<TSPBDETAIL>(formData, "TSPBDETAIL");
            _newTSPBDetails.ForEach(d =>
            {
                record.TSPBDETAIL.Add(d);
            });

            _newKartuTimbangs = new List<TKARTUTIMBANG>();
            _newKartuTimbangs.CopyFrom<TKARTUTIMBANG>(formData, "TKARTUTIMBANG");
            _newKartuTimbangs.ForEach(d =>
            {
                record.TKARTUTIMBANG.Add(d);
            });


            return record;
        }

        protected override TSPB BeforeSave(TSPB record, string userName, bool newRecord)
        {
            VDIVISI divisi = _divisiService.GetSingle(record.DIVID);
            if (divisi == null)
                throw new Exception("Divisi tidak valid");

            record.DIV = _context.MDIVISI.Find(record.DIVID);
            record.SPBNODHS = StandardUtility.IsNull(record.SPBNODHS, string.Empty);
            record.VEHICLENO = StandardUtility.IsNull(record.VEHICLENO, string.Empty);
            record.DRIVERNAME = StandardUtility.IsNull(record.DRIVERNAME, string.Empty);
            record.OPERATORNAME = StandardUtility.IsNull(record.OPERATORNAME, string.Empty);
            record.REMARK = StandardUtility.IsNull(record.REMARK, string.Empty);
            record.TOTALSPB = StandardUtility.IsNull(record.TOTALSPB, 0);
            record.TOTALKT = StandardUtility.IsNull(record.TOTALKT, 0);
            record.TOTALTIMBANG = StandardUtility.IsNull(record.TOTALTIMBANG, 0);
            if (newRecord)
                record.SPBNO = divisi.UNITCODE + "-TBS-" + record.SPBDATE.ToString("MM") + "-" + record.SPBDATE.ToString("yy") + "-" + HelperService.GetCurrentDocumentNumber(record.DIVID, _context).ToString("0000");
            

            foreach (var item in record.TSPBDETAIL)
            {
                item.SPBNO = record.SPBNO;
                var block = _serviceBlock.GetSingle(item.BLOCKID);
                item.PLANTINGMONTH = block.BLNTANAM;
                item.PLANTINGYEAR = block.THNTANAM;
            }

            foreach (var item in record.TKARTUTIMBANG)
            {
                item.SPBNO = record.SPBNO;
            }
            if (newRecord)
            {
                record.STATUS = "P";
                this.InsertValidate(record);
                record.CREATEBY = userName;
                record.CREATED = GetServerTime();
            }
            else
            {
                _context.TSPBDETAIL.RemoveRange(_context.TSPBDETAIL.Where(d => d.SPBNO.Equals(record.SPBNO)));
                _context.TKARTUTIMBANG.RemoveRange(_context.TKARTUTIMBANG.Where(d => d.SPBNO.Equals(record.SPBNO)));
            }
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
            

            _saveDetails = (record.TKARTUTIMBANG.Any() || record.TSPBDETAIL.Any());

            return record;
        }

        protected override TSPB AfterSave(TSPB record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber( record.DIVID, _context);
            return record;
        }

       

        protected override TSPB BeforeDelete(TSPB record, string userName)
        {
            DeleteValidate(record);
            return record;
        }

        protected override TSPB SaveInsertDetailsToDB(TSPB record, string userName)
        {
            if (_newTSPBDetails != null)
            _context.TSPBDETAIL.AddRange(_newTSPBDetails);
            if(_newKartuTimbangs != null)
            _context.TKARTUTIMBANG.AddRange(_newKartuTimbangs);
            return record;
        }

        protected override TSPB SaveUpdateDetailsToDB(TSPB record, string userName)
        {
            DeleteDetailsFromDB(record, userName);
            return SaveInsertDetailsToDB(record, userName);
        }

        protected override bool DeleteDetailsFromDB(TSPB record, string userName)
        {
            _context.TSPBDETAIL.RemoveRange(_context.TSPBDETAIL.Where(d => d.SPBNO.Equals(record.SPBNO)));
            _context.TKARTUTIMBANG.RemoveRange(_context.TKARTUTIMBANG.Where(d => d.SPBNO.Equals(record.SPBNO)));
            return true; ;
        }

        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["SBNO"];
            TSPB spb = GetSingle(no);
            try
            {
                this.ApproveValidate(spb);
                spb.UPDATEBY = userName;
                spb.UPDATED = GetServerTime();
                spb.STATUS = "A";

                if (HelperService.GetConfigValue(PMSConstants.CfgSpbAutoUpload + spb.DIV.UNITCODE, _context)
                        == PMSConstants.CfgSpbAutoUploadTrue)
                {
                    spb.UPLOAD = 1;
                    spb.UPLOADDATE = GetServerTime();



                    base.SaveUpdate(spb, userName);
                    Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Approve {spb.SPBNO}", _context);
                }
            }

            catch (Exception ex)
            {
                HelperService.SetSapResult(spb.SPBNO, ex.Message, _context);
                throw;
            }

            base.SaveUpdate(spb, userName);
            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Approve {spb.SPBNO}", _context);

            return true;
        }

        public bool Cancel(IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["SPBNO"]; 
            TSPB spb = GetSingle(no);

            this.CancelValidate(spb);
            spb.UPDATEBY = userName;
            spb.UPDATED = GetServerTime();
            spb.STATUS = "C";

            var rfcResult = string.Empty;

            if (HelperService.GetConfigValue(PMSConstants.CfgSpbAutoUpload + spb.DIV.UNITCODE, _context)
           == PMSConstants.CfgSpbAutoUploadTrue)
            {
                spb.UPLOAD = 2;
                spb.UPLOADDATE = spb.UPLOADDATE;
            }

            base.SaveUpdate(spb,userName);
            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"ReOpen {spb.SPBNO}", _context);

            return true;
        }

        public override IEnumerable<TSPB> GetList(FilterSPB filter)
        {
            
            var criteria = PredicateBuilder.True<TSPB>();

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.SPBNO.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.SPBNODHS.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.DIVID.StartsWith(filter.UnitID));

                if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                    criteria = criteria.And(p => p.DIVID.Equals(filter.UnitID));

                if (!string.IsNullOrWhiteSpace(filter.SPBNo))
                    criteria = criteria.And(p => p.SPBNO.Equals(filter.SPBNo));

                if (!string.IsNullOrWhiteSpace(filter.SPBNoDHS))
                    criteria = criteria.And(p => p.SPBNODHS.Equals(filter.SPBNoDHS));

                if (!string.IsNullOrWhiteSpace(filter.MillCode))
                    criteria = criteria.And(p => p.MILLCODE.Equals(filter.MillCode));

                if (!string.IsNullOrWhiteSpace(filter.VehicleNo))
                    criteria = criteria.And(p => p.VEHICLENO.Contains(filter.VehicleNo));

                if (!string.IsNullOrWhiteSpace(filter.DriverName))
                    criteria = criteria.And(p => p.DRIVERNAME.Contains(filter.DriverName));

                if (!string.IsNullOrWhiteSpace(filter.OperatorName))
                    criteria = criteria.And(p => p.OPERATORNAME.Contains(filter.OperatorName));

                if (!filter.Date.Equals(DateTime.MinValue))
                    criteria = criteria.And 
                        (p => p.SPBDATE.Date == filter.Date.Date);

                if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.SPBDATE.Date >= filter.StartDate.Date && p.SPBDATE.Date <= filter.EndDate.Date);

                var result = _context.TSPB.Where(criteria);
                if (filter.PageSize <= 0)
                    return result;
                return result.GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return new List<TSPB>(); }

        }

        public bool DownloadMillResult(string source, string unitId, DateTime date, string userName)
        {
            _servicePeriod.CheckValidPeriod(unitId, date.Date);

            List<jbs_Download_Get_Doket_Result> dtDoket = _context.jbs_Download_Get_Doket_Result(unitId, date.Date).ToList();

            //Check Double Doket
            var c = (from i in dtDoket.AsEnumerable()
                     group i by new { DokId = i.DOKID, }
                             into g
                     select new { g.Key.DokId, DokCount = g.Select(x => x.DOKID).Count() }
                             ).ToList();

            foreach (var item in c)
            {
                if (item.DokCount > 1)
                {
                    string result = "Doket " + item.DokId + " ada di SPB" + Environment.NewLine;
                    foreach (var row in dtDoket)
                    {
                        if (row.DOKID.ToString() == item.DokId.ToString())
                        {
                            result += row.SPBNO.ToString() + Environment.NewLine;
                        }
                    }
                    throw new Exception(result);
                }
            }

            List<jbs_Download_tonase_SPB_Result> dtSpb;

            if (source == "SPB" || source == string.Empty || source == null)
                dtSpb = _context.GetSPBResult(unitId, date).ToList();
            else
            {
                MUNITDBSERVER unitDBServer = _context.MUNITDBSERVER.Where(d => d.ALIAS.Equals(source)).SingleOrDefault();
                if (unitDBServer == null)
                    throw new Exception("Invalid DB Server");

                using (PMSContextBase contextWB = new PMSContextBase(DBContextOption<PMSContextBase>.GetOptions(unitDBServer.SERVERNAME, unitDBServer.DBNAME, unitDBServer.DBUSER, unitDBServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
                {dtSpb = contextWB.GetMillResult(unitId, date.Date).ToList();}
            }

            List<string[]> spbColl = new List<string[]>();
            List<string[]> doketColl = new List<string[]>();

            foreach (var row in dtSpb)
            {
                string spbNo = row.NO;
                DateTime spbDate = row.DATE;
                decimal spbWeight1 = row.WEIGHT1;
                decimal spbWeight2 = row.WEIGHT2;
                decimal spbNetto = spbWeight1 - spbWeight2;

                spbColl.Add(new string[] { spbNo, source, spbDate.ToString("yyyyMMdd"), spbWeight1.ToString(), spbWeight2.ToString(), spbNetto.ToString() });

                var doketList = (from i in dtDoket.AsEnumerable()
                                 where i.SPBNO.ToString() == spbNo
                                 select i).ToList();

                var bk = from i in doketList.AsEnumerable() select Convert.ToDecimal(i.BRD);
                var jgEst = from i in doketList.AsEnumerable() select Convert.ToDecimal(i.JJG) * Convert.ToDecimal(i.BJR);

                decimal brdKgSum = bk.Sum();
                decimal jjgKgSumEst = jgEst.Sum();
                decimal jjgKgSum = spbNetto - brdKgSum;

                int dokCount = 1;
                decimal kgUsed = 0;
                foreach (var dkRow in doketList)
                {
                    string doketId = dkRow.DOKID.ToString();
                    decimal jjg = Convert.ToDecimal(dkRow.JJG);
                    decimal bjr = Convert.ToDecimal(dkRow.BJR);
                    decimal brd = Convert.ToDecimal(dkRow.BRD);
                    decimal jjgKgEst = jjg * bjr;

                    decimal jjgKg = 0;
                    if (dokCount >= doketList.Count)
                    {
                        jjgKg = jjgKgSum - kgUsed;
                    }
                    else
                    {
                        if (jjgKgSumEst > 0)
                        {
                            jjgKg = Math.Round(jjgKgEst / jjgKgSumEst * jjgKgSum, 2);
                        }
                        else
                        { jjgKg = 0; }
                    }

                    doketColl.Add(new string[] { doketId, source, jjg.ToString(), bjr.ToString(), jjgKg.ToString(), brd.ToString() });

                    kgUsed += jjgKg;
                    dokCount++;
                }
            }

            foreach (var item in spbColl)
            {
                string[] spb = item;
                string query = string.Empty;

                query += "Delete From [SPBKG] Where SPBID = '" + spb[0].Replace("'", "''") + "'; ";
                query += Environment.NewLine;
                query += "Insert Into [SPBKG] (SPBID, SOURCE, [DATEIN], WEIGHT1, WEIGHT2, NETTO, STATUS, UPDATED) ";
                query += "Values( ";
                query += "'" + spb[0].Replace("'", "''") + "', ";
                query += "'" + spb[1].Replace("'", "''") + "', ";
                query += "'" + spb[2].Replace("'", "''") + "', ";
                query += spb[3].Replace("'", "''") + ", ";
                query += spb[4].Replace("'", "''") + ", ";
                query += spb[5].Replace("'", "''") + ", ";
                query += "'A', GetDate()) ";

                _context.ExecuteSqlText(query).ExecNonQuery();

            }

            foreach (var item in doketColl)
            {
                string[] doket = item;
                string query = string.Empty;
                query += "Delete From [DOKETKG] Where DOKID = '" + doket[0].Replace("'", "''") + "'; ";
                query += Environment.NewLine;
                query += "Insert Into [DOKETKG] (DOKID, SOURCE, JJG, BJR, JJGKG, BRDKG, STATUS, UPDATED) ";
                query += "Values( ";
                query += "'" + doket[0].Replace("'", "''") + "', ";
                query += "'" + doket[1].Replace("'", "''") + "', ";
                query += doket[2].Replace("'", "''") + ", ";
                query += doket[3].Replace("'", "''") + ", ";
                query += doket[4].Replace("'", "''") + ", ";
                query += doket[5].Replace("'", "''") + ", ";
                query += "'A', GetDate()) ";

                _context.ExecuteSqlText(query).ExecNonQuery();

            }

            return true;

        }

        public List<sp_SPB_GetBlockQty_Result>GetTonaseBlock(string unitId, DateTime from, DateTime to)
        {
            return _context.sp_SPB_GetBlockQty_Result(unitId, from, to).ToList();
        }

        //public TSPB NewRecord(string unitCode, string divisionCode, DateTime dateTime)
        //{
        //    return new TSPB
        //    {
        //        SPBNO = unitCode + "/TBS/" + divisionCode + "/" + dateTime.Month + "/" + dateTime.ToString("yy") + "/"
        //    };
        //}

        public override TSPB NewRecord(string userName)
        {
            TSPB record = new TSPB();
            record.SPBDATE = GetServerTime().Date;
            record.STATUS = "";

            return record;
        }

    }
}
