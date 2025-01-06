using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;

using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using System.Linq;

using PMS.EFCore.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WF.EFCore.Models;

using WF.EFCore.Data;

using PMS.EFCore.Helper;
using AM.EFCore.Services;
using PMS.EFCore.Services.Approval;
using WF.EFCore.Services;
using PMS.Shared.Services;
using System.Threading.Tasks;
using PMS.EFCore.Model;
using Remotion.Linq.Parsing.ExpressionVisitors.MemberBindings;
using PMS.Shared.Exceptions;

namespace PMS.EFCore.Services.Organization
{
    public class EmployeeChange:EntityFactoryWithWorkflow< TEMPLOYEECHANGE,TEMPLOYEECHANGE,GeneralFilter, PMSContextBase,WFContext>
    {

        AuthenticationServiceHO _authenticationService;
        Employee _employeeService;
        private const string CFG_BACKDATED_EFFECTIVE_DATE = "EMPCHANGEBACKDATEDDAYS";

        public EmployeeChange(PMSContextBase context, WFContext wfContext, AuthenticationServiceHO authenticationService, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext, authenticationService,authenticationService, taskQueue,emailSender,auditContext)
        {
            _serviceName = "EMPCHANGE";
            _wfDocumentType = "EMPCHANGE";
            _authenticationService = authenticationService;
            _employeeService = new Employee(context, authenticationService, auditContext);
        }

        

        public override IEnumerable<TEMPLOYEECHANGE> GetList(GeneralFilter filter)
        {
            
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                throw new Exception("Tanggal harus diisi");
            if (string.IsNullOrWhiteSpace(filter.UnitID))
                throw new Exception("Estate harus dipilih");

            var criteria = PredicateBuilder.True<TEMPLOYEECHANGE>();
            criteria = criteria.And(d => d.CREATED.Date >= filter.StartDate.Date && d.CREATED.Date <= filter.EndDate.Date && d.UNITCODE.Equals(filter.UnitID));
            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(d => d.DIVID.Equals(filter.DivisionID));
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                criteria = criteria.And(d => d.EMPID.ToLower().Contains(filter.LowerCasedSearchTerm));
            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);
            return _context.TEMPLOYEECHANGE.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        

        

        protected override TEMPLOYEECHANGE BeforeSave(TEMPLOYEECHANGE record, string userName, bool newRecord)
        {

            

            //Check Back Date untuk record baru atau edit record dan ada perubahan joint date
            int backDatedDays = 0;
            string configKey = $"{CFG_BACKDATED_EFFECTIVE_DATE}{record.UNITCODE}";
            var config = _context.MCONFIG.FirstOrDefault(d => d.NAME.Equals(configKey));
            if (config != null && !string.IsNullOrWhiteSpace(config.VALUE))
                int.TryParse(config.VALUE, out backDatedDays);
            if (backDatedDays < 0)
                throw new Exception($"Konfigurasi '{configKey}' tidak valid, nilai harus lebih besar atau sama dengan 0");

            DateTime allowedBackDate = DateTime.Today.AddDays(-backDatedDays);

            if (record.EFFECTIVEDATE.Date < allowedBackDate)
                throw new Exception("Tanggal efektif tidak boleh berlaku mundur");



            
                

            var employee = _context.MEMPLOYEE.FirstOrDefault(d => d.EMPID.Equals(record.EMPID));
            if (employee == null)
                throw new Exception("Data karyawan tidak ditemukan");

            if (employee.STATUS == "D")
                throw new Exception("Karyawan sudah terhapus");
            
            if (record.EMPTYPE.StartsWith("SKU") && !string.IsNullOrWhiteSpace(record.NEWEMPTYPE) && record.NEWEMPTYPE.StartsWith("BHL"))
                throw new Exception("Karyawan SKU tidak boleh diubah menjadi BHL");

            record.UNITCODE = employee.UNITCODE;
            if (string.IsNullOrWhiteSpace(record.NEWUNITCODE))
            {
                record.NEWUNITCODE = record.UNITCODE;
            }

            record.DIVID = employee.DIVID;
            if (string.IsNullOrWhiteSpace(record.NEWDIVID))
                record.NEWDIVID = record.DIVID;

            record.EMPTYPE = employee.EMPTYPE;
            if (string.IsNullOrWhiteSpace(record.NEWEMPTYPE))
                record.NEWEMPTYPE = record.EMPTYPE;

            record.STATUSID = employee.STATUSID;
            if (string.IsNullOrWhiteSpace(record.NEWSTATUSID))
                record.NEWSTATUSID = record.STATUSID;

            record.POSITIONID = employee.POSITIONID;
            if (string.IsNullOrWhiteSpace(record.NEWPOSITIONID))
                record.NEWPOSITIONID = record.POSITIONID;

            record.RECORDSTATUS = employee.STATUS;
            if (string.IsNullOrWhiteSpace(record.NEWRECORDSTATUS))
                record.NEWRECORDSTATUS = record.RECORDSTATUS;

            record.BASICWAGES = employee.BASICWAGES;
            if (!record.NEWBASICWAGES.HasValue || record.NEWBASICWAGES.Value <= 0)
                record.NEWBASICWAGES = record.BASICWAGES;
            
            

            List<string> allowedRecordStatus = new List<string> { "A", "C", "D" };
            if (!allowedRecordStatus.Contains(record.NEWRECORDSTATUS))
                throw new Exception("Record status tidak valid");

            

            if (record.NEWRECORDSTATUS != "D")
            {   
                //employee.UNITCODE = record.NEWUNITCODE;
                //employee.DIVID = record.NEWDIVID;
                employee.EMPTYPE = record.NEWEMPTYPE;
                employee.STATUSID = record.NEWSTATUSID;
                employee.POSITIONID = record.NEWPOSITIONID;
                employee.STATUS = record.NEWRECORDSTATUS;
                employee.BASICWAGES = record.NEWBASICWAGES.Value;

                
                if (record.NEWDIVID != record.DIVID) // Mutasi
                    employee.SUPERVISORID = string.Empty; // Hapus atasan lama ketika mutasi
                _employeeService.ValidateRecord(employee, false, userName);

                //ValidateEmployeeRecord(record, userName);
            }

            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATED = currentDate;
                record.CREATEDBY = userName;
                record.ID = record.UNITCODE + "-" + currentDate.ToString("yyyyMM") + "-" + HelperService.GetCurrentDocumentNumber(_serviceName + record.UNITCODE, _context).ToString("0000");
                HelperService.IncreaseRunningNumber(_serviceName + record.UNITCODE, _context);
            }
            record.UPDATED = currentDate;
            record.UPDATEDBY = userName;

            return record;

        }


       

       

        //private void ValidateEmployeeRecord(TEMPLOYEECHANGE record, string userName)
        //{
        //    var employeee = (from a in _context.MEMPLOYEE
        //                     where a.EMPID.Equals(record.EMPID) && (a.STATUS.Equals("A") || a.STATUS.Equals("C")) 
        //                     select a).FirstOrDefault();

        //    if (employeee == null)
        //        throw new Exception("Data karyawan tidak ditemukan");

        //    employeee.UNITCODE = record.NEWUNITCODE;
        //    employeee.DIVID = record.NEWDIVID;
        //    employeee.EMPTYPE = record.NEWEMPTYPE;
        //    employeee.STATUSID = record.NEWSTATUSID;
        //    employeee.POSITIONID = record.NEWPOSITIONID;
        //    employeee.STATUS = record.NEWRECORDSTATUS;
        //    employeee.BASICWAGES = record.NEWBASICWAGES.Value;
        //    _employeeService.ValidateRecord(employeee,false, userName);

        //}
        

        public override TEMPLOYEECHANGE NewRecord(string userName)
        {
            TEMPLOYEECHANGE record = new TEMPLOYEECHANGE
            {
                CREATED = GetServerTime().Date,
                WFDOCSTATUS = string.Empty,
                WFDOCSTATUSTEXT = "CREATED",
                NEWRECORDSTATUS = "A",
                EFFECTIVEDATE  = DateTime.Today
            };
            return record;
        }


        protected override Document WFGenerateDocument(TEMPLOYEECHANGE record, string userName)
        {
            
            string wfFlag = string.Empty;

            if (!string.IsNullOrWhiteSpace(record.NEWUNITCODE) && !record.UNITCODE.Equals(record.NEWUNITCODE))
                wfFlag = "RELOCATION";
            else
                wfFlag = "UPDATE";
            var employee = _context.MEMPLOYEE.Find(record.EMPID);
            Document document = new Document
            {
                DocDate = DateTime.Today,
                Description = "Employee Change of " + record.EMPID + " - " + _context.MEMPLOYEE.Find(record.EMPID).EMPNAME,
                DocType = _wfDocumentType,
                UnitID = record.UNITCODE,
                DocOwner = userName,
                DocStatus = "",
                WFFlag = wfFlag,
                Title = "Employee Change of " + record.EMPID
            };
            return document;
        }


        



        public override TEMPLOYEECHANGE GetSingleByWorkflow(Document document, string userName)
        {

            
            return _context.TEMPLOYEECHANGE                
                .Include(d => d.POSITION)
                .Include(d => d.STATUS)
                .Include(d => d.NEWSTATUS)
                .SingleOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);


            
        }

        protected override TEMPLOYEECHANGE GetSingleFromDB(params object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            return _context.TEMPLOYEECHANGE
                
                .Include(d=>d.POSITION)
                .Include(d=>d.STATUS)
                .Include(d=>d.NEWSTATUS)
                .SingleOrDefault(d => d.ID.Equals(Id));
            
        }


        
        


        
        protected override TEMPLOYEECHANGE WFBeforeSendApproval(TEMPLOYEECHANGE record, string userName, string actionCode, string approvalNote,bool newRecord)
        {
            Employee _serviceEmployeeMaster = new Employee(_context, _authenticationService, _auditContext);

            //if (actionCode.Equals("FAPV"))
            //{
            //    //UpdateFromEmployeeChange
            //    _serviceEmployeeMaster.UpdateFromEmployeeChange(record, userName);                
            //    return record;
            //}

            //if (actionCode.Equals("FAPV2"))
            //{
            //    //UpdateFromEmployeeChange and Update on estate
            //    _serviceEmployeeMaster.UpdateFromEmployeeChange(record, userName);
            //    if (!string.IsNullOrWhiteSpace(record.UNITCODE) && !record.NEWUNITCODE.Equals(record.UNITCODE))
            //    {
            //        //Mutasi
            //        //1. Deactivate Master Data in Estate
            //        if (string.IsNullOrWhiteSpace(record.NEWUNITCODE))
            //            throw new Exception("Invalid destination unit");

            //        MUNITDBSERVER unitDBServer = _context.MUNITDBSERVER.Find(record.UNITCODE);
            //        if (unitDBServer == null)
            //            throw new Exception("Invalid origin DB Server");

            //        using (PMS.EFCore.Model.PMSContextBase contexSource = new PMS.EFCore.Model.PMSContextBase(DBContextOption<PMS.EFCore.Model.PMSContextBase>.GetOptions(unitDBServer.SERVERNAME, unitDBServer.DBNAME, unitDBServer.DBUSER, unitDBServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
            //        {
            //            Employee employeeMaster = new Employee(contexSource, _authenticationService, _auditContext);
            //            //Deactivate Employee
            //            employeeMaster.Delete(userName, record.EMPID);
            //        }

            //        //Activate Employee
            //        _serviceEmployeeMaster.CopyEmployeeMaster(record.EMPID, record.NEWUNITCODE, userName);

            //    }
            //    else
            //    {
            //        _serviceEmployeeMaster.CopyEmployeeMaster(record.EMPID, userName);
            //    }

            //    return record;
            //}

            if (actionCode.Equals("EMPUPD"))
            {
                //No location change
                //1. Update Master Data in Estate
                //2. Add Employement History in Estate
                //3. Update Master Data HO base on Estate Data
                _serviceEmployeeMaster.UpdateFromEmployeeChange(record, userName);
                _serviceEmployeeMaster.CopyEmployeeMaster(record.EMPID, userName);
                //UpdateEmployeeChangeToEstate(record, userName, _serviceEmployeeMaster);
            }


            if (actionCode.Equals("EMPOUT"))
            {

                //1. Deactivate Master Data in Estate
                if (string.IsNullOrWhiteSpace(record.NEWUNITCODE))
                    throw new Exception("Invalid destination unit");

                MUNITDBSERVER unitDBServer = _context.MUNITDBSERVER.Find(record.UNITCODE);
                if (unitDBServer == null)
                    throw new Exception("Invalid origin DB Server");

                using (PMS.EFCore.Model.PMSContextBase contexSource = new PMS.EFCore.Model.PMSContextBase(DBContextOption<PMS.EFCore.Model.PMSContextBase>.GetOptions(unitDBServer.SERVERNAME, unitDBServer.DBNAME, unitDBServer.DBUSER, unitDBServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
                {
                    //var employeeServiceSource = new Employee(contexSource, _authenticationService, _auditContext);
                    var employeeSource = contexSource.MEMPLOYEE.FirstOrDefault(d=>d.EMPID.Equals(record.EMPID));


                    if (employeeSource != null)
                    {
                        employeeSource.STATUS = "D";
                        employeeSource.RESIGNEDDATE = record.EFFECTIVEDATE;
                        employeeSource.UPDATED = DateTime.Today;
                        employeeSource.UPDATEDBY = userName;
                        contexSource.MEMPLOYEE.Update(employeeSource);
                        contexSource.SaveChanges();
                        //employeeServiceSource.SaveUpdate(employeeSource, userName);
                    }
                    //Deactivate Employee
                    //employeeMaster.Delete(userName, record.EMPID);
                }

                _serviceEmployeeMaster.UpdateFromEmployeeChange(record, userName);
                //Activate Employee
                _serviceEmployeeMaster.CopyEmployeeMaster(record.EMPID, record.NEWUNITCODE, userName);
               

                return record;
                
            }
            if (actionCode.Equals("EMPIN"))
            {
                _serviceEmployeeMaster.UpdateFromEmployeeChange(record, userName);
                _serviceEmployeeMaster.CopyEmployeeMaster(record.EMPID, record.NEWUNITCODE, userName);
                return record;
            }
            return record;
        }


        public void DeactivateEmployeeOnSource()
        {
            var listEmployeeMutation = (from a in _context.TEMPLOYEECHANGE.AsNoTracking().Where(d => d.WFDOCSTATUS == "9999" && d.NEWUNITCODE != d.UNITCODE)
                                        join b in _context.MUNITDBSERVER on a.UNITCODE equals b.UNITCODE
                                        select new { TEMPLOYEECHANGE = a, MUNITDBSERVER = b, UNITCODE = a.UNITCODE }
                                       ).ToList();

            if (StandardUtility.IsEmptyList(listEmployeeMutation))
                return;
            var dbServers = listEmployeeMutation.Select(d => d.MUNITDBSERVER).Distinct().ToList();
            dbServers.ForEach(dbServer => {

                if (StandardUtility.NetWorkPing(dbServer.SERVERNAME))
                {   
                    try
                    {
                        using (PMS.EFCore.Model.PMSContextEstate contextEstate = new PMS.EFCore.Model.PMSContextEstate(DBContextOption<PMS.EFCore.Model.PMSContextEstate>.GetOptions(dbServer.SERVERNAME, dbServer.DBNAME, dbServer.DBUSER, dbServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
                        {
                            
                            var listEmployeeMutationByUnit = listEmployeeMutation.Where(d => d.UNITCODE.Equals(dbServer.UNITCODE)).Select(d=>d.TEMPLOYEECHANGE).ToList();
                            var listEmployeeIDs = listEmployeeMutationByUnit.Select(d => d.EMPID).ToList();
                            var employees = contextEstate.MEMPLOYEE.AsNoTracking().Where(d => listEmployeeIDs.Contains(d.EMPID)).ToList();
                            
                            
                            if (!StandardUtility.IsEmptyList(employees))
                            {
                                (
                                    from a in employees
                                    join b in listEmployeeMutationByUnit on a.EMPID equals b.EMPID
                                    select new { a, b }
                                ).ToList().ForEach(d => {
                                    d.a.STATUS = "D";
                                    d.a.RESIGNEDDATE = d.b.EFFECTIVEDATE;
                                    d.a.UPDATED = DateTime.Today;
                                    d.a.UPDATEDBY = d.b.ID;
                                    d.b.WFDOCSTATUS = "EMPOUT";
                                    d.b.WFDOCSTATUSTEXT = "Employee Deactivated";
                                });
                                contextEstate.UpdateRange(employees);
                                contextEstate.SaveChanges();

                                _context.UpdateRange(listEmployeeMutationByUnit);
                                _context.SaveChanges();
                            }
                                
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                    
                    }
                }
                else
                {
                    
                }


                
                
            });
        }

        public void UpdateEmployeeOnDestination()
        {
            var listEmployeeMutation = (from a in _context.TEMPLOYEECHANGE.AsNoTracking().Where(d => (d.WFDOCSTATUS == "EMPOUT" && d.NEWUNITCODE != d.UNITCODE) || (d.WFDOCSTATUS == "9999" && d.NEWUNITCODE == d.UNITCODE))
                                        join b in _context.MUNITDBSERVER on a.NEWUNITCODE equals b.UNITCODE
                                        select new { TEMPLOYEECHANGE = a, MUNITDBSERVER = b, UNITCODE = a.NEWUNITCODE }
                                       ).ToList();

            if (StandardUtility.IsEmptyList(listEmployeeMutation))
                return;

            Employee _serviceEmployeeMaster = new Employee(_context, _authenticationService, _auditContext);

            var dbServers = listEmployeeMutation.Select(d => d.MUNITDBSERVER).Distinct().ToList();
            dbServers.ForEach(dbServer => {

                if (StandardUtility.NetWorkPing(dbServer.SERVERNAME))
                {
                    try
                    {
                        using (PMS.EFCore.Model.PMSContextEstate contextEstate = new PMS.EFCore.Model.PMSContextEstate(DBContextOption<PMS.EFCore.Model.PMSContextEstate>.GetOptions(dbServer.SERVERNAME, dbServer.DBNAME, dbServer.DBUSER, dbServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey)))
                        {

                            var listEmployeeMutationByUnit = listEmployeeMutation.Where(d => d.UNITCODE.Equals(dbServer.UNITCODE)).Select(d => d.TEMPLOYEECHANGE).ToList();
                            var listEmployeeIDs = listEmployeeMutationByUnit.Select(d => d.EMPID).ToList();

                            _serviceEmployeeMaster.CopyEmployees(listEmployeeIDs, dbServer.UNITCODE, contextEstate);

                            listEmployeeMutationByUnit.ForEach(d => {
                                if (d.NEWUNITCODE == d.UNITCODE) {
                                    d.WFDOCSTATUS = "EMPUPD";
                                    d.WFDOCSTATUSTEXT = "Employee Updated";
                                }
                                else
                                {
                                    d.WFDOCSTATUS = "EMPIN";
                                    d.WFDOCSTATUSTEXT = "Employee Activated";
                                }

                            });
                            _context.UpdateRange(listEmployeeMutationByUnit);
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
