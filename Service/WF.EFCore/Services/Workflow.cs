using AM.EFCore.Models;
using AM.EFCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PMS.EFCore.Helper;
using PMS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF.EFCore.Data;
using WF.EFCore.Models;
using WF.EFCore.Models.Filters;

namespace WF.EFCore.Services
{
    public class WorkflowService
    {
        private WFContext _context;
        private AuthenticationServiceHO _authenticationService;        
        const string AMDOCTYPE = "WF.DOCTYPE";
        const string AMWFAPPROVER = "WF.Approve";
        

        public WorkflowService(WFContext context, AuthenticationServiceHO authenticationService)
        {
            _context = context;
            _authenticationService = authenticationService;
        }

        public IEnumerable<WorkflowRoute> GetNextWorkflowRoutes(Document doc, string userName)
        {
            
            if (doc == null)
                throw new Exception("Dokumen tidak ditemukan");
            if (doc.WorkflowTransaction == null)
                throw new Exception("Permintaan approval dokumen belum diajukan");
            List<string> wfFlags = new List<string>();
            if (!string.IsNullOrWhiteSpace(doc.WFFlag))
                wfFlags.AddRange(doc.WFFlag.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries));

            var openedTransactions = doc.WorkflowTransaction.WorkflowTransactionHistory.Where(d => string.IsNullOrEmpty(d.ActionTypeID));
            if (openedTransactions.Count() != 1)
                throw new Exception("Kesalahan konfigurasi dokumen");

            var lastTransactionHistory = openedTransactions.First();
            var currentActivity = _context.WorkflowActivity.Find(lastTransactionHistory.ActivityID);
            
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

        public IEnumerable<WorkflowAction> GetNextActions(Document doc, string userName)
        {

            IEnumerable<WorkflowRoute> nextRoutes = GetNextWorkflowRoutes(doc, userName);
            
            if (nextRoutes != null && nextRoutes.Count() > 0)
            {
                return
                (from a in _context.WorkflowAction
                 join b in nextRoutes on a.ActionTypeID equals b.ActionTypeID
                 select a).AsNoTracking().Distinct().ToList();
            }
            return null;

        }

        public List<WFAPPROVER> GetNextApprovers(Document doc)
        {
            if (doc == null || doc.WorkflowTransaction == null || doc.WorkflowTransaction.WorkflowTransactionHistory == null)
                return new List<WFAPPROVER>();

            var openedTransactions = doc.WorkflowTransaction.WorkflowTransactionHistory.Where(d => string.IsNullOrWhiteSpace(d.ActionTypeID));

            if (openedTransactions.Count() <= 0)
                throw new Exception("Approval terakhir tidak valid atau document sudah diapprove sebelumnya");
            if (openedTransactions.Count() != 1)
                throw new Exception("Kesalahan pada data approval history");
            var currentTransaction = openedTransactions.First();
            return GetNextApprovers(doc, currentTransaction);
        }

        public List<WFAPPROVER> GetNextApprovers(Document doc, WorkflowTransactionHistory currentTransaction)
        {
            if (currentTransaction == null)
                return new List<WFAPPROVER>();
            
            var currentActivity = _context.WorkflowActivity.Find(currentTransaction.ActivityID);


            var nextApprovers =
            (
                from a in _authenticationService.GetAthorizedUserNamesByUnit(doc.UnitID)
                join b in _authenticationService.GetAthorizedUserNamesByPermission(AMDOCTYPE, doc.DocumentType.AMAlias) on a equals b
                join c in _authenticationService.GetAthorizedUserNamesByPermission(string.Empty, currentActivity.AMAlias) on b equals c
                join d in _authenticationService.GetAllUsers() on c equals d.ALIAS
                join e in _authenticationService.GetAthorizedUserNamesByPermissionDetails(AMWFAPPROVER) on a equals e into e1
                from e2 in e1.DefaultIfEmpty()
                select new WFAPPROVER { VAMUSER = d, IsApprover = (e2 == null) ? false : true }
            ).ToList();

            return nextApprovers;
        }

        public List<VAMUSER> GetPrevApprovers(Document doc, bool includeOwner, string excludeUserName)
        {
            if (doc == null || doc.WorkflowTransaction == null || doc.WorkflowTransaction.WorkflowTransactionHistory == null)
                return new List<VAMUSER>();

            var approvers =
            (
                from d in doc.WorkflowTransaction.WorkflowTransactionHistory.Where(e=>!string.IsNullOrWhiteSpace(e.ActionTypeID))
                join b in _authenticationService.GetAllUsers() on d.FromUserID equals b.ALIAS
                where (includeOwner || !b.ALIAS.Equals(doc.DocOwner)) && !b.ALIAS.Equals(excludeUserName)
                select b
            ).ToList();

            return approvers;
        }


        
        public string SendApproval(Document doc, string userName, string actionCode, string approvalNote,  WFContext context)
        {
            if (doc == null)
                throw new Exception("Dokumen tidak ditemukan");
            if (!doc.DocumentType.IsActive)
                throw new Exception($"Tipe dokumen {doc.DocType} - {doc.DocumentType.Description} tidak aktif");
            if (string.IsNullOrWhiteSpace(doc.DocumentType.AMAlias))
                throw new Exception($"Tipe dokumen {doc.DocType} - {doc.DocumentType.Description} belum tersambung dengan AM System");
            if (!doc.DocumentType.Workflow.IsActive)
                throw new Exception($"Workflow {doc.DocumentType.WorkflowID} - {doc.DocumentType.Workflow.Description} (Versi {doc.DocumentType.Workflow.Version}) tidak aktif");

            //context.Update(doc);
            //context.SaveChanges();

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

            if (!_authenticationService.IsAuthorizedUnit(userName, doc.UnitID))
                throw new ExceptionNoUnitAccess(doc.UnitID);

            //Cek Otorisasi Document Type
            var docTypeAuthorization = _authenticationService.GetPermissionMatrix(userName, AMDOCTYPE, doc.DocumentType.AMAlias);

            if (docTypeAuthorization == null || !docTypeAuthorization.Any())
                throw new Exception("Anda tidak memiliki akses pada dokumen " + doc.DocumentType.Description);

            

            WorkflowActivity currentActivity = null;
            string workflowId = doc.DocumentType.WorkflowID;
            List<string> wfFlags = new List<string>();
            int lastSequenceNo = 0;
            bool firstSubmit = false;
            if (!string.IsNullOrWhiteSpace(doc.WFFlag))
                wfFlags.AddRange(doc.WFFlag.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
            WorkflowTransactionHistory lastTransactionHistory = null;

            if (doc.WorkflowTransaction == null)
            {
                //First submit

                var authorizedActivities = _authenticationService.GetPermissionMatrix(userName, doc.DocumentType.Workflow.AMAlias, string.Empty)
                    .Select(d => d.PermissionDetails)
                    .Distinct()
                    .ToList();
                var startActivities = doc.DocumentType.Workflow.WorkflowActivity
                    .Where(d => d.ActivityType == "S" && authorizedActivities.Contains(d.AMAlias)).ToList();
                if (startActivities == null || !startActivities.Any())
                    throw new Exception("Anda tidak memiliki akses approval workflow " + doc.DocumentType.Workflow.Description);
                if (startActivities.Count != 1)
                    throw new Exception("Kesalan konfigurasi workflow " + doc.DocumentType.Workflow.Description);
                currentActivity = startActivities.FirstOrDefault();

                firstSubmit = true;
            }
            else
            {
                var openedTransactions = doc.WorkflowTransaction.WorkflowTransactionHistory.Where(d => string.IsNullOrEmpty(d.ActionTypeID));
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
            var activityAuthorization = _authenticationService.GetPermissionMatrix(userName, doc.DocumentType.Workflow.AMAlias, currentActivity.AMAlias);
            if (activityAuthorization == null || !activityAuthorization.Any())
                throw new Exception("Anda tidak memiliki akses pada aktifitas " + currentActivity.Description);



            var possibleRoutes = context.WorkflowRoute
                .Where(d => d.ActivityID.Equals(currentActivity.ActivityID)
                    && d.ActionTypeID.Equals(actionCode)
                    && (string.IsNullOrWhiteSpace(d.WFFlag) || wfFlags.Contains(d.WFFlag)));

            if (possibleRoutes.Count() != 1)
                throw new Exception("Kesalahan konfigurasi alur tahapan approval");

            var nextRoute = possibleRoutes.FirstOrDefault();
            DateTime now = DateTime.Now;




            WorkflowTransactionHistory newTransactionHistory;
            //Add Approval History
            if (firstSubmit)
            {

                var newTransaction = new WorkflowTransaction
                {
                    WorkflowID = workflowId,
                    Creator = userName,
                    StartActivity = currentActivity.ActivityID,
                    SubmissionDate = now,
                    LastActivityID = nextRoute.NextActivity,
                    LastActivityDate = now,
                    LastStatus = nextRoute.WorkflowStatus
                };
                context.WorkflowTransaction.Add(newTransaction);
                context.SaveChanges();

                

                newTransactionHistory = new WorkflowTransactionHistory
                {
                    TransactionNo = newTransaction.TransactionNo,
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
                    TransactionNo = newTransaction.TransactionNo,
                    SequenceNo = 2,
                    ActivityID = nextRoute.NextActivity,
                    FromActivityID = nextRoute.NextActivity,
                    FromActionID = actionCode,
                    FromUserID = userName,
                    InDate = now
                };
                context.WorkflowTransactionHistory.Add(newTransactionHistory);

                var docX = context.Document.Find(doc.DocTransNo);
                docX.WorkflowTransactionNo = newTransaction.TransactionNo;
                docX.DocStatus = newTransaction.LastStatus;
                docX.LastUpdateDate = now;
                docX.LastUpdateUser = userName;
                context.Update(docX);
                context.SaveChanges();

            }
            else
            {



                if (workflowAction.ActionType.Equals("R2"))
                    nextRoute.NextActivity = doc.WorkflowTransaction.StartActivity;

                var workflowTransaction = doc.WorkflowTransaction;

                workflowTransaction.LastActivityDate = now;
                workflowTransaction.LastStatus = nextRoute.WorkflowStatus;
                workflowTransaction.LastActivityID = nextRoute.NextActivity;
                context.WorkflowTransaction.Update(workflowTransaction);


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
                        TransactionNo = workflowTransaction.TransactionNo,
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

                var docX = context.Document.Find(doc.DocTransNo);
                docX.DocStatus = nextRoute.WorkflowStatus;
                docX.LastUpdateDate = now;
                docX.LastUpdateUser = userName;
                context.Update(docX);

            }


            //var allDocumentToken = _context.DocumentAccessToken.Where(d => d.DocTransNo == doc.DocTransNo && d.IsActive).ToList();
            //allDocumentToken.ForEach(d => d.IsActive = false);
            //_context.UpdateRange(allDocumentToken);
            
            ////Generate Document Access Token For Notification            

            //var notificationTemplate = _context.DocumentTypeNotificationTemplate
            //.Where(d => d.DocTypeID == doc.DocType && d.ActionTypeID == actionCode)
            //.FirstOrDefault();

            

            //var amUser = _authenticationService.GetUserInfo(doc.DocOwner);
            string sessionId = UniqueId.CreateRandomId();

            //string emailSubject = "No Subject", emailBody = "No Contents";
            //if (workflowAction.SendNotifToOwner)
            //{
            //    if (notificationTemplate != null)
            //    {
            //        if (!string.IsNullOrWhiteSpace(notificationTemplate.NotifToOwnerTemplate))
            //            emailBody = notificationTemplate.NotifToOwnerTemplate;
            //        if (!string.IsNullOrWhiteSpace(notificationTemplate.NotifToOwnerEmailSubject))
            //            emailSubject = notificationTemplate.NotifToOwnerEmailSubject;
            //    }
            //    _context.DocumentAccessToken.Add
            //        (
            //            new DocumentAccessToken
            //            {
            //                TokenID = UniqueId.CreateRandomId(),
            //                DocTransNo = doc.DocTransNo,
            //                UserID = doc.DocOwner,
            //                AccessType = workflowAction.OwnerNotifType,
            //                CreatedDate = now,
            //                ExpiredDate = now.AddDays(2),
            //                IsActive = true,
            //                SessionID = sessionId,
            //                To = amUser.EMAIL,
            //                Body = emailBody,
            //                Subject = emailSubject
            //                    .Replace("[DOCNO]", doc.DocNo)
            //                    .Replace("[USERID]", userName)
            //            }
            //        );
            //}
            //if (workflowAction.SendNotifToNextApprovers)
            //{
            //    if (notificationTemplate != null)
            //    {
            //        if (!string.IsNullOrWhiteSpace(notificationTemplate.NotifToNextApproversTemplate))
            //            emailBody = notificationTemplate.NotifToNextApproversTemplate;
            //        if (!string.IsNullOrWhiteSpace(notificationTemplate.NotifToNextApproversEmailSubject))
            //            emailSubject = notificationTemplate.NotifToNextApproversEmailSubject;
            //    }
            //    _context.AddRange(GetNextApprovers(doc,newTransactionHistory).Select(d => new DocumentAccessToken
            //    {
            //        TokenID = UniqueId.CreateRandomId(),
            //        DocTransNo = doc.DocTransNo,
            //        UserID = d.VAMUSER.ALIAS,
            //        AccessType = d.IsApprover ? workflowAction.NextApproverNotifType : "V",
            //        CreatedDate = now,
            //        ExpiredDate = now.AddDays(2),
            //        IsActive = true,
            //        SessionID = sessionId,
            //        To = d.VAMUSER.EMAIL,
            //        Body = emailBody,
            //        Subject = emailSubject
            //                    .Replace("[DOCNO]", doc.DocNo)
            //                    .Replace("[USERID]", userName)
            //    }));

            //}
            //if (workflowAction.SendNotifToPrevApprovers)
            //{
            //    if (notificationTemplate != null)
            //    {
            //        if (!string.IsNullOrWhiteSpace(notificationTemplate.NotifToPrevApproversTemplate))
            //            emailBody = notificationTemplate.NotifToPrevApproversTemplate;
            //        if (!string.IsNullOrWhiteSpace(notificationTemplate.NotifToPrevApproversEmailSubject))
            //            emailSubject = notificationTemplate.NotifToPrevApproversEmailSubject;
            //    }
            //    _context.AddRange(GetPrevApprovers(doc, false, userName).Select(d => new DocumentAccessToken
            //    {
            //        TokenID = UniqueId.CreateRandomId(),
            //        DocTransNo = doc.DocTransNo,
            //        UserID = d.ALIAS,
            //        AccessType = "V",
            //        CreatedDate = now,
            //        ExpiredDate = now.AddDays(2),
            //        IsActive = true,
            //        SessionID = sessionId,
            //        To = d.EMAIL,
            //        Body = emailBody,
            //        Subject = emailSubject
            //                     .Replace("[DOCNO]", doc.DocNo)
            //                     .Replace("[USERID]", userName)
            //    }));
            //}
            _context.SaveChanges();
            return sessionId;
        }

        public string SendApproval(Document doc, string userName, string actionCode, string approvalNote)
        {
            return SendApproval(doc, userName, actionCode, approvalNote, _context);
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
