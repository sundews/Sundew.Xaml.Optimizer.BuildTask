// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LineEndingDetector.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Xaml;

using System;
using System.Linq;
using System.Xml.Linq;

/// <summary>
/// Detects line ending in a <see cref="XDocument"/>.
/// </summary>
public static class LineEndingDetector
{
    private const string WinNewLine = "\r\n";
    private const string UnixNewLine = "\n";

    /// <summary>
    /// Gets the line ending.
    /// </summary>
    /// <param name="xDocument">The x document.</param>
    /// <returns>The line ending.</returns>
    public static string GetLineEnding(XDocument xDocument)
    {
        var element = xDocument.Root;
        if (element != null)
        {
            var node = element.FirstNode;
            for (var i = 0; i < 10; i++)
            {
                if (node is XText xText)
                {
                    var text = xText.Value;
                    if (text != null)
                    {
                        if (text.Contains(WinNewLine))
                        {
                            return WinNewLine;
                        }

                        if (text.Contains(UnixNewLine))
                        {
                            return UnixNewLine;
                        }
                    }
                }

                node = element.Elements()?.FirstOrDefault()?.FirstNode;
            }
        }

        return Environment.NewLine;
    }
}