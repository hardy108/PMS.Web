using AM.EFCore.Services;
using PMS.EFCore.Helper;
using PMS.EFCore.Model;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF.EFCore.Data;
using WF.EFCore.Models;
using WF.EFCore.Models.Filters;
using WF.EFCore.Services;

namespace PMS.EFCore.Services.Approval
{

   
    public class PMSWorkflow
    {
        private AuthenticationServiceHO _authenticationService;
        private WFContext _wfContext;
        private PMSContextBase _pmsContext;
        private WorkflowService _workflowService;
        private DocumentService _documentService;
        private List<string> _pmsDocTypes;
        public PMSWorkflow(PMSContextBase pmsContext, WFContext wfContext, AuthenticationServiceHO authenticationService,AuditContext auditContext)
        {
            _authenticationService = authenticationService;
            _pmsContext = pmsContext;
            _wfContext = wfContext;
            _workflowService = new WorkflowService(_wfContext, _authenticationService);
            _documentService = new DocumentService(_wfContext, _authenticationService,auditContext);
            _pmsDocTypes = new List<string> { "EMPREGIST", "EMPCHANGE","ICR","ITINV","ITEMAIL","ITIOM","ATTPROBLEM","HVLB","HVMAXBRONDOL", "HVDAYVALID","PAYADJHK","LEAVE" };
            
        }

        public IEnumerable<vwApprovalInbox> GetInbox(FilterWorkflow filter)
        {
           
            var inbox = _workflowService.GetInboxByUserName(filter).Where(d => d.DocStatus != string.Empty && d.DocStatus != "0");

            
            if (inbox != null && inbox.Any())
            {
                var query =
                inbox
                    .Join(_pmsContext.TEMPLOYEEREGISTRATION, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "EMPREGIST").
                Union(
                inbox
                    .Join(_pmsContext.TEMPLOYEECHANGE, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "EMPCHANGE")
                ).
                Union(
                inbox
                    .Join(_pmsContext.TICR, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "ICR")
                ).
                Union(
                inbox
                    .Join(_pmsContext.TITINVOICE, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "ITINV")
                ).
                Union(
                inbox
                    .Join(_pmsContext.TITEMAILREQUEST, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "ITEMAIL")
                ).
                Union(
                inbox
                    .Join(_pmsContext.TITIOM, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "ITIOM")
                ).
                Union(
                inbox
                    .Join(_pmsContext.THARVESTWFLEBIHBASIS, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "HVLB")
                ).
                Union(
                inbox
                    .Join(_pmsContext.THARVESTWFMAXBRONDOL, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "HVMAXBRONDOL")
                ).
                Union(
                inbox
                    .Join(_pmsContext.THARVESTWFDAYVALIDATION, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "HVDAYVALID")
                ).
                Union(
                inbox
                    .Join(_pmsContext.TPAYMENTWFADJUSTHK, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Inbox = a })
                    .Where(d => d.Inbox.DocType == "PAYADJHK")
                ).
                Select(d => d.Inbox);

                if (filter.PageSize <= 0)
                    return query;
                return query.AsQueryable().GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            return new List<vwApprovalInbox>();
        }

        public IEnumerable<vwApprovalInbox> GetInbox2(FilterWorkflow filter)
        {
            if (string.IsNullOrWhiteSpace(filter.DocType) && (filter.DocTypes == null || filter.DocTypes.Count <= 0))
                filter.DocTypes = _pmsDocTypes;

            var inbox = _workflowService.GetInboxByUserName(filter).Where(d => d.DocStatus != string.Empty && d.DocStatus != "0");

            
            if (inbox != null && inbox.Any())
            {
                if (filter.PageSize <= 0)
                    return inbox;
                return inbox.AsQueryable().GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            return new List<vwApprovalInbox>();
        }

        public int GetInboxCount(FilterWorkflow filter)
        {
            filter.PageSize = 0;
            return GetInbox(filter).Count();
        }

        public int GetInbox2Count(FilterWorkflow filter)
        {
            filter.PageSize = 0;
            return GetInbox2(filter).Count();
        }

        public IEnumerable<vwApprovalOutbox> GetOutbox(FilterWorkflow filter)
        {
            
            var outbox = _workflowService.GetOutboxByUserName(filter);
            if (outbox != null && outbox.Any())
            {
                var query =
            outbox
                .Join(_pmsContext.TEMPLOYEEREGISTRATION, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "EMPREGIST").
            Union(
            outbox
                .Join(_pmsContext.TEMPLOYEECHANGE, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "EMPCHANGE")
            ).
            Union(
            outbox
                .Join(_pmsContext.TICR, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "ICR")
            ).
            Union(
            outbox
                .Join(_pmsContext.TITINVOICE, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "ITINV")
            ).
            Union(
            outbox
                .Join(_pmsContext.TITEMAILREQUEST, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "ITEMAIL")
            ).
            Union(
            outbox
                .Join(_pmsContext.TITIOM, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "ITIOM")
            ).
            Union(
            outbox
                .Join(_pmsContext.THARVESTWFLEBIHBASIS, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "HVLB")
            ).
            Union(
            outbox
                .Join(_pmsContext.THARVESTWFMAXBRONDOL, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "HVMAXBRONDOL")
            ).
            Union(
            outbox
                .Join(_pmsContext.THARVESTWFDAYVALIDATION, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "HVDAYVALID")
            ).
            Union(
            outbox
                .Join(_pmsContext.TPAYMENTWFADJUSTHK, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Outbox = a })
                .Where(d => d.Outbox.DocType == "PAYADJHK")
            ).
            Select(d => d.Outbox);


                if (filter.PageSize < 0)
                    return query;
                return query.AsQueryable().GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            return new List<vwApprovalOutbox>();
            
            
        }


        public IEnumerable<vwApprovalOutbox> GetOutbox2(FilterWorkflow filter)
        {
            if (string.IsNullOrWhiteSpace(filter.DocType) && (filter.DocTypes == null || filter.DocTypes.Count <= 0))
                filter.DocTypes = _pmsDocTypes;

            var outbox = _workflowService.GetOutboxByUserName(filter).Where(d => d.DocStatus != string.Empty && d.DocStatus != "0");
            if (outbox != null && outbox.Any())
            {
                if (filter.PageSize < 0)
                    return outbox;
                return outbox.AsQueryable().GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            return new List<vwApprovalOutbox>();
        }

        public int GetOutboxCount(FilterWorkflow filter)
        {
            filter.PageSize = 0;
            return GetOutbox(filter).Count();
        }

        public int GetOutbox2Count(FilterWorkflow filter)
        {
            filter.PageSize = 0;
            return GetOutbox2(filter).Count();
        }


        public IEnumerable<vwDocument> GetDraft(FilterWorkflow filter)
        {
            
            var draft = _workflowService.GetDraftByUserName(filter);

            var query =
            draft
                .Join(_pmsContext.TEMPLOYEEREGISTRATION, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "EMPREGIST").
            Union(
            draft
                .Join(_pmsContext.TEMPLOYEECHANGE, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "EMPCHANGE")
            ).
            Union(
            draft
                .Join(_pmsContext.TICR, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "ICR")
            ).
            Union(
            draft
                .Join(_pmsContext.TITINVOICE, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "ITINV")
            ).
            Union(
            draft
                .Join(_pmsContext.TITIOM, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "ITIOM")
            ).
            Union(
            draft
                .Join(_pmsContext.TITEMAILREQUEST, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "ITEMAIL")
            ).
            Union(
            draft
                .Join(_pmsContext.THARVESTWFLEBIHBASIS, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "HVLB")
            ).
            Union(
            draft
                .Join(_pmsContext.THARVESTWFMAXBRONDOL, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "HVMAXBRONDOL")
            ).
            Union(
            draft
                .Join(_pmsContext.THARVESTWFDAYVALIDATION, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "HVDAYVALID")
            ).
            Union(
            draft
                .Join(_pmsContext.TPAYMENTWFADJUSTHK, a => a.DocTransNo, b => b.WFDOCTRANSNO, (a, b) => new { Draft = a })
                .Where(d => d.Draft.DocType == "PAYADJHK")
            ).
            Select(d => d.Draft);

            if (filter.PageSize < 0)
                return query;
            return query.AsQueryable().GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public int GetDraftCount(FilterWorkflow filter)
        {
            filter.PageSize = 0;
            return GetDraft(filter).Count();
        }

        public IEnumerable<DocumentType> GetDocumentTypesByUserName(string userName)
        {
            return _workflowService.GetDocumentTypesByUserName(userName).Where(d => _pmsDocTypes.Contains(d.DocTypeID));
        }


        
    }
}

