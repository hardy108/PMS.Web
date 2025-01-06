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
    public class ITIOM : EntityFactoryWithWorkflow<TITIOM, TITIOM, GeneralFilter, PMSContextBase,WFContext>
    {




        private AuthenticationServiceHO _authenticationService;

        public ITIOM(PMSContextHO context, WFContext wfContext, AuthenticationServiceHO authenticationService, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext,authenticationService,authenticationService, taskQueue,emailSender,auditContext)
        {
            _serviceName = "IOM";
            _wfDocumentType = "ITIOM";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<TITIOM> GetList(GeneralFilter filter)
        {

            DateTime startDate = filter.StartDate, endDate = filter.EndDate;
            StandardUtility.NullDateRange(ref startDate, ref endDate);

            var criteria = PredicateBuilder.True<TITIOM>();
            criteria = criteria.And(d =>
                (d.REGISTERDATE.Date >= startDate && d.REGISTERDATE.Date <= endDate) 
                );

            //var unitIds = filter.UnitIDs;
            //if (unitIds.Count > 0)
            //    criteria = criteria.And(d => unitIds.Contains(d.UNITID));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                criteria = criteria.And(d => d.REGISTERID.ToLower().Contains(filter.LowerCasedSearchTerm) || d.TITLE.ToLower().Contains(filter.LowerCasedSearchTerm) || d.DESCRIPTION.ToLower().Contains(filter.LowerCasedSearchTerm));

            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);

            if (filter.PageSize <= 0)
                return _context.TITIOM.Where(criteria);
            return _context.TITIOM.Where(criteria).GetPaged(filter.PageNo,filter.PageSize).Results;
            
        }

        

        

        protected override TITIOM BeforeSave(TITIOM record, string userName, bool newRecord)
        {
            /*Custom Code - Start*/
            /*Validation before save existing or new record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/
            if (string.IsNullOrWhiteSpace(record.UNITID))
                throw new Exception("Unit can not be empty");

            if (string.IsNullOrWhiteSpace(record.TITLE))
                throw new Exception("Title can not be empty");
            if (string.IsNullOrWhiteSpace(record.DESCRIPTION))
                throw new Exception("Description can not be empty");

            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                
                record.CREATED = currentDate;
             
                record.CREATEDBY = userName;
                record.REGISTERDATE = record.CREATED.Date;
                record.REGISTERID = record.UNITID + "-IOM-" + currentDate.ToString("yyyyMM") + "-" + HelperService.GetCurrentDocumentNumber(_serviceName + record.UNITID, _context).ToString("0000");
            }
            record.UPDATED = currentDate;
            record.UPDATEDBY = userName;

            return record;
        }

       
       
        
        protected override TITIOM AfterSave(TITIOM record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(_serviceName + record.UNITID, _context);
            return record;
        }
        
       



        

        public override TITIOM NewRecord(string userName)
        {
            return new TITIOM
            {
                CREATED = DateTime.Today,
                REGISTERDATE = DateTime.Today,
                WFDOCSTATUS = "0",
                WFDOCSTATUSTEXT = "Created"
            };
        }

        protected override Document WFGenerateDocument(TITIOM record, string userName)
        {
            Document document = new Document
            {
                DocDate = DateTime.Today,
                Description = "IOM of " + record.TITLE,
                DocType = _wfDocumentType,
                UnitID = record.UNITID,
                DocOwner = userName,
                DocStatus = "",
                WFFlag = string.Empty,
                Title = record.TITLE
            };
            return document;
        }



        public override TITIOM GetSingleByWorkflow(Document document, string userName)
        {
            return _context.TITIOM                
                .SingleOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
        }
       
    }
}
