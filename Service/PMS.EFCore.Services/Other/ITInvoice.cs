using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;

using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using WF.EFCore.Models;

using WF.EFCore.Data;
using FileStorage.EFCore;

using AM.EFCore.Services;
using PMS.EFCore.Services.Approval;
using PMS.Shared.Models;
using PMS.Shared.Services;

using PMS.EFCore.Model;
using AM.EFCore.Data;

namespace PMS.EFCore.Services.Organization
{
    public class ITInvoice : EntityFactoryWithWorkflow<TITINVOICE,TITINVOICE,GeneralFilter, PMSContextBase,WFContext>
    {

        
        private AuthenticationServiceHO _authenticationService;


        
        public ITInvoice(PMSContextHO context, WFContext wfContext, AuthenticationServiceHO authenticationService, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext,authenticationService,authenticationService, taskQueue,emailSender,auditContext)
        {
            _serviceName = "ITInvoice";
            _wfDocumentType = "ITINV";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<TITINVOICE> GetList(GeneralFilter filter)
        {

            DateTime startDate = filter.StartDate, endDate = filter.EndDate;
            StandardUtility.NullDateRange(ref startDate, ref endDate);

            var criteria = PredicateBuilder.True<TITINVOICE>();
            criteria = criteria.And(d => 
                (d.REGISTERDATE.Date >= startDate && d.REGISTERDATE.Date <= endDate) || 
                (d.INVOICEDATE.Date >= startDate && d.INVOICEDATE.Date <= endDate) ||
                (d.DUEDATE.Date >= startDate && d.DUEDATE.Date <= endDate)
                );

            //var unitIds = filter.UnitIDs;
            //if (unitIds.Count > 0)
            //    criteria = criteria.And(d => unitIds.Contains(d.UNITID));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                criteria = criteria.And(d => d.INVOICENO.ToLower().Contains(filter.LowerCasedSearchTerm) || d.VENDORID.ToLower().Contains(filter.LowerCasedSearchTerm) || d.VENDORNAME.ToLower().Contains(filter.LowerCasedSearchTerm));

            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);

            if (filter.PageSize <= 0)
                return _context.TITINVOICE.Where(criteria);
            return _context.TITINVOICE.Where(criteria).GetPaged(filter.PageNo,filter.PageSize).Results;
            
        }

        

        

        protected override TITINVOICE BeforeSave(TITINVOICE record, string userName, bool newRecord)
        {
            /*Custom Code - Start*/
            /*Validation before save existing or new record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/
            if (string.IsNullOrWhiteSpace(record.UNITID))
                throw new Exception("Unit can not be empty");

            if (string.IsNullOrWhiteSpace(record.VENDORNAME))
                throw new Exception("Vendor can not be empty");
            if (string.IsNullOrWhiteSpace(record.INVOICENO))
                throw new Exception("Invoice No can not be empty");
            if (record.INVOICEDATE == default)
                throw new Exception("Invoice date can not be empty");
            
            if (record.AMOUNT<=0)
                throw new Exception("Invoice amount can not be empty");
            if (!record.INVOICEFILE.HasValue)
                throw new Exception("Please attach the invoice file");


            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                
                record.CREATED = currentDate;                
                record.CREATEDBY = userName;
                record.REGISTERDATE = record.CREATED.Date;
                if (string.IsNullOrWhiteSpace(record.CURRENCYCODE))
                    record.CURRENCYCODE = "IDR";
                if (string.IsNullOrWhiteSpace(record.VENDORID))
                    record.VENDORID = "IDR";
                record.VENDORID = string.Empty;
                record.REGISTERID = record.UNITID + "-INV-" + currentDate.ToString("yyyyMM") + "-" + HelperService.GetCurrentDocumentNumber(_serviceName + record.UNITID, _context).ToString("0000");
                if (string.IsNullOrWhiteSpace(record.CURRENCYCODE))
                    record.CURRENCYCODE = "IDR";
            }
            record.UPDATED = currentDate;
            record.UPDATEDBY = userName;
            return record;
        }

       

        

       
        
        protected override TITINVOICE AfterSave(TITINVOICE record, string userName,bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(_serviceName + record.UNITID, _context);
            return record;
        }
        

       



        

        public override TITINVOICE NewRecord(string userName)
        {
            return new TITINVOICE
            {
                CREATED = DateTime.Today,
                REGISTERDATE = DateTime.Today,
                INVOICEDATE = DateTime.Today,
                DUEDATE = DateTime.Today,
                WFDOCSTATUS = "0",
                WFDOCSTATUSTEXT = "Created"
            };
        }

        protected override Document WFGenerateDocument(TITINVOICE record, string userName)
        {
            Document document = new Document
            {
                DocDate = DateTime.Today,
                Description = "Invoice of " + record.UNITID + " - " + record.REGISTERID,
                DocType = _wfDocumentType,
                UnitID = record.UNITID,
                DocOwner = userName,
                DocStatus = "",
                WFFlag = record.INVOICETYPE,
                Title = "Invoice of " + record.UNITID + " - " + record.REGISTERID
            };
            return document;
        }



        public override TITINVOICE GetSingleByWorkflow(Document document, string userName)
        {
            return _context.TITINVOICE                
                .SingleOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
        }
       
    }
}
