// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Optimizer.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Settings
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// SxoSettings for optimizations.
    /// </summary>
    public class Optimizer
    {
        /// <summary>Initializes a new instance of the <see cref="Optimizer"/> class.</summary>
        /// <param name="name">The name.</param>
        /// <param name="isEnabled">if set to <c>true</c> the optimization is enabled.</param>
        /// <param name="settings">The settings.</param>
        public Optimizer(string name, bool isEnabled, JObject settings)
        {
            this.Name = name;
            this.IsEnabled = isEnabled;
            this.Settings = settings;
        }

        /// <summary>Gets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>Gets a value indicating whether this instance is enabled.</summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
        public bool IsEnabled { get; }

        /// <summary>Gets the settings.</summary>
        /// <value>The settings.</value>
        public JObject Settings { get; }
    }
}