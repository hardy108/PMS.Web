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
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Attendances
{
    public class SuratPeringatan : EntityFactory<TSURATPERINGATAN,TSURATPERINGATAN,FilterSuratPeringatan, PMSContextBase>
    {
        public SuratPeringatan(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "SuratPeringatan";
        }


        protected override TSURATPERINGATAN GetSingleFromDB(params  object[] keyValues)
        {
            return _context.TSURATPERINGATAN
            .Include(d => d.EMP)
            .SingleOrDefault(d => d.NOSP.Equals(keyValues[0]));
        }

        public override IEnumerable<TSURATPERINGATAN> GetList(FilterSuratPeringatan filter)
        {
            
            var result = _context.TSURATPERINGATAN.Where
                (
                    d =>
                    (d.UNITCODE.Equals(filter.UnitID) || string.IsNullOrWhiteSpace(filter.UnitID)) &&
                    (d.TYPESP.Equals(filter.Type) || string.IsNullOrWhiteSpace(filter.Type)) &&
                    d.TGLSP.Equals(filter.StartDate) &&
                    (d.STATUS.Equals(filter.RecordStatus) || string.IsNullOrWhiteSpace(filter.RecordStatus))
                );

            return result.ToList();
            //try
            //{
            //    if (filter.IsActive.HasValue)
            //        return _context.TSURATPERINGATAN.Where(d => d.STATUS == "A" &&
            //        (d.STATUS.Equals(filter.RecordStatus) || string.IsNullOrWhiteSpace(filter.RecordStatus)).ToList();
            //    return _context.TSURATPERINGATAN.ToList();
            //}
            //catch { return null; }
        }

        private void Validate(TSURATPERINGATAN SP, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(SP.NOSP)) result += "No SP harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(SP.EMPID)) result += "Pilih salah satu karyawan." + Environment.NewLine;
            if (string.IsNullOrEmpty(SP.TYPESP)) result += "Type SP harus dipilih." + Environment.NewLine;
            if (string.IsNullOrEmpty(SP.TEMPATPANGGILAN)) result += "Lokasi Panggilan harus diisiLokasi Panggilan harus diisi." + Environment.NewLine;  

            if (result != string.Empty) throw new Exception(result);
        }

        protected override TSURATPERINGATAN BeforeSave(TSURATPERINGATAN record, string userName, bool newRecord)
        {
            DateTime now = GetServerTime();
            record.UPDATED = now;
            record.UPDATEDBY = userName;
            if (newRecord)
            {
                if (_context.TSURATPERINGATAN.SingleOrDefault(d => d.NOSP.Equals(record.NOSP)) != null)
                    throw new Exception("Surat Peringatan sudah pernah dibuat");

                record.CREATED = now;                
                record.CREATEDBY = userName;                
                record.STATUS = "A";
            }
            this.Validate(record, userName);
            return record;

            
        }

        
    }

}