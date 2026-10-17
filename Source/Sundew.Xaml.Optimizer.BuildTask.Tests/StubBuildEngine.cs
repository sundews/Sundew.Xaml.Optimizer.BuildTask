// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubBuildEngine.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Tests;

using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

internal sealed class StubBuildEngine : IBuildEngine
{
    private readonly List<string> messages = [];

    public bool ContinueOnError => false;

    public int LineNumberOfTaskNode => 0;

    public int ColumnNumberOfTaskNode => 0;

    public string ProjectFileOfTaskNode => "test.proj";

    public IReadOnlyList<string> Messages => this.messages;

    public TaskLoggingHelper CreateLog() => new(this, "Test");

    public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs) => true;

    public void LogCustomEvent(CustomBuildEventArgs e)
    {
    }

    public void LogErrorEvent(BuildErrorEventArgs e) => this.messages.Add(e.Message ?? string.Empty);

    public void LogMessageEvent(BuildMessageEventArgs e) => this.messages.Add(e.Message ?? string.Empty);

    public void LogWarningEvent(BuildWarningEventArgs e) => this.messages.Add(e.Message ?? string.Empty);
}
