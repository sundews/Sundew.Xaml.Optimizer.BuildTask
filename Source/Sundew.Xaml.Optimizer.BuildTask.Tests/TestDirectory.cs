// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDirectory.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Tests;

using System;
using System.IO;

/// <summary>
/// A unique directory that stands in for a project directory and is removed when the test completes.
/// </summary>
internal sealed class TestDirectory : IDisposable
{
    public TestDirectory()
    {
        this.ProjectDirectory = Path.Combine(Path.GetTempPath(), "sxo-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(this.ProjectDirectory);
    }

    public string ProjectDirectory { get; }

    public string BaseIntermediateOutputPath => Path.Combine(this.ProjectDirectory, "obj") + Path.DirectorySeparatorChar;

    public string IntermediateOutputPath => this.GetIntermediateOutputPath("Debug", "net10.0-windows10.0.17763.0");

    public string GetIntermediateOutputPath(string configuration, string targetFramework)
    {
        return Path.Combine(this.ProjectDirectory, "obj", configuration, targetFramework) + Path.DirectorySeparatorChar;
    }

    public string CreateFile(string path, string content)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(this.ProjectDirectory, path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(this.ProjectDirectory))
            {
                Directory.Delete(this.ProjectDirectory, true);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }
}
