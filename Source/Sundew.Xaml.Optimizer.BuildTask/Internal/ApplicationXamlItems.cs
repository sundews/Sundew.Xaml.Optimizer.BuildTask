// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationXamlItems.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal;

using System;

internal record ApplicationXamlItems<TItems>(
    TItems PageItems,
    TItems AvaloniaXamlItems,
    TItems MauiXamlItems,
    TItems EmbeddedResourceItems,
    TItems ApplicationDefinition)
{
    public ApplicationXamlItems(Func<TItems> factory)
        : this(factory(), factory(), factory(), factory(), factory())
    {
    }
}