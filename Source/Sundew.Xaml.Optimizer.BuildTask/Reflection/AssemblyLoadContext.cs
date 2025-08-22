// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyLoadContext.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Reflection;

/// <summary>Determines which context to load assemblies into.</summary>
public enum AssemblyLoadContext
{
    /// <summary>The default context.</summary>
    Default,

    /// <summary>Use the From context.</summary>
    From,

    /// <summary>Use the File context.</summary>
    File,
}