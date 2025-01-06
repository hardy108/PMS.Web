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
namespace PMS.EFCore.Services.General
{
    public class Vehicle : EntityFactory<MVEHICLE,MVEHICLE,GeneralFilter, PMSContextBase>
    {
        public Vehicle(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Vehicle";
        }

        private void Validate(MVEHICLE record, string userName)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.UNITID)) result += "Unit harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.TYPEID)) result += "Tipe Kendaraan tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(vehicle.Code)) result += "Kode SAP tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(vehicle.No)) result += "No Polisi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.NAME)) result += "Nama tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(vehicle.DriverId)) result += "Driver tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(vehicle.HelperId)) result += "Helper tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(vehicle.Driver)) result += "Pengemudi tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(vehicle.SIM)) result += "SIM tidak boleh kosong." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        private void ValidateInsert(MVEHICLE record, string userName)
        {
            Validate(record, userName);

            string result = string.Empty;
            if (_context.MVEHICLE.SingleOrDefault(p => p.ID.Equals(record.ID)) != null)
                result += "Id sudah ada." + Environment.NewLine;
            int countNo = _context.MVEHICLE.Where(d => d.NO == record.NO).Count();
            if (countNo > 0)
                result += "No Kendaraan sudah ada." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        public override MVEHICLE NewRecord(string userName)
        {
            return new MVEHICLE
            {
                ID = "AUTONUMBER"
            };
        }

        protected override MVEHICLE BeforeSave(MVEHICLE record, string userName, bool newRecord)
        {
            // start : column is not active
            record.DRIVERID = null;
            record.HELPERID = null;
            record.ROT = 0;
            // end

            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                ValidateInsert(record, userName);
                int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.VehicleIdPrefix + record.UNITID, _context);
                string NewId = PMSConstants.VehicleIdPrefix + record.UNITID + lastNumber.ToString().PadLeft(4, '0');
                record.ID = NewId;
            }
            else
                Validate(record, userName);
            record.UPDATED = now;
            return record;
        }

       

       

        protected override MVEHICLE AfterSave(MVEHICLE record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.VehicleIdPrefix + record.UNITID, _context);
            return record;
        }
        

        public override MVEHICLE CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MVEHICLE record = base.CopyFromWebFormData(formData, newRecord);

            if (string.IsNullOrWhiteSpace(formData["CODE"]))
                record.CODE = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["NAME"]))
                record.NAME = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["DRIVER"]))
                record.DRIVER = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["SIM"]))
                record.SIM = string.Empty;

            return record;
        }

        protected override MVEHICLE GetSingleFromDB(params object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.MVEHICLE.SingleOrDefault(d => d.ID.Equals(id));
        }

        public override IEnumerable<MVEHICLE> GetList(GeneralFilter filter)
        {            
            var criteria = PredicateBuilder.True<MVEHICLE>();
            try
            {
                //Added By Hardi 2020-04-06 - Start
                if (filter.Ids.Any())
                    criteria = criteria.And(
                        p => filter.Ids.Contains(p.NO) || 
                        filter.Ids.Contains(p.CODE)
                        );
                //Added By Hardi 2020-04-06 - End

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(p =>
                        p.TYPEID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.CODE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.UNITID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.NO.ToLower().Contains(filter.LowerCasedSearchTerm));

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(d => d.ID == filter.Id);
                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.ID));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(d => d.UNITID == filter.UnitID);
                if (filter.UnitIDs.Any())
                    criteria = criteria.And(d => filter.UnitIDs.Contains(d.UNIT.ID));

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(d => d.ACTIVE == filter.IsActive.Value);

                if (filter.PageSize <= 0)
                    return _context.MVEHICLE.Where(criteria);
                return _context.MVEHICLE.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }

        protected override bool DeleteFromDB(MVEHICLE record, string userName)
        {
            record = GetSingle(record.ID);
            record.ACTIVE = false;
            record.UPDATED = GetServerTime();
            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }
    }
}