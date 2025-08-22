// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskItemChanges.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal.Build;

using Microsoft.Build.Framework;

/// <summary>
/// Defines the <see cref="ITaskItem"/> to be replaced.
/// </summary>
internal class TaskItemChanges
{
    /// <summary>Initializes a new instance of the <see cref="TaskItemChanges"/> class.</summary>
    /// <param name="includeTaskItem">The include task item.</param>
    /// <param name="removeTaskItem">The remove task item.</param>
    public TaskItemChanges(ITaskItem includeTaskItem, ITaskItem removeTaskItem)
    {
        this.IncludeTaskItem = includeTaskItem;
        this.RemoveTaskItem = removeTaskItem;
    }

    /// <summary>
    /// Gets the include task item.
    /// </summary>
    /// <value>
    /// The include task item.
    /// </value>
    public ITaskItem IncludeTaskItem { get; }

    /// <summary>
    /// Gets the remove task item.
    /// </summary>
    /// <value>
    /// The remove task item.
    /// </value>
    public ITaskItem RemoveTaskItem { get; }
}