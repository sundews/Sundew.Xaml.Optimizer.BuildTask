// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlFileProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Xaml;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sundew.Xaml.Optimization;
using Sundew.Xaml.Optimization.Xml;
using Sundew.Xaml.Optimizer.BuildTask.Internal;
using Sundew.Xaml.Optimizer.BuildTask.Internal.Build;

/// <summary>Provides efficient loading of XDocuments.</summary>
/// <seealso cref="Sundew.Xaml.Optimization.Xml.IXamlFileProvider" />
public class XamlFileProvider : IXamlFileProvider
{
    /// <summary>Initializes a new instance of the <see cref="XamlFileProvider"/> class.</summary>
    /// <param name="xamlFiles">The file references.</param>
    public XamlFileProvider(IReadOnlyList<XamlFile> xamlFiles)
    {
        this.XamlFiles = xamlFiles;
    }

    /// <summary>Gets the file references.</summary>
    /// <value>The file references.</value>
    public IReadOnlyList<XamlFile> XamlFiles { get; }

    /// <summary>
    /// Load the xaml file.
    /// </summary>
    /// <param name="fileReference">The file reference.</param>
    /// <returns>An async task with the xaml file.</returns>
    public Task<XamlFile> LoadAsync(IFileReference fileReference)
    {
        return InternalLoadAsync(fileReference);
    }

    /// <summary>
    /// Load the xaml file.
    /// </summary>
    /// <param name="fileReference">The file reference.</param>
    /// <returns>An async task with the xaml file.</returns>
    internal static async Task<XamlFile> InternalLoadAsync(IFileReference fileReference)
    {
        var xDocument = XDocument.Parse(
            await FileHelper.ReadAllTextAsync(fileReference.Path),
            LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
        return new XamlFile(
            xDocument,
            fileReference,
            LineEndingDetector.GetLineEnding(xDocument));
    }

    /// <summary>
    /// Load the xaml file.
    /// </summary>
    /// <param name="taskItemFileReference">The file reference.</param>
    /// <returns>An async task with the xaml file.</returns>
    internal static async Task<TaskItemXamlFile> InternalLoadAsync(TaskItemFileReference taskItemFileReference)
    {
        var xDocument = XDocument.Parse(
            await FileHelper.ReadAllTextAsync(taskItemFileReference.Path),
            LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
        return new TaskItemXamlFile(
            xDocument,
            taskItemFileReference,
            LineEndingDetector.GetLineEnding(xDocument),
            taskItemFileReference.TaskItem);
    }
}