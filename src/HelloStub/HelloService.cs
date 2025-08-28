namespace HelloStub;

public class HelloService
{
    private readonly HttpClient _http;

    public HelloService(HttpClient http) => _http = http;

    public async Task<string> GetHelloAsync(string name)
    {
        var url = $"https://api.example.com/hello?name={name}";
        return await _http.GetStringAsync(url);
    }
}