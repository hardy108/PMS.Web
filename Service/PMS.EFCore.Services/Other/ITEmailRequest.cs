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
    public class ITEmailRequest : EntityFactoryWithWorkflow<TITEMAILREQUEST, TITEMAILREQUEST, GeneralFilter, PMSContextBase,WFContext>
    {




        private AuthenticationServiceHO _authenticationService;

        public ITEmailRequest(PMSContextHO context, WFContext wfContext,AuthenticationServiceHO  authenticationService, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext,authenticationService,authenticationService, taskQueue,emailSender,auditContext)
        {
            _serviceName = "EmailRequest";
            _wfDocumentType = "ITEMAIL";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<TITEMAILREQUEST> GetList(GeneralFilter filter)
        {
            DateTime startDate = filter.StartDate, endDate = filter.EndDate;
            StandardUtility.NullDateRange(ref startDate, ref endDate);

            var criteria = PredicateBuilder.True<TITEMAILREQUEST>();
            criteria = criteria.And(d => ((d.REGISTERDATE.Date >= startDate && d.REGISTERDATE.Date <= endDate) || (d.REQUESTDATE.Date >= startDate && d.REQUESTDATE.Date <= endDate)));

            var unitIds = filter.UnitIDs;
            if (unitIds.Count > 0)
                criteria = criteria.And(d => unitIds.Contains(d.UNITID));
            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                criteria = criteria.And(d => d.REGISTERID.ToLower().Contains(filter.LowerCasedSearchTerm) || d.REQUESTER.ToLower().Contains(filter.LowerCasedSearchTerm));

            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);

            if (filter.PageSize <= 0)
                return _context.TITEMAILREQUEST.Where(criteria);
            return _context.TITEMAILREQUEST.Where(criteria).GetPaged(filter.PageNo,filter.PageSize).Results;
            
        }

        

        

        protected override TITEMAILREQUEST BeforeSave(TITEMAILREQUEST record, string userName, bool newRecord)
        {
            /*Custom Code - Start*/
            /*Validation before save existing or new record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/
            if (string.IsNullOrWhiteSpace(record.UNITID))
                throw new Exception("Unit can not be empty");

            if (string.IsNullOrWhiteSpace(record.REQUESTER))
                throw new Exception("Requested can not be empty");
            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATED = currentDate;
                record.CREATEDBY = userName;
                record.REGISTERDATE = record.CREATED.Date;
                record.REGISTERID = record.UNITID + "-ITREMAIL-" + currentDate.ToString("yyyyMM") + "-" + HelperService.GetCurrentDocumentNumber(_serviceName + record.UNITID, _context).ToString("0000");

            }
            record.UPDATED = currentDate;
            record.UPDATEDBY = userName;
            
            return record;
        }

       
       
        
        protected override TITEMAILREQUEST AfterSave(TITEMAILREQUEST record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(_serviceName + record.UNITID, _context);
            return record;
        }
        

       



        

        public override TITEMAILREQUEST NewRecord(string userName)
        {
            return new TITEMAILREQUEST
            {
                CREATED = DateTime.Today,
                REGISTERDATE = DateTime.Today,
                REQUESTDATE = DateTime.Today,
                WFDOCSTATUS = "0",
                WFDOCSTATUSTEXT = "Created"
            };
        }

        protected override Document WFGenerateDocument(TITEMAILREQUEST record, string userName)
        {
            Document document = new Document
            {
                DocDate = DateTime.Today,
                Description = "Email Request of " + record.REQUESTER,
                DocType = _wfDocumentType,
                UnitID = record.UNITID,
                //DepartmentID = record.DEPTID,
                DocOwner = userName,
                DocStatus = "",
                WFFlag = string.Empty,
                Title = "Email Request of " + record.REQUESTER
            };
            return document;
        }



        public override TITEMAILREQUEST GetSingleByWorkflow(Document document, string userName)
        {
            return _context.TITEMAILREQUEST                
                .SingleOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
        }
       
    }
}
