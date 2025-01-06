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

namespace PMS.EFCore.Services.Organization
{
    public class ICR:EntityFactoryWithWorkflow<TICR,TICR,GeneralFilter, PMSContextBase,WFContext>
    {

        private List<TICRAPP> _newApps = new List<TICRAPP>();
        private List<TICRAPP> _deletedApps = new List<TICRAPP>();
        private List<TICRAPP> _editedApps = new List<TICRAPP>();

        private AuthenticationServiceHO _authenticationService;

        

        public ICR(PMSContextHO context, WFContext wfContext, AuthenticationServiceHO authenticationService, IBackgroundTaskQueue taskQueue,IEmailSender emailSender,AuditContext auditContext):base(context,wfContext,authenticationService,authenticationService, taskQueue,emailSender,auditContext)
        {
            _serviceName = "ICR";
            _wfDocumentType = "ICR";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<TICR> GetList(GeneralFilter filter)
        {
            
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                return null;
            if (string.IsNullOrWhiteSpace(filter.UnitID))
                return null;

            var criteria = PredicateBuilder.True<TICR>();
            criteria = criteria.And(d => d.ICRDATE.Date >= filter.StartDate.Date && d.ICRDATE.Date <= filter.EndDate.Date && d.UNITID.Equals(filter.UnitID));
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                criteria = criteria.And(d => d.DESCRIPTION.ToLower().Contains(filter.LowerCasedSearchTerm));

            if (filter.WFTransNo > 0)
                criteria = criteria.And(d => d.WFDOCTRANSNO.HasValue && d.WFDOCTRANSNO.Value == filter.WFTransNo);

            if (filter.PageSize <= 0)
                return _context.TICR.Where(criteria);
            return _context.TICR.Where(criteria).GetPaged(filter.PageNo,filter.PageSize).Results;
            
        }

        public override TICR CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TICR record = base.CopyFromWebFormData(formData, newRecord);

            /*Custom Code - Start*/
            /*Employee File*/
            List<TICRAPP> _potentialApps = new List<TICRAPP>();
            _potentialApps.CopyFrom<TICRAPP>(formData, "TICRAPP");            
            if (_potentialApps == null || _potentialApps.Count<=0)
                throw new Exception("Application type can not be empty");

            var _potentialAppIds = _potentialApps.Select(d => d.APPID).ToList();
            _newApps = new List<TICRAPP>();
            _editedApps = new List<TICRAPP>();
            _deletedApps = new List<TICRAPP>();

            
            
            if (newRecord)
            {
                _newApps = _potentialApps;                
            }
            else
            {
                if (!_potentialAppIds.Any())
                    _deletedApps = _context.TICRAPP.Where(x => x.ICRNO.Equals(record.ICRNO)).ToList();
                else
                {
                    var existingItems = _context.TICRAPP.Where(x => x.ICRNO.Equals(record.ICRNO));
                    var existingItemsId = existingItems.Select(x => x.APPID).ToList();

                    
                }
            }

            _saveDetails = _editedApps.Any() || _deletedApps.Any() || _newApps.Any();

            /*Custom Code - Here*/

            return record;
        }

        protected override TICR BeforeDelete(TICR record, string userName)
        {
            /*Custom Code - Start*/
            /*Validation before delete existing record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/


            /*Custom Code - Here*/
            return record;
        }

        protected override TICR BeforeSave(TICR record, string userName, bool newRecord)
        {
            /*Custom Code - Start*/
            /*Validation before save existing or new record, throw exception if invalid condition found*/
            /*Example : throw new Exception("Error");*/
            if (string.IsNullOrWhiteSpace(record.TITLE))
                throw new Exception("Title can not be empty");
            if (string.IsNullOrWhiteSpace(record.DESCRIPTION))
                throw new Exception("Description can not be empty");
            if (string.IsNullOrWhiteSpace(record.REASON))
                throw new Exception("Reason can not be empty");

            
            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);

            if (newRecord)
            {
                record.CREATED = currentDate;
                record.CREATEDBY = userName;
                record.ICRDATE = record.CREATED.Date;
                record.ICRNO = record.UNITID + "-" + currentDate.ToString("yyyyMM") + "-" + HelperService.GetCurrentDocumentNumber(_serviceName + record.UNITID, _context).ToString("0000");
            }
            record.UPDATED = currentDate;
            record.UPDATEDBY = userName;

            if (_newApps.Count > 0)
                _newApps.ForEach(d => { d.ICRNO = record.ICRNO; });
            if (_editedApps.Count > 0)
                _editedApps.ForEach(d => { d.ICRNO = record.ICRNO; });
            if (_deletedApps.Count > 0)
                _deletedApps.ForEach(d => { d.ICRNO = record.ICRNO; });

            return record;
        }

        

        protected override TICR SaveInsertDetailsToDB(TICR record, string userName)
        {

            
            
            

            if (_newApps.Any()) 
                _context.TICRAPP.AddRange(_newApps);
            

            return record;
        }

        
        protected override TICR AfterSave(TICR record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(_serviceName + record.UNITID, _context);
            return record;
        }
       

        protected override TICR SaveUpdateDetailsToDB(TICR record, string userName)
        {
            

           
            if (_deletedApps.Any())
                _context.TICRAPP.RemoveRange(_deletedApps);
            if (_newApps.Any())
                _context.TICRAPP.AddRange(_newApps);
            if (_editedApps.Any())
                _context.TICRAPP.UpdateRange(_editedApps);

            return record;
        }

        

        protected override TICR GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            return _context.TICR
                .Include(d => d.TICRAPP)                
                .SingleOrDefault(d => d.ICRNO.Equals(Id));
        }

        

        public override TICR NewRecord(string userName)
        {
            return new TICR
            {
                CREATED = DateTime.Today,
                ICRDATE = DateTime.Today,
                WFDOCSTATUS = "0",
                WFDOCSTATUSTEXT = "Created"
            };
        }

        protected override Document WFGenerateDocument(TICR record, string userName)
        {
            Document document = new Document
            {
                DocDate = DateTime.Today,
                Description = "ICR of " + record.UNITID + " - " + record.TITLE,
                DocType = _wfDocumentType,
                UnitID = record.UNITID,
                DocOwner = userName,
                DocStatus = "",
                WFFlag = "",
                Title = "ICR of " + record.UNITID + " - " + record.TITLE
            };
            return document;
        }



        public override TICR GetSingleByWorkflow(Document document, string userName)
        {
            return _context.TICR
                .Include(d => d.TICRAPP)                
                .SingleOrDefault(d => d.WFDOCTRANSNO.Value == document.DocTransNo);
        }


        private void BeforeDevelopment(TICR record)
        {
            if (!record.DEVENDPLAN.HasValue || !record.DEVSTARTPLAN.HasValue)
                throw new Exception("Development date plan can not be empty");

        }

        private void BeforeUAT(TICR record)
        {
            if (!record.UATSTARTPLAN.HasValue || !record.UATENDPLAN.HasValue)
                throw new Exception("UAT date plan can not be empty");

        }

        private void BeforeDeployment(TICR record)
        {
            if (!record.DEPLOYSTARTPLAN.HasValue || !record.DEPLOYENDPLAN.HasValue)
                throw new Exception("Deployment date plan can not be empty");

        }

        private void BeforeExecution(TICR record)
        {
            if (!record.ACTIONSTARTPLAN.HasValue || !record.ACTIONENDPLAN.HasValue)
                throw new Exception("Execution date plan can not be empty");

        }

        private void BeforeRN(TICR record)
        { 
            if (string.IsNullOrWhiteSpace(record.RNNO))
                throw new Exception("RN no. can not be empty");
            if (string.IsNullOrWhiteSpace(record.RN))
                throw new Exception("Release notes can not be empty");
        }

        private void BeforeModeration(TICR record)
        {
            if (string.IsNullOrWhiteSpace(record.ACTIONTYPE))            
                throw new Exception("Please specify action type");
            
            if (record.PRIORITYLEVEL <= 0)
                throw new Exception("Please specify priority level");
            if (record.IMPACTLEVEL <= 0)
                throw new Exception("Please specify impact level");
            
        }

        private void BeforeFinalApprove(TICR record)
        {
            if (record.PRIORITYLEVEL <= 0)
                throw new Exception("Please specify priority level");
            if (record.IMPACTLEVEL <= 0)
                throw new Exception("Please specify impact level");
        }
        protected override TICR WFBeforeSendApproval(TICR record, string userName, string actionCode, string approvalNote,bool newRecord)
        {
            switch(actionCode)
            {
                case "ICRASSIGN":                    
                    BeforeModeration(record);
                    break;
                //case "ICRMD":
                //    record.ACTIONTYPE = "MD";
                //    BeforeModeration(record);
                //    break;
                //case "ICRAUTH":
                //    record.ACTIONTYPE = "AUTH";
                //    BeforeModeration(record);
                //    break;
                //case "ICRHD":
                //    record.ACTIONTYPE = "HD";
                //    BeforeModeration(record);
                //    break;
                //case "ICRSAP":
                //    record.ACTIONTYPE = "SAP";
                //    BeforeModeration(record);
                //    break;
                //case "ICRPMS":
                //    record.ACTIONTYPE = "PMS";
                //    BeforeModeration(record);
                //    break;
                //case "ICRINFRA":
                //    record.ACTIONTYPE = "INFRA";
                //    BeforeModeration(record);
                //    break;
                case "FAPV":
                    BeforeFinalApprove(record);                    
                    break;                
                case "SDEV":
                    BeforeDevelopment(record);
                    record.DEVSTARTACTUAL = DateTime.Today;                    
                    break;
                case "FDEV":
                    BeforeDevelopment(record);
                    if (!record.DEVSTARTACTUAL.HasValue)
                        throw new Exception("Please start development before finishing");
                    record.DEVENDACTUAL = DateTime.Today;
                    break;
                case "UDEV":
                    BeforeDevelopment(record);
                    if (!record.DEVSTARTACTUAL.HasValue)
                        throw new Exception("Please start development before updating");
                    break;
                case "SUAT":
                    BeforeUAT(record);
                    record.UATSTARTACTUAL = DateTime.Today;
                    break;
                case "FUAT":
                    BeforeUAT(record);
                    if (!record.UATSTARTACTUAL.HasValue)
                        throw new Exception("Please start UAT before finishing");
                    record.UATENDACTUAL = DateTime.Today;
                    break;
                case "UUAT":
                    BeforeUAT(record);
                    if (!record.UATSTARTACTUAL.HasValue)
                        throw new Exception("Please start UAT before updating");
                    break;
                case "RN":
                    BeforeRN(record);
                    record.RNDATE = DateTime.Today;
                    break;
                case "SDEP":
                    BeforeDeployment(record);
                    record.DEPLOYSTARTACTUAL = DateTime.Today;
                    break;
                case "FDEP":
                    BeforeDeployment(record);
                    if (!record.DEPLOYSTARTACTUAL.HasValue)
                        throw new Exception("Please start deployment before finishing");
                    record.DEPLOYENDACTUAL = DateTime.Today;
                    break;
                case "UDEP":
                    BeforeDeployment(record);
                    if (!record.DEPLOYSTARTACTUAL.HasValue)
                        throw new Exception("Please start deployment before updating");
                    break;
                case "SEXC":
                    BeforeExecution(record);
                    record.ACTIONSTARTACTUAL = DateTime.Today;
                    break;
                case "FEXC":
                    BeforeExecution(record);
                    if (!record.ACTIONSTARTACTUAL.HasValue)
                        throw new Exception("Please start execution before finishing");
                    record.ACTIONENDACTUAL = DateTime.Today;
                    break;
                case "UEXC":
                    BeforeExecution(record);
                    if (!record.ACTIONSTARTACTUAL.HasValue)
                        throw new Exception("Please start execution before updating");
                    break;
            }
            return base.WFBeforeSendApproval(record, userName, actionCode, approvalNote,newRecord);
        }

        protected override Document WFUpdateDocument(TICR record, Document document, string userName, string actionCode, string approvalNote)
        {
            switch (actionCode)
            {
                case "ICRASSIGN":
                    document.WFFlag = record.ACTIONTYPE;
                    break;
                //case "ICRMD":
                //    document.WFFlag = "MD";
                //    break;
                //case "ICRAUTH":
                //    document.WFFlag = "AUTH";
                //    break;
                //case "ICRHD":
                //    document.WFFlag = "HD";
                //    break;
                //case "ICRSAP":
                //    document.WFFlag = "SAP";
                //    break;
                //case "ICRPMS":
                //    document.WFFlag = "PMS";
                //    break;
                //case "ICRINFRA":
                //    document.WFFlag = "INFRA";
                //    break;
            }
            return document;
        }

       
    }
}
