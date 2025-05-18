using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.File.Replacer.Abstract;

namespace Soenneker.Utils.File.Replacer.Registrars;

/// <summary>
/// FileReplacer is a utility that processes files in a directory and applies replacement logic to their contents, supporting recursion and asynchronous file operations.
/// </summary>
public static class FileReplacerRegistrar
{
    /// <summary>
    /// Adds <see cref="IFileReplacer"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddFileReplacerAsSingleton(this IServiceCollection services)
    {
        services.AddFileUtilAsSingleton().TryAddSingleton<IFileReplacer, FileReplacer>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IFileReplacer"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddFileReplacerAsScoped(this IServiceCollection services)
    {
        services.AddFileUtilAsScoped().TryAddScoped<IFileReplacer, FileReplacer>();

        return services;
    }
}
