// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XDocumentProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Xaml;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Sundew.Xaml.Optimization;
using Sundew.Xaml.Optimization.Xml;

/// <summary>Provides efficient loading of XDocuments.</summary>
/// <seealso cref="Sundew.Xaml.Optimization.Xml.IXDocumentProvider" />
public class XDocumentProvider : IXDocumentProvider
{
    private readonly ConcurrentDictionary<string, XDocument> xDocuments = new ConcurrentDictionary<string, XDocument>();

    /// <summary>Initializes a new instance of the <see cref="XDocumentProvider"/> class.</summary>
    /// <param name="fileReferences">The file references.</param>
    public XDocumentProvider(IReadOnlyList<IFileReference> fileReferences)
    {
        this.FileReferences = fileReferences;
    }

    /// <summary>Gets the file references.</summary>
    /// <value>The file references.</value>
    public IReadOnlyList<IFileReference> FileReferences { get; }

    /// <summary>Gets the specified file reference.</summary>
    /// <param name="fileReference">The file reference.</param>
    /// <returns>The xDocument.</returns>
    public XDocument Get(IFileReference fileReference)
    {
        return this.xDocuments.GetOrAdd(fileReference.Path, key =>
            XDocument.Parse(
                File.ReadAllText(fileReference.Path),
                LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri));
    }
}