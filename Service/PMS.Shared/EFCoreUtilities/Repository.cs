using Microsoft.EntityFrameworkCore;
using PMS.Shared.Exceptions;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using PMS.Shared.EFCoreUtilities;

namespace PMS.Shared.EFCoreUtilities
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> Get(Expression<Func<T, bool>> predicate);




        T GetOne(Expression<Func<T, bool>> predicate);
        T GetOneByKey(params object[] keyValues);

        void Insert(IEnumerable<T> entities);
        void InsertOne(T entity);

        void Delete(Expression<Func<T, bool>> predicate);
        void Delete(IEnumerable<T> entities);
        void DeleteOneByKey(params object[] keyValues);
        void DeleteOne(T entity);

        void Update(IEnumerable<T> entities);
        void UpdateOne(T entity);
        void UpdateOneByKey(T entity, params object[] keyValues);

        List<string> GetPrimaryKeyNames();
        object[] GetPrimaryKeyValues(T entity);
    }

    public class Repository<T, Ctx> : IRepository<T>
        where T : class
        where Ctx : DbContext
    {
        protected List<string> _primaryKeyNames = new List<string>();
        private readonly Ctx _dbContext;
        public Repository(Ctx dbContext)
        {
            _dbContext = dbContext;
        }

        public void Delete(Expression<Func<T, bool>> predicate)
        {
            var entities = Get(predicate);
            Delete(entities);
        }

        public void Delete(IEnumerable<T> entities)
        {
            if (entities != null && entities.Any())
            {
                _dbContext.Set<T>().RemoveRange(entities);
            }
        }

        public void DeleteOne(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public void DeleteOneByKey(params object[] keyValues)
        {
            DeleteOne(GetOneByKey(keyValues));
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return _dbContext.Set<T>().Where(predicate).AsQueryable();
        }

        public IQueryable<T> GetAll()
        {
            return _dbContext.Set<T>();
        }

        public T GetOne(Expression<Func<T, bool>> predicate)
        {
            var entity = _dbContext.Set<T>().Where(predicate).FirstOrDefault();
            _dbContext.Entry<T>(entity).State = EntityState.Detached;
            return entity;
        }

        public T GetOneByKey(params object[] keyValues)
        {
            var entity = _dbContext.Set<T>().Find(keyValues);
            _dbContext.Entry<T>(entity).State = EntityState.Detached;
            return entity;
        }

        public void Insert(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                InsertOne(entity);
            }
        }

        public void InsertOne(T entity)
        {
            if (entity != null) _dbContext.Set<T>().Add(entity);
        }

        public void Update(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                UpdateOne(entity);
            }
        }

        public void UpdateOne(T entity)
        {
            if (entity != null)
                _dbContext.Update<T>(entity);


        }

        public void UpdateOneByKey(T entity, params object[] keyValues)
        {
            if (entity == null)
                throw new BaseException(Constants.MsgRecordNull);

            var existingEntity = GetOneByKey(keyValues);
            if (existingEntity == null)
                throw new BaseException(Constants.MsgRecordNotFound);
            existingEntity.CopyFrom(entity);
            UpdateOne(entity);
        }

        public List<string> GetPrimaryKeyNames()
        {
            if (_primaryKeyNames == null || _primaryKeyNames.Count <= 0)
                _primaryKeyNames = _dbContext.FindPrimaryKeyNames((T)Activator.CreateInstance(typeof(T))).ToList();
            return _primaryKeyNames;
        }

        public object[] GetPrimaryKeyValues(T entity)
        {
            return _dbContext.FindPrimaryKeyValues(entity).ToArray();
        }
    }
}
