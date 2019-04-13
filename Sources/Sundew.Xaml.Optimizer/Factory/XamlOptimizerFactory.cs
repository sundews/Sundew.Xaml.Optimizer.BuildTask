// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlOptimizerFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Factory
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Sundew.Base.ControlFlow;
    using Sundew.Xaml.Optimization;
    using Sundew.Xaml.Optimization.Xml;
    using Sundew.Xaml.Optimizer.Settings;

    /// <summary>Prepares optimizer runners.</summary>
    public class XamlOptimizerFactory
    {
        private const string SxoSettingsFileName = "sxo-settings.json";
        private const string BasePathText = "BasePath";
        private const string RemainingPathText = "RemainingPath";
        private const string AsteriskText = "*";
        private const string SortAtBeginningCharacter = "\0xFFFF";
        private const string VersionWithPrerelease = "VersionWithPrerelease";
        private const string VersionWithRevision = "VersionWithRevision";
        private const string Prerelease = "Prerelease";
        private static readonly Regex VersionRegex = new Regex(@"(?<VersionWithRevision>(?<Major>\d+)\.(?<Minor>\d+)\.(?<Build>\d+)\.(?<Revision>\d+))|(?<VersionWithPrerelease>(?<Major>\d+)\.(?<Minor>\d+)\.(?<Build>\d+))(?<Prerelease>.*)");
        private static readonly Regex LatestVersionRegex = new Regex(@"(?<BasePath>.+)(\?LATEST_VERSION\?)(?<RemainingPath>.+)");
        private readonly IXamlOptimizerFactoryReporter xamlOptimizerFactoryReporter;

        /// <summary>Initializes a new instance of the <see cref="XamlOptimizerFactory"/> class.</summary>
        /// <param name="xamlOptimizerFactoryReporter">The xaml optimizer factory reporter.</param>
        public XamlOptimizerFactory(IXamlOptimizerFactoryReporter xamlOptimizerFactoryReporter)
        {
            this.xamlOptimizerFactoryReporter = xamlOptimizerFactoryReporter;
        }

        /// <summary>Prepares the optimizers.</summary>
        /// <param name="projectDirectory">The project directory.</param>
        /// <param name="xamlPlatform">The xaml platform.</param>
        /// <param name="xamlPlatformInfo">The framework xml definitions.</param>
        /// <param name="packagesDirectory">The packages directory.</param>
        /// <returns>The optimization runners.</returns>
        public IEnumerable<IXamlOptimizer> CreateXamlOptimizers(
            string projectDirectory,
            XamlPlatform xamlPlatform,
            XamlPlatformInfo xamlPlatformInfo,
            string packagesDirectory)
        {
            var sxoSettings = this.GetSettings(projectDirectory);
            var optimizerLibraries = sxoSettings.Libraries.Select(libraryPath =>
            {
                if (!Path.IsPathRooted(libraryPath))
                {
                    libraryPath = Path.Combine(packagesDirectory, libraryPath);
                }

                var match = LatestVersionRegex.Match(libraryPath);
                if (match.Success)
                {
                    var basePath = match.Groups[BasePathText].Value;
                    var baseDirectory = Path.GetDirectoryName(basePath);
                    var suggestedPath = Directory
                        .EnumerateDirectories(baseDirectory, Path.GetFileName(basePath) + AsteriskText)
                        .Select(path =>
                        {
                            var versionMatch = VersionRegex.Match(path.Substring(basePath.Length));
                            if (versionMatch.Success)
                            {
                                var group = versionMatch.Groups[VersionWithRevision];
                                if (group.Success)
                                {
                                    return new { Path = path, Version = Version.Parse(group.Value), Prerelease = SortAtBeginningCharacter };
                                }

                                group = versionMatch.Groups[VersionWithPrerelease];
                                if (group.Success)
                                {
                                    var prerelease = versionMatch.Groups[Prerelease].Value;
                                    return new
                                    {
                                        Path = path, Version = Version.Parse(group.Value),
                                        Prerelease = string.IsNullOrEmpty(prerelease) ? SortAtBeginningCharacter : prerelease,
                                    };
                                }
                            }

                            return new { Path = path, Version = new Version(0, 0, 0, 0), Prerelease = string.Empty };
                        })
                        .OrderByDescending(pathAndVersion => pathAndVersion.Version).ThenByDescending(pathAndVersion => pathAndVersion.Prerelease)
                        .FirstOrDefault()
                        ?.Path;
                    if (suggestedPath != null)
                    {
                        return suggestedPath + match.Groups[RemainingPathText].Value;
                    }

                    throw new InvalidOperationException($"Could not find versioned path for: {libraryPath}");
                }

                return libraryPath;
            }).ToList();

            var exportedTypes = optimizerLibraries.Select(Assembly.LoadFrom).SelectMany(x => x.ExportedTypes);
            var xamlOptimizerTypes = exportedTypes.Where(type => typeof(IXamlOptimizer).IsAssignableFrom(type)).ToList();
            foreach (var enabledOptimizer in sxoSettings.Optimizers.Where(x => x.IsEnabled))
            {
                foreach (var xamlOptimizer in xamlOptimizerTypes
                    .Where(x => x.Name.EndsWith(enabledOptimizer.Name))
                    .Select(x =>
                    {
                        var xamlOptimizer = CreateXamlOptimizer(x, xamlPlatformInfo, enabledOptimizer.Settings);
                        return xamlOptimizer.SupportedPlatforms.Contains(xamlPlatform) ? xamlOptimizer : null;
                    }))
                {
                    if (xamlOptimizer != null)
                    {
                        this.xamlOptimizerFactoryReporter.FoundOptimizer(xamlOptimizer.GetType());
                        yield return xamlOptimizer;
                    }
                }
            }
        }

        private static IXamlOptimizer CreateXamlOptimizer(Type type, XamlPlatformInfo xamlPlatformInfo, JObject settings)
        {
            var constructorInfos = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(x => x.GetParameters().Length);
            foreach (var constructorInfo in constructorInfos)
            {
                var parameterInfos = constructorInfo.GetParameters();
                object[] arguments = new object[parameterInfos.Length];
                switch (parameterInfos.Length)
                {
                    case 2:
                        FillArgument(arguments, parameterInfos, xamlPlatformInfo, settings, 0);
                        FillArgument(arguments, parameterInfos, xamlPlatformInfo, settings, 1);
                        return (IXamlOptimizer)constructorInfo.Invoke(arguments);
                    case 1:
                        FillArgument(arguments, parameterInfos, xamlPlatformInfo, settings, 0);
                        return (IXamlOptimizer)constructorInfo.Invoke(arguments);
                    case 0:
                        return (IXamlOptimizer)constructorInfo.Invoke(arguments);
                    default:
                        continue;
                }
            }

            throw new NotSupportedException($"Could not create the type: {type}");
        }

        private static void FillArgument(object[] arguments, ParameterInfo[] parameterInfos, XamlPlatformInfo xamlPlatformInfo, JObject settings, int index)
        {
            var parameterInfo = parameterInfos[index];
            if (parameterInfo.ParameterType == typeof(XamlPlatformInfo))
            {
                arguments[index] = xamlPlatformInfo;
            }
            else if (parameterInfo.ParameterType == typeof(JObject))
            {
                arguments[index] = settings;
            }
            else
            {
                arguments[index] = settings.ToObject(parameterInfo.ParameterType);
            }
        }

        private SxoSettings GetSettings(string projectDirectory)
        {
            var directoryInfo = new DirectoryInfo(projectDirectory);
            var settingsPath = Path.Combine(directoryInfo.FullName, SxoSettingsFileName);
            var attempter = new Attempter(4);
            while (attempter.Attempt())
            {
                if (File.Exists(settingsPath))
                {
                    this.xamlOptimizerFactoryReporter.FoundSettings(settingsPath);
                    return JsonConvert.DeserializeObject<SxoSettings>(File.ReadAllText(settingsPath));
                }

                directoryInfo = directoryInfo.Parent;
                if (directoryInfo == null)
                {
                    break;
                }

                settingsPath = Path.Combine(directoryInfo.FullName, SxoSettingsFileName);
            }

            throw new InvalidOperationException($"A sxo-settings.json file could not be found, based on the project path: {projectDirectory}");
        }
    }
}