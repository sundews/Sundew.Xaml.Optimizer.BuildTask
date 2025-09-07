// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskItemXamlFile.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal;

using System.Xml.Linq;
using Microsoft.Build.Framework;
using Sundew.Xaml.Optimization;

internal record TaskItemXamlFile(XDocument Document, IFileReference Reference, string LineEndings, ITaskItem TaskItem) : XamlFile(Document, Reference, LineEndings);