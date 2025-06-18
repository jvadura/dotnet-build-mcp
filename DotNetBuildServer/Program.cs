using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace DotNetBuildServer;

// claude mcp add dotnet-build --transport sse http://10.0.0.186:5000/sse

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.Error.WriteLine($"WPF Build MCP Server starting...");
        Console.Error.WriteLine($"This server enables building WPF desktop applications from WSL.");

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://*:5000");

        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Information;
        });

        // HTTP transport only - for access from WSL to Windows host
        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();   // Auto-discover tools in this assembly

        var app = builder.Build();
        app.MapMcp();
        await app.RunAsync();

    }
}