using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FP.EFCore.Model;
using FP.EFCore.Services;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using System.Globalization;
using PMS.EFCore.Services.Organization;
using AM.EFCore.Services;

namespace FP.EFCore.Services
{
    public class EmployeeFingerPrint: EntityFactory<MEMPLOYEEFP,MEMPLOYEEFP,FilterEmployee, FPContext>
    {
        PMSContextBase _pmsContext = null;
        Employee _employeeService = null;
        AuthenticationServiceBase _authenticationService;
        public EmployeeFingerPrint(FPContext context,PMSContextBase pmsContext,AuthenticationServiceBase authenticationService,AuditContext auditContext) : base(context,auditContext)
        {
            _serviceName = "Employee Finger Print";
            _pmsContext = pmsContext;
            _authenticationService = authenticationService;
            _employeeService = new Employee(_pmsContext, _authenticationService, auditContext);
        }

        public MEMPLOYEEFP SaveUpdate(IFormCollection formDataCollection, byte[] photo, byte[] fp1, byte[] fp2, byte[] fp3,string userName)
        {
            MEMPLOYEEFP record = CopyFromWebFormData(formDataCollection, false);
            if (photo != null)
                record.FOTO = photo;
            if (fp1 != null)
                record.FP1 = fp1;
            if (fp2 != null)
                record.FP2 = fp2;
            if (fp3 != null)
                record.FP3 = fp3;
            return SaveUpdate(record, userName);
        }


        public MEMPLOYEEFP SaveInsert(IFormCollection formDataCollection, byte[] photo, byte[] fp1, byte[] fp2, byte[] fp3, string userName)
        {
            MEMPLOYEEFP record = CopyFromWebFormData(formDataCollection, false);
            if (photo != null)
                record.FOTO = photo;
            if (fp1 != null)
                record.FP1 = fp1;
            if (fp2 != null)
                record.FP2 = fp2;
            if (fp3 != null)
                record.FP3 = fp3;
            return SaveInsert(record, userName);
        }

        

        public override IEnumerable<MEMPLOYEEFP> GetList(FilterEmployee filter)
        {
            
            
            return 
                (
                    from a in _context.MEMPLOYEEFP
                    join b in _employeeService.GetList(filter) on a.EMPID equals b.EMPID
                    select a
                ).ToList();
        }

        protected override MEMPLOYEEFP BeforeSave(MEMPLOYEEFP record, string userName, bool newRecord)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.EMPCODE)) result += "Id tidak boleh kosong.\r\n";
            if (string.IsNullOrEmpty(record.UNITCODE)) result += "Unit tidak boleh kosong.\r\n";
            if (string.IsNullOrEmpty(record.STATUS)) result += "Status tidak boleh kosong.\r\n";
            record.EMPID = $"{record.UNITCODE}-{record.EMPCODE}";

            MEMPLOYEE existingEmployee = _employeeService.GetSingle(userName,record.EMPID);
            if (existingEmployee == null)
                result += "Data karyawan tidak ditemukan.\r\n";

            DateTime now = GetServerTime();
            record.UPDATEDBY = userName;
            record.UPDATED = now;
            

            if (newRecord)
            {
                record.CREATEDBY = userName;
                record.CREATED = now;
            }
            return record;
        }

       
        protected override MEMPLOYEEFP BeforeDelete(MEMPLOYEEFP record, string userName)
        {
            record.STATUS = "D";
            return BeforeSave(record, userName,false);
        }

        protected override bool DeleteFromDB(MEMPLOYEEFP record, string userName)
        {
            MEMPLOYEEFP deletedRecord= SaveUpdateToDB(record, userName);
            return true;
        }

        public VEMPLOYEE GetDetails(string Id)
        {
            return _employeeService.GetDetails(Id);
        }

        public IEnumerable<VEMPLOYEE> GetListWithDetails(FilterEmployee filter)
        {


            return _employeeService.GetList(filter);
        }
    }
}
