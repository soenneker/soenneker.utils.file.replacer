using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Utils.File.Replacer.Abstract;

/// <summary>
/// FileReplacer is a utility that processes files in a directory and applies replacement logic to their contents, supporting recursion and asynchronous file operations.
/// </summary>
public interface IFileReplacer
{
    /// <summary>
    /// Replaces a target string with a replacement string in all files under the given directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to scan for files.</param>
    /// <param name="searchPattern">The file search pattern (e.g., "*.cs" or "*.*").</param>
    /// <param name="targetString">The string to search for.</param>
    /// <param name="replacementString">The string to replace with.</param>
    /// <param name="includeSubdirectories">Indicates whether to process subdirectories.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask<bool> ReplaceString(string directoryPath, string searchPattern, string targetString, string replacementString,
        bool includeSubdirectories = true, CancellationToken cancellationToken = default);
}
