// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlWriter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Xaml;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Xml;
using System.Xml.Linq;

/// <summary>
/// Optimizer for xaml files, which optimizes the use of merge resource dictionaries.
/// </summary>
public static class XamlWriter
{
    /// <summary>
    /// Optimizes the xaml.
    /// </summary>
    /// <param name="xDocument">The x document.</param>
    /// <param name="sxoDirectory">The sxo directory.</param>
    /// <param name="inputIdentity">The input identity.</param>
    /// <param name="lineEnding">The line ending.</param>
    /// <returns>
    /// The output file path.
    /// </returns>
    public static string Save(XDocument xDocument, string sxoDirectory, string inputIdentity, string lineEnding)
    {
        var outputPath = Path.Combine(sxoDirectory, inputIdentity);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        var xmlWriterSettings = new XmlWriterSettings
        {
            OmitXmlDeclaration = xDocument.Declaration == null,
            NewLineChars = lineEnding,
            NewLineHandling = NewLineHandling.Replace,
            ConformanceLevel = xDocument.Declaration == null ? ConformanceLevel.Fragment : ConformanceLevel.Document,
            Indent = true,
        };

        using (var streamWriter = new StreamWriter(outputPath))
        using (var rootElementAttributesLinePreserverXmlWriter = new RootElementAttributesLinePreserverXmlWriter(xDocument, streamWriter, xmlWriterSettings))
        using (var xmlWriter = XmlWriter.Create(rootElementAttributesLinePreserverXmlWriter, xmlWriterSettings))
        {
            xDocument.Save(xmlWriter);
        }

        return outputPath;
    }

    private sealed class RootElementAttributesLinePreserverXmlWriter : XmlTextWriter
    {
        private readonly IReadOnlyList<XAttribute> attributes;
        private readonly StreamWriter streamWriter;
        private int elementCount = -1;
        private int attributeCount = 0;

        public RootElementAttributesLinePreserverXmlWriter(XDocument xDocument, StreamWriter streamWriter, XmlWriterSettings xmlWriterSettings)
            : base(streamWriter)
        {
            this.attributes = xDocument.Root?.Attributes().ToImmutableArray() ?? [];
            this.streamWriter = streamWriter;
            this.Settings = xmlWriterSettings;
        }

        public override XmlWriterSettings Settings { get; }

        public override void WriteStartDocument()
        {
            if (!this.Settings.OmitXmlDeclaration)
            {
                base.WriteStartDocument();
            }
        }

        public override void WriteEndDocument()
        {
            if (!this.Settings.OmitXmlDeclaration)
            {
                base.WriteEndDocument();
            }
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            base.WriteStartElement(prefix, localName, ns);
            if (this.elementCount < 1)
            {
                this.elementCount++;
            }
        }

        public override void WriteEndAttribute()
        {
            base.WriteEndAttribute();
            if (this.elementCount == 0)
            {
                if (this.attributes[this.attributeCount++] is IXmlLineInfo xmlLineInfo)
                {
                    var nextLineNumber = 0;
                    if (this.attributeCount < this.attributes.Count && this.attributes[this.attributeCount] is IXmlLineInfo nextXmlLineInfo)
                    {
                        nextLineNumber = nextXmlLineInfo.LineNumber;
                    }

                    if (xmlLineInfo.LineNumber < nextLineNumber)
                    {
                        this.streamWriter.Write(this.Settings.NewLineChars);
                    }
                }
            }
        }
    }
}