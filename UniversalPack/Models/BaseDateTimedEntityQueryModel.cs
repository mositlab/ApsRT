using UniversalPack.Data.Entities;
using System.Linq;

namespace UniversalPack.Models
{
    /// <summary>
    /// Base query model for base datetimed entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class BaseDateTimedEntityQueryModel<TEntity> : BaseEntityQueryModel<TEntity>
        where TEntity : BaseDateTimedEntity
    {
        /// <summary>
        /// Include deleted entitied.
        /// </summary>
        public bool IncludeDeleted { get; set; }

        public override IQueryable<TEntity> MakeQuery(IQueryable<TEntity> dataSource)
        {
            var q = base.MakeQuery(dataSource);

            if (!IncludeDeleted)
            {
                q = q.Where(e => e.DeletedAt == null);
            }

            return q;
        }
    }
}
