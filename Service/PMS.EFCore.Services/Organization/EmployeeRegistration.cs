using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model.Filter;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using WF.EFCore.Models;
using WF.EFCore.Data;

using AM.EFCore.Services;

using PMS.EFCore.Services.Approval;

using PMS.Shared.Services;

using PMS.EFCore.Model;
using PMS.Shared.Exceptions;

namespace PMS.EFCore.Services.Organization
{
    public class EmployeeRegistration : EntityFactoryWithWorkflow<TEMPLOYEEREGISTRATION, TEMPLOYEEREGISTRATION, GeneralFilter, PMSContextBase, WFContext>
    {

        private List<TEMPLOYEEREGISTRATIONFAMILY> _newFamilies = new List<TEMPLOYEEREGISTRATIONFAMILY>();
        private List<TEMPLOYEEREGISTRATIONFAMILY> _deletedFamilies = new List<TEMPLOYEEREGISTRATIONFAMILY>();
        private List<TEMPLOYEEREGISTRATIONFAMILY> _editedFamilies = new List<TEMPLOYEEREGISTRATIONFAMILY>();


        private List<TEMPLOYEEREGISTRATIONFILE> _newFiles = new List<TEMPLOYEEREGISTRATIONFILE>();
        private List<TEMPLOYEEREGISTRATIONFILE> _deletedFiles = new List<TEMPLOYEEREGISTRATIONFILE>();
        private List<TEMPLOYEEREGISTRATIONFILE> _editedFiles = new List<TEMPLOYEEREGISTRATIONFILE>();
        private AuthenticationServiceBase _authenticationService;

        private const string CFG_BACKDATED_EFFECTIVE_DATE = "EMPREGISTBACKDATEDDAYS";



        public EmployeeRegistration(PMSContextHO context, WFContext wfContext, AuthenticationServiceHO authenticationService, IBackgroundTaskQueue taskQueue, IEmailSender emailSender, AuditContext auditContext) : base(context, wfContext, authenticationService, authenticationService, taskQueue, emailSender, auditContext)
        {
            _serviceName = "EMPREGIST";
            _wfDocumentType = "EMPREGIST";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<TEMPLOYEEREGISTRATION> GetList(GeneralFilter filter)
        {

            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                return null;
            if (string.IsNullOrWhiteSpace(filter.UnitID))
                return null;

            var criteria = PredicateBuilder.True<TEMPLOYEEREGISTRATION>();
            criteria = criteria.And(d => d.CREATED.Date >= filter.StartDate.Date && d.CREATED.Date <= filter.EndDate.Date && d.UNITCODE.Equals(filter.UnitID));
            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(d => d.DIVID.Equals(filter.DivisionID));
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                criteria = criteria.And(d => d.EMPNAME.ToLower().Contains(filter.LowerCasedSearchTerm));

            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);

            if (filter.PageSize <= 0)
                return _context.TEMPLOYEEREGISTRATION.Where(criteria);
            return _context.TEMPLOYEEREGISTRATION.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public override TEMPLOYEEREGISTRATION CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TEMPLOYEEREGISTRATION record = base.CopyFromWebFormData(formData, newRecord);

            /*Custom Code - Start*/
            /*Employee File*/
            List<TEMPLOYEEREGISTRATIONFILE> _potentialFiles = new List<TEMPLOYEEREGISTRATIONFILE>();
            _potentialFiles.CopyFrom<TEMPLOYEEREGISTRATIONFILE>(formData, "TEMPLOYEEREGISTRATIONFILE");
            _potentialFiles.ForEach(d => d.REGISTRATIONID = record.REGISTRATIONID);
            List<long> _potentialFileIds = _potentialFiles.Select(d => d.FILEID).ToList();
            _newFiles = new List<TEMPLOYEEREGISTRATIONFILE>();
            _editedFiles = new List<TEMPLOYEEREGISTRATIONFILE>();
            _deletedFiles = new List<TEMPLOYEEREGISTRATIONFILE>();



            /*Employee Family*/
            List<TEMPLOYEEREGISTRATIONFAMILY> _potentialItems = new List<TEMPLOYEEREGISTRATIONFAMILY>();
            _potentialItems.CopyFrom<TEMPLOYEEREGISTRATIONFAMILY>(formData, "TEMPLOYEEREGISTRATIONFAMILY");
            _potentialItems.ForEach(d => d.REGISTRATIONID = record.REGISTRATIONID);
            List<string> _potentialIds = _potentialItems.Select(d => d.KTPID).ToList();
            _newFamilies = new List<TEMPLOYEEREGISTRATIONFAMILY>();
            _editedFamilies = new List<TEMPLOYEEREGISTRATIONFAMILY>();
            _deletedFamilies = new List<TEMPLOYEEREGISTRATIONFAMILY>();


            if (newRecord)
            {
                _newFamilies = _potentialItems;
                _newFiles = _potentialFiles;
            }
            else
            {
                if (!_potentialIds.Any())
                    _deletedFamilies = _context.TEMPLOYEEREGISTRATIONFAMILY.Where(x => x.REGISTRATIONID.Equals(record.REGISTRATIONID)).ToList();
                else
                {
                    var existingItems = _context.TEMPLOYEEREGISTRATIONFAMILY.Where(x => x.REGISTRATIONID.Equals(record.REGISTRATIONID));
                    var existingItemsId = existingItems.Select(x => x.KTPID).ToList();

                    _newFamilies = _potentialItems.Where(o => !existingItemsId.Contains(o.KTPID)).ToList();
                    _deletedFamilies = existingItems.Where(o => !_potentialIds.Contains(o.KTPID)).ToList();
                    _editedFamilies = _potentialItems.Where(o => existingItemsId.Contains(o.KTPID)).ToList();
                }


                if (!_potentialFileIds.Any())
                    _deletedFiles = _context.TEMPLOYEEREGISTRATIONFILE.Where(x => x.REGISTRATIONID.Equals(record.REGISTRATIONID)).ToList();
                else
                {
                    var existingItems = _context.TEMPLOYEEREGISTRATIONFILE.Where(x => x.REGISTRATIONID.Equals(record.REGISTRATIONID));
                    var existingItemsId = existingItems.Select(x => x.FILEID).ToList();

                    _newFiles = _potentialFiles.Where(o => !existingItemsId.Contains(o.FILEID)).ToList();
                    _deletedFiles = existingItems.Where(o => !_potentialFileIds.Contains(o.FILEID)).ToList();
                    _editedFiles = _potentialFiles.Where(o => existingItemsId.Contains(o.FILEID)).ToList();
                }
            }

            _saveDetails = _editedFamilies.Any() || _deletedFamilies.Any() || _newFamilies.Any()
                        || _editedFiles.Any() || _deletedFiles.Any() || _newFiles.Any();

            /*Custom Code - Here*/

            return record;
        }

        protected override TEMPLOYEEREGISTRATION BeforeDelete(TEMPLOYEEREGISTRATION record, string userName)
        {
            /*Custom Code - Start*/
            /*Validation before delete existing record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/


            /*Custom Code - Here*/
            return record;
        }

        private void ValidateFamily(TEMPLOYEEREGISTRATIONFAMILY family)
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

        protected override TEMPLOYEEREGISTRATION BeforeSave(TEMPLOYEEREGISTRATION record, string userName, bool newRecord)
        {
            /*Custom Code - Start*/
            /*Validation before save existing or new record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/

            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);

            if (newRecord)
            {
                record.CREATED = currentDate;
                record.CREATEDBY = userName;
                record.REGISTRATIONID = record.UNITCODE + "-" + currentDate.ToString("yyyyMM") + "-" + HelperService.GetCurrentDocumentNumber(_serviceName + record.UNITCODE, _context).ToString("0000");
                
            }
            record.UPDATED = currentDate;
            record.UPDATEDBY = userName;


            MEMPLOYEE otherEmployee = null;
            TEMPLOYEEREGISTRATION otherRegist = null;



            if (!string.IsNullOrWhiteSpace(record.NPWP))
            {

                if (record.NPWP.Length != 15 || !StandardUtility.IsNumericString(record.NPWP))
                    throw new Exception("NPWP harus 15 digit dan semuanya numerik");


                //otherRegist = _context.TEMPLOYEEREGISTRATION.FirstOrDefault(d => d.NPWP.Equals(record.NPWP) && !d.REGISTRATIONID.Equals(record.REGISTRATIONID) && d.WFDOCSTATUS != "75" );
                //if (otherRegist != null)
                //    throw new Exception($"NPWP sudah terdaftar sebelumnya, a.n {otherRegist.REGISTRATIONID}-{otherRegist.EMPNAME}");

                otherEmployee = _context.MEMPLOYEE.FirstOrDefault(d => d.NPWP.Equals(record.NPWP) && (d.STATUS == "A" || d.STATUS == "C"));
                if (otherEmployee != null)
                    throw new Exception($"NPWP sudah terdaftar sebelumnya, a.n {otherEmployee.EMPID}-{otherEmployee.EMPNAME}");
            }
            else
                record.NPWP = string.Empty;

            if (string.IsNullOrWhiteSpace(record.KTPID))
                throw new Exception("No KTP harus diisi");

            if (record.KTPID.Length != 16 || !StandardUtility.IsNumericString(record.KTPID))
                throw new Exception("No KTP harus 16 digit dan semuanya numerik");



            //otherRegist = _context.TEMPLOYEEREGISTRATION.FirstOrDefault(d => d.KTPID.Equals(record.KTPID) && !d.REGISTRATIONID.Equals(record.REGISTRATIONID) && d.WFDOCSTATUS != "75");
            //if (otherRegist != null)
            //    throw new Exception($"No KTP sudah terdaftar sebelumnya, a.n {otherRegist.REGISTRATIONID}-{otherRegist.EMPNAME}");


            otherEmployee = _context.MEMPLOYEE.FirstOrDefault(d => d.KTPID.Equals(record.KTPID) && (d.STATUS == "A" || d.STATUS == "C"));
            if (otherEmployee != null)
                throw new Exception($"No KTP sudah terdaftar sebelumnya, a.n {otherEmployee.EMPID}-{otherEmployee.EMPNAME}");


            //Check Back Date untuk record baru atau edit record dan ada perubahan joint date
            if (_existingRecord == null || _existingRecord.JOINTDATE.Date != record.JOINTDATE)
            {
                int backDatedDays = 0;
                string configKey = $"{CFG_BACKDATED_EFFECTIVE_DATE}{record.UNITCODE}";
                var config = _context.MCONFIG.FirstOrDefault(d => d.NAME.Equals(configKey));
                if (config != null && !string.IsNullOrWhiteSpace(config.VALUE))
                    int.TryParse(config.VALUE, out backDatedDays);
                if (backDatedDays < 0)
                    throw new Exception($"Konfigurasi '{configKey}' tidak valid, nilai harus lebih besar atau sama dengan 0");

                DateTime allowedBackDate = DateTime.Today.AddDays(-backDatedDays);

                if (record.JOINTDATE.Date < allowedBackDate)
                    throw new Exception("Tanggal join tidak boleh berlaku mundur");
            }

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



            //Validation against employee master
            MEMPLOYEE employee = new MEMPLOYEE();
            employee.CopyFrom(record);
            Employee employeeService = new Employee(_context, _authenticationService, _auditContext);
            employeeService.Validate(employee, userName);


            /*Custom Code - Here*/
            int spouse = 0, children = 0;

            if (_newFamilies.Any())
            {
                _newFamilies.ForEach(d =>
                {
                    ValidateFamily(d);
                    d.REGISTRATIONID = record.REGISTRATIONID;
                });
                spouse += _newFamilies.Where(d => d.RELATIONSHIP.Equals("Istri") || d.RELATIONSHIP.Equals("Suami")).Count();
                children += _newFamilies.Where(d => d.RELATIONSHIP.Equals("Anak")).Count();
            }
            if (_newFiles.Any())
                _newFiles.ForEach(d => d.REGISTRATIONID = record.REGISTRATIONID);
            if (_editedFamilies.Any())
            {
                _editedFamilies.ForEach(d =>
                {
                    ValidateFamily(d);
                    d.REGISTRATIONID = record.REGISTRATIONID;
                });
                spouse += _editedFamilies.Where(d => d.RELATIONSHIP.Equals("Istri") || d.RELATIONSHIP.Equals("Suami")).Count();
                children += _editedFamilies.Where(d => d.RELATIONSHIP.Equals("Anak")).Count();
            }
            if (_editedFiles.Any())
                _editedFiles.ForEach(d => d.REGISTRATIONID = record.REGISTRATIONID);

            if (_deletedFamilies.Any())
                _deletedFamilies.ForEach(d => d.REGISTRATIONID = record.REGISTRATIONID);
            if (_deletedFiles.Any())
                _deletedFiles.ForEach(d => d.REGISTRATIONID = record.REGISTRATIONID);

            var status = _context.MSTATUS.Find(record.STATUSID);
            if (status == null || !status.ACTIVE)
                throw new Exception("Status perkawinan tidak valid");

            if (spouse < status.SPOUSE || children < status.CHILDREN)
                throw new Exception("Data keluarga tidak sesuai dengan Status perkawinan");
            if (newRecord)
                HelperService.IncreaseRunningNumber(_serviceName + record.UNITCODE, _context);
            return record;
        }

       

        protected override TEMPLOYEEREGISTRATION SaveInsertDetailsToDB(TEMPLOYEEREGISTRATION record, string userName)
        {


            //if (_newFamilies.Any())
            //{
            //    foreach (TEMPLOYEEREGISTRATIONFAMILY family in _newFamilies)
            //    {
            //        family.REGISTRATIONID = record.REGISTRATIONID;
            //    }
            //    _context.TEMPLOYEEREGISTRATIONFAMILY.AddRange(_newFamilies);
            //}

            //if (_newFiles.Any())
            //{
            //    foreach (TEMPLOYEEREGISTRATIONFILE file in _newFiles)
            //    {
            //        file.REGISTRATIONID = record.REGISTRATIONID;
            //    }
            //    _context.TEMPLOYEEREGISTRATIONFILE.AddRange(_newFiles);
            //}

            if (_newFamilies.Any())
                _context.TEMPLOYEEREGISTRATIONFAMILY.AddRange(_newFamilies);


            if (_newFiles.Any())
                _context.TEMPLOYEEREGISTRATIONFILE.AddRange(_newFiles);


            return record;
        }


        
        
        protected override TEMPLOYEEREGISTRATION SaveUpdateDetailsToDB(TEMPLOYEEREGISTRATION record, string userName)
        {


            if (_deletedFamilies.Any())
                _context.TEMPLOYEEREGISTRATIONFAMILY.RemoveRange(_deletedFamilies);
            if (_newFamilies.Any())
                _context.TEMPLOYEEREGISTRATIONFAMILY.AddRange(_newFamilies);
            if (_editedFamilies.Any())
                _context.TEMPLOYEEREGISTRATIONFAMILY.UpdateRange(_editedFamilies);

            if (_deletedFiles.Any())
                _context.TEMPLOYEEREGISTRATIONFILE.RemoveRange(_deletedFiles);
            if (_newFiles.Any())
                _context.TEMPLOYEEREGISTRATIONFILE.AddRange(_newFiles);
            if (_editedFiles.Any())
                _context.TEMPLOYEEREGISTRATIONFILE.UpdateRange(_editedFiles);

            return record;
        }



        protected override TEMPLOYEEREGISTRATION GetSingleFromDB(params object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            return _context.TEMPLOYEEREGISTRATION
                .Include(d => d.TEMPLOYEEREGISTRATIONFAMILY)
                .Include(d => d.TEMPLOYEEREGISTRATIONFILE)
                .SingleOrDefault(d => d.REGISTRATIONID.Equals(Id));
        }



        public override TEMPLOYEEREGISTRATION NewRecord(string userName)
        {
            return new TEMPLOYEEREGISTRATION
            {
                BIRTHDAY = DateTime.Today.AddYears(-17),
                JOINTDATE = DateTime.Today,
                EMPSEX = "L",
                BASICWAGES = 0,
                RACE = string.Empty,
                NATURA = false,
                CREATED = DateTime.Today
            };
        }

        protected override Document WFGenerateDocument(TEMPLOYEEREGISTRATION record, string userName)
        {

            bool isHarvester = false;
            isHarvester = (_context.MPOSITION.FirstOrDefault(d => d.POSITIONID.Equals(record.POSITIONID)).POSFLAG == 6);

            Document document = new Document
            {
                DocDate = DateTime.Today,
                Description = "Employee Registration of " + record.DIVID + " - " + record.EMPNAME,
                DocType = _wfDocumentType,
                UnitID = record.UNITCODE,
                DocOwner = userName,
                DocStatus = "",
                //WFFlag = (isHarvester?"HVT":"OTH"),
                Title = "Employee Registration of " + record.DIVID + " - " + record.EMPNAME
            };
            return document;
        }



        public override TEMPLOYEEREGISTRATION GetSingleByWorkflow(Document document, string userName)
        {
            return _context.TEMPLOYEEREGISTRATION
                .Include(d => d.TEMPLOYEEREGISTRATIONFAMILY)
                .Include(d => d.TEMPLOYEEREGISTRATIONFILE)
                .SingleOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
        }


        protected override TEMPLOYEEREGISTRATION WFBeforeSendApproval(TEMPLOYEEREGISTRATION record, string userName, string actionCode, string approvalNote, bool newRecord)
        {
            Employee _serviceEmployeeMaster = new Employee(_context, _authenticationService, _auditContext);

            if (actionCode.Equals("EMPIN"))
            {
                //No location change
                var newEmployee = _serviceEmployeeMaster.NewFromEmployeeRegistration(record, userName);
                record.EMPID = newEmployee.EMPID;
                _context.TEMPLOYEEREGISTRATION.Update(record);
                _context.SaveChanges();
                _serviceEmployeeMaster.CopyEmployeeMaster(record.EMPID, record.UNITCODE, userName);

                return record;
            }
            //if (actionCode.Equals("FAPV"))
            //{
            //    var newEmployee = _serviceEmployeeMaster.NewFromEmployeeRegistration(record, userName);
            //    record.EMPID = newEmployee.EMPID;
            //    _context.TEMPLOYEEREGISTRATION.Update(record);
            //    _context.SaveChanges();

            //}
            //if (actionCode.Equals("FAPV2"))
            //{
            //    //Final Approve and Activate
            //    var newEmployee = _serviceEmployeeMaster.NewFromEmployeeRegistration(record, userName);
            //    record.EMPID = newEmployee.EMPID;
            //    _context.TEMPLOYEEREGISTRATION.Update(record);
            //    _context.SaveChanges();
            //    _serviceEmployeeMaster.CopyEmployeeMaster(record.EMPID, record.UNITCODE, userName);
            //    return record;
            //}
            return record;
        }






        protected override bool DeleteDetailsFromDB(TEMPLOYEEREGISTRATION record, string userName)
        {

            _context.TEMPLOYEEREGISTRATIONFAMILY.RemoveRange(record.TEMPLOYEEREGISTRATIONFAMILY);
            _context.TEMPLOYEEREGISTRATIONFILE.RemoveRange(record.TEMPLOYEEREGISTRATIONFILE);
            return true;
        }


        public void UpdateEmployeeOnDestination()
        {
            var listRegistrations = (from a in _context.TEMPLOYEEREGISTRATION.AsNoTracking().Where(d => (d.WFDOCSTATUS == "9999"))
                                     join b in _context.MUNITDBSERVER on a.UNITCODE equals b.UNITCODE
                                     join c in _context.MEMPLOYEE.AsNoTracking() on new { a.EMPID, a.UNITCODE } equals new { c.EMPID, c.UNITCODE }
                                     select new { TEMPLOYEEREGISTRATION = a, MUNITDBSERVER = b, UNITCODE = a.UNITCODE }
                                       ).ToList();

            if (StandardUtility.IsEmptyList(listRegistrations))
                return;

            Employee _serviceEmployeeMaster = new Employee(_context, _authenticationService, _auditContext);

            var dbServers = listRegistrations.Select(d => d.MUNITDBSERVER).Distinct().ToList();
            dbServers.ForEach(dbServer =>
            {

                if (StandardUtility.NetWorkPing(dbServer.SERVERNAME))
                {
                    try
                    {
                        using (PMS.EFCore.Model.PMSContextEstate contextEstate = new PMS.EFCore.Model.PMSContextEstate(DBContextOption<PMS.EFCore.Model.PMSContextEstate>.GetOptions(dbServer.SERVERNAME, dbServer.DBNAME, dbServer.DBUSER, dbServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
                        {
                            var listRegistrationsByUnit = listRegistrations.Where(d => d.UNITCODE.Equals(dbServer.UNITCODE)).Select(d => d.TEMPLOYEEREGISTRATION).ToList();
                            var listEmployeeIDs = listRegistrationsByUnit.Select(d => d.EMPID).ToList();

                            _serviceEmployeeMaster.CopyEmployees(listEmployeeIDs, dbServer.UNITCODE, contextEstate);
                            listRegistrationsByUnit.ForEach(d =>
                            {
                                d.WFDOCSTATUS = "EMPIN";
                                d.WFDOCSTATUSTEXT = "Employee Activated";
                            });
                            _context.UpdateRange(listRegistrationsByUnit);
                            _context.SaveChanges();

                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                        throw new Exception(errorMessage);
                    }
                }
                else
                {

                }




            });
        }


        
    }
}
