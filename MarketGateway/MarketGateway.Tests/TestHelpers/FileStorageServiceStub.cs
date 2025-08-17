using MarketGateway.Data;

namespace MarketGateway.Tests.TestHelpers;


public sealed class FileStorageServiceStub : FileStorageService
{
    public FileStorageServiceStub(string? root = null) : base(root ?? Path.GetTempPath()) {}
    public Task SaveApiResponseAsync(string vendor, string key, string json)
    {
        // Optionally assert json content or do nothing to keep tests fast
        return Task.CompletedTask;
    }
}