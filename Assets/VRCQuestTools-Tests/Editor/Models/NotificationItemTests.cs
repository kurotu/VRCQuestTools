// <copyright file="NotificationItemTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="NotificationItem"/>.
    /// </summary>
    public class NotificationItemTests
    {
        [Test]
        public void Constructor_SetsGuiDelegate()
        {
            NotificationItem.OnGuiDelegate del = () => true;
            var item = new NotificationItem(del);
            Assert.AreSame(del, item.GuiDelegate);
        }

        [Test]
        public void GuiDelegate_CanBeInvoked()
        {
            var called = false;
            var item = new NotificationItem(() =>
            {
                called = true;
                return true;
            });

            var result = item.GuiDelegate();
            Assert.IsTrue(called);
            Assert.IsTrue(result);
        }

        [Test]
        public void GuiDelegate_ReturnsFalse()
        {
            var item = new NotificationItem(() => false);
            Assert.IsFalse(item.GuiDelegate());
        }
    }
}
