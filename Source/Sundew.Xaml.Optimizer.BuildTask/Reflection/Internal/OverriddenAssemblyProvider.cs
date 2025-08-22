// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverriddenAssemblyProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Reflection.Internal;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal class OverriddenAssemblyProvider : IOverriddenAssemblyProvider
{
    public IEnumerable<Assembly> GetOverriddenAssemblies()
    {
        /*yield return typeof(IXamlOptimization).Assembly;
        yield return typeof(ˍ).Assembly;*/
        return Enumerable.Empty<Assembly>();
    }
}