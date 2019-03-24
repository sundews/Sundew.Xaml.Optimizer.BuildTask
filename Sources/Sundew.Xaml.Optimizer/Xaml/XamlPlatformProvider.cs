// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlPlatformProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.Xaml
{
    using System.Linq;
    using Microsoft.Build.Framework;
    using Sundew.Xaml.Optimization;

    internal class XamlPlatformProvider
    {
        private const string UapPlatformText = "UAP";
        private const string XamarinFormsText = "Xamarin.Forms";

        public static XamlPlatform DefectFramework(string targetPlatformIdentifier, ITaskItem[] packageReferenceItems)
        {
            if (targetPlatformIdentifier != null && targetPlatformIdentifier == UapPlatformText)
            {
                return XamlPlatform.UWP;
            }

            return packageReferenceItems.FirstOrDefault(x => x.ItemSpec.Contains(XamarinFormsText)) != null ? XamlPlatform.XF : XamlPlatform.WPF;
        }
    }
}