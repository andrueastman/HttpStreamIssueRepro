namespace TestProject;

public class DowngrageHttpVersionHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Version = new Version(1, 1);
        return base.SendAsync(request, cancellationToken);
    }
}