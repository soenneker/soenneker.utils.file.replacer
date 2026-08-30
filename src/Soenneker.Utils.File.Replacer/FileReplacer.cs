using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.File.Replacer.Abstract;
using Soenneker.Utils.File.Replacer.Utils;
using System;
using System.IO;
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
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = includeSubdirectories,
                IgnoreInaccessible = false,
                AttributesToSkip = FileAttributes.ReparsePoint
            };

            foreach (string file in System.IO.Directory.EnumerateFiles(directoryPath, searchPattern, options))
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
                    string temporaryPath = file + "." + Guid.NewGuid().ToString("N") + ".tmp";

                    try
                    {
                        await _fileUtil.Write(temporaryPath, updatedContent, true, cancellationToken)
                                       .NoSync();
                        await _fileUtil.Move(temporaryPath, file, log: false, cancellationToken)
                                       .NoSync();
                    }
                    finally
                    {
                        await _fileUtil.Delete(temporaryPath, log: false, cancellationToken: CancellationToken.None)
                                       .NoSync();
                    }

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
