using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using PQT.Domain.Entities;
using PQT.Domain.Helpers;
using NS;
using NS.Entity;

namespace PQT.Domain
{
    /// <summary>
    ///     Adjust Repository interaction with business object (Entity derived)
    /// </summary>
    public abstract class Repository : RepositoryBase
    {
        protected Repository(DbContext db)
            : base(db)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetNextCounter(string name, int defaultValue = 1)
        {
            Counter counter = Get<Counter>(c => c.Name == name)
                              ?? Create(new Counter(name, defaultValue - 1));

            counter.Value++;
            Update(counter);

            return counter.Value;
        }
        public void SetCounter(string name, string value)
        {
            var resultString = Regex.Match(value, @"\d+").Value;
            if (!string.IsNullOrEmpty(resultString))
            {
                Counter counter = Get<Counter>(c => c.Name == name)
                                  ?? Create(new Counter(name, 0));
                counter.Value = Convert.ToInt32(resultString);
                Update(counter);
            }
        }
        public int GetNextTempCounter(string name, int defaultValue = 1)
        {
            Counter counter = Get<Counter>(c => c.Name == name )
                              ?? Create(new Counter(name, defaultValue - 1));

            counter.Value++;

            return counter.Value;
        }
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public IEnumerable<Counter> GetAllCounter(string name)
        {
            var counter = GetAll<Counter>(c => c.Name == name);
            return counter;
        }
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetCounter(string name, int defaultValue = 1)
        {
            Counter counter = Get<Counter>(c => c.Name == name)
                              ?? Create(new Counter(name, defaultValue));

            return counter.Value;
        }
        public int UpdateCounter(string name, int defaultValue = 1)
        {
            var counter = Get<Counter>(c => c.Name == name);
            if (counter != null)
            {
                counter.Value = defaultValue;
                Update(counter);
            }
            else
                Create(new Counter(name, defaultValue));

            return defaultValue;
        }
        public bool UpdateCounter(Counter counter)
        {
            return Update(counter);
        }
        protected new TEntity Create<TEntity>(TEntity entity, bool force = false) where TEntity : class
        {
            var entity1 = (object)entity as NS.Entity.Entity;
            if (entity1 != null)
            {
                if (entity1.CreatedTime == default(DateTime))
                {
                    entity1.CreatedTime = DateTime.Now;
                }
                entity1.EntityStatus = EntityStatus.Normal;
            }
            return base.Create(entity, force) ? entity : null;
        }

        protected override IQueryable<TEntity> ApplyFilter<TEntity>(IQueryable<TEntity> source)
        {
            if (typeof(TEntity).IsSubclassOf(typeof(NS.Entity.Entity)))
                return Queryable.AsQueryable<TEntity>(Enumerable.Where<TEntity>((IEnumerable<TEntity>)Enumerable.ToList<TEntity>((IEnumerable<TEntity>)source), (Func<TEntity, bool>)(e =>
                {
                    if ((NS.Enumeration)((object)e as NS.Entity.Entity).EntityStatus != (NS.Enumeration)EntityStatus.Archived)
                        return (NS.Enumeration)((object)e as NS.Entity.Entity).EntityStatus != (NS.Enumeration)EntityStatus.Deleted;
                    else
                        return false;
                })));
            else
                return base.ApplyFilter<TEntity>(source);
        }

        protected override TEntity ApplyFilter<TEntity>(TEntity entity)
        {
            var entity1 = (object)entity as NS.Entity.Entity;
            if (entity1 == null)
                return entity;
            if ((NS.Enumeration)entity1.EntityStatus == (NS.Enumeration)EntityStatus.Archived || (NS.Enumeration)entity1.EntityStatus == (NS.Enumeration)EntityStatus.Deleted)
                return default(TEntity);
            else
                return base.ApplyFilter<TEntity>(entity);
        }
        //protected override bool Create<TEntity>(TEntity entity, bool force = false)
        //{
        //    var entity1 = (object)entity as NS.Entity.Entity;
        //    if (entity1 != null)
        //    {
        //        entity1.CreatedTime = UserDateTime.Now;
        //        entity1.EntityStatus = EntityStatus.Normal;
        //    }
        //    return base.Create<TEntity>(entity, force);
        //}
        protected override bool Update<TEntity>(TEntity entity)
        {
            var entity1 = (object)entity as NS.Entity.Entity;
            if (entity1 != null)
                entity1.UpdatedTime = new DateTime?(DateTime.Now);
            return base.Update<TEntity>(entity);
        }
        protected override bool Delete<TEntity>(TEntity entity)
        {
            var entity1 = (object)entity as NS.Entity.Entity;
            if (entity1 == null)
                return base.Delete<TEntity>(entity);
            entity1.DeletedTime = new DateTime?(DateTime.Now);
            entity1.EntityStatus = EntityStatus.Deleted;
            return base.Update<TEntity>(entity);
        }
        /// <summary>
        /// Permanently delete record from database.
        /// 
        /// </summary>
        protected virtual bool PermanentlyDelete<TEntity>(TEntity entity) where TEntity : class
        {
            return base.Delete<TEntity>(entity);
        }

        /// <summary>
        /// Archive a specific record so it will no longer retrievable.
        /// 
        /// </summary>
        protected bool Archive(Entity entity)
        {
            entity.EntityStatus = EntityStatus.Archived;
            return base.Update<Entity>(entity);
        }

        /// <summary>
        /// Unarchive a specific record to normal state.
        /// 
        /// </summary>
        protected bool Unarchive(Entity entity)
        {
            entity.EntityStatus = EntityStatus.Normal;
            return base.Update<Entity>(entity);
        }
        //protected List<SelectItem> QuerySelectors<T>(string query, params object[] parameters) where T : class
        //{
        //    //            DbSet<T> dbSet = this._db.Set<T>();
        //    //            if (dbSet != null)
        //    //                return Enumerable.ToList<object>((IEnumerable<T>)dbSet.SqlQuery(query, parameters));
        //    //            else
        //    //                return Enumerable.ToList<object>(this._db.Database.SqlQuery<T>(query, parameters));
        //    return Enumerable.ToList<SelectItem>(this._db.Database.SqlQuery<SelectItem>(query, parameters));
        //}
    }
}
