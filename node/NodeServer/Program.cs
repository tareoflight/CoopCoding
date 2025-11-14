namespace NodeServer;

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Node;
using NodeServer.handlers;

public partial class Program
{
    static async Task Main(string[] args)
    {

        const string Name = "NodeServer";
        HostApplicationBuilderSettings settings = new()
        {
            ApplicationName = Name,
            // clear the confiuration defaults
            Configuration = new ConfigurationManager(),
            ContentRootPath = Directory.GetCurrentDirectory(),
            DisableDefaults = true,
            EnvironmentName = Name,
        };
        // main settings file
        PhysicalFileProvider cwd = new(settings.ContentRootPath);
        settings.Configuration.AddJsonFile(cwd, "config.json", false, false);
        // override settings
        settings.Configuration.AddJsonFile(cwd, "config.override.json", true, false);
        // also check the app data
        string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Name, "config.json");
        settings.Configuration.AddJsonFile(null, configPath, true, false);
        // also check the args (if passed)
        if (args.Length > 0)
        {
            settings.Configuration.AddJsonFile(null, args[0], true, false);
        }

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(settings);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        if (Enum.TryParse(builder.Configuration[$"{nameof(NodeServerOptions)}:LogLevel"], out LogLevel defaultLevel))
        {
            builder.Logging.SetMinimumLevel(defaultLevel);
        }

        builder.Services.AddOptions<NodeServerOptions>().Bind(builder.Configuration.GetSection(nameof(NodeServerOptions)));

        builder.Services.AddActivatedSingleton<ControlHandler>();
        builder.Services.AddSingleton<IHandlerMap, HandlerMap>();

        builder.Services.AddSingleton<IAsyncQueue<Request>, AsyncQueue<Request>>();
        builder.Services.AddSingleton<ISocketConnection, SocketConnection>();

        builder.Services.AddHostedService<RequestDispatcher>();
        builder.Services.AddHostedService<ReceiveService>();
        builder.Services.AddHostedService<HeartbeatService>();
        builder.Services.AddHostedService<SpacetimeService>();

        using IHost host = builder.Build();
        await host.RunAsync();
    }
}