# HelloStub – prosty Stub dla HttpClient w testach jednostkowych

Ten przykład pokazuje, jak w C# i xUnit stworzyć **własny Stub dla `HttpClient`**,   aby testować kod bez potrzeby odpytywania prawdziwego serwera HTTP.

---

## 📌 Klasa StubHttpMessageHandler

W projekcie znajduje się klasa:

```csharp
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

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var key = request.RequestUri!.ToString();
        if (_map.TryGetValue(key, out var resp))
            return Task.FromResult(resp);

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
```

- `When(url, status, content)` pozwala skonfigurować odpowiedź dla konkretnego adresu URL.  
- Jeżeli adres nie został skonfigurowany → zwracane jest **404 NotFound**.  

---

## 📌 Testowana usługa

Na potrzeby przykładu zakładamy istnienie prostej usługi:

```csharp
public class HelloService
{
    private readonly HttpClient _http;

    public HelloService(HttpClient http) => _http = http;

    public async Task<string> GetHelloAsync(string name)
    {
        return await _http.GetStringAsync($"https://api.example.com/hello?name={name}");
    }
}
```

---

## 📌 Testy jednostkowe (xUnit)

Chwyt polega na tym, że klienta `HttpClient` przekazujemy poprzez konstruktor własny `HttpMessageHandler`, aby kontrolować odpowiedzi.

```csharp
public class HelloServiceTests
{
    [Fact]
    public async Task ReturnsConfiguredResponse()
    {
        // Arrange
        var stub = new StubHttpMessageHandler();
        stub.When("https://api.example.com/hello?name=Alice", HttpStatusCode.OK, "Hello Alice!");
        var http = new HttpClient(stub);
        var service = new HelloService(http);

        // Act
        var result = await service.GetHelloAsync("Alice");

        // Assert
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
```

---

## ✅ Zalety rozwiązania

- Brak potrzeby uruchamiania zewnętrznego serwera testowego.  
- Można łatwo symulować różne odpowiedzi HTTP (200, 404, itp.).  
- Testy są szybkie, deterministyczne i proste do utrzymania.  

---

## 🚀 Jak uruchomić testy

1. Przygotuj środowisko:
   ```bash
   dotnet restore
   ```

2. Uruchom testy:
   ```bash
   dotnet test
   ```

---

## Podsumowanie

👉 To podejście jest szczególnie przydatne, gdy chcesz przetestować logikę korzystającą z `HttpClient`,  
a nie zależy Ci na integracji z prawdziwym API.
