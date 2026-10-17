// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StagingDirectoryProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal.Build;

using System;
using System.Globalization;
using System.IO;

/// <summary>
/// Resolves the directory that the optimized xaml is staged in.
/// The markup compiler resolves its own output as the intermediate output path combined with the path of the xaml file
/// relative to the project directory, so every character of the staging directory is repeated in the path of every
/// generated file. The staging directory therefore identifies the build it belongs to by a hash rather than by the
/// configuration and target framework, which keeps it short while remaining unique per inner build. The latter matters
/// because inner builds run in parallel and the staging directory is emptied at the start of each build.
/// </summary>
internal static class StagingDirectoryProvider
{
    private const string StagingDirectoryName = "sxo";
    private const uint FnvOffsetBasis = 2166136261;
    private const uint FnvPrime = 16777619;

    /// <summary>
    /// Resolves the staging directory.
    /// </summary>
    /// <param name="projectDirectory">The project directory.</param>
    /// <param name="baseIntermediateOutputPath">The base intermediate output path.</param>
    /// <param name="intermediateOutputPath">The intermediate output path.</param>
    /// <param name="stagingDirectoryOverride">The staging directory specified by the project, if any.</param>
    /// <returns>The staging directory and the build it belongs to.</returns>
    public static (DirectoryInfo Directory, string Pivot) Resolve(
        string projectDirectory,
        string baseIntermediateOutputPath,
        string intermediateOutputPath,
        string? stagingDirectoryOverride)
    {
        var pivot = GetPivot(projectDirectory, baseIntermediateOutputPath, intermediateOutputPath);
        if (!string.IsNullOrEmpty(stagingDirectoryOverride))
        {
            return (new DirectoryInfo(stagingDirectoryOverride!), pivot);
        }

        return (new DirectoryInfo(Path.Combine(baseIntermediateOutputPath, StagingDirectoryName, GetStableHash(pivot))), pivot);
    }

    /// <summary>
    /// Gets the staging directory used before the staging directory was shortened.
    /// </summary>
    /// <param name="intermediateOutputPath">The intermediate output path.</param>
    /// <returns>The obsolete staging directory.</returns>
    public static DirectoryInfo GetObsoleteDirectory(string intermediateOutputPath)
    {
        return new DirectoryInfo(Path.Combine(intermediateOutputPath, StagingDirectoryName));
    }

    private static string GetPivot(string projectDirectory, string baseIntermediateOutputPath, string intermediateOutputPath)
    {
        // Prefer a relative pivot, so that the staging directory does not depend on where the project is checked out.
        var pivot = TryGetRelativePath(baseIntermediateOutputPath, intermediateOutputPath)
                    ?? TryGetRelativePath(projectDirectory, intermediateOutputPath)
                    ?? intermediateOutputPath;
        return pivot.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .Trim(Path.DirectorySeparatorChar);
    }

    private static string? TryGetRelativePath(string rootPath, string path)
    {
        var root = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return path.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? path.Substring(root.Length) : null;
    }

    /// <summary>
    /// Computes an FNV-1a hash. <see cref="object.GetHashCode"/> is randomized per process for strings, which would
    /// move the staging directory on every build and leave the previous one behind.
    /// </summary>
    private static string GetStableHash(string value)
    {
        var hash = FnvOffsetBasis;
        foreach (var character in value.ToUpperInvariant())
        {
            hash = (hash ^ character) * FnvPrime;
        }

        return hash.ToString("x8", CultureInfo.InvariantCulture);
    }
}
