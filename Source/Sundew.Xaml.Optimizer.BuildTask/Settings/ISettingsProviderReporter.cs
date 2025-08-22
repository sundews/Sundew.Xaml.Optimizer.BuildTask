// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISettingsProviderReporter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Settings;

using System.Collections.Generic;
using System.IO;

/// <summary>Interface for implementing a setting provider reporter.</summary>
public interface ISettingsProviderReporter
{
    /// <summary>Founds the settings.</summary>
    /// <param name="path">The path.</param>
    void FoundSettings(IEnumerable<FileInfo> path);
}