// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileHelper.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Optimizer.BuildTask.Internal;

using System.IO;
using System.Text;
using System.Threading.Tasks;

internal static class FileHelper
{
    public static async Task<string> ReadAllTextAsync(string filePath)
    {
        var stringBuilder = new StringBuilder();
        using var fileStream = File.OpenRead(filePath);
        using var streamReader = new StreamReader(fileStream);
        var line = await streamReader.ReadLineAsync();
        while (line != null)
        {
            stringBuilder.AppendLine(line);
            line = await streamReader.ReadLineAsync();
        }

        return stringBuilder.ToString();
    }

    public static async Task WriteAllTextAsync(string filePath, string content)
    {
        using var fileStream = File.OpenWrite(filePath);
        using var streamWriter = new StreamWriter(fileStream);
        await streamWriter.WriteAsync(content).ConfigureAwait(false);
    }
}