// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IXamlOptimizerFactoryReporter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Factory;

using System;

/// <summary>Interface for implementing a reporter for <see cref="XamlOptimizerFactory"/>.</summary>
public interface IXamlOptimizerFactoryReporter
{
    /// <summary>Founds the optimizer.</summary>
    /// <param name="optimizerType">Type of the optimizer.</param>
    void FoundOptimizer(Type optimizerType);
}