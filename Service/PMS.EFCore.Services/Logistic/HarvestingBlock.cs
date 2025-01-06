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
using PMS.EFCore.Services.Payroll;
using PMS.EFCore.Services.Attendances;
using PMS.EFCore.Services.Organization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Logistic
{
    public class HarvestingBlock:EntityFactory<THARVESTBLOCK, THARVESTBLOCK, FilterHarvestBlock, PMSContextBase>
    {
        Block _blockService;
        Divisi _divisiService;
        private AuthenticationServiceBase _authenticationService;
        public HarvestingBlock(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Harvesting Block";
            _authenticationService = authenticationService;
            _blockService = new Block(_context,_authenticationService, auditContext);
            _divisiService = new Divisi(_context,_authenticationService, auditContext);
        }

        protected override THARVESTBLOCK BeforeSave(THARVESTBLOCK record, string userName, bool newRecord)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.HARVESTCODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.EMPLOYEEID)) result += "Karyawan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.BLOCKID)) result += "Block tidak boleh kosong." + Environment.NewLine;
            VBLOCK block = _blockService.GetSingle(record.BLOCKID);
            if (block == null)
                result += "Block tidak valid." + Environment.NewLine;
            if (!block.ACTIVE.HasValue || !block.ACTIVE.Value)
                result += "Block tidak aktif." + Environment.NewLine;
            if (block.CURRENTPLANTED < record.HARVESTAREA) result += "Luas panen tidak boleh melebihi luas blok." + Environment.NewLine;
            if (record.HARVESTAREA < record.EMPAREA) result += "Luas panen karyawan tidak boleh melebihi luas panen blok." + Environment.NewLine;


            if (!string.IsNullOrWhiteSpace(result))
                throw new Exception(result);

            return record;
        }

        public override IEnumerable<THARVESTBLOCK> GetList(FilterHarvestBlock filter)
        {
            var criteriaHeader = PredicateBuilder.True<THARVEST>();

            //Single Unit ID
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
            {
                List<string> divisionIds = _divisiService.GetList(new GeneralFilter { UnitID = filter.UnitID }).Select(d => d.DIVID).ToList();
                criteriaHeader = criteriaHeader.And(d => divisionIds.Contains(d.DIVID));
            }

            //Multi Unit ID
            if (filter.UnitIDs.Any())
            {
                List<string> divisionIds = _divisiService.GetList(new GeneralFilter { UnitIDs = filter.UnitIDs }).Select(d => d.DIVID).ToList();
                criteriaHeader = criteriaHeader.And(d => divisionIds.Contains(d.DIVID));
            }

            //Single Division ID
            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteriaHeader = criteriaHeader.And(d => d.DIVID.Equals(d.DIVID));

            //Multi Division ID
            if (filter.DivisionIDs.Any())
                criteriaHeader = criteriaHeader.And(d => filter.DivisionIDs.Contains(d.DIVID));


            
                criteriaHeader = criteriaHeader.And(d =>
                (d.HARVESTDATE.Date >= filter.StartDate.Date && d.HARVESTDATE.Date <= filter.EndDate.Date));

            if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteriaHeader = criteriaHeader.And(d => !d.STATUS.Equals(PMSConstants.TransactionStatusDeleted));
            else
                criteriaHeader = criteriaHeader.And(d => d.STATUS.Equals(filter.RecordStatus));

            if (filter.HarvestType >= 0)
                criteriaHeader = criteriaHeader.And(d => d.HARVESTTYPE == filter.HarvestType);
            if (filter.HarvestTypes.Any())
                criteriaHeader = criteriaHeader.And(d => filter.HarvestTypes.Contains(d.HARVESTTYPE));

            if (filter.PaymentType >= 0)
                criteriaHeader = criteriaHeader.And(d => d.HARVESTPAYMENTTYPE == filter.PaymentType);

            if (filter.PaymentTypes.Any())
                criteriaHeader = criteriaHeader.And(d => filter.PaymentTypes.Contains(d.HARVESTPAYMENTTYPE.Value));

            var criteria = PredicateBuilder.True<THARVESTBLOCK>();
            

            if (!string.IsNullOrWhiteSpace(filter.BlockID))
                criteria = criteria.And(d => d.BLOCKID.Equals(filter.BlockID));
            if (filter.BlockIDs.Any())
                criteria = criteria.And(d => filter.BlockIDs.Contains(d.BLOCKID));

            if (!string.IsNullOrWhiteSpace(filter.EmployeeID))
                criteria = criteria.And(d => d.EMPLOYEEID.Equals(filter.EmployeeID));
            if (filter.EmployeeIDs.Any())
                criteria = criteria.And(d => filter.EmployeeIDs.Contains(d.EMPLOYEEID));

            var query =
            from a in _context.THARVEST.Where(criteriaHeader)
            join b in _context.THARVESTBLOCK.Where(criteria) on a.HARVESTCODE equals b.HARVESTCODE
            select b;

            if (filter.PageSize <= 0)
                return query;
            return query.GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public List<THARVESTBLOCK> GetByCode(string code)
        {
            return GetList(new FilterHarvestBlock { Id = code }).ToList();
        }
    }
}
