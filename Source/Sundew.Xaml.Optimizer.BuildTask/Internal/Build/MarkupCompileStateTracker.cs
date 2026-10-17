// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MarkupCompileStateTracker.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal.Build;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Sundew.Xaml.Optimizer.BuildTask.Logging;

/// <summary>
/// Tracks the xaml files staged by the optimizer between builds.
/// The markup compiler resolves its output as the intermediate output path combined with the path of the xaml file
/// relative to the project directory, and it never removes the files it generated for a xaml file that has been
/// moved, renamed or removed. Since the optimizer stages its xaml files itself, moving a xaml file leaves behind
/// generated files that the markup compiler still considers up to date, which previously required a clean build.
/// </summary>
internal sealed class MarkupCompileStateTracker
{
    private static readonly string[] GeneratedFileExtensions = [".baml", ".g.cs", ".g.i.cs", ".g.vb", ".g.i.vb"];

    private readonly string stateFilePath;
    private readonly string projectDirectory;
    private readonly string? markupCompileOutputPath;
    private readonly TaskLoggingHelper log;

    public MarkupCompileStateTracker(string stateFilePath, string projectDirectory, string? markupCompileOutputPath, TaskLoggingHelper log)
    {
        this.stateFilePath = stateFilePath;
        this.projectDirectory = projectDirectory;
        this.markupCompileOutputPath = string.IsNullOrEmpty(markupCompileOutputPath) ? null : markupCompileOutputPath;
        this.log = log;
    }

    /// <summary>
    /// Evicts the files generated for xaml files that the optimizer no longer stages.
    /// </summary>
    /// <param name="stagedFilePaths">The xaml files staged by this build.</param>
    /// <param name="obsoleteStagingDirectory">A staging directory used by a previous version of the optimizer.</param>
    /// <returns><c>true</c> if the markup compiler's incremental state has to be discarded; otherwise <c>false</c>.</returns>
    public bool Evict(IEnumerable<string> stagedFilePaths, DirectoryInfo? obsoleteStagingDirectory)
    {
        var stagedFiles = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var stagedFilePath in stagedFilePaths)
        {
            var projectRelativePath = this.TryGetProjectRelativePath(stagedFilePath);
            if (projectRelativePath != null)
            {
                stagedFiles.Add(projectRelativePath);
            }
        }

        var isInvalidated = this.EvictObsoleteStagingDirectory(obsoleteStagingDirectory);
        foreach (var orphanedFile in this.ReadState().Where(x => !stagedFiles.Contains(x)))
        {
            this.log.LogMessage(MessageImportance.Normal, LogMessages.EvictingOrphanedXamlFile, orphanedFile);
            this.TryDeleteFile(Path.Combine(this.projectDirectory, orphanedFile));
            this.EvictGeneratedFiles(orphanedFile);
            isInvalidated = true;
        }

        this.SaveState(stagedFiles);
        if (isInvalidated)
        {
            this.log.LogMessage(MessageImportance.Normal, LogMessages.MarkupCompileStateInvalidated);
        }

        return isInvalidated;
    }

    private bool EvictObsoleteStagingDirectory(DirectoryInfo? obsoleteStagingDirectory)
    {
        if (obsoleteStagingDirectory is not { Exists: true })
        {
            return false;
        }

        var projectRelativePath = this.TryGetProjectRelativePath(obsoleteStagingDirectory.FullName);
        if (projectRelativePath != null && this.markupCompileOutputPath != null)
        {
            this.TryDeleteDirectory(Path.Combine(this.markupCompileOutputPath, projectRelativePath));
        }

        this.TryDeleteDirectory(obsoleteStagingDirectory.FullName);
        this.log.LogMessage(MessageImportance.Normal, LogMessages.ObsoleteStagingDirectoryEvicted, obsoleteStagingDirectory.FullName);
        return true;
    }

    private void EvictGeneratedFiles(string projectRelativeFilePath)
    {
        if (this.markupCompileOutputPath == null)
        {
            return;
        }

        foreach (var generatedFileExtension in GeneratedFileExtensions)
        {
            this.TryDeleteFile(Path.Combine(this.markupCompileOutputPath, Path.ChangeExtension(projectRelativeFilePath, generatedFileExtension)));
        }
    }

    private string? TryGetProjectRelativePath(string path)
    {
        try
        {
            var rootPath = Path.GetFullPath(this.projectDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) ? fullPath.Substring(rootPath.Length) : null;
        }
        catch (Exception e) when (e is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }

    private IEnumerable<string> ReadState()
    {
        if (!File.Exists(this.stateFilePath))
        {
            return [];
        }

        try
        {
            return File.ReadAllLines(this.stateFilePath).Where(x => !string.IsNullOrWhiteSpace(x));
        }
        catch (IOException e)
        {
            this.log.LogMessage(MessageImportance.High, LogMessages.StateFileNotRead, this.stateFilePath, e.Message);
            return [];
        }
    }

    private void SaveState(IEnumerable<string> stagedFiles)
    {
        try
        {
            var directory = Path.GetDirectoryName(this.stateFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(this.stateFilePath, stagedFiles.ToArray());
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            this.log.LogMessage(MessageImportance.High, LogMessages.StateFileNotWritten, this.stateFilePath, e.Message);
        }
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            this.log.LogMessage(MessageImportance.High, LogMessages.FileNotDeleted, path, e.Message);
        }
    }

    private void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            this.log.LogMessage(MessageImportance.High, LogMessages.DirectoryNotDeleted, path, e.Message);
        }
    }
}
