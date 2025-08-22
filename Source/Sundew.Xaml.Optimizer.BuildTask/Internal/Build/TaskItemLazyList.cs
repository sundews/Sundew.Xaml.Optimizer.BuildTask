// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskItemLazyList.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal.Build;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Build.Framework;
using Sundew.Base.Collections;

internal class TaskItemLazyList<TItem> : IReadOnlyList<TItem>
{
    private readonly Lazy<IReadOnlyList<TItem>> items;

    public TaskItemLazyList(ITaskItem[] referencesPaths, Func<ITaskItem, TItem> factory)
    {
        this.items = new Lazy<IReadOnlyList<TItem>>(
            () => referencesPaths.ToArray(factory),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public int Count => this.items.Value.Count;

    public TItem this[int index] => this.items.Value[index];

    public IEnumerator<TItem> GetEnumerator()
    {
        return this.items.Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.items.Value.GetEnumerator();
    }
}