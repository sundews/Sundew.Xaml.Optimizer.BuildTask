// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlFileChangeIdentityEqualityComparer.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal;

using System.Collections.Generic;
using Sundew.Xaml.Optimization;

internal sealed class XamlFileChangeIdentityEqualityComparer : IEqualityComparer<XamlFileChange>
{
    public bool Equals(XamlFileChange? x, XamlFileChange? y)
    {
        return x?.File.Reference.Path == y?.File.Reference.Path;
    }

    public int GetHashCode(XamlFileChange xamlFilesChange)
    {
        return xamlFilesChange.File.Reference.Path.GetHashCode();
    }
}
