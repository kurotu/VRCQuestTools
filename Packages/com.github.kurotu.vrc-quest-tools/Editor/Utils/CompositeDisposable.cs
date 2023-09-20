// <copyright file="CompositeDisposable.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Alternative for System.Reactive.Disposables.CompositeDisposable.
    /// https://docs.microsoft.com/en-us/previous-versions/dotnet/reactive-extensions/hh228980(v=vs.103) for detail.
    /// </summary>
    internal class CompositeDisposable : IDisposable
    {
        private List<IDisposable> disposables = new List<IDisposable>();

        /// <summary>
        /// Dispose internal IDisposable.
        /// </summary>
        public void Dispose()
        {
            foreach (var d in disposables)
            {
                d.Dispose();
            }
        }

        /// <summary>
        /// Add a disposable object to the list.
        /// </summary>
        /// <param name="item">Disposable object.</param>
        internal void Add(IDisposable item)
        {
            disposables.Add(item);
        }
    }
}
