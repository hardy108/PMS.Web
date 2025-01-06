using AM.EFCore.Models;
using AM.EFCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PMS.EFCore.Helper;
using PMS.Shared.Models;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF.EFCore.Data;
using WF.EFCore.Models;
using WF.EFCore.Models.Filters;

namespace WF.EFCore.Services
{
    public class WorkflowServiceV2
    {
        private WFContext _context;
        private AuthenticationServiceHO _authenticationService;        
        const string AMDOCTYPE = "WF.DOCTYPE";
        const string AMWFAPPROVER = "WF.Approve";
        

        public WorkflowServiceV2(WFContext context, AuthenticationServiceHO authenticationService)
        {
            _context = context;
            _authenticationService = authenticationService;
        }

        public IEnumerable<WorkflowRoute> GetNextWorkflowRoutes(WorkflowTransaction wfTrans, string userName)
        {
            
            
            if (wfTrans == null)
                throw new Exception("Permintaan approval dokumen belum diajukan");
            List<WorkflowTransactionHistory> openedTransactions = null;
            if (StandardUtility.IsEmptyList(wfTrans.WorkflowTransactionHistory))
                openedTransactions = _context.WorkflowTransactionHistory.Where(d=>d.TransactionNo.Equals(wfTrans.TransactionNo) && string.IsNullOrEmpty(d.ActionTypeID)).ToList();
            else
                openedTransactions = wfTrans.WorkflowTransactionHistory.Where(d => d.TransactionNo.Equals(wfTrans.TransactionNo) && string.IsNullOrEmpty(d.ActionTypeID)).ToList();
            if (openedTransactions.Count() != 1)
                throw new Exception("Kesalahan konfigurasi dokumen");

            var lastTransactionHistory = openedTransactions.First();
            var currentActivity = _context.WorkflowActivity.FirstOrDefault(d=>d.ActivityID.Equals(lastTransactionHistory.ActivityID));
            if (currentActivity == null)
                throw new Exception("Kesalahan konfigurasi dokumen - aktivitas approval tidak valid");

            if (!_authenticationService.IsAuthorizedPermission(userName,string.Empty, currentActivity.AMAlias))
                return null;

            var possibleRoutes = _context.WorkflowRoute
                .Where(d => d.ActivityID.Equals(currentActivity.ActivityID)
                    //&& (string.IsNullOrWhiteSpace(d.WFFlag) || wfFlags.Contains(d.WFFlag))
                    );

            if (possibleRoutes.Count() <= 0)
                throw new Exception("Kesalahan konfigurasi alur tahapan approval");


            
            bool isApprover = false;
            try
            {
                isApprover = (_authenticationService.GetPermissionMatrix(userName, string.Empty, AMWFAPPROVER).Any());
            }
            catch { }

            if (isApprover)
                return possibleRoutes.ToList();
            return null;
        }

        public IEnumerable<WorkflowAction> GetNextActions(WorkflowTransaction wfTrans, string userName)
        {

            IEnumerable<WorkflowRoute> nextRoutes = GetNextWorkflowRoutes(wfTrans, userName);
            
            if (nextRoutes != null && nextRoutes.Count() > 0)
            {
                return
                (from a in _context.WorkflowAction
                 join b in nextRoutes on a.ActionTypeID equals b.ActionTypeID
                 select a).AsNoTracking().Distinct().ToList();
            }
            return null;

        }

        public List<WFAPPROVER> GetNextApprovers(WorkflowTransaction wfTrans)
        {
            if (wfTrans == null)
                return new List<WFAPPROVER>();

            List<WorkflowTransactionHistory> openedTransactions = null;
            if (StandardUtility.IsEmptyList(wfTrans.WorkflowTransactionHistory))
                openedTransactions = _context.WorkflowTransactionHistory.Where(d => d.TransactionNo.Equals(wfTrans.TransactionNo) && string.IsNullOrEmpty(d.ActionTypeID)).ToList();
            else
                openedTransactions = wfTrans.WorkflowTransactionHistory.Where(d => d.TransactionNo.Equals(wfTrans.TransactionNo) && string.IsNullOrEmpty(d.ActionTypeID)).ToList();

            if (openedTransactions.Count() <= 0)
                throw new Exception("Approval terakhir tidak valid atau document sudah diapprove sebelumnya");
            if (openedTransactions.Count() != 1)
                throw new Exception("Kesalahan pada data approval history");
            var currentTransaction = openedTransactions.First();
            return GetNextApprovers(currentTransaction);
        }

        public List<WFAPPROVER> GetNextApprovers(WorkflowTransactionHistory currentTransaction)
        {
            
            if (currentTransaction == null)
                return new List<WFAPPROVER>();
            
            var currentActivity = _context.WorkflowActivity.FirstOrDefault(d=>d.ActivityID.Equals(currentTransaction.ActivityID));

            var wfTrans =
                (
                    from a in _context.WorkflowTransaction.Where(d => d.TransactionNo.Equals(currentTransaction.TransactionNo))
                    join b in _context.DocumentType on a.DocType equals b.DocTypeID
                    select new { WorkflowTransaction = a, DocumentType = b }
                ).FirstOrDefault();

            var nextApprovers =
            (
                from a in _authenticationService.GetAthorizedUserNamesByUnit(wfTrans.WorkflowTransaction.UnitID)
                join b in _authenticationService.GetAthorizedUserNamesByPermission(AMDOCTYPE, wfTrans.DocumentType.AMAlias) on a equals b
                join c in _authenticationService.GetAthorizedUserNamesByPermission(string.Empty, currentActivity.AMAlias) on b equals c
                join d in _authenticationService.GetAllUsers() on c equals d.ALIAS
                join e in _authenticationService.GetAthorizedUserNamesByPermissionDetails(AMWFAPPROVER) on a equals e into e1
                from e2 in e1.DefaultIfEmpty()
                select new WFAPPROVER { VAMUSER = d, IsApprover = (e2 == null) ? false : true }
            ).ToList();

            return nextApprovers;
        }

        public List<VAMUSER> GetPrevApprovers(WorkflowTransaction wfTrans, bool includeOwner, string excludeUserName)
        {
            if (wfTrans == null || StandardUtility.IsEmptyList(wfTrans.WorkflowTransactionHistory))
                return new List<VAMUSER>();


            var approvers =
            (
                from d in wfTrans.WorkflowTransactionHistory.Where(e=>!string.IsNullOrWhiteSpace(e.ActionTypeID))
                join b in _authenticationService.GetAllUsers() on d.FromUserID equals b.ALIAS
                where (includeOwner || !b.ALIAS.Equals(wfTrans.Creator)) && !b.ALIAS.Equals(excludeUserName)
                select b
            ).ToList();

            return approvers;
        }


        
        public WorkflowTransactionHistory SendApproval(WorkflowTransaction wfTrans, string userName, string actionCode, string approvalNote, string approvalFlag,  WFContext context)
        {
            if (wfTrans == null)            
                throw new Exception($"Dokumen approval tidak valid");
            
            var documentType = _context.DocumentType.AsNoTracking().Include(d=>d.Workflow).FirstOrDefault(d => d.DocTypeID.Equals(wfTrans.DocType));

            if (documentType == null)
                throw new Exception($"Tipe dokumen {wfTrans.DocType} tidak ditemukan");

            if (!documentType.IsActive)
                throw new Exception($"Tipe dokumen {documentType.DocTypeID} - {documentType.Description} tidak aktif");
            if (string.IsNullOrWhiteSpace(documentType.AMAlias))
                throw new Exception($"Tipe dokumen {documentType.DocTypeID} - {documentType.Description} belum tersambung dengan AM System");
            if (!documentType.Workflow.IsActive)
                throw new Exception($"Workflow {documentType.WorkflowID} - {documentType.Workflow.Description} (Versi {documentType.Workflow.Version}) tidak aktif");

            
            var workflowAction = context.WorkflowAction.Find(actionCode);
            if (workflowAction == null)
                throw new Exception("Kode approval tidak valid");

            if (workflowAction.RequireApprovalNotes && string.IsNullOrWhiteSpace(approvalNote))
                throw new Exception("Approval note wajib diisi");

            if (string.IsNullOrWhiteSpace(workflowAction.OwnerNotifType))
                workflowAction.OwnerNotifType = "V";
            if (string.IsNullOrWhiteSpace(workflowAction.NextApproverNotifType))
                workflowAction.NextApproverNotifType = "A";
            if (string.IsNullOrWhiteSpace(workflowAction.PrevApproverNotifType))
                workflowAction.PrevApproverNotifType = "V";

            if (!_authenticationService.IsAuthorizedUnit(userName, wfTrans.UnitID))
                throw new ExceptionNoUnitAccess(wfTrans.UnitID);

            //Cek Otorisasi Document Type
            var docTypeAuthorization = _authenticationService.GetPermissionMatrix(userName, AMDOCTYPE, documentType.AMAlias);

            if (docTypeAuthorization == null || !docTypeAuthorization.Any())
                throw new Exception("Anda tidak memiliki akses pada dokumen " + documentType.Description);

            

            WorkflowActivity currentActivity = null;
            string workflowId = documentType.WorkflowID;
            
            int lastSequenceNo = 0;
            bool firstSubmit = false;

            WorkflowTransactionHistory lastTransactionHistory = null;

            if (wfTrans.TransactionNo<=0)
            {
                //First submit

                var authorizedActivities = _authenticationService.GetPermissionMatrix(userName, documentType.Workflow.AMAlias, string.Empty)
                    .Select(d => d.PermissionDetails)
                    .Distinct()
                    .ToList();
                var startActivities = _context.WorkflowActivity.Where(d => d.WorkflowID.Equals(documentType.WorkflowID) && d.ActivityType == "S" && authorizedActivities.Contains(d.AMAlias)).ToList();
                if (startActivities == null || !startActivities.Any())
                    throw new Exception("Anda tidak memiliki akses approval workflow " + documentType.Workflow.Description);
                if (startActivities.Count != 1)
                    throw new Exception("Kesalan konfigurasi workflow " + documentType.Workflow.Description);
                currentActivity = startActivities.FirstOrDefault();

                firstSubmit = true;
            }
            else
            {
                var openedTransactions = _context.WorkflowTransactionHistory.Where(d => string.IsNullOrEmpty(d.ActionTypeID) && d.TransactionNo.Equals(wfTrans.TransactionNo));
                if (openedTransactions.Count() != 1)
                    throw new Exception("Kesalahan konfigurasi dokumen");

                lastTransactionHistory = openedTransactions.First();
                currentActivity = context.WorkflowActivity.Find(lastTransactionHistory.ActivityID);
                lastSequenceNo = lastTransactionHistory.SequenceNo;


                if (currentActivity == null)
                    throw new Exception("Approval terakhir tidak valid atau document sudah diapprove sebelumnya");

            }



            //Cek Otorisasi Activity
            if (string.IsNullOrWhiteSpace(currentActivity.AMAlias))
                throw new Exception("Aktifitas " + currentActivity.Description + " belum tersambung dengan AM System");
            var activityAuthorization = _authenticationService.GetPermissionMatrix(userName, documentType.Workflow.AMAlias, currentActivity.AMAlias);
            if (activityAuthorization == null || !activityAuthorization.Any())
                throw new Exception("Anda tidak memiliki akses pada aktifitas " + currentActivity.Description);



            var possibleRoutes = context.WorkflowRoute
                .Where(d => d.ActivityID.Equals(currentActivity.ActivityID)
                    && d.ActionTypeID.Equals(actionCode)
                    && (string.IsNullOrWhiteSpace(d.WFFlag) || d.WFFlag.Equals(approvalFlag)));

            if (possibleRoutes.Count() != 1)
                throw new Exception("Kesalahan konfigurasi alur tahapan approval");

            var nextRoute = possibleRoutes.FirstOrDefault();
            DateTime now = DateTime.Now;




            WorkflowTransactionHistory newTransactionHistory = null;
            //Add Approval History
            if (firstSubmit)
            {

                wfTrans.WorkflowID = workflowId;
                wfTrans.Creator = userName;
                wfTrans.StartActivity = currentActivity.ActivityID;
                wfTrans.SubmissionDate = now;
                wfTrans.LastActivityID = nextRoute.NextActivity;
                wfTrans.LastActivityDate = now;
                wfTrans.LastStatus = nextRoute.WorkflowStatus;
                context.WorkflowTransaction.Add(wfTrans);
                context.SaveChanges();

                

                newTransactionHistory = new WorkflowTransactionHistory
                {
                    TransactionNo = wfTrans.TransactionNo,
                    SequenceNo = 1,
                    ActionTypeID = actionCode,
                    ActivityID = currentActivity.ActivityID,
                    FromActivityID = currentActivity.ActivityID,
                    FromActionID = actionCode,
                    FromUserID = userName,
                    NextActivityID = nextRoute.NextActivity,
                    Notes = approvalNote,
                    UserID = userName,
                    WorkflowStatus = nextRoute.WorkflowStatus,
                    InDate = now,
                    OutDate = now
                };
                context.WorkflowTransactionHistory.Add(newTransactionHistory);
                newTransactionHistory = new WorkflowTransactionHistory
                {
                    TransactionNo = wfTrans.TransactionNo,
                    SequenceNo = 2,
                    ActivityID = nextRoute.NextActivity,
                    FromActivityID = nextRoute.NextActivity,
                    FromActionID = actionCode,
                    FromUserID = userName,
                    InDate = now
                };
                context.WorkflowTransactionHistory.Add(newTransactionHistory);
                context.SaveChanges();
                

            }
            else
            {



                if (workflowAction.ActionType.Equals("R2"))
                    nextRoute.NextActivity = wfTrans.StartActivity;


                wfTrans.LastActivityDate = now;
                wfTrans.LastStatus = nextRoute.WorkflowStatus;
                wfTrans.LastActivityID = nextRoute.NextActivity;
                context.WorkflowTransaction.Update(wfTrans);


                lastTransactionHistory.NextActivityID = nextRoute.NextActivity;
                lastTransactionHistory.ActionTypeID = actionCode;
                lastTransactionHistory.WorkflowStatus = nextRoute.WorkflowStatus;
                lastTransactionHistory.UserID = userName;
                lastTransactionHistory.Notes = approvalNote;
                lastTransactionHistory.OutDate = now;
                context.Update(lastTransactionHistory);
                if (!string.IsNullOrWhiteSpace(nextRoute.NextActivity))
                {
                    newTransactionHistory = new WorkflowTransactionHistory
                    {
                        TransactionNo = wfTrans.TransactionNo,
                        SequenceNo = lastTransactionHistory.SequenceNo + 1,
                        ActivityID = nextRoute.NextActivity,
                        FromActivityID = nextRoute.NextActivity,
                        FromActionID = actionCode,
                        FromUserID = userName,
                        InDate = now
                    };
                    context.WorkflowTransactionHistory.Add(newTransactionHistory);
                }
                context.SaveChanges();
            }

            return newTransactionHistory;
        }

        public WorkflowTransactionHistory SendApproval(WorkflowTransaction wfTrans, string userName, string actionCode, string approvalFlag, string approvalNote)
        {
            return SendApproval(wfTrans, userName, actionCode, approvalNote, approvalFlag, _context);
        }





        public IEnumerable<vwApprovalInbox> GetInboxByUserName(FilterWorkflow filter)
        {
            if (string.IsNullOrWhiteSpace(filter.UserName))
                throw new Exception("Username tidak boleh kosong");


            var activityIds =
            _context.WorkflowActivity.Where(d => d.IsActive && !string.IsNullOrWhiteSpace(d.AMAlias))
                .Join(_authenticationService.GetPermissionMatrix(filter.UserName, string.Empty, string.Empty), a => a.AMAlias, b => b.PermissionDetails, (a, b) => new { Activity = a })
                .Select(d => d.Activity.ActivityID).ToList();

            if (activityIds == null || !activityIds.Any())
                return new List<vwApprovalInbox>();


            List<string> unitIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                unitIds.Add(filter.UnitID);
            if (filter.UnitIDs.Any())
                unitIds.AddRange(filter.UnitIDs);
            if (unitIds.Any())
                unitIds = _authenticationService.GetAuthorizedUnitByUserName(filter.UserName, string.Empty).Where(d => unitIds.Contains(d.UNITCODE)).Select(d=>d.UNITCODE).ToList();
            else
                unitIds = _authenticationService.GetAuthorizedUnitByUserName(filter.UserName, string.Empty).Select(d => d.UNITCODE).ToList();


            List<string> docTypeIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.DocType))
                docTypeIds.Add(filter.DocType);
            if (filter.DocTypes.Any())
                docTypeIds.AddRange(filter.DocTypes);
            if (docTypeIds.Any())
                docTypeIds = _authenticationService.GetPermissionMatrix(filter.UserName,AMDOCTYPE,string.Empty)
                            .Join(_context.DocumentType,a=>a.PermissionDetails,b=>b.AMAlias,(a,b)=>new { DocumentType = b })                            
                            .Where(d => docTypeIds.Contains(d.DocumentType.DocTypeID))
                            .Select(d => d.DocumentType.DocTypeID).ToList();
            else
                docTypeIds = _authenticationService.GetPermissionMatrix(filter.UserName, AMDOCTYPE, string.Empty)
                            .Join(_context.DocumentType, a => a.PermissionDetails, b => b.AMAlias, (a, b) => new { DocumentType = b })                            
                            .Select(d => d.DocumentType.DocTypeID).ToList();

            var criteria = PredicateBuilder.True<vwApprovalInbox>();

            criteria = criteria.And(d =>
                docTypeIds.Contains(d.DocType) && unitIds.Contains(d.UnitID)
                && d.InDate.HasValue && d.InDate.Value.Date >= filter.StartDate.Date && d.InDate.Value.Date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                filter.SearchTerm = filter.SearchTerm.ToLower();
                criteria = criteria.And(d =>
                    d.Description.ToLower().Contains(filter.SearchTerm)
                    || d.Title.ToLower().Contains(filter.SearchTerm)
                    || d.UnitID.ToLower().Contains(filter.SearchTerm)
                    || d.DocOwner.ToLower().Contains(filter.SearchTerm)
                    || d.DocNo.ToLower().Contains(filter.SearchTerm)
                    || d.DocTransNo.ToString().Equals(filter.SearchTerm)
                );
            }

            criteria = criteria.And(d => activityIds.Contains(d.FromActivityID));

            return _context.vwApprovalInbox.Where(criteria);
            



        }

        public IEnumerable<vwApprovalOutbox> GetOutboxByUserName(FilterWorkflow filter)
        {
            if (string.IsNullOrWhiteSpace(filter.UserName))
                throw new Exception("Username tidak boleh kosong");

            List<string> unitIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                unitIds.Add(filter.UnitID);
            if (filter.UnitIDs.Any())
                unitIds.AddRange(filter.UnitIDs);
            if (unitIds.Any())
                unitIds = _authenticationService.GetAuthorizedUnitByUserName(filter.UserName, string.Empty).Where(d => unitIds.Contains(d.UNITCODE)).Select(d => d.UNITCODE).ToList();
            else
                unitIds = _authenticationService.GetAuthorizedUnitByUserName(filter.UserName, string.Empty).Select(d => d.UNITCODE).ToList();


            List<string> docTypeIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.DocType))
                docTypeIds.Add(filter.DocType);
            if (filter.DocTypes.Any())
                docTypeIds.AddRange(filter.DocTypes);
            if (docTypeIds.Any())
                docTypeIds = _authenticationService.GetPermissionMatrix(filter.UserName, AMDOCTYPE, string.Empty)
                            .Join(_context.DocumentType, a => a.PermissionDetails, b => b.AMAlias, (a, b) => new { DocumentType = b })
                            .Where(d => docTypeIds.Contains(d.DocumentType.DocTypeID))
                            .Select(d => d.DocumentType.DocTypeID).ToList();
            else
                docTypeIds = _authenticationService.GetPermissionMatrix(filter.UserName, AMDOCTYPE, string.Empty)
                            .Join(_context.DocumentType, a => a.PermissionDetails, b => b.AMAlias, (a, b) => new { DocumentType = b })
                            .Select(d => d.DocumentType.DocTypeID).ToList();

            var criteria = PredicateBuilder.True<vwApprovalOutbox>();

            criteria = criteria.And(d =>
                docTypeIds.Contains(d.DocType) && unitIds.Contains(d.UnitID)
                && d.OutDate.HasValue && (d.OutDate.Value >= filter.StartDate && d.OutDate.Value <= filter.EndDate));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                filter.SearchTerm = filter.SearchTerm.ToLower();
                criteria = criteria.And(d =>
                    d.Description.ToLower().Contains(filter.SearchTerm)
                    || d.Title.ToLower().Contains(filter.SearchTerm)
                    || d.UnitID.ToLower().Contains(filter.SearchTerm)
                    || d.DocOwner.ToLower().Contains(filter.SearchTerm)
                    || d.DocNo.ToLower().Contains(filter.SearchTerm)
                    || d.DocTransNo.ToString().Equals(filter.SearchTerm)
                );
            }

            if (filter.PageSize < 0)
                return _context.vwApprovalOutbox.Where(criteria);
            return _context.vwApprovalOutbox.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public IEnumerable<vwDocument> GetDraftByUserName(FilterWorkflow filter)
        {
            if (string.IsNullOrWhiteSpace(filter.UserName))
                throw new Exception("Username tidak boleh kosong");

            List<string> unitIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                unitIds.Add(filter.UnitID);
            if (filter.UnitIDs.Any())
                unitIds.AddRange(filter.UnitIDs);
            if (unitIds.Any())
                unitIds = _authenticationService.GetAuthorizedUnitByUserName(filter.UserName, string.Empty).Where(d => unitIds.Contains(d.UNITCODE)).Select(d => d.UNITCODE).ToList();
            else
                unitIds = _authenticationService.GetAuthorizedUnitByUserName(filter.UserName, string.Empty).Select(d => d.UNITCODE).ToList();


            List<string> docTypeIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.DocType))
                docTypeIds.Add(filter.DocType);
            if (filter.DocTypes.Any())
                docTypeIds.AddRange(filter.DocTypes);
            if (docTypeIds.Any())
                docTypeIds = _authenticationService.GetPermissionMatrix(filter.UserName, AMDOCTYPE, string.Empty)
                            .Join(_context.DocumentType, a => a.PermissionDetails, b => b.AMAlias, (a, b) => new { DocumentType = b })
                            .Where(d => docTypeIds.Contains(d.DocumentType.DocTypeID))
                            .Select(d => d.DocumentType.DocTypeID).ToList();
            else
                docTypeIds = _authenticationService.GetPermissionMatrix(filter.UserName, AMDOCTYPE, string.Empty)
                            .Join(_context.DocumentType, a => a.PermissionDetails, b => b.AMAlias, (a, b) => new { DocumentType = b })
                            .Select(d => d.DocumentType.DocTypeID).ToList();

            var criteria = PredicateBuilder.True<vwDocument>();

            criteria = criteria.And(d =>
                (!d.WorkflowTransactionNo.HasValue || d.DocStatus=="50") &&
                docTypeIds.Contains(d.DocType) && unitIds.Contains(d.UnitID) &&
                (d.DocDate >= filter.StartDate && d.DocDate <= filter.EndDate));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                filter.SearchTerm = filter.SearchTerm.ToLower();
                criteria = criteria.And(d =>
                    d.Description.ToLower().Contains(filter.SearchTerm)
                    || d.Title.ToLower().Contains(filter.SearchTerm)
                    || d.UnitID.ToLower().Contains(filter.SearchTerm)
                    || d.DocOwner.ToLower().Contains(filter.SearchTerm)
                    || d.DocNo.ToLower().Contains(filter.SearchTerm)
                    || d.DocTransNo.ToString().Equals(filter.SearchTerm)
                );
            }



            if (filter.PageSize < 0)
                return _context.vwDocument.Where(criteria);
            return _context.vwDocument.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

            
        }

        public IEnumerable<DocumentType> GetDocumentTypesByUserName(string userName)
        {
            return _authenticationService.GetPermissionMatrix(userName, AMDOCTYPE, string.Empty)
                .Join(_context.DocumentType, a => a.PermissionDetails, b => b.AMAlias, (a, b) => new { DocumentType = b })
                .Select(d => d.DocumentType);
                
        }

        

    }
}
