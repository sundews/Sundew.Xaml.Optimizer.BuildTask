// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileHelperTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Tests;

using System.IO;
using System.Threading.Tasks;
using AwesomeAssertions;
using Sundew.Xaml.Optimizer.BuildTask.Internal;
using TUnit.Core;

public class FileHelperTests
{
    private const string LongContent = "// class PreviousGeneratedContent { } // trailing tail that must not survive";
    private const string ShortContent = "// class Short { }";

    [Test]
    public async Task WriteAllTextAsync_WhenTheFileDoesNotExist_ThenTheFileIsCreated()
    {
        using var testDirectory = new TestDirectory();
        var filePath = Path.Combine(testDirectory.ProjectDirectory, "Generated.g.cs");

        await FileHelper.WriteAllTextAsync(filePath, ShortContent);

        File.ReadAllText(filePath).Should().Be(ShortContent);
    }

    /// <summary>
    /// The previous content has to be truncated rather than overwritten in place, otherwise a generated file that
    /// shrinks keeps the tail of the previous build at the end of it.
    /// </summary>
    [Test]
    public async Task WriteAllTextAsync_WhenThePreviousContentWasLonger_ThenTheFileIsTruncated()
    {
        using var testDirectory = new TestDirectory();
        var filePath = testDirectory.CreateFile("Generated.g.cs", LongContent);

        await FileHelper.WriteAllTextAsync(filePath, ShortContent);

        var actual = File.ReadAllText(filePath);
        actual.Should().Be(ShortContent);
        actual.Should().NotContain("trailing tail");
        new FileInfo(filePath).Length.Should().Be(ShortContent.Length);
    }

    [Test]
    public async Task WriteAllTextAsync_WhenWrittenTwiceWithTheSameContent_ThenTheContentIsUnchanged()
    {
        using var testDirectory = new TestDirectory();
        var filePath = Path.Combine(testDirectory.ProjectDirectory, "Generated.g.cs");

        await FileHelper.WriteAllTextAsync(filePath, ShortContent);
        await FileHelper.WriteAllTextAsync(filePath, ShortContent);

        File.ReadAllText(filePath).Should().Be(ShortContent);
    }

    [Test]
    public async Task ReadAllTextAsync_ThenTheContentIsReturned()
    {
        using var testDirectory = new TestDirectory();
        var filePath = testDirectory.CreateFile("Content.txt", ShortContent);

        var result = await FileHelper.ReadAllTextAsync(filePath);

        result.Should().StartWith(ShortContent);
    }
}
