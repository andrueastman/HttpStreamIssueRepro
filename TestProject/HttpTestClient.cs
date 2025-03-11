using System.Text;
using System.Text.Json;

namespace TestProject;

public interface IHttpTestClient
{
    HttpClient Client { get; set; }
    Task<HttpResponseMessage> Test();
    Task<HttpResponseMessage> Health();
}

public class HttpTestClient(HttpClient client) : IHttpTestClient
{
    public HttpClient Client { get; set; } = client;
    
    public async Task<HttpResponseMessage> Test()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:8304/api/test/error");
        var content = new { token = "test" };
        request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
        return await Client.SendAsync(request);
    }
    
    public async Task<HttpResponseMessage> Health()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:8304/api/health");
        return await Client.SendAsync(request);
    }
}