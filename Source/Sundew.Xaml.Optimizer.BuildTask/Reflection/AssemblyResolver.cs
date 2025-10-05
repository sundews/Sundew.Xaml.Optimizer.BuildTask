// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyResolver.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Reflection;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sundew.Base;
using Sundew.Base.Collections.Linq;

/// <summary>Helps load and resolve assemblies.</summary>
/// <seealso cref="System.IDisposable" />
public class AssemblyResolver : IDisposable
{
    private const string DllExtension = ".dll";
    private const string ExeExtension = ".exe";
    private readonly Dictionary<string, Assembly> overridenAssemblies = new();

    private readonly IReadOnlyList<string> searchPaths;
    private readonly Dictionary<string, string?> assemblySearchPaths;

    /// <summary>Initializes a new instance of the <see cref="AssemblyResolver"/> class.</summary>
    /// <param name="searchPaths">The search paths.</param>
    /// <param name="xamlOptimizerAssemblies">The xaml optimizer assemblies.</param>
    /// <param name="overriddenAssemblyProvider">The overridden assembly provider.</param>
    public AssemblyResolver(IReadOnlyList<string> searchPaths, IReadOnlyList<(string Path, AssemblyName AssemblyName)> xamlOptimizerAssemblies, IOverriddenAssemblyProvider overriddenAssemblyProvider)
    {
        this.searchPaths = searchPaths;
        this.assemblySearchPaths = xamlOptimizerAssemblies.ToDictionary(x => x.AssemblyName.FullName, x => Path.GetDirectoryName(x.Path) ?? null);

        foreach (var overridenAssembly in overriddenAssemblyProvider.GetOverriddenAssemblies())
        {
            if (overridenAssembly.FullName.HasValue)
            {
                this.overridenAssemblies.Add(overridenAssembly.FullName, overridenAssembly);
            }
        }

        AppDomain.CurrentDomain.TypeResolve += this.OnCurrentDomainTypeResolve;
        AppDomain.CurrentDomain.AssemblyResolve += this.OnCurrentDomainAssemblyResolve;
    }

    /// <summary>Loads the specified path.</summary>
    /// <param name="assemblyName">The assembly name.</param>
    /// <param name="path">The path.</param>
    /// <returns>The loaded assembly.</returns>
    public Assembly Load(AssemblyName assemblyName, string path)
    {
        return this.Load(assemblyName, path, false);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= this.OnCurrentDomainAssemblyResolve;
        AppDomain.CurrentDomain.TypeResolve -= this.OnCurrentDomainTypeResolve;
    }

    private Assembly OnCurrentDomainTypeResolve(object? sender, ResolveEventArgs args)
    {
        throw new NotSupportedException("Could not find type: " + args.Name);
    }

    private Assembly? OnCurrentDomainAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);
        if (assemblyName.Name.HasValue &&
            (assemblyName.Name.StartsWith("System.") ||
            assemblyName.Name.StartsWith("Microsoft.") ||
            assemblyName.Name.StartsWith("mscorlib")))
        {
            return null;
        }

#if NET8_0_OR_GREATER
        var referencedAssembly = System.Runtime.Loader.AssemblyLoadContext.All.SelectMany(x => x.Assemblies).FirstOrDefault(x => x.FullName == assemblyName.FullName);
        if (referencedAssembly.HasValue)
        {
            return referencedAssembly;
        }
#endif
        if (this.overridenAssemblies.TryGetValue(args.Name, out var assembly))
        {
            return assembly;
        }

        IEnumerable<string> searchFolders = this.searchPaths;
        if (this.assemblySearchPaths.TryGetValue(assemblyName.FullName, out var path) && path != null)
        {
            searchFolders = searchFolders.Concat([path]);
        }

        if (args.RequestingAssembly != null)
        {
            var requestingAssemblyFolder = Path.GetDirectoryName(args.RequestingAssembly.Location)?.ToEnumerable().WhereNotNull() ?? [];
            searchFolders = searchFolders.Concat(requestingAssemblyFolder).ToArray();
        }

        var paths = this.GetPaths(searchFolders, assemblyName.Name + DllExtension).ToList();
        var exactVersion = paths.FirstOrDefault(x => x.Version == assemblyName.Version);
        if (exactVersion != default)
        {
            return this.Load(exactVersion.AssemblyName, exactVersion.Path, true);
        }

        var latestMajorMatch = paths.FirstOrDefault(x => x.Version.Major == assemblyName.Version?.Major);
        if (latestMajorMatch != default)
        {
            return this.Load(latestMajorMatch.AssemblyName, latestMajorMatch.Path, true);
        }

        return null;
    }

    private IEnumerable<(string Name, Version Version, string Path, AssemblyName AssemblyName)> GetPaths(IEnumerable<string> searchPaths, string assemblyFileName)
    {
        return searchPaths
            .Select(x => Path.Combine(x, assemblyFileName))
            .Where(File.Exists)
            .Select(x => (AssemblyName: AssemblyName.GetAssemblyName(x), Path: x))
            .Where(x => !string.IsNullOrEmpty(x.AssemblyName.Name) && x.AssemblyName.Version.HasValue)
            .Select(x => (Name: x.AssemblyName.Name!, Version: x.AssemblyName.Version!, x.Path, AssemblyName: x.AssemblyName))
            .OrderByDescending(x => x.Version);
    }

    private Assembly Load(AssemblyName assemblyName, string assemblyPath, bool usePath)
    {
#if NET8_0_OR_GREATER
        var xamlOptimizerAssemblyLoadContext =
            System.Runtime.Loader.AssemblyLoadContext.All.FirstOrDefault(x =>
                x.Assemblies.Contains(Assembly.GetExecutingAssembly())) ?? System.Runtime.Loader.AssemblyLoadContext.Default;
        if (usePath)
        {
            return xamlOptimizerAssemblyLoadContext.LoadFromAssemblyPath(assemblyPath);
        }

        return xamlOptimizerAssemblyLoadContext.LoadFromAssemblyName(assemblyName);
#else
        return Assembly.Load(assemblyName);
#endif
    }
}