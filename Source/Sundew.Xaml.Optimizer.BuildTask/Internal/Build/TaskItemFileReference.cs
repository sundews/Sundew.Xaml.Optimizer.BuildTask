// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskItemFileReference.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal.Build;

using System.Collections.Generic;
using Microsoft.Build.Framework;
using Sundew.Xaml.Optimization;

internal class TaskItemFileReference : IFileReference
{
    public TaskItemFileReference(ITaskItem taskItem, BuildAction buildAction)
    {
        this.TaskItem = taskItem;
        this.BuildAction = buildAction;
    }

    public ITaskItem TaskItem { get; }

    public BuildAction BuildAction { get; }

    public string Path => this.TaskItem.GetMetadata(MetadataNames.FullPath);

    public string Id => this.TaskItem.ItemSpec;

    public IReadOnlyCollection<string> Names => (IReadOnlyCollection<string>)this.TaskItem.MetadataNames;

    public string this[string name] => this.TaskItem.GetMetadata(name);

    public override string ToString()
    {
        return this.Id;
    }
}