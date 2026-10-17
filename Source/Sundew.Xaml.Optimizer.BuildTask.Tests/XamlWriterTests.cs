// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlWriterTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Tests;

using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Sundew.Xaml.Optimizer.BuildTask.Internal;
using Sundew.Xaml.Optimizer.BuildTask.Xaml;
using TUnit.Core;

public class XamlWriterTests
{
    private const string SingleLineRoot = """
                                          <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">{0}  <SolidColorBrush x:Key="B" Color="Red" />{0}</ResourceDictionary>
                                          """;

    private const string MultiLineRoot = """
                                         <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"{0}                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">{0}  <SolidColorBrush x:Key="B" Color="Red" />{0}</ResourceDictionary>
                                         """;

    /// <summary>
    /// <see cref="XDocument"/> normalizes all line endings to a line feed while parsing, so the line endings of the
    /// original file cannot be recovered. The staged xaml therefore has to use line feeds throughout rather than a mix
    /// of the two, which is what happened while the root attribute line breaks were written separately.
    /// </summary>
    [Test]
    [Arguments("single line root, CRLF source", SingleLineRoot, "\r\n")]
    [Arguments("single line root, LF source", SingleLineRoot, "\n")]
    [Arguments("multi line root attributes, CRLF source", MultiLineRoot, "\r\n")]
    [Arguments("multi line root attributes, LF source", MultiLineRoot, "\n")]
    public async Task Save_ThenTheStagedXamlUsesLineFeedsThroughout(string name, string template, string sourceLineEnding)
    {
        using var testDirectory = new TestDirectory();
        var sourcePath = testDirectory.CreateFile(name + ".xaml", string.Format(template, sourceLineEnding));
        var xDocument = XDocument.Parse(
            await FileHelper.ReadAllTextAsync(sourcePath),
            LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

        var stagedPath = XamlWriter.Save(xDocument, testDirectory.ProjectDirectory, name + ".staged.xaml");

        var staged = File.ReadAllText(stagedPath);
        staged.Should().NotContain("\r");
        Regex.IsMatch(staged, "(?<!\r)\n").Should().BeTrue();
    }

    [Test]
    [Arguments(SingleLineRoot)]
    [Arguments(MultiLineRoot)]
    public async Task Save_ThenTheStagedXamlRemainsValid(string template)
    {
        using var testDirectory = new TestDirectory();
        var sourcePath = testDirectory.CreateFile("Source.xaml", string.Format(template, "\r\n"));
        var xDocument = XDocument.Parse(
            await FileHelper.ReadAllTextAsync(sourcePath),
            LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

        var stagedPath = XamlWriter.Save(xDocument, testDirectory.ProjectDirectory, "Staged.xaml");

        var staged = XDocument.Parse(File.ReadAllText(stagedPath));
        staged.Root.Should().NotBeNull();
        staged.Root!.Name.LocalName.Should().Be("ResourceDictionary");
        staged.ToString().Should().Contain("SolidColorBrush");
    }

    [Test]
    public async Task Save_WhenTheStagedPathIsNested_ThenTheDirectoriesAreCreated()
    {
        using var testDirectory = new TestDirectory();
        var sourcePath = testDirectory.CreateFile("Source.xaml", string.Format(SingleLineRoot, "\n"));
        var xDocument = XDocument.Parse(
            await FileHelper.ReadAllTextAsync(sourcePath),
            LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);

        var stagedPath = XamlWriter.Save(xDocument, testDirectory.ProjectDirectory, @"AAA\BBB\CCC\Staged.xaml");

        File.Exists(stagedPath).Should().BeTrue();
    }

    [Test]
    public void LineEnding_ThenItIsALineFeed()
    {
        XamlWriter.LineEnding.Should().Be("\n");
    }
}
