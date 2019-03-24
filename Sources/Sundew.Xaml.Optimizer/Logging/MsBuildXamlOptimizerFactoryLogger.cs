// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsBuildXamlOptimizerFactoryLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Logging
{
    using System;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Sundew.Xaml.Optimizer.Factory;

    /// <summary>MsBuild logger for <see cref="XamlOptimizerFactory"/>.</summary>
    public class MsBuildXamlOptimizerFactoryLogger : IXamlOptimizerFactoryReporter
    {
        private readonly TaskLoggingHelper log;

        /// <summary>Initializes a new instance of the <see cref="MsBuildXamlOptimizerFactoryLogger"/> class.</summary>
        /// <param name="log">The log.</param>
        public MsBuildXamlOptimizerFactoryLogger(TaskLoggingHelper log)
        {
            this.log = log;
        }

        /// <summary>Founds the optimizer.</summary>
        /// <param name="optimizerType">Type of the optimizer.</param>
        public void FoundOptimizer(Type optimizerType)
        {
            this.log.LogMessage(MessageImportance.Low, string.Format(LogMessages.OptimizerFound, optimizerType));
        }

        /// <summary>Founds the settings.</summary>
        /// <param name="path">The path.</param>
        public void FoundSettings(string path)
        {
            this.log.LogMessage(MessageImportance.Normal, string.Format(LogMessages.SettingsFound, path));
        }
    }
}