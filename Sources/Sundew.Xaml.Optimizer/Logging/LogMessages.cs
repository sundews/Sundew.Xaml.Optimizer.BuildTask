// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogMessages.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Logging
{
    internal static class LogMessages
    {
        public const string StartingOptimization = "SXO > Starting optimization";

        public const string OptimizerFound = "SXO > Optimizer enabled: {0}";

        public const string SettingsFound = "SXO > Using settings: {0}";

        public const string ItemOptimized = "SXO > Optimized item: {0}, with optimizer: {1}";

        public const string NothingToOptimize = "SXO > Nothing to optimize for: {0}, for optimizer: {1}";

        public const string OptimizationCompleted = "SXO > Optimizer completed";
    }
}