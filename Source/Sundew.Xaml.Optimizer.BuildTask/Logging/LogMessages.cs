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

    public const string StagingDirectoryResolved = "SXO > Staging optimized xaml for: {0} in: {1}";

    public const string StagingDirectoryNotCleaned = "SXO > Could not clean the staging directory: {0}, reason: {1}";

    public const string EvictingOrphanedXamlFile = "SXO > Evicting files generated for the no longer optimized xaml file: {0}";

    public const string ObsoleteStagingDirectoryEvicted = "SXO > Evicted the staging directory used by a previous version: {0}";

    public const string MarkupCompileStateInvalidated = "SXO > The set of optimized xaml files changed, discarding the markup compiler's incremental state";

    public const string StateFileNotRead = "SXO > Could not read the state file: {0}, reason: {1}";

    public const string StateFileNotWritten = "SXO > Could not write the state file: {0}, reason: {1}";

    public const string FileNotDeleted = "SXO > Could not delete the file: {0}, reason: {1}";

    public const string DirectoryNotDeleted = "SXO > Could not delete the directory: {0}, reason: {1}";
}