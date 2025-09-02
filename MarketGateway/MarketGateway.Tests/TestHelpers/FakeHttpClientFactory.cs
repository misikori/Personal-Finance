namespace MarketGateway.Tests.TestHelpers;

public sealed class FakeHttpClientFactory : IHttpClientFactory, IDisposable
{
    private readonly Dictionary<string, HttpClient> _map = new(StringComparer.OrdinalIgnoreCase);
    public void Add(string name, HttpClient client) => _map[name] = client;
    public HttpClient CreateClient(string name) => _map[name];

    public void Dispose()
    {
        foreach(var val in _map.Values) val?.Dispose();
        _map.Clear();
    }
}