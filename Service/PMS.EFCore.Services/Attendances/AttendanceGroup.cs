using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Attendances
{
    public class AttendanceGroup : EntityFactory<MATTENDANCEGROUP, MATTENDANCEGROUP,GeneralFilter, PMSContextBase>
    {

        private List<MATTENDANCEGROUPITEM> _newAttendanceGroupItems = null;
        private List<MATTENDANCEGROUPITEM> _deletedAttendanceGroupItems = null;
        private List<MATTENDANCEGROUPITEM> _editedAttendanceGroupItems = null;
        private AuthenticationServiceBase _authenticationService;

        public AttendanceGroup(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "AttendanceGroup";
            _authenticationService = authenticationService;
        }

        protected override MATTENDANCEGROUP GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            return _context.MATTENDANCEGROUP
                .Include(d => d.MATTENDANCEGROUPITEM)
                .SingleOrDefault(d => d.ID.Equals(Id));
        }

        public override MATTENDANCEGROUP CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MATTENDANCEGROUP record = base.CopyFromWebFormData(formData, newRecord);//If Required Replace with your custom code
            /*Custom Code - Start*/
            List<MATTENDANCEGROUPITEM> _potentialItems = new List<MATTENDANCEGROUPITEM>();
            List<string> _potentialIds = new List<string>();
            int i = 0;

            //while (true)
            //{
            //    string arrayFieldName = "MATTENDANCEGROUPITEM[" + i.ToString() + "]";
            //    if (string.IsNullOrWhiteSpace(formData[arrayFieldName + "[ID]"]))
            //        break;

            //    DateTime breakstart;
            //    DateTime breakend;
            //    DateTime outtime;

            //    breakstart = DateTime.Parse(formData[arrayFieldName + "[BREAKSTART]"]);
            //    breakend = DateTime.Parse(formData[arrayFieldName + "[BREAKEND]"]);
            //    outtime = DateTime.Parse(formData[arrayFieldName + "[OUTTIME]"]);

            //    MATTENDANCEGROUPITEM mattendancegroupItem = new MATTENDANCEGROUPITEM
            //    {
            //        ID = record.ID,
            //        GROUPID = formData[arrayFieldName + "[GROUPID]"],
            //        DAY = Convert.ToInt16(formData[arrayFieldName + "[DAY]"]),
            //        BREAKSTART = breakstart,
            //        BREAKEND = breakend,
            //        OUTTIME = outtime,
            //        WORKHOUR = Convert.ToInt16(formData[arrayFieldName + "[WORKHOUR]"]),
            //        STATUS = formData[arrayFieldName + "[STATUS]"]
            //    };

            //    _potentialItems.Add(mattendancegroupItem);
            //    _potentialIds.Add(mattendancegroupItem.ID);
            //    i++;
            //}

            //_newAttendanceGroupItems = null;
            //_editedAttendanceGroupItems = null;
            //_deletedAttendanceGroupItems = null;

            //if (newRecord)
            //    _newAttendanceGroupItems = _potentialItems;
            //else if (!_potentialIds.Any())
            //    _deletedAttendanceGroupItems = _context.MATTENDANCEGROUPITEM.Where(x => x.GROUPID.Equals(record.ID)).ToList();
            //else
            //{
            //    var existingItems = _context.MATTENDANCEGROUPITEM.Where(x => x.GROUPID.Equals(record.ID));
            //    var existingItemsId = existingItems.Select(x => x.GROUPID).ToList();

            //    _newAttendanceGroupItems = _potentialItems.Where(o => !existingItemsId.Contains(o.ID)).ToList();
            //    _deletedAttendanceGroupItems = existingItems.Where(o => !_potentialIds.Contains(o.ID)).ToList();
            //    _editedAttendanceGroupItems = _potentialItems.Where(o => existingItemsId.Contains(o.ID)).ToList();

            //}

            /*Custom Code - Here*/

            //upd priyo
            _newAttendanceGroupItems = new List<MATTENDANCEGROUPITEM>();
            _newAttendanceGroupItems.CopyFrom<MATTENDANCEGROUPITEM>(formData, "MATTENDANCEGROUPITEM");

            _saveDetails = _newAttendanceGroupItems.Any();

            return record;
        }

        protected override MATTENDANCEGROUP BeforeSave(MATTENDANCEGROUP record, string userName, bool newRecord)
        {
            /*Custom Code - Here*/

            Validate(record, userName);
            if (newRecord)
            {
                if (_context.MATTENDANCEGROUP.SingleOrDefault(d => d.ID.Equals(record.ID)) != null)
                    throw new Exception("Group Id Absensi sudah terdaftar sebelumnya");
            }

            DateTime now = HelperService.GetServerDateTime(1, _context);
            record.UPDATED = now;
            
            return record;
        }

        private void Validate(MATTENDANCEGROUP record, string userName)
        {

            string result = string.Empty;
            if (string.IsNullOrEmpty(record.ID)) result += "ID Absensi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.NAME)) result += "Nama tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.STATUS)) result += "Status tidak boleh kosong." + Environment.NewLine;


            if (result != string.Empty) throw new Exception(result);

        }

        protected override MATTENDANCEGROUP SaveInsertDetailsToDB(MATTENDANCEGROUP record, string userName)
        {
            
            if (_newAttendanceGroupItems.Any())
                _context.MATTENDANCEGROUPITEM.AddRange(_newAttendanceGroupItems);
            return record;
        }

        

      

        protected override MATTENDANCEGROUP SaveUpdateDetailsToDB(MATTENDANCEGROUP record, string userName)
        {

            //if (_deletedAttendanceGroupItems.Any())
            //    _context.MATTENDANCEGROUPITEM.RemoveRange(_deletedAttendanceGroupItems);
            //if (_newAttendanceGroupItems.Any())
            //    _context.MATTENDANCEGROUPITEM.AddRange(_newAttendanceGroupItems);
            //if (_editedAttendanceGroupItems.Any())
            //    _context.MATTENDANCEGROUPITEM.UpdateRange(_editedAttendanceGroupItems);

            //upd priyo
            if (_newAttendanceGroupItems.Any())
            {
                _context.MATTENDANCEGROUPITEM.RemoveRange(_context.MATTENDANCEGROUPITEM.Where(a => a.GROUPID.Equals(record.ID)));
                _context.MATTENDANCEGROUPITEM.AddRange(_newAttendanceGroupItems);
            }
            return record;
        }

       

        protected override bool DeleteFromDB(MATTENDANCEGROUP record, string userName)
        {
            record = GetSingle(record.ID);
            if (record == null)
                throw new Exception("Record not found");


            DateTime now = HelperService.GetServerDateTime(1, _context);
            record.STATUS = "D";
            record.UPDATED = now;
            _context.Entry<MATTENDANCEGROUP>(record).State = EntityState.Modified;
            Security.Audit.Insert(userName, _serviceName, now, $"Delete {record.ID}", _context);
            _context.SaveChanges();
            return true;
        }

        public override MATTENDANCEGROUP NewRecord(string userName)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);

            MATTENDANCEGROUP record = new MATTENDANCEGROUP
            {
                STATUS = "A",                             
                UPDATED = now
            };

            return record; 
            
        }

        public override IEnumerable<MATTENDANCEGROUP> GetList(GeneralFilter filter)
        {
            
            try
            {
                if (filter.IsActive.HasValue)
                    return _context.MATTENDANCEGROUP.Where(d => d.STATUS == "A").ToList();
                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    return _context.MATTENDANCEGROUP.Where(d => d.ID.ToLower().Contains(filter.LowerCasedSearchTerm) || d.NAME.ToLower().Contains(filter.LowerCasedSearchTerm)).ToList();
                return _context.MATTENDANCEGROUP.ToList();
            }
            catch { return null; }
        }


    }
}
