// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SxoSettings.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Settings
{
    using System.Collections.Generic;

    /// <summary>SxoSettings for Sundew.Xaml.Optimizer.</summary>
    public class SxoSettings
    {
        /// <summary>Initializes a new instance of the <see cref="SxoSettings"/> class.</summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <param name="libraries">The libraries.</param>
        /// <param name="optimizers">The optimizers.</param>
        public SxoSettings(bool isEnabled, IEnumerable<string> libraries, IEnumerable<Optimizer> optimizers)
        {
            this.IsEnabled = isEnabled;
            this.Libraries = libraries;
            this.Optimizers = optimizers;
        }

        /// <summary>Gets a value indicating whether this instance is enabled.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
        public bool IsEnabled { get; }

        /// <summary>Gets the libraries.</summary>
        /// <value>The libraries.</value>
        public IEnumerable<string> Libraries { get; }

        /// <summary>Gets the optimizers.</summary>
        /// <value>The optimizers.</value>
        public IEnumerable<Optimizer> Optimizers { get; }
    }
}