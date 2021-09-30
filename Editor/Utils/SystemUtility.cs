// <copyright file="SystemUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility for C# and System.
    /// </summary>
    internal static class SystemUtility
    {
        /// <summary>
        /// Gets type object by full name.
        /// </summary>
        /// <param name="fullName">Full type name to get.</param>
        /// <returns>Type or null.</returns>
        internal static System.Type GetTypeByName(string fullName)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.FullName == fullName)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
    }
}