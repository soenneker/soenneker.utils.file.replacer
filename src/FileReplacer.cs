using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.File.Replacer.Abstract;
using Soenneker.Utils.File.Replacer.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.String;

namespace Soenneker.Utils.File.Replacer;

/// <inheritdoc cref="IFileReplacer"/>
public sealed class FileReplacer : IFileReplacer
{
    private readonly ILogger<FileReplacer> _logger;
    private readonly IFileUtil _fileUtil;

    public FileReplacer(ILogger<FileReplacer> logger, IFileUtil fileUtil)
    {
        _logger = logger;
        _fileUtil = fileUtil;
    }

    public async ValueTask<bool> ReplaceString(string directoryPath, string searchPattern, string targetString, string replacementString,
        bool includeSubdirectories = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            Log.DirectoryDoesNotExist(_logger, directoryPath);
            return false;
        }

        if (targetString.IsNullOrEmpty())
            return false;

        if (searchPattern.IsNullOrEmpty())
            searchPattern = "*";

        SearchOption searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        var madeChanges = false;

        try
        {
            foreach (string file in Directory.EnumerateFiles(directoryPath, searchPattern, searchOption))
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