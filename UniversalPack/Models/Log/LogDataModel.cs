using UniversalPack.Extensions;
using Newtonsoft.Json.Linq;
using static UniversalPack.Data.Entities.Log;
using Entity = UniversalPack.Data.Entities.Log;

namespace UniversalPack.Models.Log
{
    /// <summary>
    /// Модель лога
    /// </summary>
    public sealed class LogDataModel : IEntityLoadable<Entity>
    {
        public int Id { get; set; }

        public string CreatedAt { get; set; }

        public Events Event { get; set; }

        public JToken Data { get; set; }

        public JToken Exception { get; set; }

        public void LoadFromEntity(Entity entity)
        {
            Id = entity.Id;
            CreatedAt = entity.CreatedAt.ToHumanFmt();
            Event = (Events)entity.EventId;
            Data = entity.Data.TryParseJson(out var jToken) ? jToken : default;
            Exception = entity.Exception.TryParseJson(out jToken) ? jToken : default;
        }
    }
}
