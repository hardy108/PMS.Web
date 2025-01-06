using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using PMS.EFCore.Helper;
using PMS.EFCore.Model;
using PMS.EFCore.Services.Logistic;
using PMS.EFCore.Services.Organization;
using WF.EFCore.Data;
using AM.EFCore.Services;
using AM.EFCore.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PMS.Shared.Utilities;
using PMS.Shared.Exceptions;
using PMS.EFCore.Services.Attendances;

namespace PMS.EFCore.Services.Utilities
{
    public class ScheduledJob
    {
        private PMSContextHO _contextHO;
        private PMSContextEstate _contextEstate;
        private WFContext _wfFContext;
        private Employee _employeeService;
        private AuditContext _auditContext;
        private HarvestingWFLebihBasis _harvestingWFLebihBasisService;
        private HarvestingWFMaxBrondol _harvestingWFMaxBrondolService;
        private HarvestingWFDayValidation _harvestingWFDayValidationService;
        private PaymentWFAdjustHK _paymentWFAdjustHKService;
        private Leave _leaveService;
        private AMContextHO _amContextHO;
        private AuthenticationServiceHO _authenticationServiceHO;
        private EmployeeChange _employeeChangeService;
        public ScheduledJob(IConfiguration configuration)
        {
            var pmsEstateConnectionString = DBContextOption<PMSContextEstate>.GetConnectionString(configuration.GetConnectionString("PMS"), "mps");
            var pmsHOConnectionString = DBContextOption<PMSContextHO>.GetConnectionString(configuration.GetConnectionString("PMSHO"), "mps");
            var auditConnectionString = DBContextOption<AuditContext>.GetConnectionString(configuration.GetConnectionString("LOG"), "mps");
            var wfConnectionString = DBContextOption<AuditContext>.GetConnectionString(configuration.GetConnectionString("WF"), "mps");
            var amConnectionStringHO = DBContextOption<AuditContext>.GetConnectionString(configuration.GetConnectionString("AMHO"), "mps");

            _contextHO = new PMSContextHO(DBContextOption<PMSContextHO>.GetOptions(pmsHOConnectionString));
            _contextEstate = new PMSContextEstate(DBContextOption<PMSContextEstate>.GetOptions(pmsEstateConnectionString));
            _auditContext = new AuditContext(DBContextOption<AuditContext>.GetOptions(auditConnectionString));
            _wfFContext = new WFContext(DBContextOption<WFContext>.GetOptions(wfConnectionString));
            _amContextHO = new AMContextHO(DBContextOption<AMContextHO>.GetOptions(amConnectionStringHO));

            _employeeService = new Employee(_contextHO, null, _auditContext);
            _authenticationServiceHO = new AuthenticationServiceHO(_contextHO, _amContextHO, _auditContext, null, null, null, null);

            _harvestingWFLebihBasisService = new HarvestingWFLebihBasis(_contextHO, _wfFContext, _authenticationServiceHO,_authenticationServiceHO, null, null, _auditContext);
            _harvestingWFMaxBrondolService = new HarvestingWFMaxBrondol(_contextHO, _wfFContext, _authenticationServiceHO, _authenticationServiceHO, null, null, _auditContext);
            _harvestingWFDayValidationService = new HarvestingWFDayValidation(_contextHO, _wfFContext, _authenticationServiceHO, _authenticationServiceHO, null, null, _auditContext);
            _paymentWFAdjustHKService = new PaymentWFAdjustHK(_contextHO, _wfFContext, _authenticationServiceHO, _authenticationServiceHO, null, null, _auditContext);
            _leaveService = new Leave(_contextHO, _wfFContext, _authenticationServiceHO, _authenticationServiceHO, null, null, _auditContext);
            _employeeChangeService = new EmployeeChange(_contextHO, _wfFContext, _authenticationServiceHO, null, null, _auditContext);
        }

        public void NonActiveEmployeeBySP3()
        {

            _auditContext.SaveAuditTrail("ScheduledJob", "NonActiveEmployee", "Start Non Active");
            try
            {
                _employeeService.UpdateEmployeeStatusFromSP3(_contextEstate, _contextHO);
                _auditContext.SaveAuditTrail("ScheduledJob", "NonActiveEmployee", "Finish Non Active");
            }
            catch(Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "NonActiveEmployee", "Error Non Active " + ex.Message);
            }
            
        }

        public void CleanWFTrashData()
        {

            _auditContext.SaveAuditTrail("ScheduledJob", "Clean WF Trash Data", "Start Cleaning WF Trash Data");
            try
            {
                _wfFContext.ExecuteSqlText("exec sp_DeleteWFTrashData");
                _auditContext.SaveAuditTrail("ScheduledJob", "Clean WF Trash Data", "Finish Cleaning WF Trash Data");
            }
            catch(Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "Clean WF Trash Data", "Error Cleaning WF Trash Data " + ex.Message );
            }
            
        }

        public void SubmitToWFByHOCRONS()
        {

            _auditContext.SaveAuditTrail("ScheduledJob", "LebihBasis", "Start SubmitToWFByHO");
            try
            {
                _harvestingWFLebihBasisService.SubmitToWFByHO(_contextHO);
                _auditContext.SaveAuditTrail("ScheduledJob", "LebihBasis", "Finish SubmitToWFByHO");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "LebihBasis", "Error SubmitToWFByHO :" + ex.Message);
            }


            _auditContext.SaveAuditTrail("ScheduledJob", "MaxBrondol", "Start SubmitToWFByHO");
            try
            {
                _harvestingWFMaxBrondolService.SubmitToWFByHO(_contextHO);
                _auditContext.SaveAuditTrail("ScheduledJob", "MaxBrondol", "Finish SubmitToWFByHO");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "MaxBrondol", "Error SubmitToWFByHO :" + ex.Message);
            }


            _auditContext.SaveAuditTrail("ScheduledJob", "Validasi3Hari", "Start SubmitToWFByHO");
            try
            {
                _harvestingWFDayValidationService.SubmitToWFByHO(_contextHO);
                _auditContext.SaveAuditTrail("ScheduledJob", "Validasi3Hari", "Finish SubmitToWFByHO");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "Validasi3Hari", "Error SubmitToWFByHO :" + ex.Message);

            }


            _auditContext.SaveAuditTrail("ScheduledJob", "AdjustmentHK", "Start SubmitToWFByHO");
            try
            {
                _paymentWFAdjustHKService.SubmitToWFByHO(_contextHO);                
                _auditContext.SaveAuditTrail("ScheduledJob", "AdjustmentHK", "Finish SubmitToWFByHO");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "AdjustmentHK", "Error SubmitToWFByHO :" + ex.Message);
            }


            //_auditContext.SaveAuditTrail("ScheduledJob", "Leave", "Start SubmitToWFByHO");
            //try
            //{
            //    _leaveService.SubmitToWFByHO(_contextHO);
            //    _auditContext.SaveAuditTrail("ScheduledJob", "Leave", "Finish SubmitToWFByHO");
            //}
            //catch (Exception ex)
            //{
            //    _auditContext.SaveAuditTrail("ScheduledJob", "Leave", "Error SubmitToWFByHO :" + ex.Message);
            //}

            try
            {
                _wfFContext.ExecuteSqlText("exec sp_DeleteWFTrashData");
            }
            catch (Exception ex)
            {
            }

            

        }


        

        public void SyncByEstateCRONS()
        {

            _auditContext.SaveAuditTrail("ScheduledJob", "LebihBasis", "Start SyncByEstate");
            try
            {
                _harvestingWFLebihBasisService.SyncByEstate(_contextEstate, _contextHO);
                _auditContext.SaveAuditTrail("ScheduledJob", "LebihBasis", "Finish SyncByEstate ");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "LebihBasis", "Error SyncByEstate " + ex.Message);
            }

            _auditContext.SaveAuditTrail("ScheduledJob", "MaxBrondol", "Start SyncByEstate");
            try
            {
                _harvestingWFMaxBrondolService.SyncByEstate(_contextEstate, _contextHO);
                _auditContext.SaveAuditTrail("ScheduledJob", "MaxBrondol", "Finish SyncByEstate");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "MaxBrondol", "Error SyncByEstate " + ex.Message);
            }

            _auditContext.SaveAuditTrail("ScheduledJob", "Validasi3Hari", "Start SyncByEstate");
            try
            {
                _harvestingWFDayValidationService.SyncByEstate(_contextEstate, _contextHO);
                _auditContext.SaveAuditTrail("ScheduledJob", "Validasi3Hari", "Finish SyncByEstate");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "Validasi3Hari", "Error SyncByEstate" + ex.Message);
            }


            _auditContext.SaveAuditTrail("ScheduledJob", "AdjustmentHK", "Start SyncByEstate");
            try
            {
                _paymentWFAdjustHKService.SyncByEstate(_contextEstate, _contextHO);
                _auditContext.SaveAuditTrail("ScheduledJob", "AdjustmentHK", "Finish SyncByEstate");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "AdjustmentHK", "Error SyncByEstate " + ex.Message);
            }

            _auditContext.SaveAuditTrail("ScheduledJob", "AdjustmentHK", "Start Proses Attendance");
            try
            {
                _paymentWFAdjustHKService.ProcessAttendance(_contextEstate);
                _auditContext.SaveAuditTrail("ScheduledJob", "AdjustmentHK", "Finish Proses Attendance");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "AdjustmentHK", "Error Proses Attendance " + ex.Message);
            }



            /*_auditContext.SaveAuditTrail("ScheduledJob", "Leave", "Start SyncByEstate");
            try
            {
                _leaveService.SyncByEstate(_contextEstate, _contextHO);
                _auditContext.SaveAuditTrail("ScheduledJob", "Leave", "Finish SyncByEstate");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "Leave", "Error SyncByEstate " + ex.Message);
            }

            _auditContext.SaveAuditTrail("ScheduledJob", "Leave", "Start Proses Attendance");
            try
            {
                _leaveService.ProcessAttendance(_contextEstate);
                _auditContext.SaveAuditTrail("ScheduledJob", "Leave", "Finish Proses Attendance");
            }
            catch (Exception ex)
            {
                _auditContext.SaveAuditTrail("ScheduledJob", "Leave", "Error Proses Attendance " + ex.Message);
            }*/


            //_auditContext.SaveAuditTrail("ScheduledJob", "EmployeeChange-Mutasi", "Start Deactivate Employee On Source");
            //try
            //{
            //    _employeeChangeService.DeactivateEmployeeOnSource();
            //    _auditContext.SaveAuditTrail("ScheduledJob", "EmployeeChange-Mutasi", "Finish Deactivate Employee On Source");
            //}
            //catch (Exception ex)
            //{
            //    _auditContext.SaveAuditTrail("ScheduledJob", "EmployeeChange-Mutasi", "Error Deactivate Employee On Source " + ex.Message);
            //}

            //_auditContext.SaveAuditTrail("ScheduledJob", "EmployeeChange", "Start Update Employee On Destination");
            //try
            //{
            //    _employeeChangeService.UpdateEmployeeOnDestination();
            //    _auditContext.SaveAuditTrail("ScheduledJob", "EmployeeChange", "Finish Update Employee On Destination");
            //}
            //catch (Exception ex)
            //{
            //    _auditContext.SaveAuditTrail("ScheduledJob", "EmployeeChange", "Error Update Employee On Destination " + ex.Message);
            //}
        }


        public void ToolsCheckWFAllEstate()
        {



            /* This Tools For Checking Invalid/Error Data */

            //Change The following parameters
            List<string> unitCodes = new List<string> { "1026"}; // Leave blank for all estates
            DateTime startDate = new DateTime(2022, 3, 1);
            DateTime endDate = DateTime.Today;

            /*_harvestingWFDayValidationService.ResyncToEstate(_contextHO, unitCodes, startDate, endDate);
            _harvestingWFLebihBasisService.ResyncToEstate(_contextHO, unitCodes, startDate, endDate);
            _harvestingWFMaxBrondolService.ResyncToEstate(_contextHO, unitCodes, startDate, endDate);
           _paymentWFAdjustHKService.ResyncToEstate(_contextHO, unitCodes, startDate, endDate);*/
            _paymentWFAdjustHKService.ReprocessAttendance(_contextHO, unitCodes, startDate, endDate);
            //_paymentWFAdjustHKService.CheckHKLebih1Dari1(_contextHO, unitCodes, startDate, endDate);
            //_paymentWFAdjustHKService.FixUnapprroveDetail(_contextHO, unitCodes);
            //_paymentWFAdjustHKService.FixProcessResultFlag(_contextHO, unitCodes);
            /* This Tools For Checking Invalid/Error Data */

        }



    }
}
