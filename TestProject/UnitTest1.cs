using Microsoft.Extensions.DependencyInjection;
using Reproduce1;

namespace TestProject;

public class Tests
{
    [Test]
    public async Task Test1()
    {
        var services = TestLibSetup.GetStandardServiceProvider();
        var client = services.GetRequiredService<IIssueTestClient>();

        // The resilience pipeline attached to this service should retry the request 3
        // times before failing, so this should take a few seconds before throwing an exception

        var result = await client.Api.Test.Error.PostAsync();
    }

    [Test]
    public async Task Test2()
    {
        var services = TestLibSetup.GetStandardServiceProvider();
        var client = services.GetRequiredService<IHttpTestClient>();

        // The resilience pipeline attached to this service should retry the request 3
        // times before failing, so this should take a few seconds before throwing an exception
        var result = await client.Test();
    }
}