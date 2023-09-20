// <copyright file="NotificationViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Models;

namespace KRT.VRCQuestTools.ViewModels
{
    /// <summary>
    /// ViewModel for Notifications.
    /// </summary>
    [Serializable]
    internal class NotificationViewModel
    {
        private Dictionary<string, NotificationItem> notifications = new Dictionary<string, NotificationItem>();

        /// <summary>
        /// Gets a value indicating whether any notifications exist.
        /// </summary>
        internal bool HasNotifications
        {
            get
            {
                return notifications.Count > 0;
            }
        }

        /// <summary>
        /// Gets notifications to display.
        /// </summary>
        internal Dictionary<string, NotificationItem> Notifications => new Dictionary<string, NotificationItem>(notifications);

        /// <summary>
        /// Register a notification.
        /// </summary>
        /// <param name="key">key for notification.</param>
        /// <param name="notification">item.</param>
        internal void RegisterNotification(string key, NotificationItem notification)
        {
            notifications[key] = notification;
        }

        /// <summary>
        /// Remove a notification.
        /// </summary>
        /// <param name="key">key for notification.</param>
        internal void RemoveNotification(string key)
        {
            notifications.Remove(key);
        }
    }
}
