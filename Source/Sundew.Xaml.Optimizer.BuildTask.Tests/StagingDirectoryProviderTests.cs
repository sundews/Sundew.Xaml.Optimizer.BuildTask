// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StagingDirectoryProviderTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Tests;

using System.IO;
using AwesomeAssertions;
using Sundew.Xaml.Optimizer.BuildTask.Internal.Build;
using TUnit.Core;

public class StagingDirectoryProviderTests
{
    [Test]
    public void Resolve_ThenPivotIsRelativeToTheBaseIntermediateOutputPath()
    {
        using var testDirectory = new TestDirectory();

        var result = Resolve(testDirectory);

        result.Pivot.Should().Be(Path.Combine("Debug", "net10.0-windows10.0.17763.0"));
    }

    [Test]
    public void Resolve_WhenCalledRepeatedly_ThenStagingDirectoryIsUnchanged()
    {
        using var testDirectory = new TestDirectory();

        var first = Resolve(testDirectory);
        var second = Resolve(testDirectory);

        second.Directory.FullName.Should().Be(first.Directory.FullName);
    }

    /// <summary>
    /// Pins the hash to a known value. The staging directory has to be identical in every process that builds the
    /// project, which rules out <see cref="object.GetHashCode"/> for strings as it is randomized per process. Were it
    /// used, the staging directory would move on every build and leave the previous one behind.
    /// </summary>
    [Test]
    public void Resolve_ThenStagingDirectoryIsNamedByAStableHashOfThePivot()
    {
        using var testDirectory = new TestDirectory();

        var result = Resolve(testDirectory);

        result.Directory.FullName.Should().Be(Path.Combine(testDirectory.ProjectDirectory, "obj", "sxo", "b5e95ae5"));
    }

    [Test]
    [Arguments("Release", "net10.0-windows10.0.17763.0")]
    [Arguments("Debug", "net8.0-windows")]
    [Arguments("Debug", "net9.0-windows10.0.17763.0")]
    public void Resolve_WhenThePivotDiffers_ThenStagingDirectoryDiffers(string configuration, string targetFramework)
    {
        using var testDirectory = new TestDirectory();

        var debug = Resolve(testDirectory);
        var other = StagingDirectoryProvider.Resolve(
            testDirectory.ProjectDirectory,
            testDirectory.BaseIntermediateOutputPath,
            testDirectory.GetIntermediateOutputPath(configuration, targetFramework),
            null);

        other.Directory.FullName.Should().NotBe(debug.Directory.FullName);
    }

    [Test]
    public void Resolve_WhenTheProjectIsCheckedOutElsewhere_ThenTheStagingDirectoryKeepsTheSameRelativePath()
    {
        using var here = new TestDirectory();
        using var there = new TestDirectory();

        var relativeHere = Relative(here, Resolve(here).Directory.FullName);
        var relativeThere = Relative(there, Resolve(there).Directory.FullName);

        relativeThere.Should().Be(relativeHere);
    }

    [Test]
    public void Resolve_WhenAnOverrideIsSpecified_ThenTheOverrideIsUsed()
    {
        using var testDirectory = new TestDirectory();
        var expected = Path.Combine(testDirectory.ProjectDirectory, "custom", "stage");

        var result = StagingDirectoryProvider.Resolve(
            testDirectory.ProjectDirectory,
            testDirectory.BaseIntermediateOutputPath,
            testDirectory.IntermediateOutputPath,
            expected);

        result.Directory.FullName.Should().Be(expected);
    }

    /// <summary>
    /// The markup compiler resolves generated files as the intermediate output path combined with the path of the xaml
    /// file relative to the project directory, so the staging directory is repeated in every generated path. Staging
    /// inside the intermediate output path repeated it in full, which is what this guards against.
    /// </summary>
    [Test]
    public void Resolve_ThenTheStagingDirectoryIsShorterThanTheIntermediateOutputPath()
    {
        using var testDirectory = new TestDirectory();

        var staging = Relative(testDirectory, Resolve(testDirectory).Directory.FullName);
        var intermediate = Relative(testDirectory, testDirectory.IntermediateOutputPath);

        staging.Length.Should().BeLessThan(intermediate.Length);
    }

    [Test]
    public void GetObsoleteDirectory_ThenTheDirectoryUsedByPreviousVersionsIsReturned()
    {
        using var testDirectory = new TestDirectory();

        var result = StagingDirectoryProvider.GetObsoleteDirectory(testDirectory.IntermediateOutputPath);

        result.FullName.Should().Be(Path.Combine(testDirectory.IntermediateOutputPath, "sxo"));
    }

    private static (DirectoryInfo Directory, string Pivot) Resolve(TestDirectory testDirectory)
    {
        return StagingDirectoryProvider.Resolve(
            testDirectory.ProjectDirectory,
            testDirectory.BaseIntermediateOutputPath,
            testDirectory.IntermediateOutputPath,
            null);
    }

    private static string Relative(TestDirectory testDirectory, string path)
    {
        return path.Substring(testDirectory.ProjectDirectory.Length + 1).TrimEnd(Path.DirectorySeparatorChar);
    }
}
