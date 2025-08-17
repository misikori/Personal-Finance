namespace MarketGateway.Tests.TestHelpers;

public sealed class FakeHttpClientFactory : IHttpClientFactory
{
    private readonly Dictionary<string, HttpClient> _map = new(StringComparer.OrdinalIgnoreCase);
    public void Add(string name, HttpClient client) => _map[name] = client;
    public HttpClient CreateClient(string name) => _map[name];
}