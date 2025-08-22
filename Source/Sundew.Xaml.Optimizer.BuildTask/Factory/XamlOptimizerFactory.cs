// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlOptimizerFactory.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Factory;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Sundew.Base;
using Sundew.Xaml.Optimization;
using Sundew.Xaml.Optimization.Xml;
using Sundew.Xaml.Optimizer.BuildTask.Reflection;
using Sundew.Xaml.Optimizer.BuildTask.Settings;

/// <summary>Prepares optimizer runners.</summary>
public class XamlOptimizerFactory
{
    private readonly IXamlOptimizerFactoryReporter xamlOptimizerFactoryReporter;

    /// <summary>Initializes a new instance of the <see cref="XamlOptimizerFactory"/> class.</summary>
    /// <param name="xamlOptimizerFactoryReporter">The xaml optimizer factory reporter.</param>
    public XamlOptimizerFactory(IXamlOptimizerFactoryReporter xamlOptimizerFactoryReporter)
    {
        this.xamlOptimizerFactoryReporter = xamlOptimizerFactoryReporter;
    }

    /// <summary>Prepares the optimizers.</summary>
    /// <param name="assemblyResolver">The assembly resolver.</param>
    /// <param name="xamlPlatformInfo">The framework xml definitions.</param>
    /// <param name="projectInfo">The project info.</param>
    /// <param name="optimizationAssemblies">The optimization assemblies.</param>
    /// <param name="sxoSettings">The sxo settings.</param>
    /// <returns>The optimization runners.</returns>
    public IEnumerable<IXamlOptimizer> CreateXamlOptimizers(
        AssemblyResolver assemblyResolver,
        XamlPlatformInfo xamlPlatformInfo,
        ProjectInfo projectInfo,
        IReadOnlyList<string> optimizationAssemblies,
        IReadOnlyCollection<SxoSettings> sxoSettings)
    {
        var exportedTypes = optimizationAssemblies.Select(assemblyResolver.Load).SelectMany(x => x.ExportedTypes);
        var xamlOptimizerTypes = exportedTypes.Where(type => typeof(IXamlOptimizer).IsAssignableFrom(type))
            .ToList();
        foreach (var sxoSetting in sxoSettings)
        {
            foreach (var enabledOptimizer in sxoSetting.Optimizers.Where(x => x.IsEnabled))
            {
                foreach (var xamlOptimizer in xamlOptimizerTypes
                             .Where(x => x.Name.EndsWith(enabledOptimizer.Name))
                             .Select(x =>
                             {
                                 var xamlOptimizer =
                                     CreateXamlOptimizer(x, xamlPlatformInfo, projectInfo, enabledOptimizer.Settings);
                                 return xamlOptimizer.SupportedPlatforms.Contains(xamlPlatformInfo.XamlPlatform)
                                     ? xamlOptimizer
                                     : null;
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
    }

    private static IXamlOptimizer CreateXamlOptimizer(Type type, XamlPlatformInfo xamlPlatformInfo, ProjectInfo projectInfo, JObject settings)
    {
        var constructorInfos = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .OrderByDescending(x => x.GetParameters().Length);
        foreach (var constructorInfo in constructorInfos)
        {
            var parameterInfos = constructorInfo.GetParameters();
            object[] arguments = new object[parameterInfos.Length];
            switch (parameterInfos.Length)
            {
                case 3:
                    FillArgument(arguments, parameterInfos, xamlPlatformInfo, projectInfo, settings, 0);
                    FillArgument(arguments, parameterInfos, xamlPlatformInfo, projectInfo, settings, 1);
                    FillArgument(arguments, parameterInfos, xamlPlatformInfo, projectInfo, settings, 2);
                    return (IXamlOptimizer)constructorInfo.Invoke(arguments);
                case 2:
                    FillArgument(arguments, parameterInfos, xamlPlatformInfo, projectInfo, settings, 0);
                    FillArgument(arguments, parameterInfos, xamlPlatformInfo, projectInfo, settings, 1);
                    return (IXamlOptimizer)constructorInfo.Invoke(arguments);
                case 1:
                    FillArgument(arguments, parameterInfos, xamlPlatformInfo, projectInfo, settings, 0);
                    return (IXamlOptimizer)constructorInfo.Invoke(arguments);
                case 0:
                    return (IXamlOptimizer)constructorInfo.Invoke(arguments);
                default:
                    continue;
            }
        }

        throw new NotSupportedException($"Could not create the type: {type}");
    }

    private static void FillArgument(object[] arguments, ParameterInfo[] parameterInfos, XamlPlatformInfo xamlPlatformInfo, ProjectInfo projectInfo, JObject settings, int index)
    {
        var parameterInfo = parameterInfos[index];
        if (parameterInfo.ParameterType == typeof(XamlPlatformInfo))
        {
            arguments[index] = xamlPlatformInfo;
        }
        else if (parameterInfo.ParameterType == typeof(ProjectInfo))
        {
            arguments[index] = projectInfo;
        }
        else if (parameterInfo.ParameterType == typeof(JObject))
        {
            arguments[index] = settings;
        }
        else
        {
            var argument = settings.ToObject(parameterInfo.ParameterType);
            if (argument.HasValue())
            {
                arguments[index] = argument;
            }
            else
            {
                throw new NotSupportedException($"Could not create the type: {parameterInfo.ParameterType} for parameter: {parameterInfo.Name}");
            }
        }
    }
}