using UniversalPack.Data.Entities;

namespace UniversalPack.Models
{
    /// <summary>
    /// Supports data loading from entity <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public interface IEntityLoadable<TEntity> where TEntity : BaseEntity, new()
    {
        /// <summary>
        /// Load model data from <typeparamref name="TEntity"/>
        /// </summary>
        /// <param name="entity">Entity.</param>
        void LoadFromEntity(TEntity entity);
    }
}
