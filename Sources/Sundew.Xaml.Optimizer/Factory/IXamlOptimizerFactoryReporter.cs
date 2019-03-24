// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IXamlOptimizerFactoryReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Factory
{
    using System;

    /// <summary>Interface for implementing a reporter for <see cref="XamlOptimizerFactory"/>.</summary>
    public interface IXamlOptimizerFactoryReporter
    {
        /// <summary>Founds the settings.</summary>
        /// <param name="path">The path.</param>
        void FoundSettings(string path);

        /// <summary>Founds the optimizer.</summary>
        /// <param name="optimizerType">Type of the optimizer.</param>
        void FoundOptimizer(Type optimizerType);
    }
}