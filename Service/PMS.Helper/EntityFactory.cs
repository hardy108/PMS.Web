using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PMS.EFCore.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PMS.Shared.Utilities;
using PMS.Shared.Models;

namespace PMS.EFCore.Helper
{
    public class EntityFactory<T, TListModel, TFilterModel, Ctx>
        where T : class, new()
        where TListModel : class
        where TFilterModel : GeneralPagingFilter
        where Ctx : DbContext
    {
        protected string _serviceName = "";
        
        protected Ctx _context;
        protected List<string> _primaryKeyNames = new List<string>();
        private object[] _keyValues = null;
        protected T _existingRecord = null;
        protected bool _saveDetails = false;
        protected bool _useTransaction = false;
        protected IDbContextTransaction _transaction;
        protected bool _saveAuditTrail = true;
        protected bool _internalCommit = true;
        protected string _autoNumberPrefix = "";
        
        private string _userName = string.Empty;
        protected AuditContext _auditContext;
        public EntityFactory(Ctx context,AuditContext auditContext)
        {
        
            _context = context;
            _auditContext = auditContext;
            LoadPrimaryKeyNames();
        }


        public string CurrentUserName
        { get { return _userName; } }

        public virtual void SetPermission(string userName)
        {
            _userName = userName;

        }

        public virtual long GetListCount(TFilterModel filter)
        {
            filter.PageSize = 0;
            try
            {
                return GetList(filter).LongCount();
            }
            catch { return 0; }
        }

        protected virtual void LoadPrimaryKeyNames()
        {
            if (_primaryKeyNames == null || _primaryKeyNames.Count <= 0)
            {
                _primaryKeyNames = _context.FindPrimaryKeyNames<T>((T)Activator.CreateInstance(typeof(T))).ToList();
            }
        }

        //Before
        #region Before Functions
        protected virtual T BeforeSave(T record, string userName, bool newRecord)
        {
            return record;
        }
       

        protected virtual void BeforeSaveUpdateMultiple(IEnumerable<T> records, string userName)
        {

        }

        protected virtual void BeforeSaveInsertMultiple(IEnumerable<T> records, string userName)
        {

        }

        protected virtual void AfterSaveUpdateMultiple(IEnumerable<T> records, string userName)
        {

        }

        protected virtual void AfterSaveInsertMultiple(IEnumerable<T> records, string userName)
        {

        }

        

        protected virtual T BeforeDelete(T record, string userName)
        {
            return record;
        }

        protected virtual void BeforeDeleteMultiple(IEnumerable<T> records, string userName)
        {

        }

        protected virtual void AfterDeleteMultiple(IEnumerable<T> records, string userName)
        {

        }

       
        #endregion

        //After
        #region After Functions
        protected virtual T AfterSave(T record, string userName, bool newRecord)
        {
            return record;
        }
        //protected virtual T AfterSaveInsert(T record, string userName)
        //{
        //    return record;
        //}
        //protected virtual T AfterSaveUpdate(T record, string userName)
        //{
        //    return record;
        //}
        protected virtual T AfterDelete(T record, string userName)
        {
            return record;
        }
        #endregion

        #region Connection
        public virtual void SetConnection(Ctx context)
        {
            _context = context;
        }
        public virtual void CloseConnection()
        {
            try
            {
                if (_context != null)
                    _context.Dispose();
                _context = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public virtual T CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            T record = (T)Activator.CreateInstance(typeof(T));
            if (!newRecord)
            {
                
                record.CopyFrom(formData, _primaryKeyNames);
                record = GetSingleFromDB(record);
                if (record == null)
                    throw new Exception("Record not found");

            }
            record.CopyFrom(formData);
            LoadPrimaryKeyNames();
            _keyValues = record.FindPrimaryKeyValues(_primaryKeyNames).ToArray();
            return record;
        }



        public virtual IEnumerable<TListModel> GetList(TFilterModel filter)
        {
            //Must implemented in inherited class, otherwise throw error
            throw new Exception("Non Implemented");
        }

        public virtual void DeleteMultiple(TFilterModel filter, string userName, bool bulkDelete)
        {

            List<TListModel> deletedCandidateRecords = GetList(filter).ToList();
            if (deletedCandidateRecords == null || !deletedCandidateRecords.Any())
                return;

            List<T> deletedRecords = new List<T>();
            if (typeof(T) == typeof(TListModel))
            {
                deletedCandidateRecords.ForEach(d =>
                {
                    deletedRecords.Add(d as T);
                });
            }
            else
            {
                deletedCandidateRecords.ForEach(d =>
                {
                    T record = new T();
                    record.CopyFrom(d);
                    deletedRecords.Add(record);
                });
            }

            DeleteMultiple(deletedRecords, userName, bulkDelete);
        }

        public virtual void DeleteMultiple(IEnumerable<T> records, string userName, bool bulkDelete)
        {
            BeforeDeleteMultiple(records, userName);
            if (bulkDelete)
                _context.RemoveRange(records);
            else
            {
                records.ToList().ForEach(d => {
                    Delete(d, userName);
                });
            }
            if (_internalCommit)
                _context.SaveChanges();
            AfterDeleteMultiple(records, userName);
        }

        public virtual void SaveUpdateMultiple(IEnumerable<T> records, string userName, bool bulkUpdate)
        {
            BeforeSaveUpdateMultiple(records, userName);
            if (bulkUpdate)
                _context.UpdateRange(records);
            else
            {
                records.ToList().ForEach(d => {
                    SaveUpdate(d, userName);
                });
            }
            if (_internalCommit)
                _context.SaveChanges();
            AfterSaveUpdateMultiple(records, userName);
        }

        public virtual void SaveInsertMultiple(IEnumerable<T> records, string userName, bool bulkInsert)
        {
            BeforeSaveInsertMultiple(records, userName);
            if (bulkInsert)
                _context.AddRange(records);
            else
            {
                records.ToList().ForEach(d => {
                    SaveInsert(d, userName);
                });
            }
            if (_internalCommit)
                _context.SaveChanges();
            AfterSaveInsertMultiple(records, userName);
        }


        protected virtual T GetSingleFromDB(T record)
        {
            LoadPrimaryKeyNames();
            object[] keyValues = record.FindPrimaryKeyValues(_primaryKeyNames).ToArray();
            return GetSingleFromDB(keyValues);
        }

        private T GetSingleFromDBBase(params object[] keyValues)
        {
            try
            {
                T record = _context.Find<T>(keyValues);
                _context.Entry<T>(record).State = EntityState.Detached;
                return record;
            }
            catch { return null; }

        }

        protected virtual T GetSingleFromDB(params object[] keyValues)
        {
            return GetSingleFromDBBase(keyValues);

        }


        public T GetSingle(T record, string userName)
        {
            
            _userName = userName;
            return GetSingleFromDB(record);
        }

        public T GetSingle(T record)
        {
            return GetSingle(record,string.Empty);
        }

        public T GetSingle(params object[] keyValues)
        {
            return GetSingleByUsername(string.Empty, keyValues);
        }

        public T GetSingleByUsername(string userName, params object[] keyValues)
        {
            
            SetPermission(userName);
            return GetSingleFromDB(keyValues);
        }



        public virtual T NewRecord(string userName)
        {
            T record = (T)Activator.CreateInstance(typeof(T));
            return record;
        }

        public T SaveInsert(IFormCollection formDataCollection, string userName)
        {
            return SaveInsert(CopyFromWebFormData(formDataCollection, true), userName);
        }

        public T SaveInsertOrUpdate(T record, string userName)
        {
            if (GetSingleFromDB(record) == null)
                return SaveInsert(record, userName);
            return SaveUpdate(record, userName);
        }


        public T SaveInsert(T record, string userName)
        {
            if (string.IsNullOrWhiteSpace(_serviceName))
                throw new Exception("Invalid service name");


            if (_useTransaction)
                _transaction = _context.Database.BeginTransaction();
            try
            {
                
                SetPermission(userName);
                //record = BeforeSaveInsert(record, userName);
                record = BeforeSave(record, userName,true);
                LoadPrimaryKeyNames();
                _keyValues = record.FindPrimaryKeyValues(_primaryKeyNames).ToArray();
                _existingRecord = GetSingleFromDBBase(_keyValues);
                if (_existingRecord != null)
                    throw new Exception("Duplicate record");

                T newrecord = SaveInsertToDB(record, userName);
                newrecord = AfterSave(newrecord, userName,true);
                //newrecord = AfterSaveInsert(newrecord, userName);
                if (_useTransaction)
                    _transaction.Commit();
                SaveAuditTrail(userName, "Insert Record", newrecord.FindPrimaryKeyValues(_primaryKeyNames).ToArray());
                return newrecord;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        protected virtual T SaveInsertDetailsToDB(T record, string userName)
        {

            return record;
        }
        protected virtual T SaveInsertToDB(T record, string userName)
        {


            try
            {
                var entry = _context.Entry<T>(record);
                entry.State = Microsoft.EntityFrameworkCore.EntityState.Added;
                if (_saveDetails)
                    SaveInsertDetailsToDB(record, userName);
                if (_internalCommit)
                    _context.SaveChanges();
                return entry.Entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public T SaveUpdate(IFormCollection formDataCollection, string userName)
        {

            return SaveUpdate(CopyFromWebFormData(formDataCollection, false), userName);
        }
        public T SaveUpdate(T record, string userName)
        {

            if (string.IsNullOrWhiteSpace(_serviceName))
                throw new Exception("Invalid service name");



            if (_useTransaction)
                _transaction = _context.Database.BeginTransaction();
            try
            {
                //Capture Existing Record Before Update            
                
                SetPermission(userName);
                LoadPrimaryKeyNames();
                _keyValues = record.FindPrimaryKeyValues(_primaryKeyNames).ToArray();
                _existingRecord = GetSingleFromDBBase(_keyValues);
                if (_existingRecord == null)
                    throw new Exception("Record not found");

                //record = BeforeSaveUpdate(record, userName);
                record = BeforeSave(record, userName,false);
                T updatedRecord = SaveUpdateToDB(record, userName);
                //updatedRecord = AfterSaveUpdate(updatedRecord, userName);
                updatedRecord = AfterSave(updatedRecord, userName,false);
                if (_useTransaction)
                    _transaction.Commit();
                LoadPrimaryKeyNames();
                SaveAuditTrail(userName, "Edit Record", updatedRecord.FindPrimaryKeyValues(_primaryKeyNames).ToArray());
                return updatedRecord;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        protected virtual T SaveUpdateDetailsToDB(T record, string userName)
        {
            return record;
        }

        protected virtual T SaveUpdateToDB(T record, string userName)
        {

            try
            {

                var entry = _context.Entry<T>(record);
                entry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                if (_saveDetails)
                    SaveUpdateDetailsToDB(record, userName);
                if (_internalCommit)
                    _context.SaveChanges();
                return entry.Entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public bool Delete(IFormCollection formDataCollection, string userName)
        {

            return Delete(CopyFromWebFormData(formDataCollection, false), userName);
        }
        public bool Delete(T record, string userName)
        {

            if (string.IsNullOrWhiteSpace(_serviceName))
                throw new Exception("Invalid service name");
            LoadPrimaryKeyNames();
            _keyValues = record.FindPrimaryKeyValues(_primaryKeyNames).ToArray();
            return Delete(userName, _keyValues);

        }

        public bool Delete(string userName, params object[] keyValues)
        {
            
            SetPermission(userName);
            _existingRecord = GetSingleFromDBBase(keyValues);
            if (_existingRecord == null)
                throw new Exception("Record not found");
            bool result = false;
            T record = new T();
            record.CopyFrom(_existingRecord);
            if (_useTransaction)
                _transaction = _context.Database.BeginTransaction();
            try
            {
                record = BeforeDelete(record, userName);
                result = DeleteFromDB(record, userName);
                record = AfterDelete(record, userName);
                if (_useTransaction)
                    _transaction.Commit();
                SaveAuditTrail(userName, "Delete Record", keyValues);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        

        protected virtual bool DeleteDetailsFromDB(T record, string userName)
        {
            return true;
        }
        protected virtual bool DeleteFromDB(T record, string userName)
        {

            try
            {
                if (_saveDetails)
                {
                    if (!DeleteDetailsFromDB(record, userName))
                        return false;
                }
                var entry = _context.Entry<T>(record);
                entry.State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                if (_internalCommit)
                    _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void CommitAllChanges()
        {
            _context.SaveChanges();
        }

        


        protected DateTime GetServerTime()
        {

            return Utility.GetServerTime(_context);
        }

        protected string GetMConfigValue(string name)
        {
            
            return Utility.GetMConfigValue(name, _context);
        }

        protected void SetMConfigValue(string name, string value)
        {
            Utility.SetMConfigValue(name, value, _context);
        }

        

        protected virtual void SaveAuditTrail(T record, string userName, string action)
        {
            LoadPrimaryKeyNames();
            SaveAuditTrail(userName, action, record.FindPrimaryKeyValues(_primaryKeyNames).ToArray());
        }

        protected virtual void SaveAuditTrail(string userName, string action, params object[] keyValues)
        {
            

            string keyText = string.Empty;
            try
            {


                if (keyValues != null)
                {
                    if (keyValues.Length > 0)
                    {
                        foreach (var keyValue in keyValues)
                        {
                            keyText += $" {keyValue}";
                        }
                        action += keyText;
                    }

                }
            }
            catch { }


            _auditContext.SaveAuditTrail(userName, _serviceName, action,GetServerTime());
        }

        protected int GetCurrentDocumentNumber()
        {

            return Utility.GetCurrentDocumentNumber(_autoNumberPrefix, _context);
        }

        protected int GetCurrentDocumentNumber(string code)
        {

            return Utility.GetCurrentDocumentNumber(code, _context);
        }

        

        protected void IncreaseRunningNumber()
        {
            Utility.IncreaseRunningNumber(_autoNumberPrefix, _context);
        }

        protected void IncreaseRunningNumber(string code)
        {
            Utility.IncreaseRunningNumber(code, _context);
        }

        
    }







}
