// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Settings;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sundew.Base.Computation;

/// <summary>Provides the settings.</summary>
public class SettingsProvider
{
    private const string SxoSettingsFileExtension = "*.sxos";
    private const string SxoPath = "sxo";
    private readonly ISettingsProviderReporter settingsProviderReporter;

    /// <summary>Initializes a new instance of the <see cref="SettingsProvider"/> class.</summary>
    /// <param name="settingsProviderReporter">The xaml optimizer factory reporter.</param>
    public SettingsProvider(ISettingsProviderReporter settingsProviderReporter)
    {
        this.settingsProviderReporter = settingsProviderReporter;
    }

    /// <summary>
    /// Gets the settings.
    /// </summary>
    /// <param name="projectDirectory">The project directory.</param>
    /// <returns>
    /// The <see cref="IReadOnlyCollection{SxoSettings}" />.
    /// </returns>
    /// <exception cref="InvalidOperationException">A sxo-settings.json file could not be found, based on the project path: {projectDirectory}.</exception>
    public IReadOnlyCollection<SxoSettings> GetSettings(string projectDirectory)
    {
        var directoryInfo = new DirectoryInfo(Path.Combine(projectDirectory, SxoPath));
        var attempter = new Attempter(4);
        while (attempter.Attempt())
        {
            if (directoryInfo.Exists)
            {
                var sxoSettingsFiles = directoryInfo.EnumerateFiles(SxoSettingsFileExtension).ToArray();
                if (sxoSettingsFiles.Any())
                {
                    this.settingsProviderReporter.FoundSettings(sxoSettingsFiles);
                    return GetSettings(sxoSettingsFiles);
                }
            }

            directoryInfo = directoryInfo.Parent;
            if (directoryInfo == null)
            {
                break;
            }
        }

        throw new InvalidOperationException(
            $"No .sxos files could not be found, based on the project path: {projectDirectory}");
    }

    private static IReadOnlyCollection<SxoSettings> GetSettings(IEnumerable<FileInfo> settingsPath)
    {
        return settingsPath
            .Select(x => File.ReadAllText(x.FullName))
            .Where(x =>
            {
                const string skip = "skip";
                return !StringComparer.CurrentCultureIgnoreCase.Equals(x, skip);
            })
            .Select(x => JsonConvert.DeserializeObject<SxoSettings>(x)!)
            .ToList();
    }
}