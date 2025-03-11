using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Extensions.Logging;

namespace IssueHost;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        Log.Information("Starting up");

        var loggerFactory = new SerilogLoggerFactory();
        var logger = loggerFactory.CreateLogger("Startup");

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ConfigureEndpointDefaults(listenOptions =>
            {
                listenOptions.KestrelServerOptions.AddServerHeader = false;
                listenOptions.KestrelServerOptions.Limits.MinResponseDataRate = null;
                listenOptions.KestrelServerOptions.Limits.MinRequestBodyDataRate = null;
                listenOptions.KestrelServerOptions.Limits.MaxRequestBodySize = 100_000_000;
                listenOptions.KestrelServerOptions.Limits.Http2.KeepAlivePingDelay = TimeSpan.MaxValue; //disable https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/options?view=aspnetcore-9.0#http2-keep-alive-ping-configuration
                listenOptions.KestrelServerOptions.Limits.Http2.KeepAlivePingTimeout = TimeSpan.MaxValue; //disable
                //listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;  could force http2 only at some point..
            });
        });

        builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(25));
        builder.Host.UseDefaultServiceProvider(o => { o.ValidateOnBuild = false; });
        var services = builder.Services;
        services.AddOptions();
        services.AddHttpContextAccessor();

        SetupOpenApi(builder);

        var app = builder.Build();

        app.SetupRouting();
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        await app.RunAsync();
    }

    private static void SetupOpenApi(WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new HeaderApiVersionReader("api-version");
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                })
            .AddApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

        builder.Services.AddOpenApi();
    }


    private static void SetupRouting(this WebApplication app)
    {
        var apiMapGroup = app.NewVersionedApi().MapGroup("/api").HasApiVersion(1.0);

        // These commented lines are required when generating the json and client, otherwise the request builder doesn't gen.
        //apiMapGroup.MapPost("/Test/Error", async (string anything, HttpContext context) =>
        apiMapGroup.MapPost("/Test/Error", async (HttpContext context) =>
            {
                var responseFeature = context.Features.Get<IHttpResponseBodyFeature>();
                if (responseFeature != null)
                {
                    responseFeature.StartAsync(default);
                    context.Abort();
                }
            })
            //.Produces((int)HttpStatusCode.OK, typeof(decimal?), "application/json")
            .ProducesProblem(StatusCodes.Status500InternalServerError).ProducesProblem(StatusCodes.Status400BadRequest);

        apiMapGroup.MapGet("/health", () => Results.Text("OK"));
    }
}