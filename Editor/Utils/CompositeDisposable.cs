// <copyright file="CompositeDisposable.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace KRT.VRCQuestTools
{
    class CompositeDisposable : IDisposable
    {
        private List<IDisposable> _disposables = new List<IDisposable>();

        public void Dispose()
        {
            foreach (var d in _disposables)
            {
                d.Dispose();
            }
        }

        internal void Add(IDisposable item)
        {
            _disposables.Add(item);
        }
    }
}
