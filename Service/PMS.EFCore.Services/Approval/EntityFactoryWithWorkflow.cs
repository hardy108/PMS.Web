using AM.EFCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.Logging.Internal;
using PMS.EFCore.Helper;
using PMS.Shared.Models;
using PMS.Shared.Services;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using WF.EFCore.Data;
using WF.EFCore.Models;
using WF.EFCore.Services;

namespace PMS.EFCore.Services.Approval
{
    public class EntityFactoryWithWorkflow<T, TListModel, TFilterModel, Ctx,WFCtx>:EntityFactory<T, TListModel, TFilterModel, Ctx>
         where T : class, new()
        where TListModel : class
        where TFilterModel : GeneralPagingFilter
        where Ctx : DbContext
        where WFCtx: WFContext
    {
        private AuthenticationServiceBase _authenticationService;
        private AuthenticationServiceHO _authenticationServiceHO;
        private WFContext _wFContext;
        protected DocumentService _documentService;
        
        protected string _wfDocumentType = "";

        protected List<string> _authorizedUnitIds = new List<string>();
        protected List<string> _authorizeddepartmentIds = new List<string>();
        protected bool _allDepartments = false;
        protected bool _isDocumentTypeAuthorized = false;
        protected bool _docTypeAuthorizationRequired = false;

        readonly IBackgroundTaskQueue _backgroundTaskQueue;
        readonly IEmailSender _emailSender;

        public EntityFactoryWithWorkflow(Ctx context, WFContext wFContext,AuthenticationServiceBase authenticationService, AuthenticationServiceHO authenticationServiceHO, IBackgroundTaskQueue backgroundTaskQueue,IEmailSender emailSender, AuditContext auditContext) : base(context, auditContext)
        {
            _wFContext = wFContext;
            _authenticationService = authenticationService;
            _authenticationServiceHO = authenticationServiceHO;
            _documentService = new DocumentService(_wFContext, authenticationServiceHO,_auditContext);
            _backgroundTaskQueue = backgroundTaskQueue;
            _emailSender = emailSender;
        }

        public override void SetPermission(string userName)
        {
            if (string.IsNullOrWhiteSpace(_wfDocumentType))
                throw new Exception("Jenis dokument belum ditentukan");
            base.SetPermission(userName);            
            if (string.IsNullOrWhiteSpace(userName))
                throw new ExceptionNoAuthorization();

            _authorizedUnitIds = _authenticationService.GetAuthorizedUnitByUserName(userName, string.Empty).Select(d => d.UNITCODE).ToList();            
            _authorizeddepartmentIds = _authenticationService.GetAuthorizedDepartmentIdByUserName(userName, out _allDepartments);
            if (_docTypeAuthorizationRequired)
            {
                _isDocumentTypeAuthorized = _documentService.IsDocumentTypeAuthorized(userName, _wfDocumentType);
                if (!_isDocumentTypeAuthorized)
                    throw new Exception("Anda tidak memiliki akses pada dokumen " + _wfDocumentType);
            }


        }

        public IEnumerable<WorkflowTransactionHistory> WFGetApprovalHistory(long wfDocTransNo, string userName, WFContext wfContext)
        {
            
            Document doc = _documentService.GetSingleByUsername(userName, wfDocTransNo);
            if (doc == null)
                return null;
            return doc.WorkflowTransaction.WorkflowTransactionHistory;
        }

        
        public IEnumerable<WorkflowAction> WFGetNextActions(long wfDocTransNo, string userName, WFContext wfContext)
        {

            
            
            Document doc = _documentService.GetSingleByUsername(userName, wfDocTransNo);
            if (doc == null)
                return null;
            if (doc.WorkflowTransaction == null)
                return null;
            WorkflowService workflowService = new WorkflowService(wfContext, _authenticationServiceHO);
            return workflowService.GetNextActions(doc, userName);
        }

        

        protected virtual T WFBeforeSendApproval(T record, string userName, string actionCode, string approvalNote, bool newRecord)
        {
            return record;
        }
        protected virtual Document WFGenerateDocument(T record,string userName)
        {
            return null;
        }

        protected virtual Document WFUpdateDocument(T record, Document document, string userName, string actionCode, string approvalNote)
        {
            return document;
        }


        public T WFSendApproval(T record, string userName,string actionCode, string approvalNote, bool newRecord)
        {

            if (string.IsNullOrWhiteSpace(_serviceName)) 
                WFError(record, "Invalid service name");                
            if (string.IsNullOrWhiteSpace(_wfDocumentType))
                WFError(record, "Invalid Approval Document Type");            
            if (newRecord)            
                SaveInsert(record, userName);
            
            
            record = WFBeforeSendApproval(record, userName,actionCode,approvalNote,newRecord);

            Document doc=null;
            bool newDocument = false;
            long? wfDocTransNo = null;
            try
            {
                wfDocTransNo = (long?)record.GetPropertyValue<T>("WFDOCTRANSNO");
            }
            catch { }

            if (wfDocTransNo.HasValue)            
                doc = _documentService.GetSingleByUsername(userName, wfDocTransNo.Value);
            if (doc == null)
            {
                doc = WFGenerateDocument(record, userName);
                newDocument = true;
            }
            else
            {
                doc = WFUpdateDocument(record, doc, userName, actionCode, approvalNote);
            }
            if (doc == null)
                WFError(record, "Dokumen approval belum digenerate");

            try
            {
                if (newDocument)
                {
                    doc = _documentService.SaveInsertAndSubmitForApproval(doc, userName, approvalNote);
                    actionCode = "SUBM";
                }
                else
                    doc = _documentService.SendApproval(doc, userName, actionCode, approvalNote);
                SaveAuditTrail(record, userName, $"Approval [{actionCode}]");
            }
            catch(Exception ex)
            {
                string errorMessage = ExceptionMessage.GetAllExceptionMessage(ex);
                WFError(record, errorMessage);
            }
            

            try { SyncronizeWorkflowStatus(record, doc); }
            catch {}

            try 
            {
                var notificationTemplate = _wFContext.DocumentTypeNotificationTemplate.SingleOrDefault(d => d.DocTypeID.Equals(doc.DocType) && d.ActionTypeID.Equals(actionCode));
                if (notificationTemplate != null)
                {

                    WFSendNotification(record, doc, notificationTemplate, userName, actionCode, approvalNote);
                }
            }
            catch { }

            return WFAfterApproval(record, doc, userName,actionCode,approvalNote);
            
        }

        public T WFSendApproval(IFormCollection formData, string userName,  string actionCode, string approvalNote, bool newRecord)
        {
            T record = CopyFromWebFormData(formData, newRecord);
            return WFSendApproval(record, userName, actionCode, approvalNote,newRecord);
        }



        protected virtual T WFAfterApproval(T record,Document document, string userName, string actionCode, string approvalNote)
        {
            return record;
        }

        public IEnumerable<WorkflowAction> GetNextActions(T record, string userName)
        {
            Document doc = null;            
            long? wfDocTransNo = null;
            try
            {
                wfDocTransNo = (long?)record.GetPropertyValue<T>("WFDOCTRANSNO");
            }
            catch { }

            if (wfDocTransNo.HasValue)
                doc = _documentService.GetSingleByUsername(userName, wfDocTransNo.Value);
            if (doc == null)
                return null;
            return _documentService.GetNextActions(doc, userName);
        }

        public IEnumerable<WorkflowTransactionHistory> GetApprovalHistory(T record, string userName)
        {
            Document doc = null;
            long? wfDocTransNo = null;
            try
            {
                wfDocTransNo = (long?)record.GetPropertyValue<T>("WFDOCTRANSNO");
            }
            catch { }

            if (wfDocTransNo.HasValue)
                doc = _documentService.GetSingleByUsername(userName, wfDocTransNo.Value);
            if (doc == null)
                return null;
            return _documentService.GetApprovalHistories(doc, userName);
        }

        public T GetForApproval(long wfTransNo, string userName)
        {
            SetPermission(userName);
            Document doc = _documentService.GetSingleByUsername(userName, wfTransNo);
            if (doc == null)
                throw new Exception("Dokumen approval tidak ditemukan");
            if (!string.IsNullOrWhiteSpace(doc.DepartmentID) && !_allDepartments && !_authorizeddepartmentIds.Contains(doc.DepartmentID))
                throw new Exception("Anda tidak memiliki akses data departemen " + doc.DepartmentID );
            if (!_authorizedUnitIds.Contains(doc.UnitID))
                throw new ExceptionNoUnitAccess(doc.UnitID);
            T record = GetSingleByWorkflow(doc, userName);
            try { SyncronizeWorkflowStatus(record, doc); }
            catch { }

            return record;
        }

        public virtual T GetSingleByWorkflow(Document document, string userName)
        {
            return null;
        }

        private void SyncronizeWorkflowStatus(T record, Document doc)
        {
            
            PropertyInfo[] properties = record.GetType().GetProperties();
            bool docStatus = false, docStatusTex = false, docTransNo = false, docErrorText = false;
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.Name == "WFDOCSTATUS")
                {
                    record.SetPropertyValue(propertyInfo, doc.DocStatus);
                    docStatus = true;
                }
                else if (propertyInfo.Name == "WFDOCSTATUSTEXT")
                {
                    if (doc.DocumentStatus != null)
                        record.SetPropertyValue(propertyInfo, doc.DocumentStatus.Description);
                    docStatusTex = true;
                }
                else if (propertyInfo.Name == "WFDOCTRANSNO")
                {
                    record.SetPropertyValue(propertyInfo, doc.DocTransNo.ToString());
                    docTransNo = true;
                }

                else if (propertyInfo.Name == "WFERRORTEXT")
                {
                    record.SetPropertyValue(propertyInfo, string.Empty);
                    docErrorText = true;
                }

                if (docStatus && docStatusTex && docTransNo && docErrorText)
                    break;
            }

            WFUpdateOriginalRecord(record, doc);
            _context.Update(record);
            _context.SaveChanges();
        }

        private void WFError(T record, string errorMessage)
        {

            PropertyInfo[] properties = record.GetType().GetProperties();            
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.Name == "WFERRORTEXT")
                {
                    record.SetPropertyValue(propertyInfo, errorMessage);
                    _context.Update(record);
                    _context.SaveChanges();
                    break;
                }
            }
            throw new Exception(errorMessage);
            
        }

        public virtual void WFUpdateOriginalRecord(T record, Document document)
        {

        }
        private string GenerateWFApprovalToken(Document document,string urlApproval, bool forApprove, string recipientUserName, string recipientPassword)
        {
            List<Claim> claims = new List<Claim>();
            if (!string.IsNullOrWhiteSpace(urlApproval))
            {
                urlApproval = urlApproval.Replace("{wfdoctransno}", document.DocTransNo.ToString());
                claims.Add(new Claim( "url", urlApproval));
            }
            claims.Add(new Claim("doctype", document.DocumentType.IndexControllerName));
            claims.Add(new Claim("doctransno", document.DocTransNo.ToString()));
            if (forApprove)
                claims.Add(new Claim("forapprove","1"));
            if (!string.IsNullOrWhiteSpace(recipientUserName))
                claims.Add(new Claim("user", recipientUserName));
            if (!string.IsNullOrWhiteSpace(recipientPassword))
                claims.Add(new Claim("password", recipientPassword));
            return JwtTokenRepository.GenerateInsecuredToken(claims, 2 * 24 * 60);//2 Days
        }

        protected virtual void WFSendNotification(T record, Document document, DocumentTypeNotificationTemplate notificationTemplate, string userName, string actionCode, string approvalNote)
        {
            //List<MailMessage> mailMessages = new List<MailMessage>();

            if (notificationTemplate == null)
                return;

            WorkflowAction workflowAction = _wFContext.WorkflowAction.Find(actionCode);
            if (workflowAction == null)
                return;

            
            var lastApprover = _authenticationService.GetUserInfo(userName);
            var lastApproverName = (lastApprover != null && !string.IsNullOrWhiteSpace(lastApprover.NAME)) ? lastApprover.NAME : userName;

            string urlTokenProcessor = document.DocumentType.UITokenProcessorUrl;
            string urlApproval = document.DocumentType.UIApprovalUrl;

            if (string.IsNullOrWhiteSpace(urlTokenProcessor))
                return;

            if (workflowAction.SendNotifToOwner)
            {
                var ownerInfo = _authenticationService.GetUserInfo(document.DocOwner);
                if (ownerInfo != null && StandardUtility.IsValidEmail(ownerInfo.EMAIL))
                {

                    MailMessage mail = new MailMessage();
                    mail.IsBodyHtml = true;
                    mail.Body = notificationTemplate.NotifToOwnerTemplate
                        .Replace("[LASTAPPROVER]", lastApproverName)
                        .Replace("[LINK]", urlTokenProcessor.Replace("{token}",GenerateWFApprovalToken(document,urlApproval, (workflowAction.OwnerNotifType == "A"), ownerInfo.ALIAS, ownerInfo.PASSWORD)))
                        .Replace("[DOCUMENT]", "");
                    mail.To.Add(new MailAddress(ownerInfo.EMAIL,ownerInfo.NAME));
                    mail.Subject = notificationTemplate.NotifToOwnerEmailSubject
                            .Replace("[WF]", "[" + document.DocumentType.Description + "]")
                            .Replace("[DOCNO]", document.Title)
                            .Replace("[USERID]", lastApproverName);
                    //mailMessages.Add(mail);
                    _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
                    {
                        await _emailSender.SendEmailAsync(mail);
                    });

                    
                }
            }

            if (workflowAction.SendNotifToNextApprovers)
            {

                _documentService.GetNextApprovers(document).ToList().ForEach(d => {
                    if (StandardUtility.IsValidEmail(d.VAMUSER.EMAIL))
                    {
                        MailMessage mail = new MailMessage();
                        mail.IsBodyHtml = true;
                        mail.Body = notificationTemplate.NotifToNextApproversTemplate
                            .Replace("[LASTAPPROVER]", lastApproverName)
                            .Replace("[LINK]", urlTokenProcessor.Replace("{token}", GenerateWFApprovalToken(document,urlApproval, (workflowAction.NextApproverNotifType == "A" && d.IsApprover), d.VAMUSER.ALIAS, d.VAMUSER.PASSWORD)))
                            .Replace("[DOCUMENT]", "");
                        mail.To.Add(new MailAddress(d.VAMUSER.EMAIL,d.VAMUSER.NAME));
                        mail.Subject = notificationTemplate.NotifToNextApproversEmailSubject
                            .Replace("[WF]", "[" + document.DocumentType.Description + "]")
                            .Replace("[DOCNO]", document.Title)
                            .Replace("[USERID]", lastApproverName);
                        //mailMessages.Add(mail);
                        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
                        {
                            await _emailSender.SendEmailAsync(mail);
                        });
                        
                    }
                });
                
            }


            if (workflowAction.SendNotifToPrevApprovers)
            {

                _documentService.GetPrevApprovers(document,false).ToList().ForEach(d => {
                    if (StandardUtility.IsValidEmail(d.EMAIL))
                    {
                        MailMessage mail = new MailMessage();
                        mail.IsBodyHtml = true;
                        mail.Body = notificationTemplate.NotifToPrevApproversTemplate
                            .Replace("[LASTAPPROVER]", lastApproverName)
                            .Replace("[LINK]", urlTokenProcessor.Replace("{token}", GenerateWFApprovalToken(document, urlApproval,workflowAction.PrevApproverNotifType == "A", d.ALIAS, d.PASSWORD)))
                            .Replace("[DOCUMENT]", "");
                        mail.To.Add(new MailAddress(d.EMAIL,d.NAME));
                        mail.Subject = notificationTemplate.NotifToPrevApproversEmailSubject
                            .Replace("[WF]", "[" + document.DocumentType.Description + "]")
                            .Replace("[DOCNO]", document.Title)
                            .Replace("[USERID]", lastApproverName);
                        //mailMessages.Add(mail);
                        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
                        {
                            await _emailSender.SendEmailAsync(mail);
                        });
                        //_emailSender.SendEmail(mail);
                    }
                });

            }
            //return mailMessages;
        }

    }
}
