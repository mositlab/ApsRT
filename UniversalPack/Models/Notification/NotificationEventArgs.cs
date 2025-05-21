using System;

namespace UniversalPack.Models.Notification
{
    public class NotificationEventArgs : EventArgs
    {
        public NotificationModel Notification { get; }

        public NotificationEventArgs(NotificationModel notification)
        {
            Notification = notification;
        }
    }
}
