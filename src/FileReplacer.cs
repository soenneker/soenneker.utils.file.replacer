using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.File.Replacer.Abstract;
using System.Collections.Generic;
using Soenneker.Utils.File.Replacer.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.String;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Utils.File.Replacer;

/// <inheritdoc cref="IFileReplacer"/>
public sealed class FileReplacer : IFileReplacer
{
    private readonly ILogger<FileReplacer> _logger;
    private readonly IFileUtil _fileUtil;
    private readonly IDirectoryUtil _directoryUtil;

    public FileReplacer(ILogger<FileReplacer> logger, IFileUtil fileUtil, IDirectoryUtil directoryUtil)
    {
        _logger = logger;
        _fileUtil = fileUtil;
        _directoryUtil = directoryUtil;
    }

    public async ValueTask<bool> ReplaceString(string directoryPath, string searchPattern, string targetString, string replacementString,
        bool includeSubdirectories = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(directoryPath) || !(await _directoryUtil.Exists(directoryPath, cancellationToken)))
        {
            Log.DirectoryDoesNotExist(_logger, directoryPath);
            return false;
        }

        if (targetString.IsNullOrEmpty())
            return false;

        if (searchPattern.IsNullOrEmpty())
            searchPattern = "*";

        var madeChanges = false;

        try
        {
            string extension = (searchPattern == "*" || searchPattern == "*.*")
                ? ""
                : (System.IO.Path.GetExtension(searchPattern)?.TrimStart('.') ?? "");
            List<string> files = await _directoryUtil.GetFilesByExtension(directoryPath, extension, includeSubdirectories, cancellationToken).NoSync();
            if (searchPattern != "*" && searchPattern != "*.*" && searchPattern.Contains('*', StringComparison.Ordinal))
            {
                string suffix = searchPattern.Replace("*", "");
                if (suffix.Length > 0)
                    files = files.FindAll(f => f.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) || f.Contains(suffix, StringComparison.OrdinalIgnoreCase));
            }
            foreach (string file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    string content = await _fileUtil.Read(file, true, cancellationToken)
                                                    .NoSync();

                    // Single scan; avoid Replace allocation unless necessary
                    if (content.IndexOf(targetString, StringComparison.Ordinal) < 0)
                        continue;

                    string updatedContent = content.Replace(targetString, replacementString, StringComparison.Ordinal);

                    await _fileUtil.Write(file, updatedContent, true, cancellationToken)
                                   .NoSync();

                    madeChanges = true;
                    Log.UpdatedFile(_logger, file);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.FailedToProcessFile(_logger, ex, file);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.FailedToEnumerate(_logger, ex, directoryPath, searchPattern);
        }

        return madeChanges;
    }
}