// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Reflection;

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Sundew.Base;

/// <summary>Contains information about an assembly.</summary>
public readonly struct AssemblyInfo
{
    private const string AssemblyNameText = nameof(AssemblyName);
    private const string VersionText = nameof(Version);
    private const string CultureText = "Culture";
    private const string PublicKeyTokenText = "PublicKeyToken";

    private static readonly Regex AssemblyNameRegex =
        new Regex(@"^(?<AssemblyName>\w[^,]+), Version=(?<Version>\d+\.\d+\.\d+\.\d+)(, Culture=(?<Culture>[^,]+))?(, PublicKeyToken=(?<PublicKeyToken>.+))?$");

    private AssemblyInfo(string name, Version version, string culture, string publicKeyToken)
    {
        this.Name = name;
        this.Version = version;
        this.Culture = culture;
        this.PublicKeyToken = publicKeyToken;
    }

    /// <summary>Gets the name.</summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>Gets the version.</summary>
    /// <value>The version.</value>
    public Version Version { get; }

    /// <summary>Gets the culture.</summary>
    /// <value>The culture.</value>
    public string Culture { get; }

    /// <summary>Gets the public key token.</summary>
    /// <value>The public key token.</value>
    public string PublicKeyToken { get; }

    /// <summary>Tries the parse.</summary>
    /// <param name="fullAssemblyName">Full name of the assembly.</param>
    /// <returns>The result.</returns>
    public static R<AssemblyInfo> TryParse(string fullAssemblyName)
    {
        var match = AssemblyNameRegex.Match(fullAssemblyName);
        if (!match.Success)
        {
            return R.Error();
        }

        return R.Success(
            new AssemblyInfo(
                match.Groups[AssemblyNameText].Value,
                Version.Parse(match.Groups[VersionText].Value),
                match.Groups[CultureText].Value,
                match.Groups[PublicKeyTokenText].Value));
    }
}