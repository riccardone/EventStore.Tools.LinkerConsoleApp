using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Linker;
using NLog;

namespace LinkerConsoleApp;

public class LinkCommand : Command
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public LinkCommand() : base("link", "Link two EventStore instances")
    {
        var originOption = new Option<Uri>("--origin")
        {
            Name = "origin",
            Description = "Connectionstring from where the data are coming out",
            IsRequired = true
        };
        originOption.AddAlias("-o");

        var originNameOption = new Option<string>("--origin-name")
        {
            Name = "origin-name",
            Description = "Name of the instance from where the data are coming out",
            IsRequired = false
        };
        originNameOption.AddAlias("-on");

        var destinationOption = new Option<Uri>("--destination")
        {
            Name = "destination",
            Description = "Connectionstring to where the data are going in",
            IsRequired = true
        };
        destinationOption.AddAlias("-d");

        var destinationNameOption = new Option<string>("--destination-name")
        {
            Name = "destination-name",
            Description = "Name of the instance to where the data are going in",
            IsRequired = false
        };
        destinationNameOption.AddAlias("-dn");

        AddOption(originOption);
        AddOption(originNameOption);
        AddOption(destinationOption);
        AddOption(destinationNameOption);

        Handler = CommandHandler.Create((Uri origin, string originName, Uri destination, string destinationName) =>
            HandleCommandAsync(origin, originName, destination, destinationName));
    }

    private static async Task<int> HandleCommandAsync(Uri origin, string? originName, Uri destination, string? destinationName)
    {
        var services = new List<LinkerService>();
        var link = new Link
        {
            Origin = new Origin
            { ConnectionName = originName ?? "origin", ConnectionString = origin.ToString() },
            Destination = new Destination
            { ConnectionName = destinationName ?? "destination", ConnectionString = destination.ToString() }
        };
        var defaultFilter = new Filter(FilterType.Stream, "*", FilterOperation.Include);
        link.Filters = new List<Filter> { defaultFilter };
        var filterService = new FilterService(link.Filters);
        var service = new LinkerService(new LinkerConnectionBuilder(new Uri(link.Origin.ConnectionString),
            ConnectionSettings.Create().SetHeartbeatInterval(TimeSpan.FromSeconds(6))
                .SetHeartbeatTimeout(TimeSpan.FromSeconds(3)),
            link.Origin.ConnectionName), new LinkerConnectionBuilder(new Uri(link.Destination.ConnectionString),
            ConnectionSettings.Create().SetHeartbeatInterval(TimeSpan.FromSeconds(6))
                .SetHeartbeatTimeout(TimeSpan.FromSeconds(3)),
            link.Destination.ConnectionName), filterService, Settings.Default(), new NLogger());
        services.Add(service);
        await StartServices(services);
        return 1;
    }

    private static async Task StartServices(IEnumerable<LinkerService> services)
    {
        foreach (var linkerService in services)
        {
            Log.Info($"Starting {linkerService.Name}");
            await linkerService.Start();
        }
    }
}