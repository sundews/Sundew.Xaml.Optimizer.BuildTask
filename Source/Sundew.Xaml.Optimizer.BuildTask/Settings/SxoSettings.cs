// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SxoSettings.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Settings;

using System.Collections.Generic;

/// <summary>SxoSettings for Sundew.Xaml.Optimizer.</summary>
public class SxoSettings
{
    /// <summary>Initializes a new instance of the <see cref="SxoSettings"/> class.</summary>
    /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
    /// <param name="optimizers">The optimizers.</param>
    public SxoSettings(bool isEnabled, IEnumerable<Optimizer> optimizers)
    {
        this.IsEnabled = isEnabled;
        this.Optimizers = optimizers;
    }

    /// <summary>Gets a value indicating whether this instance is enabled.</summary>
    /// <value>
    ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
    public bool IsEnabled { get; }

    /// <summary>Gets the optimizers.</summary>
    /// <value>The optimizers.</value>
    public IEnumerable<Optimizer> Optimizers { get; }
}