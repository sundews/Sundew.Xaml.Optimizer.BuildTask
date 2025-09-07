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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Sundew.Base;
using Sundew.Base.Collections;
using Sundew.Base.Collections.Linq;
using Sundew.Xaml.Optimization;
using Sundew.Xaml.Optimization.Xml;
using Sundew.Xaml.Optimizer.BuildTask.Factory;
using Sundew.Xaml.Optimizer.BuildTask.Internal;
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
    /// Gets or sets a value indicating whether warnings are treated as errors.
    /// </summary>
    public bool TreatWarningsAsErrors { get; set; }

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
    public ITaskItem[]? NewPages { get; private set; }

    /// <summary>Gets the new avalonia xaml.</summary>
    [Output]
    public ITaskItem[]? NewAvaloniaXaml { get; private set; }

    /// <summary>Gets the new maui xaml.</summary>
    [Output]
    public ITaskItem[]? NewMauiXaml { get; private set; }

    /// <summary>Gets the new embedded resources.</summary>
    [Output]
    public ITaskItem[]? NewEmbeddedResources { get; private set; }

    /// <summary>Gets the new compiles.</summary>
    [Output]
    public ITaskItem[]? NewCompiles { get; private set; }

    /// <summary>Gets the new additional files.</summary>
    [Output]
    public ITaskItem[]? NewAdditionalFiles { get; private set; }

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
            var compiles = new TaskItemLazyList<TaskItemFileReference>(
                this.Compiles,
                (x, index) => new TaskItemFileReference(x, BuildAction.Compile));
            var assemblyReferences =
                new TaskItemLazyList<AssemblyReference>(
                    this.ReferencePaths,
                    (x, index) => new AssemblyReference(x));
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

    private static async System.Threading.Tasks.Task AddAdditionalFiles(
        IReadOnlyCollection<AdditionalFile> additionalFiles,
        NewItems<List<ITaskItem>> newItems)
    {
        foreach (var additionalFile in additionalFiles)
        {
            var directory = additionalFile.FileInfo.Directory;
            if (directory is { Exists: false })
            {
                Directory.CreateDirectory(directory.FullName);
            }

            await FileHelper.WriteAllTextAsync(additionalFile.FileInfo.FullName, additionalFile.Content).ConfigureAwait(false);

            var taskItem = new TaskItem(additionalFile.FileInfo.FullName);
            if (additionalFile.Link.HasValue())
            {
                taskItem.SetMetadata(MetadataNames.Link, additionalFile.Link);
            }

            switch (additionalFile.ItemType)
            {
                case ItemType.Compile:
                    newItems.CompileItems.Add(taskItem);
                    break;
                case ItemType.Page:
                    newItems.PageItems.Add(taskItem);
                    break;
                case ItemType.EmbeddedResource:
                    newItems.EmbeddedResourceItems.Add(taskItem);
                    break;
                case ItemType.AdditionalFile:
                    newItems.AdditionalFileItems.Add(taskItem);
                    break;
                case ItemType.AvaloniaXaml:
                    newItems.AvaloniaXamlItems.Add(taskItem);
                    break;
                case ItemType.MauiXaml:
                    newItems.MauiXamlItems.Add(taskItem);
                    break;
            }
        }
    }

    private static List<TaskItemChanges>? GetTaskItems(
        XamlFileChange xamlFileChange,
        ApplicationXamlItems<List<TaskItemChanges>> applicationXamlTaskItems)
    {
        return xamlFileChange.File.Reference.BuildAction switch
        {
            BuildAction.Page => applicationXamlTaskItems.PageItems,
            BuildAction.AssemblyReference => null,
            BuildAction.Compile => null,
            BuildAction.MauiXaml => applicationXamlTaskItems.MauiXamlItems,
            BuildAction.AvaloniaXaml => applicationXamlTaskItems.AvaloniaXamlItems,
            BuildAction.EmbeddedResource => applicationXamlTaskItems.EmbeddedResourceItems,
            BuildAction.ApplicationDefinition => applicationXamlTaskItems.ApplicationDefinition,
            _ => null,
        };
    }

    private static string GetLinkPath(IFileReference fileReference)
    {
        var link = fileReference[MetadataNames.Link];
        if (string.IsNullOrEmpty(link))
        {
            link = fileReference[MetadataNames.Identity];
        }

        return link;
    }

    private void Optimize(
        XamlPlatformInfo xamlPlatformInfo,
        XamlOptimizerFactory xamlOptimizerFactory,
        TaskItemLazyList<TaskItemFileReference> compiles,
        TaskItemLazyList<AssemblyReference> assemblyReferences,
        DirectoryInfo intermediateDirectory,
        IReadOnlyCollection<SxoSettings> sxoSettings,
        IReadOnlyList<string> xamlOptimizerPaths,
        AssemblyResolver assemblyResolver)
    {
        var pages = this.Pages.Select((x, i) => new TaskItemFileReference(x, BuildAction.Page));
        var avaloniaXaml =
            this.AvaloniaXaml.Select((x, i) => new TaskItemFileReference(x, BuildAction.AvaloniaXaml));
        var mauiXaml = this.MauiXaml.Select((x, i) => new TaskItemFileReference(x, BuildAction.MauiXaml));
        var embeddedXamlResources =
            this.EmbeddedXamlResources.Select((x, i) => new TaskItemFileReference(x, BuildAction.EmbeddedResource));
        var applicationDefinitions = this.ApplicationDefinitions.Select((x, i) =>
            new TaskItemFileReference(x, BuildAction.ApplicationDefinition));

        var newItems = new NewItems<List<ITaskItem>>(() => new List<ITaskItem>());
        var pagesData = pages.ToArray(XamlFileProvider.InternalLoadAsync);
        var avaloniaXamlData = avaloniaXaml.ToArray(XamlFileProvider.InternalLoadAsync);
        var mauiXamlData = mauiXaml.ToArray(XamlFileProvider.InternalLoadAsync);
        var embeddedXamlResourcesData = embeddedXamlResources.ToArray(XamlFileProvider.InternalLoadAsync);
        var applicationDefinitionsData = applicationDefinitions.ToArray(XamlFileProvider.InternalLoadAsync);
        System.Threading.Tasks.Task.WaitAll(pagesData.Concat(avaloniaXamlData, mauiXamlData, embeddedXamlResourcesData, applicationDefinitionsData));
        var applicationXamlItems = new ApplicationXamlItems<IEnumerable<TaskItemXamlFile>>(
            System.Threading.Tasks.Task.WhenAll(pagesData).Result,
            System.Threading.Tasks.Task.WhenAll(avaloniaXamlData).Result,
            System.Threading.Tasks.Task.WhenAll(mauiXamlData).Result,
            System.Threading.Tasks.Task.WhenAll(embeddedXamlResourcesData).Result,
            System.Threading.Tasks.Task.WhenAll(applicationDefinitionsData).Result);

        var xamlFileProvider = new XamlFileProvider(
            applicationXamlItems.PageItems
                .Concat(
                    applicationXamlItems.AvaloniaXamlItems,
                    applicationXamlItems.MauiXamlItems,
                    applicationXamlItems.EmbeddedResourceItems,
                    applicationXamlItems.ApplicationDefinition));

        var projectInfo = new ProjectInfo(this.AssemblyName, this.RootNamespace, new DirectoryInfo(this.ProjectDirectory), new DirectoryInfo(this.SolutionDirectory), intermediateDirectory, assemblyReferences, compiles, xamlFileProvider, this.Debug);
        var xamlOptimizers = xamlOptimizerFactory
            .CreateXamlOptimizers(assemblyResolver, xamlPlatformInfo, projectInfo, xamlOptimizerPaths, sxoSettings)
            .ToArray();
        var stopwatch = new Stopwatch();
        var optimizationResults = xamlOptimizers.SelectAsync(async xamlOptimizer =>
        {
            try
            {
                stopwatch.Restart();
                var optimizationResult = await xamlOptimizer.OptimizeAsync(xamlFileProvider.XamlFiles);
                if (optimizationResult.IsSuccess)
                {
                    this.Log.LogMessage(MessageImportance.Normal, LogMessages.ItemsOptimized, xamlOptimizer.GetType().Name, stopwatch.Elapsed);
                    await AddAdditionalFiles(optimizationResult.AdditionalFiles, newItems).ConfigureAwait(false);
                }
                else
                {
                    this.Log.LogMessage(MessageImportance.Normal, LogMessages.NothingToOptimize, xamlOptimizer.GetType().Name, stopwatch.Elapsed);
                }

                this.ReportDiagnostics(optimizationResult.XamlDiagnostics, xamlOptimizer);
                return optimizationResult;
            }
            catch (Exception e)
            {
                this.Log.LogErrorFromException(e);
                throw;
            }
        }).Result;

        var applicationXamlTaskItems =
            new ApplicationXamlItems<List<TaskItemChanges>>(
                () => new List<TaskItemChanges>());

        var xamlFileChanges = new HashSet<XamlFileChange>(new XamlFileChangeIdentityEqualityComparer());
        optimizationResults.Reverse();
        foreach (var xamlFileChange in optimizationResults.SelectMany(x => x.XamlFileChanges))
        {
            xamlFileChanges.Add(xamlFileChange);
        }

        foreach (var xamlFileChange in xamlFileChanges)
        {
            switch (xamlFileChange.Action)
            {
                case XamlFileAction.None:
                    break;
                case XamlFileAction.Remove:
                    if (xamlFileChange.File.Reference is TaskItemFileReference taskItemFileReferenceToRemove)
                    {
                        var taskItems = GetTaskItems(xamlFileChange, applicationXamlTaskItems);
                        if (taskItems.HasValue())
                        {
                            taskItems.Add(new TaskItemChanges(null, taskItemFileReferenceToRemove.TaskItem));
                        }
                    }

                    break;
                case XamlFileAction.Update:
                    var evaluatedPath = GetLinkPath(xamlFileChange.File.Reference);
                    var optimizedXamlFilePath = XamlWriter.Save(
                        xamlFileChange.File.Document,
                        intermediateDirectory.FullName,
                        evaluatedPath,
                        xamlFileChange.File.LineEndings);
                    if (xamlFileChange.File.Reference is TaskItemFileReference taskItemFileReference)
                    {
                        var oldTaskItem = taskItemFileReference.TaskItem;
                        var optimizedTaskItem = new TaskItem(optimizedXamlFilePath);
                        oldTaskItem.CopyMetadataTo(optimizedTaskItem);
                        optimizedTaskItem.SetMetadata(MetadataNames.Link, evaluatedPath);
                        var taskItems = GetTaskItems(xamlFileChange, applicationXamlTaskItems);
                        if (taskItems.HasValue())
                        {
                            taskItems.Add(new TaskItemChanges(optimizedTaskItem, oldTaskItem));
                        }
                    }

                    break;
            }
        }

        this.ObsoletePages = applicationXamlTaskItems.PageItems.ToArray(changes => changes.RemoveTaskItem);
        this.OptimizedPages = applicationXamlTaskItems.PageItems.Select(x => x.IncludeTaskItem).WhereNotNull().ToArray();
        this.ObsoleteAvaloniaXaml = applicationXamlTaskItems.AvaloniaXamlItems.ToArray(changes => changes.RemoveTaskItem);
        this.OptimizedAvaloniaXaml = applicationXamlTaskItems.AvaloniaXamlItems.Select(x => x.IncludeTaskItem).WhereNotNull().ToArray();
        this.ObsoleteMauiXaml = applicationXamlTaskItems.MauiXamlItems.ToArray(changes => changes.RemoveTaskItem);
        this.OptimizedMauiXaml = applicationXamlTaskItems.MauiXamlItems.Select(x => x.IncludeTaskItem).WhereNotNull().ToArray();
        this.ObsoleteEmbeddedXamlResources = applicationXamlTaskItems.EmbeddedResourceItems.ToArray(changes => changes.RemoveTaskItem);
        this.OptimizedEmbeddedXamlResources = applicationXamlTaskItems.EmbeddedResourceItems.Select(x => x.IncludeTaskItem).WhereNotNull().ToArray();
        this.ObsoleteApplicationDefinitions = applicationXamlTaskItems.ApplicationDefinition.ToArray(changes => changes.RemoveTaskItem);
        this.OptimizedApplicationDefinitions = applicationXamlTaskItems.ApplicationDefinition.Select(x => x.IncludeTaskItem).WhereNotNull().ToArray();
        this.NewCompiles = newItems.CompileItems.ToArray();
        this.NewPages = newItems.PageItems.ToArray();
        this.NewAvaloniaXaml = newItems.AvaloniaXamlItems.ToArray();
        this.NewMauiXaml = newItems.MauiXamlItems.ToArray();
        this.NewEmbeddedResources = newItems.EmbeddedResourceItems.ToArray();
        this.NewAdditionalFiles = newItems.AdditionalFileItems.ToArray();
    }

    private void ReportDiagnostics(IReadOnlyCollection<XamlDiagnostic> xamlDiagnostics, IXamlOptimizer xamlOptimizer)
    {
        foreach (var xamlDiagnostic in xamlDiagnostics)
        {
            var diagnosticSeverity = xamlDiagnostic.DiagnosticSeverity;
            diagnosticSeverity = diagnosticSeverity == DiagnosticSeverity.Warning && this.TreatWarningsAsErrors ? DiagnosticSeverity.Error : diagnosticSeverity;
            switch (diagnosticSeverity)
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
}