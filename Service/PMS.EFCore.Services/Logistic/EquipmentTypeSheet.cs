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
using PMS.EFCore.Services.Attendances;
using PMS.EFCore.Services.Organization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Logistic
{
    public class EquipmentTypeSheet : EntityFactory<TEQUIPTIMESHEET,TEQUIPTIMESHEET,GeneralFilter, PMSContextBase>
    {
        private Period _servicePeriod;
        private Activity _activityService;
        private Material _materialService;
        private Employee _employeeService;
        private Attendance _attendanceService;
        private AuthenticationServiceBase _authenticationService;

        private List<TEQUIPTIMESHEETACTIVITY> _newTEQUIPTIMESHEETACTIVITY = new List<TEQUIPTIMESHEETACTIVITY>();
        private List<TEQUIPTIMESHEETEMPLOYEE> _newTEQUIPTIMESHEETEMPLOYEE = new List<TEQUIPTIMESHEETEMPLOYEE>();
        private List<TEQUIPTIMESHEETMATERIAL> _newTEQUIPTIMESHEETMATERIAL = new List<TEQUIPTIMESHEETMATERIAL>();

        private List<TEQUIPTIMESHEETACTIVITY> _deleteTEQUIPTIMESHEETACTIVITY = new List<TEQUIPTIMESHEETACTIVITY>();
        private List<TEQUIPTIMESHEETEMPLOYEE> _deleteTEQUIPTIMESHEETEMPLOYEE = new List<TEQUIPTIMESHEETEMPLOYEE>();
        private List<TEQUIPTIMESHEETMATERIAL> _deleteTEQUIPTIMESHEETMATERIAL = new List<TEQUIPTIMESHEETMATERIAL>();

        private List<TEQUIPTIMESHEETACTIVITY> _editedTEQUIPTIMESHEETACTIVITY = new List<TEQUIPTIMESHEETACTIVITY>();
        private List<TEQUIPTIMESHEETEMPLOYEE> _editedTEQUIPTIMESHEETEMPLOYEE = new List<TEQUIPTIMESHEETEMPLOYEE>();
        private List<TEQUIPTIMESHEETMATERIAL> _editedTEQUIPTIMESHEETMATERIAL = new List<TEQUIPTIMESHEETMATERIAL>();

        public EquipmentTypeSheet(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "EquipmentTypeSheet";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(_context,_authenticationService,auditContext);
            _attendanceService = new Attendance(context,_authenticationService,auditContext);
            _activityService = new Activity(context,_authenticationService,auditContext);
            _materialService = new Material(context,_authenticationService,auditContext);
            _employeeService = new Employee(context,_authenticationService,auditContext);
        }

        private string FieldsValidation(TEQUIPTIMESHEET TEQ)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(TEQ.ID)) result += "ID tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(TEQ.UNITID)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(TEQ.DIVID)) result += "Divisi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(TEQ.EQUIPID)) result += "Equipment ID tidak boleh kosong." + Environment.NewLine;
            if (TEQ.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;

            //EquipmentActvity
            foreach(var item in TEQ.TEQUIPTIMESHEETACTIVITY)
            {
                if (string.IsNullOrEmpty(item.ID)) result += "Id harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.EQUIPTIMESHEETID)) result += "Time sheet id harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.ACTID)) result += "Kegiatan harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.OPRID)) result += "Operator harus diisi." + Environment.NewLine;
                if (item.VOL == 0) result += "Volume harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.BLOCKFROMID)) result += "Lokasi asal harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.BLOCKTOID)) result += "Lokasi akhir harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.RECID)) result += "Receiver harus diisi." + Environment.NewLine;
                if (item.KMSTART == 0) result += "KM/HM awal harus diisi." + Environment.NewLine;
                if (item.KMEND == 0) result += "KM/HM akhir harus diisi." + Environment.NewLine;
            }

            //EquipmentEmployee
            foreach (var item in TEQ.TEQUIPTIMESHEETEMPLOYEE)
            {
                if (string.IsNullOrEmpty(item.ID)) result += "Id harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.EQUIPTIMESHEETID)) result += "Time sheet id harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.EMPID)) result += "Karyawan harus diisi." + Environment.NewLine;
                //if (equipmentTimeSheetEmployee.Value == 0) result += "HK harus diisi." + Environment.NewLine;
            }

            //EquipmentMaterial
            foreach (var item in TEQ.TEQUIPTIMESHEETMATERIAL)
            {
                if (string.IsNullOrEmpty(item.ID)) result += "Id harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.EQUIPTIMESHEETID)) result += "Time sheet id harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.MATID)) result += "Bahan harus diisi." + Environment.NewLine;
                if (item.QTY == 0) result += "Jumlah harus diisi." + Environment.NewLine;
            }

            return result;

        }

        private void Validate(TEQUIPTIMESHEET equipmentTimeSheet)
        {
            string result = this.FieldsValidation(equipmentTimeSheet);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            if (equipmentTimeSheet.TEQUIPTIMESHEETEMPLOYEE.Count == 0)
                throw new Exception("Karyawan tidak boleh kosong.");

            if (equipmentTimeSheet.TEQUIPTIMESHEETACTIVITY.Count == 0)
                throw new Exception("Kegiatan tidak boleh kosong.");

            var currentDate = HelperService.GetServerDateTime(1, _context);
            if (equipmentTimeSheet.DATE > currentDate)
                throw new Exception("Tanggal transaksi tidak boleh lebih besar dari tanggal server.");

            foreach (var act in equipmentTimeSheet.TEQUIPTIMESHEETACTIVITY)
            {
                if (act.KMEND < act.KMSTART) throw new Exception("KM/HM akhir tidak boleh lebih kecil dari KM/HM awal.");
                if (act.TIMEEND< act.TIMESTART) throw new Exception("Jam akhir tidak boleh lebih besar dari jam awal.");
                if (act.RECID.ToUpper().Contains("ORBIAR") && string.IsNullOrEmpty(act.NOTE)) throw new Exception("Keterangan ORBIAR harus diisi.");
            }

            var actList = new List<TEQUIPTIMESHEETACTIVITY>();
            actList.AddRange(equipmentTimeSheet.TEQUIPTIMESHEETACTIVITY);

            DateTime timeEnd = new DateTime();
            actList = actList.OrderBy(x => x.TIMESTART).ToList();
            foreach (var item in actList)
            {
                if (timeEnd == new DateTime())
                    timeEnd = item.TIMEEND;
                else
                {
                    if (timeEnd < item.TIMESTART ) throw new Exception("Jam tidak boleh overlap.");
                    else timeEnd = item.TIMEEND;
                }
            }

            decimal kmEnd = 0;
            actList = actList.OrderBy(x => x.KMSTART).ToList();
            foreach (var item in actList)
            {
                if (kmEnd == 0)
                    kmEnd = item.KMEND;
                else
                {
                    if (kmEnd < item.KMSTART) throw new Exception("KH/HM tidak boleh overlap.");
                    else kmEnd = item.KMEND;
                }
            }

            //_servicePeriod.CheckValidPeriod(equipmentTimeSheet.UNITID, equipmentTimeSheet.DATE);
        }


        protected override TEQUIPTIMESHEET GetSingleFromDB(params  object[] keyValues)
        {

            string Id = keyValues[0].ToString();
            TEQUIPTIMESHEET record = _context.TEQUIPTIMESHEET
                .Include(a => a.TEQUIPTIMESHEETACTIVITY)
                .Include(b => b.TEQUIPTIMESHEETEMPLOYEE)
                .Include(c => c.TEQUIPTIMESHEETMATERIAL)
                .Include(d => d.DIV)
                .Include(e => e.EQUIP)
                .Include(f => f.DIV)
                .Include(g => g.UNIT)
                //.FirstOrDefault(d => d.ID.Equals(keyValues[0]));
                .Where(i => i.ID.Equals(Id)).SingleOrDefault();

            if (record != null)
            {
                foreach (var act in record.TEQUIPTIMESHEETACTIVITY)
                {
                    act.ACT = _activityService.GetSingle(act.ACTID);
                }
                foreach (var emp in record.TEQUIPTIMESHEETEMPLOYEE)
                {
                    emp.EMP = _employeeService.GetSingle(emp.EMPID);
                }
                foreach (var mat in record.TEQUIPTIMESHEETMATERIAL)
                {
                    mat.MAT = _materialService.GetSingle(mat.MATID);
                }
                //List<string> activityIds = record.TEQUIPTIMESHEETACTIVITY.Select(d => d.ACTID).Distinct().ToList(),
                //             empIds = record.TEQUIPTIMESHEETEMPLOYEE.Select(d => d.EMPID).Distinct().ToList(),
                //             materialIds = record.TEQUIPTIMESHEETMATERIAL.Select(d => d.MATID).Distinct().ToList();
                //if (activityIds.Any())
                //    record.VBKUACTIVITYREF = _activityService.GetList(new FilterActivity { Ids = activityIds })
                //                        .Select(d => new VBKUACTIVITY
                //                        {
                //                            ACTID = d.ACTIVITYID,
                //                            ACTNAME = $"{d.ACTIVITYID} - {d.ACTIVITYNAME}",
                //                        })
                //                        .ToList();


                //if (materialIds.Any())
                //    record.VBKUMATERIALREF = _materialService.GetList(new FilterMaterial { Ids = materialIds })
                //                    .Select(d => new VBKUMATERIAL
                //                    {

                //                        MATID = d.MATERIALID,
                //                        MATNAME = $"{d.MATERIALID} - {d.MATERIALNAME}",
                //                        UOM = d.UOM
                //                    })
                //                    .ToList();

                //if (empIds.Any())
                //    record.VBKUEMPLOYEEREF = _employeeService.GetList(new FilterEmployee { Ids = empIds })
                //                    .Select(d => new VBKUEMPLOYEE
                //                    {

                //                        EMPID = d.EMPID,
                //                        EMPNAME = $"{d.EMPID} - {d.EMPNAME}",

                //                    })
                //                    .ToList();

            }

                return record;
        }

        public TEQUIPTIMESHEET AddDetail(TEQUIPTIMESHEET record)
        {

            //TEQUIPTIMESHEETACTIVITY
            foreach (var activity in record.TEQUIPTIMESHEETACTIVITY)
            {
                activity.ACT = _context.MACTIVITY.Where(d => d.ACTIVITYID.Equals(activity.ACTID)).SingleOrDefault();
            }
            //TEQUIPTIMESHEETEMPLOYEE
            foreach (var emp in record.TEQUIPTIMESHEETEMPLOYEE)
            {
                emp.EMP = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(emp.EMPID)).SingleOrDefault();
            }
            //TEQUIPTIMESHEETMATERIAL
            foreach (var material in record.TEQUIPTIMESHEETMATERIAL)
            {
                material.MAT = _context.MMATERIAL.Where(d => d.MATERIALID.Equals(material.MATID)).SingleOrDefault();
            }

            return record;
        }


        public override TEQUIPTIMESHEET CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TEQUIPTIMESHEET record = base.CopyFromWebFormData(formData, newRecord);
            _newTEQUIPTIMESHEETACTIVITY = new List<TEQUIPTIMESHEETACTIVITY>();
            _newTEQUIPTIMESHEETEMPLOYEE = new List<TEQUIPTIMESHEETEMPLOYEE>();
            _newTEQUIPTIMESHEETMATERIAL = new List<TEQUIPTIMESHEETMATERIAL>();


            _newTEQUIPTIMESHEETACTIVITY = CopyActivityFromWebFormData(formData);
            _newTEQUIPTIMESHEETEMPLOYEE = CopyEmployeeFromWebFormData(formData);
            _newTEQUIPTIMESHEETMATERIAL = CopyMaterialFromWebFormData(formData);


            return record;
        }

        private List<TEQUIPTIMESHEETACTIVITY> CopyActivityFromWebFormData(IFormCollection formData)
        {
            List<TEQUIPTIMESHEETACTIVITY> result = new List<TEQUIPTIMESHEETACTIVITY>();
            result.CopyFrom<TEQUIPTIMESHEETACTIVITY>(formData, "TEQUIPTIMESHEETACTIVITY");
            return result;
        }

        private List<TEQUIPTIMESHEETEMPLOYEE> CopyEmployeeFromWebFormData(IFormCollection formData)
        {
            List<TEQUIPTIMESHEETEMPLOYEE> result = new List<TEQUIPTIMESHEETEMPLOYEE>();
            result.CopyFrom<TEQUIPTIMESHEETEMPLOYEE>(formData, "TEQUIPTIMESHEETEMPLOYEE");
            return result;
        }

        private List<TEQUIPTIMESHEETMATERIAL> CopyMaterialFromWebFormData(IFormCollection formData)
        {
            List<TEQUIPTIMESHEETMATERIAL> result = new List<TEQUIPTIMESHEETMATERIAL>();
            result.CopyFrom<TEQUIPTIMESHEETMATERIAL>(formData, "TEQUIPTIMESHEETMATERIAL");
            return result;
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

        protected override TEQUIPTIMESHEET BeforeSave(TEQUIPTIMESHEET record, string userName, bool newRecord)
        {
            if (string.IsNullOrEmpty(record.ID))
            {
                int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.EquipmentTimeSheetIdPrefix + record.UNITID, _context);
                record.ID = PMSConstants.EquipmentTimeSheetIdPrefix + "-" + record.UNITID
                + "-" + record.DATE.ToString("yyyyMMdd") + "-" + lastNumber.ToString().PadLeft(4, '0');

            }

            
            if (_context.TEQUIPTIMESHEET.FirstOrDefault(d => d.ID.Equals(record.ID)) != null)
                throw new Exception("Kartu kerja dengan nomor tersebut sudah ada.");

                

                record.STATUS = "P";
                record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
                
                int counter = 0;

                counter = 1;

                //TEQUIPTIMESHEETACTIVITY

                if (_newTEQUIPTIMESHEETACTIVITY.Any())

                foreach (var activity in _newTEQUIPTIMESHEETACTIVITY)
                {
                    activity.ID = record.ID + counter.ToString().PadLeft(2, '0');
                    activity.EQUIPTIMESHEETID = record.ID;
                    activity.NOTE = record.NOTE;
                    counter++;
                }

                //TEQUIPTIMESHEETMATERIAL

                if (_newTEQUIPTIMESHEETMATERIAL.Any())

                foreach (var material in _newTEQUIPTIMESHEETMATERIAL)
                {
                    material.ID = record.ID + counter.ToString().PadLeft(2, '0');
                    material.EQUIPTIMESHEETID = record.ID;
                    counter++;
                }

                //TEQUIPTIMESHEETEMPLOYEE

                if (_newTEQUIPTIMESHEETEMPLOYEE.Any())

                foreach (var employee in _newTEQUIPTIMESHEETEMPLOYEE)
                {
                    employee.ID = record.ID + counter.ToString().PadLeft(2, '0');
                    employee.EQUIPTIMESHEETID = record.ID;
                    counter++;
                }


            _saveDetails = _newTEQUIPTIMESHEETACTIVITY.Any() || _newTEQUIPTIMESHEETEMPLOYEE.Any() || _newTEQUIPTIMESHEETMATERIAL.Any();



            return record;

        }

        protected override TEQUIPTIMESHEET AfterSave(TEQUIPTIMESHEET record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.EquipmentTimeSheetIdPrefix + record.UNITID, _context);
            return record;
        }

        private void UpdateValidate(TEQUIPTIMESHEET TEQUP)
        {

            var currEquip = GetSingle(TEQUP.ID);
            if (currEquip != null)
            {
                if (currEquip.STATUS == "A")
                    throw new Exception("Data sudah di approve.");
                if (currEquip.STATUS == "C")
                    throw new Exception("Data sudah di cancel.");

            }
        }

        //private void ApproveValidate(TEQUIPTIMESHEET teq)
        //{
        //    if (teq.STATUS == "A")
        //        throw new Exception("Data sudah di approve.");
        //    if (teq.STATUS == "C")
        //        throw new Exception("Data sudah di cancel.");

        //    this.Validate(teq);
        //    _servicePeriod.CheckValidPeriod(teq.UNITID, teq.DATE);

        //    //var logs = _context.Database.ExecuteSqlCommand($"Exec sp_EquipmentTimeSheet_GetByDate {teq.EQUIPID},{teq.DATE}");
        //    //foreach (var log in logs)
        //    //{
        //    //    if (log.STATUS == "A") throw new Exception("Kartu kerja sudah pernah dibuat.");
        //    //}

        //    var lastKm = _context.Database.ExecuteSqlCommand($"Exec sp_EquipmentTimeSheet_GetLastKm {teq.EQUIPID},{teq.ROT}");
        //    foreach (var act in teq.TEQUIPTIMESHEETACTIVITY)
        //    {
        //        if (act.KMSTART < lastKm) throw new Exception("KM/HM terakhir = " + lastKm.ToString());
        //    }
        //}

        private void CancelValidate(TEQUIPTIMESHEET TEQUP)
        {
            if (TEQUP.STATUS != "A")
                throw new Exception("Data belum di approve.");
            if (TEQUP.STATUS == "C")
                throw new Exception("Data sudah di cancel.");

           _servicePeriod.CheckValidPeriod(TEQUP.UNITID, TEQUP.DATE);        
        }

        private void DeleteValidate(TEQUIPTIMESHEET TEQUP)
        {
            if (TEQUP.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (TEQUP.STATUS == "C")
                throw new Exception("Data sudah di cancel.");

           // _servicePeriod.CheckValidPeriod(TEQUP.UNITID, TEQUP.DATE);
        }



        

        public override TEQUIPTIMESHEET NewRecord(string userName)
        {
            TEQUIPTIMESHEET record = new TEQUIPTIMESHEET();
            record.DATE = GetServerTime().Date;
            record.STATUS = "";

            return record;
        }

        public void SaveActivities(IFormCollection formDataCollection, string userName, bool externalCommit)
        {
            string bkuId = formDataCollection["Id"];
            if (string.IsNullOrWhiteSpace(bkuId))
                throw new Exception("Id tidak boleh kosong");
            List<TEQUIPTIMESHEETACTIVITY> activities = CopyActivityFromWebFormData(formDataCollection);
            SaveActivities(bkuId, activities, userName, externalCommit);
        }

        public void SaveActivities(string bkuId, List<TEQUIPTIMESHEETACTIVITY> activities, string userName, bool externalCommit)
        {
            activities.ForEach(d => { d.EQUIPTIMESHEETID = bkuId; });
            _context.TEQUIPTIMESHEETACTIVITY.RemoveRange(_context.TEQUIPTIMESHEETACTIVITY.Where(d => d.EQUIPTIMESHEETID.Equals(bkuId)));
            if (activities != null && activities.Any())
                _context.TEQUIPTIMESHEETACTIVITY.AddRange(activities);
            if (!externalCommit)
                _context.SaveChanges();
        }

        public void SaveEmployees(IFormCollection formDataCollection, string userName, bool externalCommit)
        {
            string bkuId = formDataCollection["Id"];
            if (string.IsNullOrWhiteSpace(bkuId))
                throw new Exception("Id tidak boleh kosong");
            List<TEQUIPTIMESHEETEMPLOYEE> employees = CopyEmployeeFromWebFormData(formDataCollection);
            SaveEmployees(bkuId, employees, userName, externalCommit);
        }

        public void SaveEmployees(string bkuId, List<TEQUIPTIMESHEETEMPLOYEE> employees, string userName, bool externalCommit)
        {
            employees.ForEach(d => { d.EQUIPTIMESHEETID = bkuId; });
            _context.TEQUIPTIMESHEETEMPLOYEE.RemoveRange(_context.TEQUIPTIMESHEETEMPLOYEE.Where(d => d.EQUIPTIMESHEETID.Equals(bkuId)));
            if (employees != null && employees.Any())
                _context.TEQUIPTIMESHEETEMPLOYEE.AddRange(employees);
            if (!externalCommit)
                _context.SaveChanges();
        }

        public void SaveMaterials(IFormCollection formDataCollection, string userName, bool externalCommit)
        {
            string bkuId = formDataCollection["Id"];
            if (string.IsNullOrWhiteSpace(bkuId))
                throw new Exception("Id tidak boleh kosong");
            List<TEQUIPTIMESHEETMATERIAL> materials = CopyMaterialFromWebFormData(formDataCollection);
            SaveMaterials(bkuId, materials, userName, externalCommit);
        }

        public void SaveMaterials(string bkuId, List<TEQUIPTIMESHEETMATERIAL> materials, string userName, bool externalCommit)
        {
            materials.ForEach(d => { d.EQUIPTIMESHEETID = bkuId; });
            _context.TEQUIPTIMESHEETMATERIAL.RemoveRange(_context.TEQUIPTIMESHEETMATERIAL.Where(d => d.EQUIPTIMESHEETID.Equals(bkuId)));
            if (materials != null && materials.Any())
                _context.TEQUIPTIMESHEETMATERIAL.AddRange(materials);
            if (!externalCommit)
                _context.SaveChanges();
        }

        protected override TEQUIPTIMESHEET BeforeDelete(TEQUIPTIMESHEET consumption, string userName)
        {
            this.DeleteValidate(consumption);

            foreach (var act in consumption.TEQUIPTIMESHEETACTIVITY)
            {
                _deleteTEQUIPTIMESHEETACTIVITY.Add(act);
            }
            consumption.TEQUIPTIMESHEETACTIVITY.Clear();

            foreach (var emp in consumption.TEQUIPTIMESHEETEMPLOYEE)
            {
                _deleteTEQUIPTIMESHEETEMPLOYEE.Add(emp);
            }
            consumption.TEQUIPTIMESHEETEMPLOYEE.Clear();

            foreach (var mat in consumption.TEQUIPTIMESHEETMATERIAL)
            {
                _deleteTEQUIPTIMESHEETMATERIAL.Add(mat);
            }
            consumption.TEQUIPTIMESHEETMATERIAL.Clear();

           

            _saveDetails = _deleteTEQUIPTIMESHEETACTIVITY.Any() || _deleteTEQUIPTIMESHEETEMPLOYEE.Any() || _deleteTEQUIPTIMESHEETMATERIAL.Any();

            return consumption;
        }

        protected override TEQUIPTIMESHEET SaveInsertToDB(TEQUIPTIMESHEET record, string userName)
        {
            _internalCommit = false;
            base.SaveInsertToDB(record, userName);
            SaveActivities(record.ID, _newTEQUIPTIMESHEETACTIVITY, userName, !_internalCommit);
            SaveMaterials(record.ID, _newTEQUIPTIMESHEETMATERIAL, userName, !_internalCommit);
            SaveEmployees(record.ID, _newTEQUIPTIMESHEETEMPLOYEE, userName, !_internalCommit);
            CommitAllChanges();
            return GetSingle(record.ID);
        }

        protected override TEQUIPTIMESHEET SaveUpdateToDB(TEQUIPTIMESHEET record, string userName)
        {
            _internalCommit = false;
            SaveActivities(record.ID, _newTEQUIPTIMESHEETACTIVITY, userName, !_internalCommit);
            SaveMaterials(record.ID, _newTEQUIPTIMESHEETMATERIAL, userName, !_internalCommit);
            SaveEmployees(record.ID, _newTEQUIPTIMESHEETEMPLOYEE, userName, !_internalCommit);
            base.SaveUpdateToDB(record, userName);
            CommitAllChanges();
            return GetSingle(record.ID);
        }

        protected override bool DeleteDetailsFromDB(TEQUIPTIMESHEET record, string userName)
        {
            if (_deleteTEQUIPTIMESHEETACTIVITY.Any())
                _context.TEQUIPTIMESHEETACTIVITY.RemoveRange(_context.TEQUIPTIMESHEETACTIVITY.Where(d => d.EQUIPTIMESHEETID.Equals(record.ID)));

            if (_deleteTEQUIPTIMESHEETEMPLOYEE.Any())
                _context.TEQUIPTIMESHEETEMPLOYEE.RemoveRange(_context.TEQUIPTIMESHEETEMPLOYEE.Where(d => d.EQUIPTIMESHEETID.Equals(record.ID)));

            if (_deleteTEQUIPTIMESHEETMATERIAL.Any())
                _context.TEQUIPTIMESHEETMATERIAL.RemoveRange(_context.TEQUIPTIMESHEETMATERIAL.Where(d => d.EQUIPTIMESHEETID.Equals(record.ID)));

            return true;
        }

        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string bkuId = formDataCollection["ID"];
            return Approve(bkuId, userName);
        }
        public bool Approve(string bkuId, string userName)
        {
            if (string.IsNullOrWhiteSpace(bkuId))
                throw new Exception("Id tidak boleh kosong");
            TEQUIPTIMESHEET record = GetSingle(bkuId);
            return Approve(record, userName);

        }

        public bool Approve(TEQUIPTIMESHEET record, string userName)
        {
            if (record == null)
                throw new Exception("BKU tidak ditemukan");

            if (record.STATUS.Equals(PMSConstants.TransactionStatusDeleted))
                throw new Exception("BKU sudah dihapus");
            if (record.STATUS.Equals(PMSConstants.TransactionStatusApproved))
                throw new Exception("BKU sudah diappove");
            if (record.STATUS.Equals(PMSConstants.TransactionStatusCanceled))
                throw new Exception("BKU sudah dicancel");

            var lastKm = _context.Database.ExecuteSqlCommand($"Exec sp_EquipmentTimeSheet_GetLastKm {record.EQUIPID},{record.ROT}");
            foreach (var act in record.TEQUIPTIMESHEETACTIVITY)
            {
                if (act.KMSTART < lastKm) throw new Exception("KM/HM terakhir = " + lastKm.ToString());
            }

            record.STATUS = PMSConstants.TransactionStatusApproved;
            record.UPDATED = GetServerTime();

            foreach (var employee in record.TEQUIPTIMESHEETEMPLOYEE)
            {
                if (employee.VALUE > 0)
                {
                    var attendance = new TATTENDANCE
                    {
                        DIVID = employee.EMP.DIVID,
                        EMPLOYEEID = employee.EMPID,
                        DATE = record.DATE,
                        REMARK = string.Empty,
                        HK = employee.VALUE,
                        PRESENT = true,
                        //AbsentId = "K",
                        //-*Constant
                        STATUS = record.STATUS,
                        REF = record.ID,
                        AUTO = true,
                        UPDATEDDATE = record.UPDATED,
                    };
                    _attendanceService.SaveInsertOrUpdate(attendance, userName);
                }
            }


            _context.SaveChanges();
            return true;
        }
        //public bool Approve(IFormCollection formDataCollection, string userName)
        //{
        //    string no = formDataCollection["ID"];
        //    var teq = GetSingle(no);
        //    this.ApproveValidate(teq);

        //    teq.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
        //    teq.STATUS = "A";

        //    foreach (var employee in teq.TEQUIPTIMESHEETEMPLOYEE)
        //    {
        //        if (employee.VALUE > 0)
        //        {
        //            var attendance = new TATTENDANCE
        //            {
        //                DIVID = employee.EMP.DIVID,
        //                EMPLOYEEID = employee.EMPID,
        //                DATE = teq.DATE,
        //                REMARK = string.Empty,
        //                HK = employee.VALUE,
        //                PRESENT = true,
        //                //AbsentId = "K",
        //                //-*Constant
        //                STATUS = teq.STATUS,
        //                REF = teq.ID,
        //                AUTO = true,
        //                UPDATEDDATE = teq.UPDATED,
        //            };
        //            _attendanceService.SaveInsertOrUpdate(attendance, userName);
        //        }
        //    }

        //    _context.Entry(teq).State = EntityState.Modified;
        //    _context.SaveChanges();

        //    Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Approve {teq.ID}", _context);

        //    return true;
        //}

        public bool Cancel(IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["ID"];
            TEQUIPTIMESHEET teq = GetSingle(no);

            this.CancelValidate(teq);
            teq.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            teq.STATUS = "C";

            var newEquipmentTimeSheet = new TEQUIPTIMESHEET
            {
                ID = GenereteNewDerivedNumber(teq.ID),
                UNITID = teq.UNITID,
                DIVID = teq.DIVID,
                DATE = teq.DATE,
                EQUIPID = teq.EQUIPID,
                NOTE = teq.NOTE,
                STATUS = teq.STATUS,
            };

            _context.Entry(teq).State = EntityState.Modified;
            _context.SaveChanges();

            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"ReOpen {teq.ID}", _context);

            return true;
        }


        public override IEnumerable<TEQUIPTIMESHEET> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<TEQUIPTIMESHEET>();

            criteria = criteria.And(d =>
              (d.DATE.Date >= filter.StartDate.Date && d.DATE.Date <= filter.EndDate.Date));

            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(p => p.UNITID.Equals(filter.UnitID));
            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(p => p.DIVID.Equals(filter.DivisionID));
            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));


            if (filter.PageSize <= 0)
                return _context.TEQUIPTIMESHEET.Where(criteria);
            return _context.TEQUIPTIMESHEET.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }


    }
}
