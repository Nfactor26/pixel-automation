using Notifications.Wpf.Core;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Helpers
{
    public static class NotificationManagerExtensions
    {
        public static async Task ShowErrorNotificationAsync(this INotificationManager notificationManager, string message)
        {
            await notificationManager.ShowAsync(new NotificationContent()
            {
                Title = "Error",
                Message = message,
                Type = NotificationType.Error
            });
        }

        public static async Task ShowErrorNotificationAsync(this INotificationManager notificationManager, Exception ex)
        {
            await notificationManager.ShowAsync(new NotificationContent()
            {
                Title = "Error",
                Message = ex.Message,
                Type = NotificationType.Error
            });
        }

        public static async Task ShowSuccessNotificationAsync(this INotificationManager notificationManager, string message)
        {
            await notificationManager.ShowAsync(new NotificationContent()
            {
                Title = "Success",
                Message = message,
                Type = NotificationType.Success
            });
        }

        public static async Task ShowWarningNotificationAsync(this INotificationManager notificationManager, string message)
        {
            await notificationManager.ShowAsync(new NotificationContent()
            {
                Title = "Warning",
                Message = message,
                Type = NotificationType.Warning
            });
        }
    }
}
