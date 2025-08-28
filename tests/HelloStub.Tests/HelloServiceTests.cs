using System.Net;

namespace HelloStub.Tests;

public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, HttpResponseMessage> _map = new();

    public void When(string url, HttpStatusCode status, string content)
    {
        _map[url] = new HttpResponseMessage(status)
        {
            Content = new StringContent(content)
        };
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        => Task.FromResult(_map.TryGetValue(request.RequestUri!.ToString(), out var resp)
            ? resp
            : new HttpResponseMessage(HttpStatusCode.NotFound));
}

public class HelloServiceTests
{
    [Fact]
    public async Task ReturnsConfiguredResponse()
    {
        // arrange
        var stub = new StubHttpMessageHandler();
        stub.When("https://api.example.com/hello?name=Alice", HttpStatusCode.OK, "Hello Alice!");

        var http = new HttpClient(stub);
        var service = new HelloService(http);

        // act
        var result = await service.GetHelloAsync("Alice");

        // assert
        Assert.Equal("Hello Alice!", result);
    }

    [Fact]
    public async Task Returns404WhenNotConfigured()
    {
        var stub = new StubHttpMessageHandler();
        var http = new HttpClient(stub);
        var service = new HelloService(http);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetHelloAsync("Bob"));
    }
}