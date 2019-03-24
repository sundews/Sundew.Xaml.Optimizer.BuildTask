// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Xaml
{
    using System.Xml.Linq;

    internal static class Constants
    {
        public const string SundewXamlOptimizationWpfNamespace = "clr-namespace:Sundew.Xaml.Optimizations;assembly=Sundew.Xaml.Wpf";
        public const string SundewXamlOptimizationUwpNamespace = "clr-namespace:Sundew.Xaml.Optimizations;assembly=Sundew.Xaml.Uwp";
        public const string SundewXamlOptimizationXfNamespace = "clr-namespace:Sundew.Xaml.Optimizations;assembly=Sundew.Xaml.Xf";
        public const string System = "system";
        public static readonly XNamespace WpfPresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        public static readonly XNamespace UwpPresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        public static readonly XNamespace XfPresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    }
}
