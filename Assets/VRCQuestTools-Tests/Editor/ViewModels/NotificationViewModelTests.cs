// <copyright file="NotificationViewModelTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="NotificationViewModel"/>.
    /// </summary>
    public class NotificationViewModelTests
    {
        [Test]
        public void HasNotifications_WhenEmpty_ReturnsFalse()
        {
            var vm = new NotificationViewModel();
            Assert.IsFalse(vm.HasNotifications);
        }

        [Test]
        public void HasNotifications_AfterRegister_ReturnsTrue()
        {
            var vm = new NotificationViewModel();
            var item = new NotificationItem(() => true);
            vm.RegisterNotification("key1", item);
            Assert.IsTrue(vm.HasNotifications);
        }

        [Test]
        public void Notifications_ReturnsAllRegistered()
        {
            var vm = new NotificationViewModel();
            var item1 = new NotificationItem(() => true);
            var item2 = new NotificationItem(() => false);
            vm.RegisterNotification("key1", item1);
            vm.RegisterNotification("key2", item2);

            var notifications = vm.Notifications;
            Assert.AreEqual(2, notifications.Count);
            Assert.IsTrue(notifications.ContainsKey("key1"));
            Assert.IsTrue(notifications.ContainsKey("key2"));
        }

        [Test]
        public void Notifications_ReturnsCopy()
        {
            var vm = new NotificationViewModel();
            var item = new NotificationItem(() => true);
            vm.RegisterNotification("key1", item);

            var copy = vm.Notifications;
            copy.Remove("key1");

            Assert.IsTrue(vm.HasNotifications);
        }

        [Test]
        public void RegisterNotification_OverwritesExistingKey()
        {
            var vm = new NotificationViewModel();
            var item1 = new NotificationItem(() => true);
            var item2 = new NotificationItem(() => false);
            vm.RegisterNotification("key1", item1);
            vm.RegisterNotification("key1", item2);

            var notifications = vm.Notifications;
            Assert.AreEqual(1, notifications.Count);
            Assert.AreSame(item2, notifications["key1"]);
        }

        [Test]
        public void RemoveNotification_RemovesExistingKey()
        {
            var vm = new NotificationViewModel();
            var item = new NotificationItem(() => true);
            vm.RegisterNotification("key1", item);
            vm.RemoveNotification("key1");

            Assert.IsFalse(vm.HasNotifications);
        }

        [Test]
        public void RemoveNotification_NonExistentKey_DoesNotThrow()
        {
            var vm = new NotificationViewModel();
            Assert.DoesNotThrow(() => vm.RemoveNotification("nonexistent"));
        }
    }
}
