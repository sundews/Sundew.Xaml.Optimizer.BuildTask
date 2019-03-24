// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlWriter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Xaml
{
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Optimizer for xaml files, which optimizes the use of merge resource dictionaries.
    /// </summary>
    public static class XamlWriter
    {
        /// <summary>Optimizes the xaml.</summary>
        /// <param name="xDocument">The x document.</param>
        /// <param name="sxoDirectory">The sxo directory.</param>
        /// <param name="inputIdentity">The input identity.</param>
        /// <returns>The output file path.</returns>
        public static string Save(XDocument xDocument, string sxoDirectory, string inputIdentity)
        {
            var outputPath = Path.Combine(sxoDirectory, inputIdentity);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var xmlWriterSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = xDocument.Declaration == null,
                NewLineHandling = NewLineHandling.None,
                NewLineOnAttributes = true,
            };
            using (var xmlWriter = XmlWriter.Create(outputPath, xmlWriterSettings))
            {
                xDocument.Save(xmlWriter);
            }

            return outputPath;
        }
    }
}