using UniversalPack.Extensions;
using System;

namespace UniversalPack.Models.Notification
{
    /// <summary>
    /// Уведомление
    /// </summary>
    public class NotificationModel
    {
        /// <summary>
        /// Типы уведомлений
        /// </summary>
        public enum NotificationType
        {
            /// <summary>
            /// Инфо
            /// </summary>
            Info,

            /// <summary>
            /// Ошибка
            /// </summary>
            Error
        }

        /// <summary>
        /// Дата уведомления
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Дата уведомления в строковом эквиваленте
        /// </summary>
        public string CreatedAtToHuman => CreatedAt.ToHumanFmt();

        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; init; }

        /// <summary>
        /// Тип уведомления
        /// </summary>
        public NotificationType Type { get; init; }

        /// <summary>
        /// Детали уведомления
        /// </summary>
        public string Details { get; init; }

        /// <summary>
        /// Уникальный ИД уведомления
        /// </summary>
        public int Uid { get; }

        public NotificationModel()
        {
            CreatedAt = DateTime.Now;
            Message = string.Empty;
            Details = string.Empty;
            Type = NotificationType.Info;
            Uid = HashCode.Combine(Type, CreatedAt, Message);
        }
    }
}
