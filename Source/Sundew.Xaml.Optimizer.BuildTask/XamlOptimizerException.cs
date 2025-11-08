// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XamlOptimizerException.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask;

using System;
using System.Collections.Generic;

/// <summary>
/// Exception for when a xaml optimizer fails with and exception.
/// </summary>
/// <param name="xamlOptimizerType">The xaml optimizer type.</param>
/// <param name="innerException">The inner exception.</param>
/// <param name="innerExceptions">The inner exceptions.</param>
public class XamlOptimizerException(Type xamlOptimizerType, Exception innerException, IReadOnlyList<Exception> innerExceptions)
    : Exception($"The xaml optimizer: {xamlOptimizerType.Name} failed due to an error: {innerException.Message}", innerException)
{
    /// <summary>
    /// Gets the collection of exceptions that are contained within this exception instance.
    /// </summary>
    /// <remarks>The collection may be empty if no inner exceptions are present. This property is typically
    /// used to access all exceptions that occurred during an aggregate operation, such as when multiple tasks fail
    /// concurrently.</remarks>
    public IReadOnlyList<Exception> InnerExceptions { get; } = innerExceptions;

    /// <summary>
    /// Gets the xaml optimizer type.
    /// </summary>
    public Type XamlOptimizerType { get; } = xamlOptimizerType;
}