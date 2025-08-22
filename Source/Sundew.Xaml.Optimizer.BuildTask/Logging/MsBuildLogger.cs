// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsBuildLogger.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Sundew.Base.Text;
using Sundew.Xaml.Optimizer.BuildTask.Factory;
using Sundew.Xaml.Optimizer.BuildTask.Settings;

/// <summary>MsBuild logger for <see cref="XamlOptimizerFactory"/>.</summary>
public class MsBuildLogger : IXamlOptimizerFactoryReporter, ISettingsProviderReporter
{
    private readonly TaskLoggingHelper log;

    /// <summary>Initializes a new instance of the <see cref="MsBuildLogger"/> class.</summary>
    /// <param name="log">The log.</param>
    public MsBuildLogger(TaskLoggingHelper log)
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
    public void FoundSettings(IEnumerable<FileInfo> path)
    {
        const string separator = ", ";
        this.log.LogMessage(MessageImportance.Normal, string.Format(LogMessages.SettingsFound, path.JoinToStringInvariant(separator)));
    }
}