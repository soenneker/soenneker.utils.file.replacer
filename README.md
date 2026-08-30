[![](https://img.shields.io/nuget/v/soenneker.utils.file.replacer.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.file.replacer/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.file.replacer/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.file.replacer/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.file.replacer.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.file.replacer/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.file.replacer/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.file.replacer/actions/workflows/codeql.yml)

# Soenneker.Utils.File.Replacer

Replaces ordinal, case-sensitive text across files selected by a filesystem search pattern.

## Installation

```bash
dotnet add package Soenneker.Utils.File.Replacer
```

## Registration

```csharp
builder.Services.AddFileReplacerAsSingleton();
```

Scoped registration is also available with `AddFileReplacerAsScoped()`.

## Usage

```csharp
bool changed = await replacer.ReplaceString(
    directoryPath: repositoryPath,
    searchPattern: "*.cs",
    targetString: "Old.Namespace",
    replacementString: "New.Namespace",
    includeSubdirectories: true,
    cancellationToken);
```

`searchPattern` uses the platform filesystem matcher and supports patterns such as `*.cs`, `Service*.json`, and `file?.txt`. An empty pattern is treated as `*`. Recursive enumeration skips symbolic links, junctions, and other reparse points.

The replacement uses `StringComparison.Ordinal`; casing must match exactly. `true` means at least one file was changed. `false` means the directory or target was invalid, no match was found, or every matching file failed to process. Individual file and enumeration failures are logged.

Each changed file is written to a temporary sibling and moved over the original only after the new contents are complete. Cancellation therefore does not leave a half-written version of the file being processed. The operation is not transactional across the directory: files completed before a later error or cancellation stay changed.

This utility reads each selected file as text and writes UTF-8 without a byte-order mark. Limit the search pattern to known text files; selecting binaries or files whose exact encoding/BOM must be preserved can corrupt or alter them. Review changes in version control before using broad replacements.
