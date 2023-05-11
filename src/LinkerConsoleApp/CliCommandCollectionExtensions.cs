using System.CommandLine;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace LinkerConsoleApp;

public static class CliCommandCollectionExtensions
{
    public static IServiceCollection AddCliCommands(this IServiceCollection services)
    {
        var linkCommandType = typeof(LinkCommand);
        var commandType = typeof(Command);

        var commands = linkCommandType
            .Assembly
            .GetExportedTypes()
            .Where(x => x.Namespace == linkCommandType.Namespace && commandType.IsAssignableFrom(x));

        foreach (var command in commands)
            services.AddSingleton(commandType, command);

        return services;
    }
}