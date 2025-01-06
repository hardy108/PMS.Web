using System;
using System.Collections.Generic;
using System.Text;
using WF.EFCore.Models;
using WF.EFCore.Models.Filters;
using WF.EFCore.Data;
using PMS.EFCore.Helper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PMS.Shared.Utilities;
using Microsoft.IdentityModel.Tokens;
using AM.EFCore.Models;
using AM.EFCore.Services;
using PMS.Shared.Models;
using PMS.EFCore.Model;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;

namespace WF.EFCore.Services
{
    public class DocumentService : EntityFactory<Document, vwDocument, FilterWorkflow, WFContext>
    {
        private AuthenticationServiceHO _authenticationService;
        private WorkflowService _WorkflowService;
        
        const string AMDOCTYPE = "WF.DOCTYPE";
        const string AMWFAPPROVER = "WF.Approve";
        const string AMACTIVITY = "WF.ACTIVITIES";
        const string FIRSTACTION = "SUBM";

        private List<string> _authorizedDepartmentIds = new List<string>();
        private bool _allDepartment = false;
        private List<string> _authorizedUnitIds = new List<string>();
        private bool _allUnits = false;
        private List<string> _authorizedDocTypeIds = new List<string>();

        public DocumentService(WFContext context, AuthenticationServiceHO authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Document";
            _authenticationService = authenticationService;
            _WorkflowService = new WorkflowService(_context,authenticationService);
        }

        

        public override IEnumerable<vwDocument> GetList(FilterWorkflow filter)
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
            if (unitIds.Any())
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

        private string GenerateAutoNumber(DocumentType documentType, string unitID, DateTime documentDate)
        {

            string result = string.Empty;
            int sequenceNo = 0;
            try
            {
                try
                {
                    var criteria = PredicateBuilder.True<AutoNumberHistory>();
                    if (documentType.ANUnitPartition)
                        criteria = criteria.And(d => d.UnitID.Equals(unitID));

                    switch (documentType.ANPeriodPartition)
                    {
                        case "Y":
                            criteria = criteria.And(d => d.DocDate.Year == documentDate.Year);
                            //sequenceNo = _context.AutoNumberHistory
                            //    .Where(d => d.DocTypeID.Equals(documentType.DocTypeID) 
                            //        && d.UnitID == (documentType.ANUnitPartition ? unitID : d.UnitID) 
                            //        && d.DocDate.Year == documentDate.Year)
                            //    .Max(d => d.SequenceNo);
                            break;
                        case "M":
                            criteria = criteria.And(d => d.DocDate.Year == documentDate.Year && d.DocDate.Month == documentDate.Month);
                            //sequenceNo = _context.AutoNumberHistory
                            //    .Where(d => d.DocTypeID.Equals(documentType.DocTypeID) 
                            //        && d.UnitID == (documentType.ANUnitPartition ? unitID : d.UnitID) 
                            //        && d.DocDate.Year == documentDate.Year && d.DocDate.Month == documentDate.Month)
                            //.Max(d => d.SequenceNo);
                            break;
                        case "D":
                            criteria = criteria.And(d => d.DocDate.Year == documentDate.Year && d.DocDate.Month == documentDate.Month && d.DocDate.Day == documentDate.Day);
                            //sequenceNo = _context.AutoNumberHistory
                            //    .Where(d => d.DocTypeID.Equals(documentType.DocTypeID) 
                            //        && d.UnitID == (documentType.ANUnitPartition ? unitID : d.UnitID) 
                            //        && d.DocDate.Year == documentDate.Year 
                            //        && d.DocDate.Date == documentDate.Date)
                            //    .Max(d => d.SequenceNo);
                            break;
                    }

                    criteria = criteria.And(d => d.DocTypeID.Equals(documentType.DocTypeID));

                    var authoNumberHistory = _context.AutoNumberHistory
                                .Where(criteria);
                    if (authoNumberHistory.Count() >= 0)
                        sequenceNo = authoNumberHistory.Max(d => d.SequenceNo);

                }
                catch { }
                sequenceNo++;

                string period = documentDate.ToString(documentType.ANPeriodFormat);
                string number = sequenceNo.ToString(documentType.ANNumberFormat);
                result = documentType.ANFormat.Replace("[PREFIX]", documentType.ANPrefix)
                                    .Replace("[POSTFIX]", documentType.ANPostFix)
                                    .Replace("[UNIT]", unitID)
                                    .Replace("[PERIOD]", period)
                                    .Replace("[NUMBER]", number);
                AutoNumberHistory autoNumberHistory = new AutoNumberHistory();
                autoNumberHistory.DocTypeID = documentType.DocTypeID;
                autoNumberHistory.DocDate = documentDate;
                autoNumberHistory.UnitID = unitID;
                autoNumberHistory.DocNo = result;
                autoNumberHistory.SequenceNo = sequenceNo;
                _context.AutoNumberHistory.Add(autoNumberHistory);
            }
            catch (Exception ex)
            {
            }
            return result;
        }



        

        public override void SetPermission(string userName)
        {
            base.SetPermission(userName);
            if (string.IsNullOrWhiteSpace(userName))
                throw new ExceptionNoAuthorization();
            _authorizedDepartmentIds = _authenticationService.GetAuthorizedDepartmentIdByUserName(CurrentUserName,out _allDepartment);
            _authorizedUnitIds = _authenticationService.GetAuthorizedUnitByUserName(CurrentUserName,string.Empty).Select(d => d.UNITCODE).ToList();
            _authorizedDocTypeIds = _authenticationService.GetPermissionMatrix(CurrentUserName, AMDOCTYPE, string.Empty)
                                    .Join(_context.DocumentType, a => a.PermissionDetails, b => b.AMAlias, (a, b) => new { DocumentType = b })
                                    .Select(a => a.DocumentType.DocTypeID).Distinct().ToList();
        }
        protected override Document BeforeSave(Document record, string userName, bool newRecord)
        {
            if (!_authorizedUnitIds.Contains(record.UnitID))
                throw new ExceptionNoUnitAccess(record.UnitID);
            if (!_authorizedDocTypeIds.Contains(record.DocType))
                throw new Exception("Anda tidak memiliki access ke dokumen " + record.DocType);
            if (!string.IsNullOrWhiteSpace(record.DepartmentID) && !_allDepartment && !_authorizedDepartmentIds.Contains(record.DepartmentID))
                throw new ExceptionNoUnitAccess(record.DepartmentID);

            if (string.IsNullOrWhiteSpace(record.Title))
                throw new Exception("Judul dokumen tidak boleh kosong");
            if (newRecord)
            {
                DocumentType documentType = _context.DocumentType.Find(record.DocType);
                record.DocNo = GenerateAutoNumber(documentType, record.UnitID, record.DocDate);
            }

            record.LastUpdateDate = GetServerTime();
            record.LastUpdateUser = userName;
            return base.BeforeSave(record, userName,newRecord);
        }

        protected override Document BeforeDelete(Document record, string userName)
        {
            if (!_authorizedUnitIds.Contains(record.UnitID))
                throw new ExceptionNoUnitAccess(record.UnitID);
            if (!_authorizedDocTypeIds.Contains(record.DocType))
                throw new Exception("Anda tidak memiliki access ke dokumen " + record.DocType);
            if (!string.IsNullOrWhiteSpace(record.DepartmentID) && !_allDepartment && !_authorizedDepartmentIds.Contains(record.DepartmentID))
                throw new ExceptionNoUnitAccess(record.DepartmentID);
            return base.BeforeDelete(record, userName);
        }



        protected override Document GetSingleFromDB(params object[] keyValues)
        {
            
            if (keyValues == null)
                throw new Exception("Invalid document");
            if (string.IsNullOrWhiteSpace(CurrentUserName))
                throw new ExceptionNoAuthorization();
            
            long transNo = (long)keyValues[0];
            var doc = _context.Document
                        .Include(d => d.WorkflowTransaction)
                        .Include(d => d.DocumentType)
                        .Include(d => d.DocumentType.Workflow)
                        .Include(d => d.DocumentType.Workflow.WorkflowActivity)
                        .Include(d => d.WorkflowTransaction)
                        .Include(d => d.WorkflowTransaction.WorkflowTransactionHistory)
                        .Include(d => d.DocumentStatus)
                        .SingleOrDefault(d => d.DocTransNo == transNo);
            if (doc!=null)
                _context.Entry(doc).State = EntityState.Detached;
            return doc;
        }

        


        
        private IQueryable<vwDocument> GetDraftByUserName(string userName)
        {
            return
            (from a in _context.vwDocument
             join f in _context.DocumentType on a.DocType equals f.DocTypeID
             join c in _authenticationService.GetAuthorizedUnitByUserName(userName, string.Empty) on a.UnitID equals c.UNITCODE
             join g in _authenticationService.GetPermissionMatrix(userName, AMDOCTYPE, string.Empty) on f.AMAlias equals g.PermissionDetails
             where a.WorkflowTransactionNo == null || a.DocStatus == "50"
             select a).Distinct();
        }


        public Document SaveInsertAndSubmitForApproval(Document record, string userName,string approvalNote)
        {
            
            SaveInsert(record, userName);
            record = GetSingleByUsername(userName, record.DocTransNo);
            return SendApproval(record, userName,FIRSTACTION,approvalNote);
        }

        public Document SendApproval(Document record, string userName, string actionCode, string approvalNote)
        {
            _WorkflowService.SendApproval(record, userName, actionCode, approvalNote);
            return GetSingleByUsername(userName, record.DocTransNo);
        }

        public Document SendApproval(long wfDocTransNo, string userName, string actionCode, string approvalNote)
        {
            Document record = GetSingleByUsername(userName, wfDocTransNo);
            return SendApproval(record, userName, actionCode, approvalNote);
        }

        public IEnumerable<WorkflowAction> GetNextActions(Document record, string userName)
        {
            if (record == null)
                return null;
            if (record.WorkflowTransaction == null)
                record = GetSingleByUsername(userName, record);
            if (record == null)
                return null;
            if (record.WorkflowTransaction == null)
            {
                List<string> activityIds = new List<string>();
                
                string wfId = record.DocumentType.WorkflowID;
                return
                    from a in _context.WorkflowActivity
                    join b in _context.WorkflowRoute on a.ActivityID equals b.ActivityID
                    join c in _context.WorkflowAction on b.ActionTypeID equals c.ActionTypeID
                    join d in _authenticationService.GetPermissionMatrix(userName, string.Empty, string.Empty) on a.AMAlias equals d.PermissionDetails
                    where a.ActivityType == "S" && a.WorkflowID.Equals(wfId)
                    select c;
            }
            return _WorkflowService.GetNextActions(record, userName);
        }

        public IEnumerable<WorkflowAction> GetNextActions(long docTransNo, string userName)
        {
            var doc = GetSingleByUsername(userName, docTransNo);
            return GetNextActions(doc,userName);
        }

        public IEnumerable<WorkflowTransactionHistory> GetApprovalHistories(Document record, string userName)
        {
            if (record == null)
                return null;
            if (record.WorkflowTransaction == null)
                record = GetSingleByUsername(userName, record);
            if (record == null)
                return null;
            if (record.WorkflowTransaction == null)
                return null;
            return record.WorkflowTransaction.WorkflowTransactionHistory.Where(d => !string.IsNullOrWhiteSpace(d.ActionTypeID));
        }

        public IEnumerable<WFAPPROVER> GetNextApprovers(Document record)
        {
            if (record == null)
                return null;
            if (record.WorkflowTransaction == null)
                record = GetSingle(record);
            if (record == null)
                return null;
            if (record.WorkflowTransaction == null)
                return null;
            return _WorkflowService.GetNextApprovers(record);
        }

        public IEnumerable<VAMUSER> GetPrevApprovers(Document record,bool includeOwner)
        {
            if (record == null)
                return null;
            if (record.WorkflowTransaction == null)
                record = GetSingle(record);
            if (record == null)
                return null;
            if (record.WorkflowTransaction == null)
                return null;
            return _WorkflowService.GetPrevApprovers(record,includeOwner,string.Empty);
        }

        public IEnumerable<WorkflowTransactionHistory> GetApprovalHistories(long docTransNo, string userName)
        {
            var doc = GetSingleByUsername(userName, docTransNo);
            return GetApprovalHistories(doc, userName);
        }

        public bool IsDocumentTypeAuthorized(string userName, string docTypeId)
        {
            return
            _authenticationService.GetPermissionMatrix(userName, AMDOCTYPE, string.Empty)
                                    .Join(_context.DocumentType.Where(d => d.DocTypeID.Equals(docTypeId)), a => a.PermissionDetails, b => b.AMAlias, (a, b) => new { DocumentType = b })
                                    .Select(a => a.DocumentType.DocTypeID).Count() > 0;
        }


        public IEnumerable<DocumentType> GetDocumentTypeAuthorized(string userName)
        {
            return
            _authenticationService.GetPermissionMatrix(userName, AMDOCTYPE, string.Empty)
                                    .Join(_context.DocumentType, a => a.PermissionDetails, b => b.AMAlias, (a, b) => new { DocumentType = b })
                                    .Select(a => a.DocumentType);
        }
    }
}
