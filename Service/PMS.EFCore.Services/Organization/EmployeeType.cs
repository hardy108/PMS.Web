using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using System.Linq;

using PMS.EFCore.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PMS.Shared.Utilities;
using PMS.EFCore.Helper;


using FileStorage.EFCore;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Organization
{
    public class EmployeeType:EntityFactory<MEMPLOYEETYPE,MEMPLOYEETYPE,GeneralFilter, PMSContextBase>
    {

        private AuthenticationServiceBase _authenticationService;
        public EmployeeType(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Employee Type";
            _authenticationService = authenticationService;
        }

        
        public override IEnumerable<MEMPLOYEETYPE> GetList(GeneralFilter filter)
        {
            try
            {
                if (filter.PageSize <= 0)
                    return _context.MEMPLOYEETYPE;
                return _context.MEMPLOYEETYPE.GetPaged(filter.PageNo,filter.PageSize).Results;

            }
            catch { return new List<MEMPLOYEETYPE>(); }
        }

        public override MEMPLOYEETYPE NewRecord(string userName)
        {
            return new MEMPLOYEETYPE
            {
                UPDATED = DateTime.Today,
            };
        }

        protected override MEMPLOYEETYPE BeforeSave(MEMPLOYEETYPE record, string userName, bool newRecord)
        {
            
            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);            
            record.UPDATED = currentDate;
            Validate(record, userName);
            return record;
        }

        private void Validate(MEMPLOYEETYPE record, string userName)
        {

            string result = string.Empty;
            if (string.IsNullOrEmpty(record.ID)) result += "ID tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.CODE)) result += "KODE tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.HVTCODE)) result += "HVTCODE tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.NAME)) result += "NAMA tidak boleh kosong." + Environment.NewLine;
            if (record.UPDATED == new DateTime()) result += "TANGGAL tidak boleh kosong." + Environment.NewLine;
           

            if (_context.MEMPLOYEETYPE.SingleOrDefault(d => d.CODE.Equals(record.CODE)) != null) result += "Tipe sudah terdaftar." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);

        }


        protected override MEMPLOYEETYPE AfterSave(MEMPLOYEETYPE record, string userName, bool newRecord)
        {
            
            HelperService.DHSUpdateMaster(userName, record.UPDATED, _serviceName, _context);

            return record;
        }


        public List<MEMPLOYEETYPE> GetHarvestingType()
        {
            var data =_context.MEMPLOYEETYPE.Select(d => d.HVTCODE).Distinct().ToList();

            List<MEMPLOYEETYPE> memptype = new List<MEMPLOYEETYPE>();
            foreach (var type in data)
            {
                memptype.Add(new MEMPLOYEETYPE
                {
                    ID = type,
                    CODE = type,
                    HVTCODE = type,
                    NAME = type
                });                
            }

            return memptype;
        }

    }
}
