using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace LinkerConsoleApp;

class Program
{
    public static async Task Main(string[] args)
    {
        var serviceProvider = BuildServiceProvider();
        var parser = BuildParser(serviceProvider);

        await parser.InvokeAsync(args).ConfigureAwait(false);
        Console.WriteLine("Press enter to exit");
        Console.ReadLine();
    }

    private static Parser BuildParser(IServiceProvider serviceProvider)
    {
        var rootCommand = new RootCommand("link");
        var commandLineBuilder = new CommandLineBuilder(rootCommand);

        foreach (var command in serviceProvider.GetServices<Command>())
        {
            rootCommand.AddCommand(command);
        }

        return commandLineBuilder.UseDefaults().Build();
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        services.AddSingleton<IConfiguration>(config);
        services.AddCliCommands();

        return services.BuildServiceProvider();
    }

    private static IConfigurationRoot BuildConfig()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
        return builder.Build();
    }
}