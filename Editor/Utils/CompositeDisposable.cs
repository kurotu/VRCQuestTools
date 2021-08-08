// <copyright file="CompositeDisposable.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

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
