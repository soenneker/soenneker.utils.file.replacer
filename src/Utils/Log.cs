using Microsoft.Extensions.Logging;
using System;

namespace Soenneker.Utils.File.Replacer.Utils;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Directory ({directoryPath}) does not exist")]
    internal static partial void DirectoryDoesNotExist(ILogger logger, string directoryPath);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Updated file: {file}")]
    internal static partial void UpdatedFile(ILogger logger, string file);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to process file: {file}")]
    internal static partial void FailedToProcessFile(ILogger logger, Exception exception, string file);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error,
        Message = "Failed to enumerate files in directory ({directoryPath}) with pattern ({searchPattern})")]
    internal static partial void FailedToEnumerate(ILogger logger, Exception exception, string directoryPath, string searchPattern);
}