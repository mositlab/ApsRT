using System;
using System.Collections.Generic;
using System.Linq;
using static UniversalPack.Data.Entities.Log;
using Entity = UniversalPack.Data.Entities.Log;

namespace UniversalPack.Models.Log
{
    /// <summary>
    /// Модель запроса для сущности лога
    /// </summary>
    public sealed class LogQueryModel : BaseEntityQueryModel<Entity>
    {
        /// <summary>
        /// ИД - поиск от
        /// </summary>
        public int IdFrom { get; set; }

        /// <summary>
        /// ИД - поиск до
        /// </summary>
        public int IdTo { get; set; }

        /// <summary>
        /// Поиск по типу события
        /// </summary>
        public Events Event { get; set; }

        /// <summary>
        /// Поиск по типам событий
        /// </summary>
        public IEnumerable<Events> Events { get; set; }

        /// <summary>
        /// Время - от
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// Время - до
        /// </summary>
        public DateTime? To { get; set; }

        public override IQueryable<Entity> MakeQuery(IQueryable<Entity> dataSource)
        {
            var q = base.MakeQuery(dataSource);

            if (IdFrom > 0)
            {
                q = q.Where(e => e.Id >= IdFrom);
            }
            if (IdTo > 0)
            {
                q = q.Where(e => e.Id <= IdTo);
            }
            if (Events != null)
            {
                var events = Events.Select(e => (int)e).Distinct();
                q = q.Where(e => events.Contains(e.EventId));
            }
            if (From != null)
            {
                q = q.Where(e => e.CreatedAt >= From);
            }
            if (To != null)
            {
                q = q.Where(e => e.CreatedAt <= To);
            }

            return q;
        }
    }
}
