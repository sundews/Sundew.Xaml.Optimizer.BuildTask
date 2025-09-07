// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyReference.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal.Build;

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Build.Framework;
using Sundew.Xaml.Optimization;

internal class AssemblyReference : IAssemblyReference
{
    private const char CommaCharacter = ',';
    private readonly ITaskItem taskItem;
    private readonly Lazy<string[]> aliases;

    public AssemblyReference(ITaskItem taskItem)
    {
        this.taskItem = taskItem;
        this.aliases = new Lazy<string[]>(
            () =>
            {
                var aliases = this.taskItem.GetMetadata(MetadataNames.Aliases);
                if (string.IsNullOrEmpty(aliases))
                {
                    return [];
                }

                return aliases.Split([CommaCharacter], StringSplitOptions.RemoveEmptyEntries);
            },
            LazyThreadSafetyMode.None);
    }

    public string Name => this.taskItem.GetMetadata(MetadataNames.FusionName);

    public string Id => this.taskItem.ItemSpec;

    public BuildAction BuildAction => Optimization.BuildAction.AssemblyReference;

    public string Path => this.taskItem.ItemSpec;

    public IReadOnlyCollection<string> Names => (IReadOnlyCollection<string>)this.taskItem.MetadataNames;

    public string[] Aliases => this.aliases.Value;

    public string this[string name] => this.taskItem.GetMetadata(name);

    public override string ToString()
    {
        return this.Id;
    }
}