using UniversalPack.Data.Entities;
using System.Linq;

namespace UniversalPack.Models
{
    /// <summary>
    /// Model can be used to generate query for repository.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntityQueryable<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// Create <see cref="IQueryable{T}"/> based on query model values.
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        IQueryable<TEntity> MakeQuery(IQueryable<TEntity> dataSource);
    }
}
