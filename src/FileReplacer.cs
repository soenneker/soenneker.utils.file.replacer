using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.File.Replacer.Abstract;

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
        var madeChanges = false;

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogError("Directory ({dir}) does not exist", directoryPath);
            return false;
        }

        // Get all files matching the search pattern
        string[] files = Directory.GetFiles(directoryPath, searchPattern,
            includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        for (var i = 0; i < files.Length; i++)
        {
            string file = files[i];

            try
            {
                string content = await _fileUtil.Read(file, true, cancellationToken).NoSync();

                // ReplaceString the target string if it exists
                if (content.Contains(targetString))
                {
                    string updatedContent = content.Replace(targetString, replacementString);

                    // Write back to the file
                    await _fileUtil.Write(file, updatedContent, true, cancellationToken).NoSync();

                    madeChanges = true;
                    _logger.LogInformation("Updated file: {File}", file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to process file: {File}. Error: {ExMessage}", file, ex.Message);
            }
        }

        return madeChanges;
    }
}