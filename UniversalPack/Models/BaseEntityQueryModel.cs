using UniversalPack.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UniversalPack.Models
{
    /// <summary>
    /// Base query model for base entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class BaseEntityQueryModel<TEntity> : IEntityQueryable<TEntity>
        where TEntity : BaseEntity
    {
        /// <summary>
        /// Entity with specified id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Entities with specified id collection
        /// </summary>
        internal IEnumerable<int> Ids { get; set; }

        /// <summary>
        /// Entities except specified id
        /// </summary>
        public int ExcludedId { get; set; }

        /// <summary>
        /// Entities except specified id collection
        /// </summary>
        internal IEnumerable<int> ExcludedIds { get; set; }

        public virtual IQueryable<TEntity> MakeQuery(IQueryable<TEntity> dataSource)
        {
            var q = dataSource;

            if (Id > 0)
            {
                q = q.Where(e => e.Id == Id);
            }
            if (Ids != null)
            {
                q = q.Where(e => Ids.Contains(e.Id));
            }
            if (ExcludedId > 0)
            {
                q = q.Where(e => e.Id != ExcludedId);
            }
            if (ExcludedIds != null)
            {
                q = q.Where(e => !ExcludedIds.Contains(e.Id));
            }

            return q;
        }
    }
}
