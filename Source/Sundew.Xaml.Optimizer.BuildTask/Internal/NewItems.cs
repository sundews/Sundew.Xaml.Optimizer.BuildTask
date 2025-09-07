// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewItems.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal;

using System;

internal record NewItems<TItems>(
    TItems PageItems,
    TItems AvaloniaXamlItems,
    TItems MauiXamlItems,
    TItems EmbeddedResourceItems,
    TItems CompileItems,
    TItems AdditionalFileItems)
{
    public NewItems(Func<TItems> factoryFunc)
    : this(factoryFunc(), factoryFunc(), factoryFunc(), factoryFunc(), factoryFunc(), factoryFunc())
    {
    }
}