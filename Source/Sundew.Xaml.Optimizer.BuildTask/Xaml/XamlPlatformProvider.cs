// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlPlatformProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Xaml;

using System.Linq;
using Microsoft.Build.Framework;
using Sundew.Xaml.Optimization;

internal static class XamlPlatformProvider
{
    private const string UapPlatformText = "UAP";
    private const string XamarinFormsText = "Xamarin.Forms";
    private const string AvaloniaText = "Avalonia";

    public static XamlPlatform DetectFramework(bool useMaui, bool useWPF, bool useWinUI, string? targetPlatformIdentifier, ITaskItem[] packageReferenceItems)
    {
        if (useWPF)
        {
            return XamlPlatform.WPF;
        }

        if (useMaui)
        {
            return XamlPlatform.Maui;
        }

        if (useWinUI)
        {
            return XamlPlatform.WinUI;
        }

        if (packageReferenceItems.FirstOrDefault(x => x.ItemSpec == AvaloniaText) != null)
        {
            return XamlPlatform.Avalonia;
        }

        if (targetPlatformIdentifier == UapPlatformText)
        {
            return XamlPlatform.UWP;
        }

        if (packageReferenceItems.FirstOrDefault(x => x.ItemSpec.Contains(XamarinFormsText)) != null)
        {
            return XamlPlatform.XF;
        }

        return XamlPlatform.WPF;
    }
}