// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetadataNames.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal.Build;

using Microsoft.Build.Framework;

/// <summary>
/// Metadata names for <see cref="ITaskItem"/>.
/// </summary>
internal static class MetadataNames
{
    /// <summary>
    /// The full path.
    /// </summary>
    public const string FullPath = "FullPath";

    /// <summary>
    /// The identity.
    /// </summary>
    public const string Identity = "Identity";

    /// <summary>
    /// The link.
    /// </summary>
    public const string Link = "Link";

    public const string FusionName = "FusionName";

    public const string Aliases = "Aliases";
}