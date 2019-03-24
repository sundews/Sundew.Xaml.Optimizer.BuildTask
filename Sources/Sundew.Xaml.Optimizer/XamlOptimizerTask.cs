// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlOptimizerTask.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Sundew.Base.Collections;
    using Sundew.Xaml.Optimization;
    using Sundew.Xaml.Optimizer.Build;
    using Sundew.Xaml.Optimizer.Factory;
    using Sundew.Xaml.Optimizer.Logging;
    using Sundew.Xaml.Optimizer.Xaml;

    /// <summary>
    /// MsBuild task that analyzes and optimizes Pages, ApplicationDefinitions and EmbeddedXamlResources.
    /// </summary>
    /// <seealso cref="Microsoft.Build.Utilities.Task" />
    public sealed class XamlOptimizerTask : Task
    {
        /// <summary>Gets or sets the package references.</summary>
        /// <value>The package references.</value>
        [Required]
        public ITaskItem[] PackageReferences { get; set; }

        /// <summary>Gets or sets the target platform identifier.</summary>
        /// <value>The target platform identifier.</value>
        [Required]
        public string TargetPlatformIdentifier { get; set; }

        /// <summary>Gets or sets the solution directory.</summary>
        /// <value>The solution directory.</value>
        [Required]
        public string SolutionDirectory { get; set; }

        /// <summary>Gets or sets the project directory.
        /// </summary>
        /// <value>
        /// The project directory.
        /// </value>
        [Required]
        public string ProjectDirectory { get; set; }

        /// <summary>
        /// Gets or sets the intermediate output path.
        /// </summary>
        /// <value>
        /// The intermediate output path.
        /// </value>
        [Required]
        public string IntermediateOutputPath { get; set; }

        /// <summary>
        /// Gets or sets the application definitions.
        /// </summary>
        /// <value>
        /// The application definitions.
        /// </value>
        [Required]
        public ITaskItem[] ApplicationDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the pages.
        /// </summary>
        /// <value>
        /// The pages.
        /// </value>
        [Required]
        public ITaskItem[] Pages { get; set; }

        /// <summary>
        /// Gets or sets the embedded xaml resources.
        /// </summary>
        /// <value>
        /// The embedded xaml resources.
        /// </value>
        [Required]
        public ITaskItem[] EmbeddedXamlResources { get; set; }

        /// <summary>
        /// Gets the optimized pages.
        /// </summary>
        /// <value>
        /// The optimized pages.
        /// </value>
        [Output]
        public ITaskItem[] OptimizedPages { get; private set; }

        /// <summary>
        /// Gets the obsolete pages.
        /// </summary>
        /// <value>
        /// The obsolete pages.
        /// </value>
        [Output]
        public ITaskItem[] ObsoletePages { get; private set; }

        /// <summary>
        /// Gets the optimized application definitions.
        /// </summary>
        /// <value>
        /// The optimized application definitions.
        /// </value>
        [Output]
        public ITaskItem[] OptimizedApplicationDefinitions { get; private set; }

        /// <summary>
        /// Gets the obsolete application definitions.
        /// </summary>
        /// <value>
        /// The obsolete application definitions.
        /// </value>
        [Output]
        public ITaskItem[] ObsoleteApplicationDefinitions { get; private set; }

        /// <summary>
        /// Gets the optimized embedded xaml resources.
        /// </summary>
        /// <value>
        /// The optimized embedded xaml resources.
        /// </value>
        [Output]
        public ITaskItem[] OptimizedEmbeddedXamlResources { get; private set; }

        /// <summary>
        /// Gets the obsolete embedded xaml resources.
        /// </summary>
        /// <value>
        /// The obsolete embedded xaml resources.
        /// </value>
        [Output]
        public ITaskItem[] ObsoleteEmbeddedXamlResources { get; private set; }

        /// <summary>Gets the new pages.</summary>
        [Output]
        public string[] NewPages { get; private set; }

        /// <summary>Gets the new embedded resources.</summary>
        [Output]
        public string[] NewEmbeddedResources { get; private set; }

        /// <summary>Gets the new compiles.</summary>
        [Output]
        public string[] NewCompiles { get; private set; }

        /// <summary>
        /// Executes the xaml resource optimization.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            this.Log.LogMessage(MessageImportance.Normal, LogMessages.StartingOptimization);
            var xamlPlatform = XamlPlatformProvider.DefectFramework(this.TargetPlatformIdentifier, this.PackageReferences);
            var frameworkXmlDefinitions = XamPlatformInfoProvider.GetFrameworkXmlDefinitions(xamlPlatform);
            var xamlOptimizerFactory = new XamlOptimizerFactory(new MsBuildXamlOptimizerFactoryLogger(this.Log));
            var xamlOptimizers = xamlOptimizerFactory.CreateXamlOptimizers(this.ProjectDirectory, xamlPlatform, frameworkXmlDefinitions, this.SolutionDirectory).ToArray();

            var newCompiles = new List<string>();
            var newPages = new List<string>();
            var newEmbeddedResources = new List<string>();
            var intermediateDirectory = new DirectoryInfo(this.IntermediateOutputPath);
            var pagesTaskItemChanges = this.OptimizeXaml(this.Pages, intermediateDirectory, xamlOptimizers, newCompiles, newPages, newEmbeddedResources);
            var embeddedXamlResourcesTaskItemChanges = this.OptimizeXaml(this.EmbeddedXamlResources, intermediateDirectory, xamlOptimizers, newCompiles, newPages, newEmbeddedResources);
            var applicationDefinitionTaskItemChanges = this.OptimizeXaml(this.ApplicationDefinitions, intermediateDirectory, xamlOptimizers, newCompiles, newPages, newEmbeddedResources);

            this.ObsoletePages = pagesTaskItemChanges.ToArray(x => x.RemoveTaskItem);
            this.OptimizedPages = pagesTaskItemChanges.ToArray(x => x.IncludeTaskItem);
            this.ObsoleteEmbeddedXamlResources = embeddedXamlResourcesTaskItemChanges.ToArray(x => x.RemoveTaskItem);
            this.OptimizedEmbeddedXamlResources = embeddedXamlResourcesTaskItemChanges.ToArray(x => x.IncludeTaskItem);
            this.ObsoleteApplicationDefinitions = applicationDefinitionTaskItemChanges.ToArray(x => x.RemoveTaskItem);
            this.OptimizedApplicationDefinitions = applicationDefinitionTaskItemChanges.ToArray(x => x.IncludeTaskItem);
            this.NewCompiles = newCompiles.ToArray();
            this.NewPages = newPages.ToArray();
            this.NewEmbeddedResources = newEmbeddedResources.ToArray();

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

        private LinkedList<TaskItemChanges> OptimizeXaml(
            ITaskItem[] taskItems,
            DirectoryInfo sxoDirectory,
            IReadOnlyCollection<IXamlOptimizer> xamlOptimizers,
            IList<string> newCompiles,
            IList<string> newPages,
            IList<string> newEmbeddedResources)
        {
            var outputTaskItems = new LinkedList<TaskItemChanges>();
            foreach (var taskItem in taskItems)
            {
                var fileInfo = new FileInfo(taskItem.GetMetadata(MetadataNames.FullPath));
                var xDocument = XDocument.Parse(
                    File.ReadAllText(fileInfo.FullName),
                    LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

                foreach (var xamlOptimizer in xamlOptimizers)
                {
                    var result = xamlOptimizer.Optimize(fileInfo, xDocument, sxoDirectory);
                    if (result)
                    {
                        xDocument = result.Value.XDocument;
                        var optimizedXamlFilePath = XamlWriter.Save(
                            xDocument,
                            sxoDirectory.FullName,
                            taskItem.GetMetadata(MetadataNames.Identity));
                        var optimizedTaskItem = new TaskItem(optimizedXamlFilePath);
                        taskItem.CopyMetadataTo(optimizedTaskItem);
                        optimizedTaskItem.SetMetadata(MetadataNames.Link, GetLink(taskItem));
                        outputTaskItems.AddLast(new TaskItemChanges(optimizedTaskItem, taskItem));

                        foreach (var additionalFile in result.Value.AdditionalFiles)
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
                            }
                        }

                        this.Log.LogMessage(MessageImportance.Normal, LogMessages.ItemOptimized, taskItem.ItemSpec, xamlOptimizer.GetType().Name);
                    }
                    else
                    {
                        this.Log.LogMessage(MessageImportance.Normal, LogMessages.NothingToOptimize, taskItem.ItemSpec, xamlOptimizer.GetType().Name);
                    }
                }
            }

            return outputTaskItems;
        }
    }
}