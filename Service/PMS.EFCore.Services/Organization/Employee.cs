using AM.EFCore.Services;
using FileStorage.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PMS.EFCore.Helper;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services.Utilities;
using PMS.Shared.Exceptions;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PMS.EFCore.Services.Organization
{
    public class Employee:EntityFactory<MEMPLOYEE,VEMPLOYEE,FilterEmployee,PMSContextBase>
    {

        private List<MEMPLOYEEFAMILY> _newFamilies = null;
        private List<MEMPLOYEEFAMILY> _deletedFamilies = null;
        private List<MEMPLOYEEFAMILY> _editedFamilies = null;


        private List<MEMPLOYEEFILE> _newFiles = null;
        private List<MEMPLOYEEFILE> _deletedFiles = null;
        private List<MEMPLOYEEFILE> _editedFiles = null;
        private AuthenticationServiceBase _authenticationService;
        
        public Employee(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "EMP";
            _authenticationService = authenticationService;

            _newFiles = new List<MEMPLOYEEFILE>();
            _editedFiles = new List<MEMPLOYEEFILE>();
            _deletedFiles = new List<MEMPLOYEEFILE>();

            _newFamilies = new List<MEMPLOYEEFAMILY>();
            _editedFamilies = new List<MEMPLOYEEFAMILY>();
            _deletedFamilies = new List<MEMPLOYEEFAMILY>();

        }


        public override MEMPLOYEE CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MEMPLOYEE record = base.CopyFromWebFormData(formData, newRecord);
            

            /*Custom Code - Start*/

            /*Employee File*/
            List<MEMPLOYEEFILE> _potentialFiles = new List<MEMPLOYEEFILE>();
            _potentialFiles.CopyFrom<MEMPLOYEEFILE>(formData, "MEMPLOYEEFILE");
            _potentialFiles.ForEach(d => d.EMPID = record.EMPID);
            List<long> _potentialFileIds = _potentialFiles.Select(d => d.FILEID).ToList();
            _newFiles = new List<MEMPLOYEEFILE>();
            _editedFiles = new List<MEMPLOYEEFILE>();
            _deletedFiles = new List<MEMPLOYEEFILE>();

            /*Employee Family*/
            List<MEMPLOYEEFAMILY> _potentialItems = new List<MEMPLOYEEFAMILY>();
            _potentialItems.CopyFrom<MEMPLOYEEFAMILY>(formData, "MEMPLOYEEFAMILY");
            _potentialItems.ForEach(d => d.EMPID = record.EMPID);            
            List<string> _potentialIds = _potentialItems.Select(d => d.KTPID).ToList();

            _newFamilies = new List<MEMPLOYEEFAMILY>();
            _editedFamilies = new List<MEMPLOYEEFAMILY>();
            _deletedFamilies = new List<MEMPLOYEEFAMILY>();

            if (newRecord)
            {
                _newFiles = _potentialFiles;
                _newFamilies = _potentialItems;
            }
            else
            {
                if (!_potentialIds.Any())
                    _deletedFamilies = _context.MEMPLOYEEFAMILY.Where(x => x.EMPID.Equals(record.EMPID)).ToList();
                else
                {
                    var existingItems = _context.MEMPLOYEEFAMILY.Where(x => x.EMPID.Equals(record.EMPID));
                    var existingItemsId = existingItems.Select(x => x.KTPID).ToList();

                    _newFamilies = _potentialItems.Where(o => !existingItemsId.Contains(o.KTPID)).ToList();
                    _deletedFamilies = existingItems.Where(o => !_potentialIds.Contains(o.KTPID)).ToList();
                    _editedFamilies = _potentialItems.Where(o => existingItemsId.Contains(o.KTPID)).ToList();
                }


                if (!_potentialFileIds.Any())
                    _deletedFiles = _context.MEMPLOYEEFILE.Where(x => x.EMPID.Equals(record.EMPID)).ToList();
                else
                {
                    var existingItems = _context.MEMPLOYEEFILE.Where(x => x.EMPID.Equals(record.EMPID));
                    var existingItemsId = existingItems.Select(x => x.FILEID).ToList();

                    _newFiles = _potentialFiles.Where(o => !existingItemsId.Contains(o.FILEID)).ToList();
                    _deletedFiles = existingItems.Where(o => !_potentialFileIds.Contains(o.FILEID)).ToList();
                    _editedFiles = _potentialFiles.Where(o => existingItemsId.Contains(o.FILEID)).ToList();
                }

            }
            _saveDetails = true;
            /*Custom Code - Here*/

            return record;
        }

        protected override MEMPLOYEE BeforeDelete(MEMPLOYEE record, string userName)
        {
            /*Custom Code - Start*/
            /*Validation before delete existing record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/

           
            /*Custom Code - Here*/
            return record;
        }

        private void ValidateFamily(MEMPLOYEEFAMILY family)
        {
            if (string.IsNullOrWhiteSpace(family.KTPID))
                throw new Exception("NIK anggota keluarga harus diisi");

            if (family.KTPID.Length != 16 || !StandardUtility.IsNumericString(family.KTPID))
                throw new Exception("NIK anggota keluarga harus 16 digit dan semuanya numerik");

            if (string.IsNullOrWhiteSpace(family.FULLNAME))
                throw new Exception("Nama anggota keluarga harus diisi");
            if (string.IsNullOrWhiteSpace(family.GENDER))
                throw new Exception("Jenis kelamin anggota keluarga harus diisi");

            if (string.IsNullOrWhiteSpace(family.RELATIONSHIP))
                throw new Exception("Hubungan anggota keluarga harus diisi");

        }

        protected override MEMPLOYEE BeforeSave(MEMPLOYEE record, string userName, bool newRecord)
        {
            /*Custom Code - Start*/
            /*Validation before save existing or new record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/

            /*Custom Code - Here*/

            DateTime currentDate = GetServerTime();
            if (newRecord)
            {
                record.CREATED = currentDate;             
                record.CREATEDBY = userName;
                record.EMPCODE = record.UNITCODE + record.BIRTHDAY.ToString("ddMMyy") + record.JOINTDATE.ToString("ddMMyy") + HelperService.GetCurrentDocumentNumber(PMSConstants.EmployeeIdPrefix + record.UNITCODE, _context).ToString("0000");
                record.EMPID = record.EMPCODE;
            }
            record.UPDATED = currentDate;
            record.UPDATEDBY = userName;

            Validate(record, userName);
            //if (HelperService.GetConfigValue(PMSConstants.CfgEmployeeForbidGa+record.UNITCODE,_context) == PMSConstants.CfgEmployeeForbidGaTrue)
            //{
            //    var vdivisi = _context.VDIVISI.SingleOrDefault(d => d.DIVID.Equals(record.DIVID)) ;

            //    if (vdivisi.CODE  == "20" || vdivisi.CODE == "30" || vdivisi.CODE == "40")
            //    {
            //        throw new Exception("Dilarang menambah/mutasi karyawan GA.");
            //    }
            //}


            if (record.PINID != 0)
            {
                var exist_pin = _context.MEMPLOYEE.FirstOrDefault(x => x.PINID == record.PINID && !x.EMPID.Equals(record.EMPID));
                if (exist_pin != null)                
                    throw new Exception($"Pin {record.PINID} sudah terdaftar atas nama {exist_pin.EMPID} - {exist_pin.EMPNAME}");
                
            }


            int spouse = 0, children = 0;

            if (!StandardUtility.IsEmptyList(_newFamilies))
            {
                _newFamilies.ForEach(d => {

                    ValidateFamily(d);
                    d.EMPID = record.EMPID;
                });
                spouse += _newFamilies.Where(d => d.RELATIONSHIP.Equals("Istri") || d.RELATIONSHIP.Equals("Suami")).Count();
                children += _newFamilies.Where(d => d.RELATIONSHIP.Equals("Anak")).Count();
            }


            if (!StandardUtility.IsEmptyList(_newFiles))                
                _newFiles.ForEach(d => d.EMPID = record.EMPID);

            if (!StandardUtility.IsEmptyList(_editedFamilies))
            {
                _editedFamilies.ForEach(d => {
                    ValidateFamily(d);
                    d.EMPID = record.EMPID;
                });
                spouse += _editedFamilies.Where(d => d.RELATIONSHIP.Equals("Istri") || d.RELATIONSHIP.Equals("Suami")).Count();
                children += _editedFamilies.Where(d => d.RELATIONSHIP.Equals("Anak")).Count();
            }

            if (!StandardUtility.IsEmptyList(_editedFiles))
            {
                _editedFiles.ForEach(d => d.EMPID = record.EMPID);
            }
            if (!StandardUtility.IsEmptyList(_deletedFamilies))
            {
                _deletedFamilies.ForEach(d => d.EMPID = record.EMPID);
            }
            if (!StandardUtility.IsEmptyList(_deletedFiles))
            {
                _deletedFiles.ForEach(d => d.EMPID = record.EMPID);
            }   
            
            var status = _context.MSTATUS.Find(record.STATUSID);
            if (status == null || !status.ACTIVE)
                throw new Exception("Status perkawinan tidak valid");

            //if (spouse < status.SPOUSE || children < status.CHILDREN)
            //    throw new Exception("Data keluarga tidak sesuai dengan Status perkawinan");

            

            if (record.MEMPLOYEEBANK == null)
            {
                var employeeeBank = _context.MEMPLOYEEBANK.Find(record.EMPID);
                if (employeeeBank == null)
                    employeeeBank = new MEMPLOYEEBANK();
                record.MEMPLOYEEBANK = employeeeBank;
            }
            

            record.MEMPLOYEEBANK.EMPID = record.EMPID;

            
            record.MEMPLOYEEBANK.NO = record.BANKACCNO;
            record.MEMPLOYEEBANK.NAME = record.BANKACCNAME;
            record.MEMPLOYEEBANK.BANKID = record.BANKID;
            record.MEMPLOYEEBANK.BPJSJKK = record.BPJSJKK;
            record.MEMPLOYEEBANK.BPJSJHT = record.BPJSJHT;
            record.MEMPLOYEEBANK.BPJSJP = record.BPJSJP;
            record.MEMPLOYEEBANK.BPJSKESEHATANNO = record.BPJSKESEHATANNO;
            record.MEMPLOYEEBANK.BPJSKESEHATANET = record.BPJSKESEHATANET;
            record.MEMPLOYEEBANK.BPJSKETENAGAKERJAANNO = record.BPJSKETENAGAKERJAANNO;
            record.MEMPLOYEEBANK.BPJSKETENAGAKERJAANNPP = record.BPJSKETENAGAKERJAANNPP;
            record.MEMPLOYEEBANK.BPJSBASE = record.BPJSBASE;
            record.MEMPLOYEEBANK.BPJSKESEHATANBASE = record.BPJSKESEHATANBASE;

            
            

            _saveDetails = true;
            return record;
        }

        

        public void ValidateRecord(MEMPLOYEE record, bool withAgeValidation, string userName)
        {
            
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.UNITCODE)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.EMPNAME)) result += "Nama tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) result += "Divisi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.POSITIONID)) result += "Posisi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.EMPTYPE)) result += "Type tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.STATUSID)) result += "Status tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.EMPSEX)) result += "Jenis Kelamin tidak boleh kosong." + Environment.NewLine;
            if (record.BIRTHDAY == new DateTime()) result += "Tanggal lahir tidak boleh kosong." + Environment.NewLine;
            if (record.JOINTDATE == new DateTime()) result += "Tanggal masuk tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(record.GOLONGAN)) result += "Golongan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.PLACEOFBIRTH)) result += "Tempat Lahir tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.RELIGION)) result += "Agama tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.EDUCATION)) result += "Pendidikan tidak boleh kosong." + Environment.NewLine;
            if (record.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;
            if (result != string.Empty) throw new Exception(result);

            if (withAgeValidation)
            {

                if (record.JOINTDATE <= record.BIRTHDAY) throw new Exception("Tanggal Masuk tidak boleh <= Tanggal Lahir");
                var zeroTime = new DateTime(1, 1, 1);
                var span = record.JOINTDATE - record.BIRTHDAY;
                var yearOld = (zeroTime + span).Year - 1;

                var maxOld = 0;
                var stringMaxOld = HelperService.GetConfigValue(PMSConstants.CfgEmployeeOldMax + record.UNITCODE, _context);
                if (!string.IsNullOrEmpty(stringMaxOld)) maxOld = StandardUtility.ToInt(stringMaxOld);

                if (maxOld > 0)
                    if (yearOld >= maxOld) throw new Exception("Umur pegawai tidak sesuai, silahkan hubungi HCCS HO.");

                var minOld = 0;
                var stringMinOld = HelperService.GetConfigValue(PMSConstants.CfgEmployeeOldMin + record.UNITCODE, _context);
                if (!string.IsNullOrEmpty(stringMinOld)) minOld = StandardUtility.ToInt(stringMinOld);

                if (minOld > 0)
                    if (yearOld < minOld) throw new Exception("Umur pegawai tidak sesuai, silahkan hubungi HCCS HO.");
            }
            

            //Check Position
            var position = _context.MPOSITION.FirstOrDefault(d => d.POSITIONID.Equals(record.POSITIONID));
            if (position == null)
                throw new Exception($"Jabatan {record.POSITIONID} tidak valid");
            if (!position.ACTIVE)
                throw new Exception($"Jabatan {record.POSITIONID} - {position.POSITIONNAME} tidak aktif");

            //Check Gender Constraint on Specific Position

            if (!string.IsNullOrWhiteSpace(position.GENDERFLAG) && record.EMPSEX.ToUpper().Trim() != position.GENDERFLAG.ToUpper().Trim())
            {
                string genderText = (record.EMPSEX == "L") ? "Laki-laki" : (record.EMPSEX == "P" ? "Perempuan" : record.EMPSEX);
                throw new Exception($"Karyawan '{genderText}' tidak diijinkan untuk Jabatan '{position.POSITIONID}-{position.POSITIONNAME}'");
            }


            var maxEmp = 0;
            var posDetail = _context.MPOSITIONDETAIL.SingleOrDefault(x => x.POSID == record.POSITIONID && x.UNITID == record.UNITCODE);

            if (posDetail != null) maxEmp = posDetail.MAXEMP;

            if (maxEmp > 0)
            {
                var emplist = _context.MEMPLOYEE.Where(d => d.POSITIONID == record.POSITIONID && d.UNITCODE == record.UNITCODE && d.STATUS == "A").ToList();
                var currEmp = emplist.Count();
                if (currEmp >= maxEmp)
                {
                    var curposition = _context.MPOSITION.SingleOrDefault(d => d.POSITIONID == record.POSITIONID);
                    throw new Exception($"Karyawan dengan jabatan {curposition.POSITIONID} - {curposition.POSITIONNAME} maksimal {maxEmp} orang, saat ini {currEmp} orang.");
                }
            }



            if (!string.IsNullOrWhiteSpace(record.NPWP))
            {
                if (record.NPWP.Length != 15 || !StandardUtility.IsNumericString(record.NPWP))
                    throw new Exception("NPWP harus 15 digit dan semuanya numerik");

                var otherEmployee = _context.MEMPLOYEE.FirstOrDefault(d => !d.EMPID.Equals(record.EMPID) && d.NPWP.Equals(record.NPWP) && (d.STATUS == "A" || d.STATUS == "C"));
                if (otherEmployee != null)
                    throw new Exception($"NPWP sudah terdaftar sebelumnya, a.n {otherEmployee.EMPID}-{otherEmployee.EMPNAME}");
            }
            else
                record.NPWP = string.Empty;

            if (string.IsNullOrWhiteSpace(record.KTPID))
                throw new Exception("No KTP harus diisi");

            if (record.KTPID.Length != 16 || !StandardUtility.IsNumericString(record.KTPID))
                throw new Exception("No KTP harus 16 digit dan semuanya numerik");


            var otherEmployeeKTP = _context.MEMPLOYEE.FirstOrDefault(d => !d.EMPID.Equals(record.EMPID) && d.KTPID.Equals(record.KTPID) && (d.STATUS == "A" || d.STATUS == "C"));
            if (otherEmployeeKTP != null)
                throw new Exception($"No KTP sudah terdaftar sebelumnya, a.n {otherEmployeeKTP.EMPID}-{otherEmployeeKTP.EMPNAME}");

            if (!string.IsNullOrWhiteSpace(record.SUPERVISORID))
            {
                var spv = _context.MEMPLOYEE.FirstOrDefault(d => d.EMPID.Equals(record.SUPERVISORID) && (d.STATUS == "A" || d.STATUS == "C"));
                if (spv == null)
                    throw new Exception($"Data atasan tidak ditemukan");
                if (!spv.UNITCODE.Equals(record.UNITCODE))
                    throw new Exception($"Atasan harus terdaftar pada estate yang sama, {spv.EMPID}-{spv.EMPNAME} terdaftar di {spv.UNITCODE}");
            }
        }

        public void Validate(MEMPLOYEE record, string userName)
        {

            ValidateRecord(record,true, userName);
            
            if (!string.IsNullOrWhiteSpace(record.EMPID))
            {
                var existingRecord = (from a in _context.MEMPLOYEE
                                     where a.EMPID.Equals(record.EMPID)
                                     select a).FirstOrDefault();
                if (existingRecord == null)
                    existingRecord = new MEMPLOYEE();
                ValidateRecordChangeAccess(record, existingRecord, userName);
                existingRecord = null;
            }
            
            

        }

        public void ValidateRecordChangeAccess(MEMPLOYEE record, MEMPLOYEE existingRecord, string userName)
        {
            var changePermssion = GetEmployeeChangePermission(userName);
            
            if (!changePermssion.ALLOWEDIT)
                throw new Exception("Anda tidak memiliki akses untuk mengubah master karyawan");

            if (changePermssion.ALLOWEDITBANK)
            {
                if (!string.IsNullOrWhiteSpace(record.BANKID))
                {
                    //if (string.IsNullOrWhiteSpace(record.BANKACCNO))
                    //    throw new Exception("Nomor rekening bank tidak boleh kosong.");
                    
                    //if (string.IsNullOrWhiteSpace(record.BANKACCNAME))
                    //    throw new Exception("Nama rekening bank tidak boleh kosong.");
                    
                    var bank = _context.MBANK.Find(record.BANKID);
                    if (bank == null)
                        throw new Exception("Nama bank tidak valid.");
                    if (!bank.ACTIVE)
                        throw new Exception("Bank tidak aktif.");
                 
                }
            }
            

            if ((record.BANKID != existingRecord.BANKID || record.BANKACCNO != existingRecord.BANKACCNO || record.BANKACCNAME != existingRecord.BANKACCNAME) && !changePermssion.ALLOWEDITBANK)
                throw new Exception("Anda tidak memiliki akses untuk mengubah data bank karyawan");

            
            if ((record.BPJSJKK != existingRecord.BPJSJKK || record.BPJSJHT != existingRecord.BPJSJHT || record.BPJSJP != existingRecord.BPJSJP ||
                 record.BPJSKESEHATANNO != existingRecord.BPJSKESEHATANNO || record.BPJSKESEHATANET != existingRecord.BPJSKESEHATANET || 
                 record.BPJSKETENAGAKERJAANNO != existingRecord.BPJSKETENAGAKERJAANNO || record.BPJSKETENAGAKERJAANNPP != existingRecord.BPJSKETENAGAKERJAANNPP || 
                 record.BPJSBASE != existingRecord.BPJSBASE || record.BPJSKESEHATANBASE != existingRecord.BPJSKESEHATANBASE
                ) 
                && (!changePermssion.ALLOWEDITBPJS))
                throw new Exception("Anda tidak memiliki akses untuk mengubah data bpcs karyawan");


            
            if (record.ATTENDGROUPID != existingRecord.ATTENDGROUPID && !changePermssion.ALLOWEDITABSENSI)
                throw new Exception("Anda tidak memiliki akses untuk mengubah data group absensi karyawan");

        }
        protected override MEMPLOYEE SaveInsertDetailsToDB(MEMPLOYEE record, string userName)
        {


            if (_newFamilies.Any())
                _context.MEMPLOYEEFAMILY.AddRange(_newFamilies);


            if (_newFiles.Any())
                _context.MEMPLOYEEFILE.AddRange(_newFiles);

            if (record.MEMPLOYEEBANK != null)
                _context.MEMPLOYEEBANK.Add(record.MEMPLOYEEBANK);


            return record;
        }
        
        


        protected override MEMPLOYEE AfterSave(MEMPLOYEE record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(_serviceName + record.UNITCODE, _context);
            HelperService.DHSUpdateMaster(userName, record.UPDATED, _serviceName, _context);


            MEMPLOYEEHISTORY history = null;
            if (_existingRecord == null)
            {
                history = new MEMPLOYEEHISTORY();
                history.CopyFrom(record);
                history.STARTDATE = record.JOINTDATE;
                history.CREATED = record.UPDATED;
                history.CREATEDBY = userName;
                _context.MEMPLOYEEHISTORY.Add(history);
            }
            else
            {
                bool hasHistory = false;
                MEMPLOYEEHISTORY prevHistory = _context.MEMPLOYEEHISTORY
                        .Where(d => d.EMPID.Equals(record.EMPID) && !d.ENDDATE.HasValue)                        
                        .OrderByDescending(d => d.STARTDATE)
                        .AsNoTracking()
                        .FirstOrDefault();

                

                if (prevHistory != null)
                    hasHistory = true;
                if (!hasHistory)
                {
                    prevHistory = new MEMPLOYEEHISTORY();
                    prevHistory.CopyFrom(_existingRecord);
                    prevHistory.STARTDATE = _existingRecord.JOINTDATE;
                    prevHistory.CREATED = record.UPDATED;
                    prevHistory.CREATEDBY = userName;
                    _context.MEMPLOYEEHISTORY.Add(prevHistory);
                }

                else if (!_existingRecord.DIVID.Equals(record.DIVID) ||
                    _existingRecord.BASICWAGES != record.BASICWAGES ||
                    !_existingRecord.POSITIONID.Equals(record.POSITIONID) ||
                    !_existingRecord.EMPTYPE.Equals(record.EMPTYPE) ||
                    !_existingRecord.STATUSID.Equals(record.STATUSID)

                )
                {
                    _context.Entry(prevHistory).State = EntityState.Detached;

                    DateTime startDate = record.UPDATED.Date;

                    //Set End Date of Last History
                    prevHistory.ENDDATE = startDate.AddDays(-1);
                    if (prevHistory.ENDDATE < prevHistory.STARTDATE)
                        prevHistory.ENDDATE = prevHistory.STARTDATE;
                    prevHistory.UPDATED = record.UPDATED;
                    prevHistory.UPDATEDBY = userName;
                    _context.Update<MEMPLOYEEHISTORY>(prevHistory);

                    //Insert New Histpory without End Date
                    history = new MEMPLOYEEHISTORY();
                    history.CopyFrom(record);
                    history.STARTDATE = record.JOINTDATE;
                    history.CREATED = record.UPDATED;
                    history.CREATEDBY = userName;
                    _context.MEMPLOYEEHISTORY.Add(history);
                }

                _context.SaveChanges();
                
            }

            return record;
        }

        protected override MEMPLOYEE SaveUpdateDetailsToDB(MEMPLOYEE record, string userName)
        {


            if (_deletedFamilies.Any())
                _context.MEMPLOYEEFAMILY.RemoveRange(_deletedFamilies);
            if (_newFamilies.Any())
                _context.MEMPLOYEEFAMILY.AddRange(_newFamilies);
            if (_editedFamilies.Any())
                _context.MEMPLOYEEFAMILY.UpdateRange(_editedFamilies);

            if (_deletedFiles.Any())
                _context.MEMPLOYEEFILE.RemoveRange(_deletedFiles);
            if (_newFiles.Any())
                _context.MEMPLOYEEFILE.AddRange(_newFiles);
            if (_editedFiles.Any())
                _context.MEMPLOYEEFILE.UpdateRange(_editedFiles);
            _context.MEMPLOYEEBANK.Update(record.MEMPLOYEEBANK);
            return record;
        }

        protected override MEMPLOYEE GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            var record = _context.MEMPLOYEE
                .Include(d => d.MEMPLOYEEFAMILY)
                .Include(d => d.MEMPLOYEEBANK)
                .Include(d => d.MEMPLOYEEFILE)
                .Include(d => d.STATUSNavigation)
                .AsNoTracking()
                .SingleOrDefault(d => d.EMPID.Equals(Id));
            if (record != null)
            {
                if (record.MEMPLOYEEBANK != null)
                {

                    record.BANKACCNO = record.MEMPLOYEEBANK.NO;
                    record.BANKACCNAME = record.MEMPLOYEEBANK.NAME;
                    record.BANKID = record.MEMPLOYEEBANK.BANKID;
                    record.BPJSJKK = record.MEMPLOYEEBANK.BPJSJKK;
                    record.BPJSJHT = record.MEMPLOYEEBANK.BPJSJHT;
                    record.BPJSJP = record.MEMPLOYEEBANK.BPJSJP;
                    record.BPJSKESEHATANNO = record.MEMPLOYEEBANK.BPJSKESEHATANNO;
                    record.BPJSKESEHATANET = record.MEMPLOYEEBANK.BPJSKESEHATANET;
                    record.BPJSKETENAGAKERJAANNO = record.MEMPLOYEEBANK.BPJSKETENAGAKERJAANNO;
                    record.BPJSKETENAGAKERJAANNPP = record.MEMPLOYEEBANK.BPJSKETENAGAKERJAANNPP;
                    record.BPJSBASE = record.MEMPLOYEEBANK.BPJSBASE;
                    record.BPJSKESEHATANBASE = record.MEMPLOYEEBANK.BPJSKESEHATANBASE;
                }
                if (record.STATUSNavigation != null)
                {
                    record.STATUSNAME = record.STATUSNavigation.STATUSNAME;
                    record.TAXSTATUS = record.STATUSNavigation.TAXSTATUS;
                    record.FAMILYSTATUS = record.STATUSNavigation.FAMILYSTATUS;
                }
            }
            
            return record;
        }

        public EmployeeChangePermission GetEmployeeChangePermission (string userName)
        {
            EmployeeChangePermission changePermission = new EmployeeChangePermission();

            var permissions = _authenticationService.GetPermissionMatrix(userName, "PMS.Organization.Employee", string.Empty);
            if (permissions != null)
            {
                foreach(var permission in permissions)
                {
                    if (permission.PermissionDetails.ToLower().Equals("edit"))
                        changePermission.ALLOWEDIT = true;
                    else if (permission.PermissionDetails.ToLower().Equals("bankaccount"))
                        changePermission.ALLOWEDITBANK = true;
                    else if (permission.PermissionDetails.ToLower().Equals("editbpjs"))
                        changePermission.ALLOWEDITBPJS = true;
                    else if(permission.PermissionDetails.ToLower().Equals("editabsensi"))
                        changePermission.ALLOWEDITABSENSI = true;
                }
            }
            return changePermission;
        }

        
        public override IEnumerable<VEMPLOYEE> GetList(FilterEmployee filter)
        {
            
            var criteria = PredicateBuilder.True<VEMPLOYEE>();
            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(d => d.EMPID.Equals(filter.Id));
            if (filter.Ids.Any())
                criteria = criteria.And(d => filter.Ids.Contains(d.EMPID));



            if (filter.IsUnitMandatory)
            {
                criteria = criteria.And(d => filter.UnitIDs.Contains(d.UNITCODE));
            }
            else
            {
                if (filter.UnitIDs.Any())
                    criteria = criteria.And(d => filter.UnitIDs.Contains(d.UNITCODE));
            }

            if (filter.IsDivisionMandatory)
            {
                criteria = criteria.And(d => filter.DivisionIDs.Contains(d.UNITCODE));
            }
            else
            {
                if (filter.DivisionIDs.Any())
                    criteria = criteria.And(d => filter.DivisionIDs.Contains(d.DIVID));
            }

            if (filter.ByUserName)
            {
                var authorizedUnitIDs = _authenticationService.GetAuthorizedUnitByUserName(filter.UserName, string.Empty).Select(d=>d.UNITCODE);
                criteria = criteria.And(d => authorizedUnitIDs.Contains(d.UNITCODE));
            }


            List<string> statuses = new List<string>();
            statuses.Add("A");
            if (filter.ShowInactive)
                statuses.Add("C");
            if (filter.ShowDeleted)
                statuses.Add("D");

            criteria = criteria.And(d => statuses.Contains(d.STATUS));

            if (filter.IsActive.HasValue)
            {
                if (filter.IsActive.Value)
                    criteria = criteria.And(d => d.STATUS.Equals("A"));
                else
                    criteria = criteria.And(d => !d.STATUS.Equals("A"));
            }

            if (!string.IsNullOrWhiteSpace(filter.PositionId))
                criteria = criteria.And(d => d.POSITIONID.Equals(filter.PositionId));
            if (filter.PinId > 0)
                criteria = criteria.And(d => d.PINID == filter.PinId);
            if (!string.IsNullOrWhiteSpace(filter.EmployeeCode))
                criteria = criteria.And(d => d.EMPCODE.Equals(filter.EmployeeCode));
            if (!string.IsNullOrWhiteSpace(filter.EmployeeName))
                criteria = criteria.And(d => d.EMPNAME.Equals(filter.EmployeeName));
            if (!string.IsNullOrWhiteSpace(filter.EmployeeType))
                criteria = criteria.And(d => d.EMPTYPE.Equals(filter.EmployeeType));
            //if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
            //    criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));
            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                criteria = criteria.And(d =>
                        d.UNITCODE.ToLower().Contains(filter.LowerCasedSearchTerm)
                        || d.DIVID.ToLower().Contains(filter.LowerCasedSearchTerm)                        
                        || d.EMPCODE.ToLower().Contains(filter.LowerCasedSearchTerm)
                        || d.EMPID.ToLower().Contains(filter.LowerCasedSearchTerm)
                        || d.EMPNAME.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );

            if (!filter.AllPostitionFlag)
            {
                General.Position position = new General.Position(_context,_authenticationService,_auditContext);
                FilterPosition filterPosition = new FilterPosition
                {
                    Mandor1 = filter.Mandor1,
                    MandorTanam = filter.MandorTanam,
                    MandorNonTanam = filter.MandorNonTanam,
                    KraniTanam = filter.KraniTanam,
                    KraniNonTanam = filter.KraniNonTanam,
                    Harvester = filter.Harvester
                };

                List<string> positionIds = position.GetList(filterPosition).Select(d => d.POSITIONID).ToList();
                criteria = criteria.And(d => positionIds.Contains(d.POSITIONID));
            }

            if (!string.IsNullOrWhiteSpace(filter.SupervisorId))
                criteria = criteria.And(d => d.SUPERVISORID.Equals(filter.SupervisorId));

            if (filter.SupervisorIds.Any())
                criteria = criteria.And(d => filter.SupervisorIds.Contains(d.SUPERVISORID));
            if (filter.PageSize <= 0)
                return _context.VEMPLOYEE.Where(criteria);
            return _context.VEMPLOYEE.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public override MEMPLOYEE NewRecord(string userName)
        {
            MEMPLOYEE record =
            new MEMPLOYEE
            {
                BIRTHDAY = DateTime.Today.AddYears(-17),
                JOINTDATE = DateTime.Today,
                EMPSEX = "L",
                BASICWAGES = 0,
                RACE = string.Empty,
                NATURA = false
            };

            record.MEMPLOYEEBANK = new MEMPLOYEEBANK();
            record.MEMPLOYEEFAMILY = new List<MEMPLOYEEFAMILY>();
            record.MEMPLOYEEFILE = new List<MEMPLOYEEFILE>();
            record.MEMPLOYEEHISTORY = new List<MEMPLOYEEHISTORY>();
            return record;
        }

        protected override bool DeleteFromDB(MEMPLOYEE record, string userName)
        {
            record = GetSingle(record.EMPID);
            record.STATUS = "D";
            record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            _context.Entry<MEMPLOYEE>(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
            
        }


#region Specific Functions
        

       

        public MEMPLOYEE NewFromEmployeeRegistration(TEMPLOYEEREGISTRATION employeeRegistration,string userName)
        {
            //Final Approve
            //Create new master record on HO
            MEMPLOYEE newEmployee = new MEMPLOYEE();
            newEmployee.CopyFrom(employeeRegistration);
            
            newEmployee.PINID = _context.sp_Employee_GenerateNewPin();
            _newFamilies = new List<MEMPLOYEEFAMILY>();
            _deletedFamilies = new List<MEMPLOYEEFAMILY>();
            _editedFamilies = new List<MEMPLOYEEFAMILY>();
            foreach (TEMPLOYEEREGISTRATIONFAMILY family in employeeRegistration.TEMPLOYEEREGISTRATIONFAMILY)
            {
                MEMPLOYEEFAMILY empFamily = new MEMPLOYEEFAMILY();
                empFamily.CopyFrom(family);
                //empFamily.EMPID = newEmployee.EMPID;
                _newFamilies.Add(empFamily);
            }


            _newFiles = new List<MEMPLOYEEFILE>();
            _deletedFiles = new List<MEMPLOYEEFILE>();
            _editedFiles = new List<MEMPLOYEEFILE>();

            foreach (var file in employeeRegistration.TEMPLOYEEREGISTRATIONFILE)
            {
                MEMPLOYEEFILE empFile = new MEMPLOYEEFILE();
                empFile.CopyFrom(file);
                //empFile.EMPID = newEmployee.EMPID;
                _newFiles.Add(empFile);
            }
            newEmployee.GOLONGAN = string.Empty;
            newEmployee.STATUS = "A";

            MEMPLOYEEBANK newEmployeeBank = new MEMPLOYEEBANK();
            newEmployeeBank.CopyFrom(employeeRegistration);
            newEmployeeBank.BANKID = string.Empty;
            newEmployeeBank.NO = string.Empty;
            newEmployeeBank.NAME = string.Empty;
            newEmployee.MEMPLOYEEBANK = newEmployeeBank;


            var newHistory = new MEMPLOYEEHISTORY
            {
                EMPID = newEmployee.EMPID,
                DIVID = newEmployee.DIVID,
                STARTDATE = newEmployee.JOINTDATE,
                POSITIONID = newEmployee.POSITIONID,
                EMPTYPE = newEmployee.EMPTYPE,
                BASICWAGES = newEmployee.BASICWAGES,
                STATUSID = newEmployee.STATUSID,
                UNITCODE = newEmployee.UNITCODE,
                CREATED = DateTime.Now,
                CREATEDBY = userName,
                UPDATED = DateTime.Now,
                UPDATEDBY = userName
            };

            newEmployee.MEMPLOYEEHISTORY.Add(newHistory);

            return SaveInsert(newEmployee, userName);
        }

        public MEMPLOYEE UpdateFromEmployeeChange(TEMPLOYEECHANGE employeeChange, string userName,PMSContextBase context)
        {

            
            PMSContextBase activeContext;

            if (context == null)            
                activeContext = _context;            
            else            
                activeContext = context;

            var  employee = activeContext.MEMPLOYEE.FirstOrDefault(d => d.EMPID.Equals(employeeChange.EMPID));
            var lastHistory = activeContext.MEMPLOYEEHISTORY.Where(d => d.EMPID.Equals(employeeChange.EMPID) && !d.ENDDATE.HasValue).OrderByDescending(d => d.STARTDATE).FirstOrDefault();

            

            if (!string.IsNullOrWhiteSpace(employeeChange.NEWDIVID))
            {
                var newDivision = activeContext.MDIVISI.Where(d => d.DIVID.Equals(employeeChange.NEWDIVID)).FirstOrDefault();
                if (newDivision == null)
                    throw new Exception("Lokasi baru tidak valid");
                employee.UNITCODE = newDivision.UNITCODE;
                employee.DIVID = newDivision.DIVID;
            }
            
            //if (!string.IsNullOrWhiteSpace(employeeChange.NEWUNITCODE))
            //    employee.UNITCODE = employeeChange.NEWUNITCODE;

            if (!string.IsNullOrWhiteSpace(employeeChange.NEWPOSITIONID))
                employee.POSITIONID = employeeChange.NEWPOSITIONID;
            if (!string.IsNullOrWhiteSpace(employeeChange.NEWEMPTYPE))
                employee.EMPTYPE = employeeChange.NEWEMPTYPE;
            if (!string.IsNullOrWhiteSpace(employeeChange.NEWSTATUSID))
                employee.STATUSID = employeeChange.NEWSTATUSID;
            if (employeeChange.NEWBASICWAGES.HasValue && employeeChange.NEWBASICWAGES.Value>0)
                employee.BASICWAGES = employeeChange.NEWBASICWAGES.Value;
            if (!string.IsNullOrWhiteSpace(employeeChange.NEWRECORDSTATUS))
                employee.STATUS = employeeChange.NEWRECORDSTATUS;
            if (employeeChange.NEWDIVID != employeeChange.DIVID)// Hapus Atasan lama jika mutasi
                employee.SUPERVISORID = string.Empty;

            if (employeeChange.EMPTYPE != employeeChange.NEWEMPTYPE && employeeChange.EMPTYPE.StartsWith("BHL") && employeeChange.NEWEMPTYPE.StartsWith("SKU")) // Dari BHL ke SKU, Update Join Date
                employee.STAFFDATE = employeeChange.EFFECTIVEDATE;

            if (employeeChange.NEWRECORDSTATUS == "D" || employeeChange.NEWRECORDSTATUS == "C")
                employee.RESIGNEDDATE = employeeChange.EFFECTIVEDATE;
            else if (employeeChange.NEWRECORDSTATUS == "A")
                employee.RESIGNEDDATE = null;


            employee.UPDATEDBY = userName;
            employee.UPDATED = DateTime.Now;

            MEMPLOYEEHISTORY newHistory = null;
            
            if (lastHistory == null || lastHistory.STARTDATE != employeeChange.EFFECTIVEDATE)
            {
                newHistory = new MEMPLOYEEHISTORY
                {
                    EMPID = employee.EMPID,
                    DIVID = employee.DIVID,
                    STARTDATE = employeeChange.EFFECTIVEDATE,
                    POSITIONID = employee.POSITIONID,
                    EMPTYPE = employee.EMPTYPE,
                    BASICWAGES = employee.BASICWAGES,
                    STATUSID = employee.STATUSID,
                    UNITCODE = employee.UNITCODE,
                    CREATED = DateTime.Now,
                    CREATEDBY = userName,
                    UPDATED = DateTime.Now,
                    UPDATEDBY = userName
                };
                if (lastHistory != null)
                    lastHistory.ENDDATE = employeeChange.EFFECTIVEDATE;
            }
            else if (lastHistory.STARTDATE == employeeChange.EFFECTIVEDATE)
            {

                lastHistory.DIVID = employee.DIVID;
                lastHistory.POSITIONID = employee.POSITIONID;
                lastHistory.EMPTYPE = employee.EMPTYPE;
                lastHistory.BASICWAGES = employee.BASICWAGES;
                lastHistory.STATUSID = employee.STATUSID;
                lastHistory.UNITCODE = employee.UNITCODE;
                lastHistory.UPDATED = DateTime.Now;
                lastHistory.UPDATEDBY = userName;
            }
            
            

            

            
            if (context == null)
            {
                
                _context.MEMPLOYEE.Update(employee);
                if (lastHistory != null)
                    _context.MEMPLOYEEHISTORY.Update(lastHistory);
                if (newHistory != null)
                    _context.MEMPLOYEEHISTORY.Add(newHistory);
                _context.SaveChanges();
            }
            else
            {
                context.MEMPLOYEE.Update(employee);
                if (lastHistory != null)
                    context.MEMPLOYEEHISTORY.Update(lastHistory);
                if (newHistory != null)
                    context.MEMPLOYEEHISTORY.Add(newHistory);

                context.SaveChanges();
            }
            if (employeeChange.NEWRECORDSTATUS == "D" && employeeChange.RECORDSTATUS != employeeChange.NEWRECORDSTATUS)
                _auditContext.SaveAuditTrail(userName, employeeChange.EMPID, "Delete employee by Perubahan Data Approval");
            else if (employeeChange.NEWRECORDSTATUS == "C" && employeeChange.RECORDSTATUS != employeeChange.NEWRECORDSTATUS)
                _auditContext.SaveAuditTrail(userName, employeeChange.EMPID, "Deactivate employee by Perubahan Data Approval");
            else if (employeeChange.NEWRECORDSTATUS == "A" && employeeChange.RECORDSTATUS != employeeChange.NEWRECORDSTATUS)
                _auditContext.SaveAuditTrail(userName, employeeChange.EMPID, "Activate employee by Perubahan Data Approval");
            else
                _auditContext.SaveAuditTrail(userName, employeeChange.EMPID, "Update employee by Perubahan Data Approval");

            return employee;
        }

        public MEMPLOYEE UpdateFromEmployeeChange(TEMPLOYEECHANGE employeeChange, string userName)
        {
            return UpdateFromEmployeeChange(employeeChange, userName, null);
        }

        public void CopyEmployeeMaster(string employeeId, string destinationUnitId, string userName)
        {
            var employee = (
                                from a in _context.MEMPLOYEE
                                where a.EMPID.Equals(employeeId)
                                select a
                           ).FirstOrDefault();

            if (employee == null)
                throw new Exception("Karyawan tidak ditemukan");

            if (string.IsNullOrWhiteSpace(destinationUnitId))
                destinationUnitId = employee.UNITCODE;

            if (!employee.UNITCODE.Equals(destinationUnitId))
                throw new Exception("Karyawan hanya boleh dicopy ke unit tempat karyawan terdaftar");

            MUNITDBSERVER db = _context.MUNITDBSERVER.Find(destinationUnitId);
            if (db == null)
                throw new Exception($"Alamat server untuk unit {destinationUnitId} tidak ditemukan");



            var employeeBank = (
                                from a in _context.MEMPLOYEEBANK
                                where a.EMPID.Equals(employeeId)
                                select a
                           ).FirstOrDefault();

            var employeeHistories = (from a in _context.MEMPLOYEEHISTORY
                                     where a.EMPID.Equals(employeeId) && a.UNITCODE.Equals(destinationUnitId)
                                     select a).ToList();

            var employeeFiles= (from a in _context.MEMPLOYEEFILE
                                where a.EMPID.Equals(employeeId) 
                                select a).ToList();


            var employeeFamilies = (from a in _context.MEMPLOYEEFAMILY
                                 where a.EMPID.Equals(employeeId)
                                 select a).ToList();


            var employeeFileIds = employeeFiles.Select(d => d.FILEID).ToList();
            var employeeHistoryIds = employeeHistories.Select(d => d.STARTDATE).ToList();
            var employeeFamilyIds = employeeFamilies.Select(d => d.KTPID).ToList();




            

            

            using (PMSContextBase unitContext = new PMSContextBase(PMS.EFCore.Helper.DBContextOption<PMSContextBase>.GetOptions(db.SERVERNAME, db.DBNAME, db.DBUSER, db.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
            {
                bool isNewEmployee = (from a in unitContext.MEMPLOYEE
                                      where a.EMPID.Equals(employeeId)
                                      select a
                                      ).Count() <= 0;


                if (isNewEmployee)
                {   
                    unitContext.MEMPLOYEE.Add(employee);
                    unitContext.MEMPLOYEEBANK.Add(employeeBank);
                    if (employeeHistories != null && employeeHistories.Count>0)
                        unitContext.MEMPLOYEEHISTORY.AddRange(employeeHistories);
                    if (employeeFiles != null && employeeFiles.Count > 0)
                        unitContext.MEMPLOYEEFILE.AddRange(employeeFiles);
                    if (employeeFamilies != null && employeeFamilies.Count > 0)
                        unitContext.MEMPLOYEEFAMILY.AddRange(employeeFamilies);

                }
                else
                {
                    unitContext.MEMPLOYEE.Update(employee);
                    bool isNewEmployeeBank = (from a in unitContext.MEMPLOYEEBANK
                                          where a.EMPID.Equals(employeeId)
                                          select a
                                      ).Count() <= 0;
                    if (isNewEmployeeBank)
                        unitContext.MEMPLOYEEBANK.Add(employeeBank);
                    else
                        unitContext.MEMPLOYEEBANK.Update(employeeBank);



                    var existingFileIds = unitContext.MEMPLOYEEFILE.Where(d => d.EMPID.Equals(employeeId)).Select(d=>d.FILEID).ToList();
                    var existingHistoryIds = unitContext.MEMPLOYEEHISTORY.Where(d => d.EMPID.Equals(employeeId)).Select(d=>d.STARTDATE).ToList();
                    var existingFamilyIds = unitContext.MEMPLOYEEFAMILY.Where(d => d.EMPID.Equals(employeeId)).Select(d => d.KTPID).ToList();

                    List<long> insertedFileIds = new List<long>(), deletedFileIds = new List<long>(), updatedFileIds = new List<long>();
                    updatedFileIds = StandardUtility.GetUpdatedItems(employeeFileIds, existingFileIds, out insertedFileIds, out deletedFileIds);

                    var deletedFiles = unitContext.MEMPLOYEEFILE.Where(d => d.EMPID.Equals(employeeId) && deletedFileIds.Contains(d.FILEID));
                    var insertedFiles = employeeFiles.Where(d => insertedFileIds.Contains(d.FILEID));
                    var updatedFiles = employeeFiles.Where(d => updatedFileIds.Contains(d.FILEID));

                    unitContext.MEMPLOYEEFILE.RemoveRange(deletedFiles);
                    unitContext.MEMPLOYEEFILE.AddRange(insertedFiles);
                    unitContext.MEMPLOYEEFILE.UpdateRange(updatedFiles);

                    List<DateTime> insertedHistoryIds = new List<DateTime>(), deletedHistoryIds = new List<DateTime>(), updatedHistoryIds = new List<DateTime>();
                    updatedHistoryIds = StandardUtility.GetUpdatedItems(employeeHistoryIds, existingHistoryIds, out insertedHistoryIds, out deletedHistoryIds);

                    
                    var deletedHistories = unitContext.MEMPLOYEEHISTORY.Where(d => d.EMPID.Equals(employeeId) && deletedHistoryIds.Contains(d.STARTDATE));
                    var insertedHistories = employeeHistories.Where(d => insertedHistoryIds.Contains(d.STARTDATE));
                    var updatedHistories = employeeHistories.Where(d => updatedHistoryIds.Contains(d.STARTDATE));

                    unitContext.MEMPLOYEEHISTORY.RemoveRange(deletedHistories);
                    unitContext.MEMPLOYEEHISTORY.AddRange(insertedHistories);
                    unitContext.MEMPLOYEEHISTORY.UpdateRange(updatedHistories);


                    List<string> insertedFamilyIds = new List<string>(), deletedFamilyIds = new List<string>(), updatedFamilyIds = new List<string>();
                    updatedFamilyIds = StandardUtility.GetUpdatedItems(employeeFamilyIds, existingFamilyIds, out insertedFamilyIds, out deletedFamilyIds);


                    var deletedFamilies = unitContext.MEMPLOYEEFAMILY.Where(d => d.EMPID.Equals(employeeId) && deletedFamilyIds.Contains(d.KTPID));
                    var insertedFamilies = employeeFamilies.Where(d => insertedFamilyIds.Contains(d.KTPID));
                    var updatedFamilies = employeeFamilies.Where(d => updatedFamilyIds.Contains(d.KTPID));

                    unitContext.MEMPLOYEEFAMILY.RemoveRange(deletedFamilies);
                    unitContext.MEMPLOYEEFAMILY.AddRange(insertedFamilies);
                    unitContext.MEMPLOYEEFAMILY.UpdateRange(updatedFamilies);


                }

                
                unitContext.SaveChanges();
                //MEMPLOYEE newEmployee = unitContext
                //    .MEMPLOYEE
                //    .Include(d => d.MEMPLOYEEBANK)
                //    .Include(d => d.MEMPLOYEEFAMILY)
                //    .Include(d => d.MEMPLOYEEFILE)
                //    .Include(d => d.MEMPLOYEEHISTORY)
                //    .AsNoTracking()
                //    .SingleOrDefault(d => d.EMPID.Equals(employeeId));

                //if (newEmployee == null)
                //{
                //    isNewEmployee = true;
                //    newEmployee = NewRecord();
                //}

                ////Copy Employee Header
                //newEmployee.CopyFrom(employee);
                //if (isNewEmployee)
                //    unitContext.MEMPLOYEE.Add(newEmployee);
                //else
                //    unitContext.MEMPLOYEE.Update(newEmployee);
               
                //unitContext.SaveChanges();
            }
                


        }

        public void CopyEmployeeMaster(string employeeId, string userName)
        {
            CopyEmployeeMaster(employeeId, string.Empty, userName);


        }

        public void CopyEmployees(List<string> employeeIds, string unitCode, PMSContextEstate contextEstate)
        {
            var hoRecords = _context.MEMPLOYEE.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID) && d.UNITCODE.Equals(unitCode)).ToList();
            var estateRecords = contextEstate.MEMPLOYEE.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID)).ToList();

            if (StandardUtility.IsEmptyList(hoRecords))
                return;

            var updatedRecords = (from a in hoRecords
                                  join b in estateRecords on a.EMPID equals b.EMPID
                                  select a
                          ).ToList();

            var newRecords = (from a in hoRecords
                              join b in estateRecords on a.EMPID equals b.EMPID into ab
                              from abLeft in ab.DefaultIfEmpty()
                              where abLeft == null
                              select a
                              ).ToList();

            if (!StandardUtility.IsEmptyList(updatedRecords))
                contextEstate.UpdateRange(updatedRecords);
            if (!StandardUtility.IsEmptyList(newRecords))
                contextEstate.AddRange(newRecords);

            CopyEmployeeBank(employeeIds, contextEstate);
            CopyEmployeeFamily(employeeIds, contextEstate);
            CopyEmployeeFile(employeeIds, contextEstate);
            CopyEmployeeHistory(employeeIds,unitCode, contextEstate);
            contextEstate.SaveChanges();
        }

        private void CopyEmployeeHistory(List<string> employeeIds, string unitCode, PMSContextEstate contextEstate)
        {
            var hoRecords = _context.MEMPLOYEEHISTORY.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID) && d.UNITCODE.Equals(unitCode)).ToList();
            var estateRecords = contextEstate.MEMPLOYEEHISTORY.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID)).ToList();


            var updatedRecords = (from a in hoRecords
                                  join b in estateRecords on new { a.EMPID, a.STARTDATE } equals new { b.EMPID, b.STARTDATE }
                                  select a
                          ).ToList();

            var newRecords = (from a in hoRecords
                              join b in estateRecords on new { a.EMPID, a.STARTDATE } equals new { b.EMPID, b.STARTDATE } into ab
                              from abLeft in ab.DefaultIfEmpty()
                              where abLeft == null
                              select a
                              ).ToList();

            var deletedRecords = (from a in estateRecords
                                  join b in hoRecords on new { a.EMPID, a.STARTDATE } equals new { b.EMPID, b.STARTDATE } into ab
                                  from abLeft in ab.DefaultIfEmpty()
                                  where abLeft == null
                                  select a
                              ).ToList();

            if (!StandardUtility.IsEmptyList(updatedRecords))
                contextEstate.UpdateRange(updatedRecords);
            if (!StandardUtility.IsEmptyList(newRecords))
                contextEstate.AddRange(newRecords);
            if (!StandardUtility.IsEmptyList(deletedRecords))
                contextEstate.RemoveRange(deletedRecords);

        }

        private void CopyEmployeeFamily(List<string> employeeIds, PMSContextEstate contextEstate)
        {
            var hoRecords = _context.MEMPLOYEEFAMILY.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID)).ToList();
            var estateRecords = contextEstate.MEMPLOYEEFAMILY.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID)).ToList();


            var updatedRecords = (from a in hoRecords
                                  join b in estateRecords on new { a.EMPID, a.KTPID } equals new { b.EMPID, b.KTPID }
                                  select a
                          ).ToList();

            var newRecords = (from a in hoRecords
                              join b in estateRecords on new { a.EMPID, a.KTPID } equals new { b.EMPID, b.KTPID } into ab
                              from abLeft in ab.DefaultIfEmpty()
                              where abLeft == null
                              select a
                              ).ToList();

            var deletedRecords = (from a in estateRecords
                                  join b in hoRecords on new { a.EMPID, a.KTPID } equals new { b.EMPID, b.KTPID } into ab
                                  from abLeft in ab.DefaultIfEmpty()
                                  where abLeft == null
                                  select a
                              ).ToList();

            if (!StandardUtility.IsEmptyList(updatedRecords))
                contextEstate.UpdateRange(updatedRecords);
            if (!StandardUtility.IsEmptyList(newRecords))
                contextEstate.AddRange(newRecords);
            if (!StandardUtility.IsEmptyList(deletedRecords))
                contextEstate.RemoveRange(deletedRecords);

        }

        private void CopyEmployeeFile(List<string> employeeIds, PMSContextEstate contextEstate)
        {
            var hoRecords = _context.MEMPLOYEEFILE.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID)).ToList();
            var estateRecords = contextEstate.MEMPLOYEEFILE.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID)).ToList();


            var updatedRecords = (from a in hoRecords
                                  join b in estateRecords on new { a.EMPID, a.FILEID } equals new { b.EMPID, b.FILEID }
                                  select a
                          ).ToList();

            var newRecords = (from a in hoRecords
                              join b in estateRecords on new { a.EMPID, a.FILEID } equals new { b.EMPID, b.FILEID } into ab
                              from abLeft in ab.DefaultIfEmpty()
                              where abLeft == null
                              select a
                              ).ToList();

            var deletedRecords = (from a in estateRecords
                                  join b in hoRecords on new { a.EMPID, a.FILEID } equals new { b.EMPID, b.FILEID } into ab
                                  from abLeft in ab.DefaultIfEmpty()
                                  where abLeft == null
                                  select a
                             ).ToList();

            if (!StandardUtility.IsEmptyList(updatedRecords))
                contextEstate.UpdateRange(updatedRecords);
            if (!StandardUtility.IsEmptyList(newRecords))
                contextEstate.AddRange(newRecords);
            if (!StandardUtility.IsEmptyList(deletedRecords))
                contextEstate.RemoveRange(deletedRecords);
        }

        private void CopyEmployeeBank(List<string> employeeIds, PMSContextEstate contextEstate)
        {
            var hoRecords = _context.MEMPLOYEEBANK.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID)).ToList();
            var estateRecords = contextEstate.MEMPLOYEEBANK.AsNoTracking().Where(d => employeeIds.Contains(d.EMPID)).ToList();


            var updatedRecords = (from a in hoRecords
                                  join b in estateRecords on a.EMPID equals b.EMPID
                                  select a
                          ).ToList();

            var newRecords = (from a in hoRecords
                              join b in estateRecords on a.EMPID equals b.EMPID into ab
                              from abLeft in ab.DefaultIfEmpty()
                              where abLeft == null
                              select a
                              ).ToList();


            if (!StandardUtility.IsEmptyList(updatedRecords))
                contextEstate.UpdateRange(updatedRecords);
            if (!StandardUtility.IsEmptyList(newRecords))
                contextEstate.AddRange(newRecords);

        }



        public IEnumerable<FileStorage.EFCore.File> GetAttachments(string Id, FilestorageContext fileContext)
        {
            return
            (
                from a in _context.MEMPLOYEEFILE
                join b in fileContext.File on a.FILEID equals b.FileID
                where a.EMPID.Equals(Id)
                select b
             ).ToList();

        }

        public VEMPLOYEE GetDetails(string Id)
        {
            return _context.VEMPLOYEE.SingleOrDefault(d => d.EMPID.Equals(Id));
        }
        #endregion

        

        public void UpdateEmployeeStatusFromSP3(PMSContextEstate contextEstate,PMSContextHO contextHO)
        {
            try
            {

                Random rnd = new Random(DateTime.Now.Millisecond);
                string sessionId = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}-{rnd.Next()}";
                contextEstate.jbs_Attendance_LastDay_V2(sessionId);

                var result = contextEstate.JOBSEMPLASTDATE.AsNoTracking().Where(d => d.SESSIONID.Equals(sessionId)).ToList();
                if (result != null && result.Count>0)
                {
                    //Delete Last 7 Days by Estate - Replacement of sp : jbs_EmpLastDate_Delete 
                    try
                    {
                        //Reset Identity
                        result.ForEach(d => {
                            d.ID = 0;
                        });
                        contextHO.JOBSEMPLASTDATE.AddRange(result);
                        contextHO.SaveChanges();

                        var unitIds = result.Select(d => d.UNITID).Distinct().ToList();
                        var deleteLast7Days = contextHO.JOBSEMPLASTDATE.AsNoTracking().Where(d => unitIds.Contains(d.UNITID) && d.TODAY.Date <= DateTime.Today.AddDays(-7)).ToList();
                        contextHO.JOBSEMPLASTDATE.RemoveRange(deleteLast7Days);
                        contextHO.SaveChanges();

                        //Insert Into  JOBSEMPLASTDATE in HO Server

                    }
                    catch(Exception ex)
                    {
                        string errorMesage = ExceptionMessage.GetAllExceptionMessage(ex);
                        _auditContext.SaveAuditTrail("PMSW-Otomatis", "NonActiveEmpSP3", "Error Insert JOBSEMPLASTDATE : " + errorMesage);
                    }
                    _auditContext.SaveAuditTrail("PMSW-Otomatis", "NonActiveEmpSP3", "Run jbs_Attendance_LastDay - Success");
                }
                else
                    _auditContext.SaveAuditTrail("PMSW-Otomatis", "NonActiveEmpSP3", "Run jbs_Attendance_LastDay - Success With No Result");


                var listEmpSP3 = contextEstate.TSURATPERINGATAN.AsNoTracking().Where(d => d.TYPESP.Equals("3") && !string.IsNullOrWhiteSpace(d.ISPROCESS) && d.ISPROCESS.Equals("0")).ToList();



                if (listEmpSP3 != null && listEmpSP3.Count > 0)
                {

                    var employees = contextHO.MEMPLOYEE.AsNoTracking().Where(d => listEmpSP3.Select(e => e.EMPID).Contains(d.EMPID) && d.STATUS.Equals("A") ).ToList();
                    var lastSP3 = listEmpSP3.GroupBy(d => d.EMPID).Select(d => new { EMPID = d.Key, TGLSP = d.Max(s => s.TGLSP) }).ToList();

                    (from a in employees
                     join b in lastSP3 on a.EMPID equals b.EMPID
                     select new { Employee = a, ResignedDate = b.TGLSP }).ToList().ForEach(d => {
                         d.Employee.RESIGNEDDATE = d.ResignedDate;
                         d.Employee.STATUS = "C";
                         d.Employee.UPDATED = DateTime.Now;
                         d.Employee.UPDATEDBY = "PMSW-Otomatis";
                     });


                    
                    contextHO.UpdateRange(employees);
                    contextHO.SaveChanges();
                    

                    contextEstate.UpdateRange(employees);
                    listEmpSP3.ForEach(d => 
                    {
                        d.ISPROCESS = "1";
                        d.UPDATED = DateTime.Now;
                        d.UPDATEDBY = "PMSW-Otomatis";
                    });
                    contextEstate.UpdateRange(listEmpSP3);
                    contextEstate.SaveChanges();
                    

                    _auditContext.SaveAuditTrail("PMSW-Otomatis", "NonActiveEmpSP3", "Non Active Employee Success : " + string.Join(",", employees.Select(d => d.EMPID).ToList()));
                }
            }
            catch(Exception ex)
            {
                string errorMesage = ExceptionMessage.GetAllExceptionMessage(ex);
                _auditContext.SaveAuditTrail("PMSW-Otomatis", "NonActiveEmpSP3", "Error : " + errorMesage);
            }
        }

        


        public IEnumerable<MEMPLOYEEDEL> ListDeleteEmployeeByPayroll(PMSContextHO contextHO, string unitCode, string keyword, bool? flag, string userName)
        {
            if (string.IsNullOrWhiteSpace(unitCode))
                throw new Exception("Estate belum dipilih");

            if (string.IsNullOrWhiteSpace(keyword))
                throw new Exception("No dokumen payroll belum dipilih");


            var unitCodes = _authenticationService.GetAuthorizedUnitByUserName(userName, unitCode).Select(d => d.UNITCODE).ToList();
            if (unitCodes == null || unitCodes.Count <= 0)
                throw new Exception("Anda tidak memiliki akses pada unit");

            var estateDBServer = contextHO.MUNITDBSERVER.FirstOrDefault(d => d.UNITCODE.Equals(unitCode));
            if (estateDBServer == null)
                throw new Exception($"Konfigurasi server untuk estate {unitCode} tidak ditemukan");

            using (PMSContextBase unitContext = new PMSContextBase(PMS.EFCore.Helper.DBContextOption<PMSContextBase>.GetOptions(estateDBServer.SERVERNAME, estateDBServer.DBNAME, estateDBServer.DBUSER, estateDBServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
            {
                var predicate = PredicateBuilder.True<MEMPLOYEEDEL>();

                predicate = predicate.And(d => d.UNITCODE.Equals(unitCode));

                keyword = keyword.ToUpper();
                predicate = predicate.And(d => d.DOCNO.ToUpper().Equals(keyword));

                if (flag.HasValue)
                    predicate = predicate.And(d => d.FLAG == flag.Value);

                var deletedEmployees = unitContext.MEMPLOYEEDEL.Where(predicate).ToList();

                if (deletedEmployees == null || deletedEmployees.Count <= 0)
                    return new List<MEMPLOYEEDEL>();


                (from a in deletedEmployees
                 join b in contextHO.MEMPLOYEE on a.EMPID equals b.EMPID
                 join c in contextHO.MPOSITION on b.POSITIONID equals c.POSITIONID into bc
                 from bcLeft in bc.DefaultIfEmpty()
                 select new { DeletedEmployee = a, EmployeeName = b.EMPNAME, PositionName = bcLeft == null ? string.Empty : bcLeft.POSITIONNAME }).ToList().ForEach(d => {
                     d.DeletedEmployee.EMPNAME = d.EmployeeName;
                     d.DeletedEmployee.EMPPOSITION = d.PositionName;
                 });

                return deletedEmployees;
            }


        }

        

        public void ProcessDeleteEmployeeByPayroll(PMSContextHO contextHO, string unitCode, string keyword, string userName)
        {
            if (string.IsNullOrWhiteSpace(unitCode))
                throw new Exception("Estate belum dipilih");

            if (string.IsNullOrWhiteSpace(keyword))
                throw new Exception("No dokumen payroll belum dipilih");


            var unitCodes = _authenticationService.GetAuthorizedUnitByUserName(userName, unitCode).Select(d => d.UNITCODE).ToList();
            if (unitCodes == null || unitCodes.Count <= 0)
                throw new Exception("Anda tidak memiliki akses pada unit");

            var estateDBServer = contextHO.MUNITDBSERVER.FirstOrDefault(d => d.UNITCODE.Equals(unitCode));
            if (estateDBServer == null)
                throw new Exception($"Konfigurasi server untuk estate {unitCode} tidak ditemukan");


            using (PMSContextBase unitContext = new PMSContextBase(PMS.EFCore.Helper.DBContextOption<PMSContextBase>.GetOptions(estateDBServer.SERVERNAME, estateDBServer.DBNAME, estateDBServer.DBUSER, estateDBServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
            {
                var predicate = PredicateBuilder.True<MEMPLOYEEDEL>();

                predicate = predicate.And(d => d.UNITCODE.Equals(unitCode));

                keyword = keyword.ToUpper();
                predicate = predicate.And(d => d.DOCNO.ToUpper().Equals(keyword));

                predicate = predicate.And(d => !d.FLAG);

                var deletedEmployees = (
                    from a in unitContext.MEMPLOYEEDEL.AsNoTracking().Where(predicate)
                    join b in unitContext.TPAYMENT on a.DOCNO equals b.DOCNO
                    select new { MEMPLOYEEDEL = a, b.DOCDATE }
                    ).ToList();


                if (StandardUtility.IsEmptyList(deletedEmployees))                
                    throw new Exception("Data tidak ditemukan");

                deletedEmployees.ForEach(d => {
                    d.MEMPLOYEEDEL.RESIGNEDDATE = d.DOCDATE;
                });




                var deletedEmployeeDate = deletedEmployees.Select(d => new { d.MEMPLOYEEDEL.EMPID, d.MEMPLOYEEDEL.RESIGNEDDATE }).Distinct().ToList();
                var deletedEmployeeIDs = deletedEmployees.Select(d => d.MEMPLOYEEDEL.EMPID).Distinct().ToList();

                var employees = (
                                from a in contextHO.MEMPLOYEE.AsNoTracking().Where(d => deletedEmployeeIDs.Contains(d.EMPID))// && d.STATUS != "D")
                                join b in deletedEmployeeDate on a.EMPID equals b.EMPID
                                select new { EMPLOYEE = a, b.RESIGNEDDATE }
                                ).ToList();

                if (StandardUtility.IsEmptyList(employees))
                    throw new Exception("Data tidak ditemukan");


                DateTime now = GetServerTime();

                employees.ForEach(d => {
                    d.EMPLOYEE.RESIGNEDDATE = d.RESIGNEDDATE;
                    d.EMPLOYEE.STATUS = "D";
                    d.EMPLOYEE.UPDATED = now;
                    d.EMPLOYEE.UPDATEDBY = userName;
                });

                deletedEmployees.ForEach(d => {
                    d.MEMPLOYEEDEL.UPDATED = now;
                    d.MEMPLOYEEDEL.FLAG = true;
                });

                contextHO.UpdateRange(employees.Select(d=>d.EMPLOYEE).ToList());
                contextHO.SaveChanges();

                unitContext.UpdateRange(deletedEmployees.Select(d=>d.MEMPLOYEEDEL).ToList());
                unitContext.UpdateRange(employees.Select(d => d.EMPLOYEE).ToList());
                unitContext.SaveChanges();

                _auditContext.SaveAuditTrail(userName, "Delete Employee By Payroll", "Delete Employee By Payroll Success: " + string.Join(", ", employees.Select(d => d.EMPLOYEE.EMPID).ToList()));
            }

        }



        #region Tools
        public void UpdateEmployeeStatusFromSP3ByHO(PMSContextHO contextHO, List<string> unitCodes)
        {

            string processName = "UpdateEmployeeStatusFromSP3ByHO";
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
                            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{d.SERVERNAME}:{d.DBNAME}]Start Process");
                            UpdateEmployeeStatusFromSP3(contextEstate, contextHO);
                            Console.WriteLine($"[{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}][{_serviceName}][{processName}][{d.SERVERNAME}:{d.DBNAME}]Finish Process");
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
        #endregion

    }
}
