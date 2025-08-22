// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileReference.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal.Build;

using System.Collections.Generic;
using Microsoft.Build.Framework;
using Sundew.Xaml.Optimization;

internal class FileReference : IFileReference
{
    private readonly ITaskItem taskItem;

    public FileReference(ITaskItem taskItem, BuildAction buildAction)
    {
        this.taskItem = taskItem;
        this.BuildAction = buildAction;
    }

    public BuildAction BuildAction { get; }

    public string Path => this.taskItem.GetMetadata(MetadataNames.FullPath);

    public string Id => this.taskItem.ItemSpec;

    public IReadOnlyCollection<string> Names => (IReadOnlyCollection<string>)this.taskItem.MetadataNames;

    public string this[string name] => this.taskItem.GetMetadata(name);
}