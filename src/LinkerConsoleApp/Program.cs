using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Linker;
using Microsoft.Extensions.Configuration;
using NLog;

namespace LinkerConsoleApp
{
    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        static async Task<int> Main(string[] args)
        {
            Log.Info("Building services...");

            IEnumerable<Link> links;
            if (args.Length > 0)
                links = await BuildLinksFromArgs(args);
            else
            {
                var config = BuildConfig();
                links = config.GetSection("links").Get<IEnumerable<Link>>();
            }

            var services = new List<LinkerService>();
            foreach (var link in links)
            {
                if (link.Filters == null || !link.Filters.Any())
                {
                    Log.Info("Setting 'include all' default filter");
                    var defaultFilter = new Filter(FilterType.Stream, "*", FilterOperation.Include);
                    link.Filters = new List<Filter> {defaultFilter};
                }
                var filters = link.Filters.Select(linkFilter => new Filter
                {
                    FilterOperation = linkFilter.FilterOperation, FilterType = linkFilter.FilterType,
                    Value = linkFilter.Value
                }).ToList();
                var filterService = new FilterService(filters);
                var service = new LinkerService(new LinkerConnectionBuilder(new Uri(link.Origin.ConnectionString),
                    ConnectionSettings.Create().SetHeartbeatInterval(TimeSpan.FromSeconds(6))
                        .SetHeartbeatTimeout(TimeSpan.FromSeconds(3)),
                    link.Origin.ConnectionName), new LinkerConnectionBuilder(new Uri(link.Destination.ConnectionString),
                    ConnectionSettings.Create().SetHeartbeatInterval(TimeSpan.FromSeconds(6))
                        .SetHeartbeatTimeout(TimeSpan.FromSeconds(3)),
                    link.Destination.ConnectionName), filterService, Settings.Default(), new NLogger());
                services.Add(service);
            }
            await StartServices(services);
            Log.Info("Press enter to exit the program");
            Console.ReadLine();
            return 0;
        }

        private static async Task<IEnumerable<Link>> BuildLinksFromArgs(string[] args)
        {
            IEnumerable<Link> links;
            var originConnStringOption = new Option<string>(
                name: "--aaa",
                description: "The connectionstring from where data are coming out");
            var originConnNameOption = new Option<string>(
                name: "--origin-connection-name",
                description: "The name of the origin connection shown in EventStore");

            var destinationConnStringOption = new Option<string>(
                name: "--destination-connection-string",
                description: "The connectionstring to where data are going in");
            var destinationConnNameOption = new Option<string>(
                name: "--destination-connection-name",
                description: "The name of the destination connection shown in EventStore");

            var rootCommand = new RootCommand("LinkerConsoleApp");
            var cmd = new Command("do something", "ciao ciao")
            {
                originConnStringOption, 
                originConnNameOption, 
                destinationConnStringOption, 
                destinationConnNameOption
            };

            rootCommand.AddCommand(cmd);

            rootCommand.SetHandler((origConnectionName, origConnectionString, destConnectionName, destConnectionString) =>
                {
                    links = BuildLinks(origConnectionName, origConnectionString, destConnectionName,
                        destConnectionString);
                },
                originConnNameOption, originConnStringOption, destinationConnStringOption);
           
            await rootCommand.InvokeAsync(args);

            //links = BuildLinks(originConnectionName, aa, destinationConnectionName, destinationConnectionString);
            return links;
        }

        private static IEnumerable<Link> BuildLinks(string originConnectionName, string originConnectionString, string destinationConnectionName,
            string destinationConnectionString)
        {
            IEnumerable<Link> links = new List<Link>
            {
                new()
                {
                    Origin = new Origin { ConnectionName = originConnectionName, ConnectionString = originConnectionString },
                    Destination = new Destination
                        { ConnectionName = destinationConnectionName, ConnectionString = destinationConnectionString }
                }
            };
            return links;
        }

        private static IEnumerable<Link> DoSomething(string originConnStringOption)
        {
            throw new NotImplementedException();
        }

        private static async Task StartServices(IEnumerable<LinkerService> services)
        {
            foreach (var linkerService in services)
            {
                Log.Info($"Starting {linkerService.Name}");
                await linkerService.Start();
            }
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
}
