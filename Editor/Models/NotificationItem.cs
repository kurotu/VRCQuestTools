// <copyright file="NotificationItem.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// An item to show notification in NotificationWindow.
    /// </summary>
    internal class NotificationItem
    {
        /// <summary>
        /// onGuiDelegate on the notification window.
        /// </summary>
        internal OnGuiDelegate GuiDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationItem"/> class.
        /// </summary>
        /// <param name="onGuiDelegate">onGuiDelegate for this item.</param>
        internal NotificationItem(OnGuiDelegate onGuiDelegate)
        {
            GuiDelegate = onGuiDelegate;
        }

        /// <summary>
        /// onGuiDelegate on the notification window.
        /// </summary>
        /// <returns>should clear this item.</returns>
        internal delegate bool OnGuiDelegate();
    }
}
