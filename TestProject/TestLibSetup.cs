using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Polly;
using Reproduce1;
using TimeSpan = System.TimeSpan;

[assembly: Parallelizable(ParallelScope.All)]

namespace TestProject;

[SetUpFixture]
public class TestLibSetup
{
    public static IServiceProvider GetStandardServiceProvider()
    {
        return GetHostBuilder().Build().Services;
    }

    private static IHostBuilder GetHostBuilder()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.UseDefaultServiceProvider(x => { x.ValidateOnBuild = false; });
        
        builder.ConfigureServices((_, services) =>
        {
            services.AddHttpClient<IIssueTestClient, IssueTestClient>(client =>
            {
                client.Timeout = new TimeSpan(0, 5, 0);
                var requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), null, null, client) { BaseUrl = "https://localhost:8304" };
                return new IssueTestClient(requestAdapter);
            }).AddResilienceHandler("custom-pipeline", builder =>
            {
                builder.AddRetry(new CustomRetryStrategyOptions());
            });
            
            services.AddHttpClient<IHttpTestClient, HttpTestClient>(client =>
            {
                client.Timeout = new TimeSpan(0, 5, 0);
                //client.DefaultRequestVersion = HttpVersion.Version20;
                //client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                return new HttpTestClient(client);
            }).AddResilienceHandler("testApi-pipeline", (builder, _) =>
            {
                builder.AddRetry(new CustomRetryStrategyOptions());
            });
        });
        
        return builder;
    }
}