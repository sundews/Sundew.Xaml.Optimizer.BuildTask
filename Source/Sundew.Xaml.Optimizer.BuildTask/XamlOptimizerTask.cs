// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlOptimizerTask.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Sundew.Base.Collections;
using Sundew.Xaml.Optimization;
using Sundew.Xaml.Optimization.Xml;
using Sundew.Xaml.Optimizer.BuildTask.Factory;
using Sundew.Xaml.Optimizer.BuildTask.Internal.Build;
using Sundew.Xaml.Optimizer.BuildTask.Logging;
using Sundew.Xaml.Optimizer.BuildTask.Reflection;
using Sundew.Xaml.Optimizer.BuildTask.Reflection.Internal;
using Sundew.Xaml.Optimizer.BuildTask.Settings;
using Sundew.Xaml.Optimizer.BuildTask.Xaml;
using Task = Microsoft.Build.Utilities.Task;

/// <summary>
/// MsBuild task that analyzes and optimizes Pages, ApplicationDefinitions and EmbeddedXamlResources.
/// </summary>
/// <seealso cref="Microsoft.Build.Utilities.Task" />
public sealed class XamlOptimizerTask : Task
{
    /// <summary>Gets or sets the optimizers.</summary>
    [Required]
    public required ITaskItem[] Optimizers { get; set; }

    /// <summary>Gets or sets the reference paths.</summary>
    /// <value>The reference paths.</value>
    [Required]
    public required ITaskItem[] ReferencePaths { get; set; }

    /// <summary>Gets or sets the package references.</summary>
    /// <value>The package references.</value>
    [Required]
    public required ITaskItem[] PackageReferences { get; set; }

    /// <summary>Gets or sets the compiles.</summary>
    /// <value>The compiles.</value>
    [Required]
    public required ITaskItem[] Compiles { get; set; } = [];

    /// <summary>Gets or sets the target platform identifier.</summary>
    /// <value>The target platform identifier.</value>
    [Required]
    public required string TargetPlatformIdentifier { get; set; }

    /// <summary>Gets or sets the solution directory.</summary>
    /// <value>The solution directory.</value>
    [Required]
    public required string SolutionDirectory { get; set; }

    /// <summary>Gets or sets the project directory.
    /// </summary>
    /// <value>
    /// The project directory.
    /// </value>
    [Required]
    public required string ProjectDirectory { get; set; }

    /// <summary>Gets or sets the assembly name.
    /// </summary>
    /// <value>
    /// The assembly name.
    /// </value>
    [Required]
    public required string AssemblyName { get; set; }

    /// <summary>Gets or sets the root namespace.
    /// </summary>
    /// <value>
    /// The root namespace.
    /// </value>
    [Required]
    public required string RootNamespace { get; set; }

    /// <summary>
    /// Gets or sets the intermediate output path.
    /// </summary>
    /// <value>
    /// The intermediate output path.
    /// </value>
    [Required]
    public required string IntermediateOutputPath { get; set; }

    /// <summary>
    /// Gets or sets the application definitions.
    /// </summary>
    /// <value>
    /// The application definitions.
    /// </value>
    [Required]
    public required ITaskItem[] ApplicationDefinitions { get; set; }

    /// <summary>
    /// Gets or sets the Avalonia Xaml.
    /// </summary>
    /// <value>
    /// The pages.
    /// </value>
    [Required]
    public required ITaskItem[] AvaloniaXaml { get; set; }

    /// <summary>
    /// Gets or sets the Maui Xaml.
    /// </summary>
    /// <value>
    /// The pages.
    /// </value>
    [Required]
    public required ITaskItem[] MauiXaml { get; set; }

    /// <summary>
    /// Gets or sets the pages.
    /// </summary>
    /// <value>
    /// The pages.
    /// </value>
    [Required]
    public required ITaskItem[] Pages { get; set; }

    /// <summary>
    /// Gets or sets the embedded xaml resources.
    /// </summary>
    /// <value>
    /// The embedded xaml resources.
    /// </value>
    [Required]
    public required ITaskItem[] EmbeddedXamlResources { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether WPF is used.
    /// </summary>
    public bool UseWPF { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether WinUI is used.
    /// </summary>
    public bool UseWinUI { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Maui is used.
    /// </summary>
    public bool UseMaui { get; set; }

    /// <summary>Gets or sets a value indicating whether this <see cref="XamlOptimizerTask"/> is debug.</summary>
    /// <value>
    ///   <c>true</c> if debug; otherwise, <c>false</c>.</value>
    public bool Debug { get; set; }

    /// <summary>
    /// Gets the optimized Avalonia Xaml.
    /// </summary>
    /// <value>
    /// The optimized pages.
    /// </value>
    [Output]
    public ITaskItem[]? OptimizedAvaloniaXaml { get; private set; }

    /// <summary>
    /// Gets the obsolete Avalonia xaml.
    /// </summary>
    /// <value>
    /// The obsolete pages.
    /// </value>
    [Output]
    public ITaskItem[]? ObsoleteAvaloniaXaml { get; private set; }

    /// <summary>
    /// Gets the optimized Maui Xaml.
    /// </summary>
    /// <value>
    /// The optimized pages.
    /// </value>
    [Output]
    public ITaskItem[]? OptimizedMauiXaml { get; private set; }

    /// <summary>
    /// Gets the obsolete Maui xaml.
    /// </summary>
    /// <value>
    /// The obsolete pages.
    /// </value>
    [Output]
    public ITaskItem[]? ObsoleteMauiXaml { get; private set; }

    /// <summary>
    /// Gets the optimized pages.
    /// </summary>
    /// <value>
    /// The optimized pages.
    /// </value>
    [Output]
    public ITaskItem[]? OptimizedPages { get; private set; }

    /// <summary>
    /// Gets the obsolete pages.
    /// </summary>
    /// <value>
    /// The obsolete pages.
    /// </value>
    [Output]
    public ITaskItem[]? ObsoletePages { get; private set; }

    /// <summary>
    /// Gets the optimized application definitions.
    /// </summary>
    /// <value>
    /// The optimized application definitions.
    /// </value>
    [Output]
    public ITaskItem[]? OptimizedApplicationDefinitions { get; private set; }

    /// <summary>
    /// Gets the obsolete application definitions.
    /// </summary>
    /// <value>
    /// The obsolete application definitions.
    /// </value>
    [Output]
    public ITaskItem[]? ObsoleteApplicationDefinitions { get; private set; }

    /// <summary>
    /// Gets the optimized embedded xaml resources.
    /// </summary>
    /// <value>
    /// The optimized embedded xaml resources.
    /// </value>
    [Output]
    public ITaskItem[]? OptimizedEmbeddedXamlResources { get; private set; }

    /// <summary>
    /// Gets the obsolete embedded xaml resources.
    /// </summary>
    /// <value>
    /// The obsolete embedded xaml resources.
    /// </value>
    [Output]
    public ITaskItem[]? ObsoleteEmbeddedXamlResources { get; private set; }

    /// <summary>Gets the new pages.</summary>
    [Output]
    public string[]? NewPages { get; private set; }

    /// <summary>Gets the new avalonia xaml-.</summary>
    [Output]
    public string[]? NewAvaloniaXaml { get; private set; }

    /// <summary>Gets the new maui xaml-.</summary>
    [Output]
    public string[]? NewMauiXaml { get; private set; }

    /// <summary>Gets the new embedded resources.</summary>
    [Output]
    public string[]? NewEmbeddedResources { get; private set; }

    /// <summary>Gets the new compiles.</summary>
    [Output]
    public string[]? NewCompiles { get; private set; }

    /// <summary>Gets the new additional files.</summary>
    [Output]
    public string[]? NewAdditionalFiles { get; private set; }

    /// <summary>
    /// Executes the xaml resource optimization.
    /// </summary>
    /// <returns>
    /// true if the task successfully executed; otherwise, false.
    /// </returns>
    public override bool Execute()
    {
        if (this.Debug)
        {
            System.Diagnostics.Debugger.Launch();
        }

        this.Log.LogMessage(MessageImportance.Normal, LogMessages.StartingOptimization);
        try
        {
            var xamlPlatform = XamlPlatformProvider.DetectFramework(this.UseMaui, this.UseWPF, this.UseWinUI, this.TargetPlatformIdentifier, this.PackageReferences);
            var xamlPlatformInfo = XamlPlatformInfoProvider.GetXamlPlatformInfo(xamlPlatform);
            var logger = new MsBuildLogger(this.Log);
            var xamlOptimizerFactory = new XamlOptimizerFactory(logger);
            var compiles = new TaskItemLazyList<IFileReference>(this.Compiles, x => new FileReference(x, BuildAction.Compile));
            var assemblyReferences = new TaskItemLazyList<IAssemblyReference>(this.ReferencePaths, x => new AssemblyReference(x));
            var intermediateDirectory = new DirectoryInfo(this.IntermediateOutputPath);
            var sxoSettings = new SettingsProvider(logger).GetSettings(this.ProjectDirectory);
            var optimizerPaths = this.Optimizers.Select(x => x.ItemSpec).ToArray();

            using (var assemblyResolver = new AssemblyResolver(optimizerPaths, AssemblyLoadContext.Default, new OverriddenAssemblyProvider()))
            {
                this.Optimize(
                    xamlPlatformInfo,
                    xamlOptimizerFactory,
                    compiles,
                    assemblyReferences,
                    intermediateDirectory,
                    sxoSettings,
                    optimizerPaths,
                    assemblyResolver);
            }
        }
        catch (Exception e)
        {
            this.Log.LogErrorFromException(e);
            return false;
        }

        this.Log.LogMessage(MessageImportance.Normal, LogMessages.OptimizationCompleted);
        return true;
    }

    private static string GetLink(ITaskItem taskItem)
    {
        var link = taskItem.GetMetadata(MetadataNames.Link);
        if (string.IsNullOrEmpty(link))
        {
            link = taskItem.GetMetadata(MetadataNames.Identity);
        }

        return link;
    }

    private void Optimize(
        XamlPlatformInfo xamlPlatformInfo,
        XamlOptimizerFactory xamlOptimizerFactory,
        TaskItemLazyList<IFileReference> compiles,
        TaskItemLazyList<IAssemblyReference> assemblyReferences,
        DirectoryInfo intermediateDirectory,
        IReadOnlyCollection<SxoSettings> sxoSettings,
        IReadOnlyList<string> xamlOptimizerPaths,
        AssemblyResolver assemblyResolver)
    {
        var pages = this.Pages.ToArray(x => new FileReference(x, BuildAction.Page));
        var avaloniaXaml = this.AvaloniaXaml.ToArray(x => new FileReference(x, BuildAction.AvaloniaXaml));
        var mauiXaml = this.MauiXaml.ToArray(x => new FileReference(x, BuildAction.MauiXaml));
        var embeddedXamlResources = this.EmbeddedXamlResources.ToArray(x => new FileReference(x, BuildAction.EmbeddedResource));
        var applicationDefinitions = this.ApplicationDefinitions.ToArray(x => new FileReference(x, BuildAction.ApplicationDefinition));
        var xDocumentProvider = new XDocumentProvider(pages.Concat(embeddedXamlResources, applicationDefinitions));

        var projectInfo = new ProjectInfo(this.AssemblyName, this.RootNamespace, intermediateDirectory, assemblyReferences, compiles, xDocumentProvider);

        var xamlOptimizers = xamlOptimizerFactory.CreateXamlOptimizers(assemblyResolver, xamlPlatformInfo, projectInfo, xamlOptimizerPaths, sxoSettings).ToArray();
        var newCompiles = new List<string>();
        var newPages = new List<string>();
        var newAvaloniaXaml = new List<string>();
        var newMauiXaml = new List<string>();
        var newEmbeddedResources = new List<string>();
        var newAdditionalFiles = new List<string>();
        var mauiXamlTaskItemChanges = this.OptimizeXaml(this.MauiXaml, mauiXaml, intermediateDirectory, xamlOptimizers, newCompiles, newPages, newAvaloniaXaml, newMauiXaml, newEmbeddedResources, newAdditionalFiles, xDocumentProvider);
        var avaloniaXamlTaskItemChanges = this.OptimizeXaml(this.AvaloniaXaml, avaloniaXaml, intermediateDirectory, xamlOptimizers, newCompiles, newPages, newAvaloniaXaml, newMauiXaml, newEmbeddedResources, newAdditionalFiles, xDocumentProvider);
        var pagesTaskItemChanges = this.OptimizeXaml(this.Pages, pages, intermediateDirectory, xamlOptimizers, newCompiles, newPages, newEmbeddedResources, newAvaloniaXaml, newMauiXaml, newAdditionalFiles, xDocumentProvider);
        var embeddedXamlResourcesTaskItemChanges = this.OptimizeXaml(this.EmbeddedXamlResources, embeddedXamlResources, intermediateDirectory, xamlOptimizers, newCompiles, newPages, newAvaloniaXaml, newMauiXaml, newAdditionalFiles, newEmbeddedResources, xDocumentProvider);
        var applicationDefinitionTaskItemChanges = this.OptimizeXaml(this.ApplicationDefinitions, applicationDefinitions, intermediateDirectory, xamlOptimizers, newCompiles, newPages, newAvaloniaXaml, newMauiXaml, newEmbeddedResources, newAdditionalFiles, xDocumentProvider);

        this.ObsoletePages = pagesTaskItemChanges.ToArray(x => x.RemoveTaskItem);
        this.OptimizedPages = pagesTaskItemChanges.ToArray(x => x.IncludeTaskItem);
        this.ObsoleteAvaloniaXaml = avaloniaXamlTaskItemChanges.ToArray(x => x.RemoveTaskItem);
        this.OptimizedAvaloniaXaml = avaloniaXamlTaskItemChanges.ToArray(x => x.IncludeTaskItem);
        this.ObsoleteMauiXaml = mauiXamlTaskItemChanges.ToArray(x => x.RemoveTaskItem);
        this.OptimizedMauiXaml = mauiXamlTaskItemChanges.ToArray(x => x.IncludeTaskItem);
        this.ObsoleteEmbeddedXamlResources = embeddedXamlResourcesTaskItemChanges.ToArray(x => x.RemoveTaskItem);
        this.OptimizedEmbeddedXamlResources = embeddedXamlResourcesTaskItemChanges.ToArray(x => x.IncludeTaskItem);
        this.ObsoleteApplicationDefinitions = applicationDefinitionTaskItemChanges.ToArray(x => x.RemoveTaskItem);
        this.OptimizedApplicationDefinitions = applicationDefinitionTaskItemChanges.ToArray(x => x.IncludeTaskItem);
        this.NewCompiles = newCompiles.ToArray();
        this.NewPages = newPages.ToArray();
        this.NewAvaloniaXaml = newAvaloniaXaml.ToArray();
        this.NewMauiXaml = newMauiXaml.ToArray();
        this.NewEmbeddedResources = newEmbeddedResources.ToArray();
        this.NewAdditionalFiles = newAdditionalFiles.ToArray();
    }

    private LinkedList<TaskItemChanges> OptimizeXaml(
        ITaskItem[] taskItems,
        FileReference[] fileReferences,
        DirectoryInfo sxoDirectory,
        IReadOnlyCollection<IXamlOptimizer> xamlOptimizers,
        IList<string> newCompiles,
        IList<string> newPages,
        IList<string> newAvaloniaXaml,
        IList<string> newMauiXaml,
        IList<string> newEmbeddedResources,
        IList<string> newAdditionalFiles,
        XDocumentProvider xDocumentProvider)
    {
        var outputTaskItems = new LinkedList<TaskItemChanges>();
        Parallel.ForEach(fileReferences, (fileReference, state, index) =>
        {
            var taskItem = taskItems[index];
            var xDocument = xDocumentProvider.Get(fileReference);
            var lineEnding = LineEndingDetector.GetLineEnding(xDocument);

            var stopwatch = new Stopwatch();
            var optimization = OptimizationResult.None();
            foreach (var xamlOptimizer in xamlOptimizers)
            {
                stopwatch.Restart();
                var result = xamlOptimizer.Optimize(xDocument, fileReference);
                if (result.IsSuccess)
                {
                    xDocument = result.XDocument;
                    foreach (var additionalFile in result.AdditionalFiles)
                    {
                        switch (additionalFile.FileAction)
                        {
                            case FileAction.Compile:
                                newCompiles.Add(additionalFile.FileInfo.FullName);
                                break;
                            case FileAction.Page:
                                newPages.Add(additionalFile.FileInfo.FullName);
                                break;
                            case FileAction.EmbeddedResource:
                                newEmbeddedResources.Add(additionalFile.FileInfo.FullName);
                                break;
                            case FileAction.AdditionalFile:
                                newAdditionalFiles.Add(additionalFile.FileInfo.FullName);
                                break;
                            case FileAction.AvaloniaXaml:
                                newAvaloniaXaml.Add(additionalFile.FileInfo.FullName);
                                break;
                            case FileAction.MauiXaml:
                                newMauiXaml.Add(additionalFile.FileInfo.FullName);
                                break;
                        }
                    }

                    optimization = result;
                    this.Log.LogMessage(MessageImportance.Normal, LogMessages.ItemOptimized, taskItem.ItemSpec, xamlOptimizer.GetType().Name, stopwatch.Elapsed);
                }
                else
                {
                    this.Log.LogMessage(MessageImportance.Normal, LogMessages.NothingToOptimize, taskItem.ItemSpec, xamlOptimizer.GetType().Name, stopwatch.Elapsed);
                }

                foreach (var xamlDiagnostic in result.XamlDiagnostics)
                {
                    switch (xamlDiagnostic.DiagnosticSeverity)
                    {
                        case DiagnosticSeverity.Info:
                            this.Log.LogMessage(
                                MessageImportance.Normal,
                                xamlDiagnostic.ToMessage());
                            break;
                        case DiagnosticSeverity.Warning:
                            this.Log.LogWarning(
                                xamlOptimizer.GetType().Name,
                                xamlDiagnostic.Code,
                                string.Empty,
                                xamlDiagnostic.FilePath,
                                xamlDiagnostic.LineNumber,
                                xamlDiagnostic.ColumnNumber,
                                xamlDiagnostic.EndLineNumber,
                                xamlDiagnostic.EndColumnNumber,
                                xamlDiagnostic.Message,
                                xamlDiagnostic.MessageArguments);
                            break;
                        case DiagnosticSeverity.Error:
                            this.Log.LogError(
                                xamlOptimizer.GetType().Name,
                                xamlDiagnostic.Code,
                                string.Empty,
                                xamlDiagnostic.FilePath,
                                xamlDiagnostic.LineNumber,
                                xamlDiagnostic.ColumnNumber,
                                xamlDiagnostic.EndLineNumber,
                                xamlDiagnostic.EndColumnNumber,
                                xamlDiagnostic.Message,
                                xamlDiagnostic.MessageArguments);
                            break;
                    }
                }
            }

            if (optimization.IsSuccess)
            {
                var optimizedXamlFilePath = XamlWriter.Save(
                    xDocument,
                    sxoDirectory.FullName,
                    GetLink(taskItem),
                    lineEnding);
                var optimizedTaskItem = new TaskItem(optimizedXamlFilePath);
                taskItem.CopyMetadataTo(optimizedTaskItem);
                optimizedTaskItem.SetMetadata(MetadataNames.Link, GetLink(taskItem));
                outputTaskItems.AddLast(new TaskItemChanges(optimizedTaskItem, taskItem));
            }
        });

        return outputTaskItems;
    }
}