using UniversalPack.Data.Entities;
using UniversalPack.Models;
using System.Collections.Generic;
using System.Linq;

namespace UniversalPack.Extensions
{
    public static class ModelExtensions
    {
        /// <summary>
        /// Makes model <typeparamref name="TModel"/> from entity <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="entity">Entity.</param>
        /// <returns></returns>
        public static TModel ToModel<TEntity, TModel>(this TEntity entity)
            where TEntity : BaseEntity, new()
            where TModel : IEntityLoadable<TEntity>, new()
        {
            var model = new TModel();
            model.LoadFromEntity(entity);

            return model;
        }

        /// <summary>
        /// Makes models enumerable <typeparamref name="TModel"/> from entity enumerable <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="entities">Entity enumerable.</param>
        /// <returns></returns>
        public static IEnumerable<TModel> ToModels<TEntity, TModel>(this IEnumerable<TEntity> entities)
            where TEntity : BaseEntity, new()
            where TModel : IEntityLoadable<TEntity>, new()
        {
            return entities.Select(e => e.ToModel<TEntity, TModel>());
        }

        /// <summary>
        /// Makes models queryable <typeparamref name="TModel"/> from entity queryable <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="entities">Entity queryable.</param>
        /// <returns></returns>
        public static IQueryable<TModel> ToModels<TEntity, TModel>(this IQueryable<TEntity> entities)
            where TEntity : BaseEntity, new()
            where TModel : IEntityLoadable<TEntity>, new()
        {
            return entities.Select(e => e.ToModel<TEntity, TModel>());
        }

        /// <summary>
        /// Makes models enumerable <typeparamref name="TModel"/> from entity enumerable <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="entities">Entity queryable.</param>
        /// <returns></returns>
        public static IEnumerable<TModel> ToModelsEnumerable<TEntity, TModel>(this IEnumerable<TEntity> entities)
            where TEntity : BaseEntity, new()
            where TModel : IEntityLoadable<TEntity>, new()
        {
            return entities.Select(e => e.ToModel<TEntity, TModel>());
        }

        /// <summary>
        /// Makes entity <typeparamref name="TEntity"/> from model.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns></returns>
        public static TEntity ToEntity<TEntity>(this IEntityFillable<TEntity> model)
            where TEntity : BaseEntity, new()
        {
            var entity = new TEntity();
            model.FillEntity(entity);

            return entity;
        }

        // TODO: доделать
        /* 
         * надо из сущности по выражению вытащить коллекцию зависимых ones,
         * обработать её и потом положить обратно в родительскую сущность
         * суть Expression только в том, чтобы получить через лямду
         * свойство родительской сущности
         * в c2c такой механизм уже реализован для конструктора пишущих запросов
        public static void RelationalUpdate<TEntity, TRelatedEntity, TRelatedModel>(this TEntity entity, Expression<Func<TEntity, ICollection<TRelatedEntity>>> relatedEntitiesSelector, IEnumerable<TRelatedModel> relatedModels)
            where TEntity : BaseEntity, new()
            where TRelatedEntity : BaseDateTimedEntity, new()
            where TRelatedModel : class, IEntityFillable<TRelatedEntity>, IHasIdModel
        {
            if (relatedModels != null)
            {
                var func = relatedEntitiesSelector.Compile();
                var relatedEntities = func.Invoke(entity);

                // Удаляем лишнее
                entity.Layouts?.Where(e => !Layouts.Any(c => c.Id == e.Id)).ToList()?.ForEach(e => e.DeletedAt ??= DateTime.Now);

                // Обновляем существующее
                entity.Layouts?.ForEach(e => Layouts.FirstOrDefault(c => c.Id == e.Id)?.FillEntity(e));
                entity.Layouts?.ForEach(e => e.UpdatedAt = DateTime.Now);

                // Оптимизация: удаление и обновление не произведётся при == null
                entity.Layouts ??= new List<LayoutEntity>();

                // Добавляем новое
                entity.Layouts.AddRange(Layouts.Where(e => !entity.Layouts.Any(c => c.Id == e.Id) || e.Id == 0).Select(e => e.ToEntity<LayoutEntity, LayoutRelationalCreateModel>()));
            }
        }
        */
    }
}
