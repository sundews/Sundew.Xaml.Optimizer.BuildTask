// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MarkupCompileStateTrackerTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Tests;

using System.Collections.Generic;
using System.IO;
using AwesomeAssertions;
using Sundew.Xaml.Optimizer.BuildTask.Internal.Build;
using TUnit.Core;

public class MarkupCompileStateTrackerTests
{
    private const string PageBeforeMove = @"XXX\YYY\ZZZ\SaveResources.xaml";
    private const string PageAfterMove = @"AAA\BBB\CCC\SaveResources.xaml";
    private const string UnrelatedPage = @"Styles\Shared.xaml";

    [Test]
    public void Evict_WhenThereIsNoPreviousState_ThenMarkupCompileStateIsNotInvalidated()
    {
        using var testDirectory = new TestDirectory();
        var scenario = new Scenario(testDirectory);

        var result = scenario.Build(PageBeforeMove, UnrelatedPage);

        result.Should().BeFalse();
        File.Exists(scenario.StateFilePath).Should().BeTrue();
    }

    [Test]
    public void Evict_WhenAPageMoved_ThenMarkupCompileStateIsInvalidated()
    {
        using var testDirectory = new TestDirectory();
        var scenario = new Scenario(testDirectory);
        scenario.Build(PageBeforeMove, UnrelatedPage);
        scenario.Generate(PageBeforeMove, UnrelatedPage);

        var result = scenario.Build(PageAfterMove, UnrelatedPage);

        result.Should().BeTrue();
    }

    [Test]
    public void Evict_WhenAPageMoved_ThenTheFilesGeneratedForTheOldPathAreEvicted()
    {
        using var testDirectory = new TestDirectory();
        var scenario = new Scenario(testDirectory);
        scenario.Build(PageBeforeMove, UnrelatedPage);
        scenario.Generate(PageBeforeMove, UnrelatedPage);

        scenario.Build(PageAfterMove, UnrelatedPage);

        File.Exists(scenario.GetGeneratedPath(PageBeforeMove, ".baml")).Should().BeFalse();
        File.Exists(scenario.GetGeneratedPath(PageBeforeMove, ".g.cs")).Should().BeFalse();
    }

    [Test]
    public void Evict_WhenAPageMoved_ThenTheFilesGeneratedForUnrelatedPagesAreKept()
    {
        using var testDirectory = new TestDirectory();
        var scenario = new Scenario(testDirectory);
        scenario.Build(PageBeforeMove, UnrelatedPage);
        scenario.Generate(PageBeforeMove, UnrelatedPage);

        scenario.Build(PageAfterMove, UnrelatedPage);

        File.Exists(scenario.GetGeneratedPath(UnrelatedPage, ".baml")).Should().BeTrue();
        File.Exists(scenario.GetGeneratedPath(UnrelatedPage, ".g.cs")).Should().BeTrue();
    }

    [Test]
    public void Evict_WhenNothingChanged_ThenMarkupCompileStateIsNotInvalidated()
    {
        using var testDirectory = new TestDirectory();
        var scenario = new Scenario(testDirectory);
        scenario.Build(PageBeforeMove, UnrelatedPage);
        scenario.Generate(PageBeforeMove, UnrelatedPage);

        var result = scenario.Build(PageBeforeMove, UnrelatedPage);

        result.Should().BeFalse();
        File.Exists(scenario.GetGeneratedPath(PageBeforeMove, ".baml")).Should().BeTrue();
    }

    [Test]
    public void Evict_WhenTheStagingDirectoryOfAPreviousVersionExists_ThenItAndItsGeneratedFilesAreEvicted()
    {
        using var testDirectory = new TestDirectory();
        var scenario = new Scenario(testDirectory);
        var obsoleteStagingDirectory = StagingDirectoryProvider.GetObsoleteDirectory(testDirectory.IntermediateOutputPath);
        testDirectory.CreateFile(Path.Combine(obsoleteStagingDirectory.FullName, UnrelatedPage), "<x/>");
        var obsoleteGeneratedFile = testDirectory.CreateFile(
            Path.Combine(
                testDirectory.IntermediateOutputPath,
                GetProjectRelativePath(testDirectory, obsoleteStagingDirectory.FullName),
                Path.ChangeExtension(UnrelatedPage, ".baml")),
            "baml");

        var result = scenario.Build([PageBeforeMove], null, obsoleteStagingDirectory);

        result.Should().BeTrue();
        Directory.Exists(obsoleteStagingDirectory.FullName).Should().BeFalse();
        File.Exists(obsoleteGeneratedFile).Should().BeFalse();
    }

    [Test]
    public void Evict_WhenAStagedPathIsOutsideTheProjectDirectory_ThenItIsNeitherTrackedNorTouched()
    {
        using var testDirectory = new TestDirectory();
        using var elsewhere = new TestDirectory();
        var scenario = new Scenario(testDirectory);
        var outsideProjectPath = elsewhere.CreateFile("Other.xaml", "<x/>");

        var result = scenario.Build([PageBeforeMove], outsideProjectPath, null);

        result.Should().BeFalse();
        File.ReadAllText(scenario.StateFilePath).Should().NotContain("Other.xaml");
        File.Exists(outsideProjectPath).Should().BeTrue();
    }

    private static string GetProjectRelativePath(TestDirectory testDirectory, string path)
    {
        return path.Substring(testDirectory.ProjectDirectory.Length + 1);
    }

    private sealed class Scenario
    {
        private readonly TestDirectory testDirectory;
        private readonly StubBuildEngine buildEngine = new();
        private readonly string stagingDirectory;

        public Scenario(TestDirectory testDirectory)
        {
            this.testDirectory = testDirectory;
            this.stagingDirectory = StagingDirectoryProvider.Resolve(
                testDirectory.ProjectDirectory,
                testDirectory.BaseIntermediateOutputPath,
                testDirectory.IntermediateOutputPath,
                null).Directory.FullName;
        }

        public string StateFilePath => Path.Combine(this.testDirectory.IntermediateOutputPath, "sxo.filelist");

        public bool Build(params string[] pages)
        {
            return this.Build(pages, null, null);
        }

        /// <summary>
        /// Stages the given pages the way the task does, emptying the staging directory first, and then evicts.
        /// </summary>
        public bool Build(IEnumerable<string> pages, string? outsideProjectPath, DirectoryInfo? obsoleteStagingDirectory)
        {
            if (Directory.Exists(this.stagingDirectory))
            {
                Directory.Delete(this.stagingDirectory, true);
            }

            var stagedFilePaths = new List<string>();
            foreach (var page in pages)
            {
                stagedFilePaths.Add(this.testDirectory.CreateFile(Path.Combine(this.stagingDirectory, page), "<x/>"));
            }

            if (outsideProjectPath != null)
            {
                stagedFilePaths.Add(outsideProjectPath);
            }

            return new MarkupCompileStateTracker(
                    this.StateFilePath,
                    this.testDirectory.ProjectDirectory,
                    this.testDirectory.IntermediateOutputPath,
                    this.buildEngine.CreateLog())
                .Evict(stagedFilePaths, obsoleteStagingDirectory);
        }

        /// <summary>
        /// Writes the files the markup compiler would generate for the given pages, being the intermediate output path
        /// combined with the path of the staged xaml file relative to the project directory.
        /// </summary>
        public void Generate(params string[] pages)
        {
            foreach (var page in pages)
            {
                this.testDirectory.CreateFile(this.GetGeneratedPath(page, ".baml"), "baml");
                this.testDirectory.CreateFile(this.GetGeneratedPath(page, ".g.cs"), "code");
            }
        }

        public string GetGeneratedPath(string page, string extension)
        {
            var stagedRelativePath = GetProjectRelativePath(this.testDirectory, Path.Combine(this.stagingDirectory, page));
            return Path.Combine(this.testDirectory.IntermediateOutputPath, Path.ChangeExtension(stagedRelativePath, extension));
        }
    }
}
