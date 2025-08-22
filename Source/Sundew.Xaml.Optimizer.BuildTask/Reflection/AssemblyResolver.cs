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

/// <summary>Helps load and resolve assemblies.</summary>
/// <seealso cref="System.IDisposable" />
public class AssemblyResolver : IDisposable
{
    private const string DllExtension = ".dll";
    private const string ExeExtension = ".exe";
    private readonly Dictionary<string, Assembly> overridenAssemblies = new Dictionary<string, Assembly>();

    private readonly IReadOnlyList<string> searchPaths;
    private readonly Func<AssemblyName, Assembly> assemblyLoad;

    /// <summary>Initializes a new instance of the <see cref="AssemblyResolver"/> class.</summary>
    /// <param name="searchPaths">The search paths.</param>
    /// <param name="assemblyLoadContext">The assembly load context.</param>
    /// <param name="overriddenAssemblyProvider">The overridden assembly provider.</param>
    public AssemblyResolver(IReadOnlyList<string> searchPaths, AssemblyLoadContext assemblyLoadContext, IOverriddenAssemblyProvider overriddenAssemblyProvider)
    {
        this.assemblyLoad = assemblyLoadContext switch
        {
            AssemblyLoadContext.Default => Assembly.Load,
            AssemblyLoadContext.From => (AssemblyName assemblyName) => Assembly.LoadFrom(assemblyName.CodeBase),
            AssemblyLoadContext.File => (AssemblyName assemblyName) => Assembly.LoadFile(assemblyName.CodeBase),
            _ => throw new NotSupportedException("Invalid load context"),
        };

        foreach (var overridenAssembly in overriddenAssemblyProvider.GetOverriddenAssemblies())
        {
            this.overridenAssemblies.Add(overridenAssembly.FullName, overridenAssembly);
        }

        AppDomain.CurrentDomain.TypeResolve += this.OnCurrentDomainTypeResolve;
        AppDomain.CurrentDomain.AssemblyResolve += this.OnCurrentDomainAssemblyResolve;
        this.searchPaths = searchPaths.Select(x =>
        {
            if (Path.HasExtension(x))
            {
                return Path.GetDirectoryName(x);
            }

            return x;
        }).ToList();
    }

    /// <summary>Loads the specified path.</summary>
    /// <param name="path">The path.</param>
    /// <returns>The loaded assembly.</returns>
    public Assembly Load(string path)
    {
        var extension = Path.GetExtension(path);
        if (extension != DllExtension && extension != ExeExtension)
        {
            path += DllExtension;
        }

        if (!Path.IsPathRooted(path))
        {
            path = this.GetPath(path);
        }

        return this.assemblyLoad(AssemblyName.GetAssemblyName(path));
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= this.OnCurrentDomainAssemblyResolve;
        AppDomain.CurrentDomain.TypeResolve -= this.OnCurrentDomainTypeResolve;
    }

    private Assembly OnCurrentDomainTypeResolve(object sender, ResolveEventArgs args)
    {
        throw new NotSupportedException("Could not find type: " + args.Name);
    }

    private Assembly? OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (this.overridenAssemblies.TryGetValue(args.Name, out var assembly))
        {
            return assembly;
        }

        var assemblyName = new AssemblyName(args.Name);
        IEnumerable<string> searchFolders = this.searchPaths;
        if (args.RequestingAssembly != null)
        {
            var requestingAssemblyFolder = new[] { args.RequestingAssembly.Location };
            requestingAssemblyFolder.Concat(this.searchPaths);
        }

        var paths = this.GetPaths(searchFolders, assemblyName.Name + DllExtension).ToList();
        var exactVersion = paths.FirstOrDefault(x => x.Name.Version == assemblyName.Version);
        if (exactVersion != default)
        {
            return this.assemblyLoad(exactVersion.Name);
        }

        var latestMajorMatch = paths.FirstOrDefault(x => x.Name.Version.Major == assemblyName.Version.Major);
        if (latestMajorMatch != default)
        {
            return this.assemblyLoad(latestMajorMatch.Name);
        }

        //// throw new NotSupportedException("Could not find assembly: " + args.Name);
        return null;
    }

    private string GetPath(string assemblyFileName)
    {
        return this.searchPaths
            .Select(x => Path.Combine(x, assemblyFileName))
            .FirstOrDefault(File.Exists);
    }

    private IEnumerable<(AssemblyName Name, string Path)> GetPaths(IEnumerable<string> searchPaths, string assemblyFileName)
    {
        return searchPaths
            .Select(x => Path.Combine(x, assemblyFileName))
            .Where(File.Exists)
            .Select(x => (Name: AssemblyName.GetAssemblyName(x), Path: x))
            .OrderByDescending(x => x.Name.Version);
    }
}