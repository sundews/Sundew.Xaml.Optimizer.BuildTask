// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOverriddenAssemblyProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Reflection;

using System.Collections.Generic;
using System.Reflection;

/// <summary>Interface for implement a fixed assembly provider.</summary>
public interface IOverriddenAssemblyProvider
{
    /// <summary>Gets the fixed assemblies.</summary>
    /// <returns>The assemblies.</returns>
    IEnumerable<Assembly> GetOverriddenAssemblies();
}