// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamPlatformInfoProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Xaml
{
    using System;
    using Sundew.Xaml.Optimization;
    using Sundew.Xaml.Optimization.Xml;

    internal static class XamPlatformInfoProvider
    {
        public static XamlPlatformInfo GetFrameworkXmlDefinitions(XamlPlatform xamlPlatform)
        {
            switch (xamlPlatform)
            {
                case XamlPlatform.WPF:
                    return new XamlPlatformInfo(
                        xamlPlatform,
                        Constants.WpfPresentationNamespace,
                        Constants.SundewXamlOptimizationWpfNamespace);
                case XamlPlatform.UWP:
                    return new XamlPlatformInfo(
                        xamlPlatform,
                        Constants.UwpPresentationNamespace,
                        Constants.SundewXamlOptimizationUwpNamespace);
                case XamlPlatform.XF:
                    return new XamlPlatformInfo(
                        xamlPlatform,
                        Constants.XfPresentationNamespace,
                        Constants.SundewXamlOptimizationXfNamespace);
                default:
                    throw new ArgumentOutOfRangeException(nameof(xamlPlatform), xamlPlatform, $"The specified value: {xamlPlatform} is not supported");
            }
        }
    }
}