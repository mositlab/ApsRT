using UniversalPack.Data.Entities;

namespace UniversalPack.Models
{
    /// <summary>
    /// Supports data saving to entity <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public interface IEntityFillable<TEntity> where TEntity : BaseEntity, new()
    {
        /// <summary>
        /// Save model data to entity <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="entity">Entity</param>
        TEntity FillEntity(TEntity entity);
    }
}
