// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogMessages.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Logging;

internal static class LogMessages
{
    public const string StartingOptimization = "SXO > Starting optimization";

    public const string OptimizerFound = "SXO > Optimizer enabled: {0}";

    public const string SettingsFound = "SXO > Using settings: {0}";

    public const string ItemsOptimized = "SXO > Items optimized by: {0} took: {1}";

    public const string NothingToOptimize = "SXO > Nothing to optimize for optimizer: {0} took: {1}";

    public const string OptimizationCompleted = "SXO > Optimizer completed";
}