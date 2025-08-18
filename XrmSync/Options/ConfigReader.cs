using Microsoft.Extensions.Configuration;

namespace XrmSync.Options;

public interface IConfigReader
{
    IConfiguration GetConfiguration();
}

internal class ConfigReader : IConfigReader
{
    public const string CONFIG_FILE_BASE = "appsettings";

    public IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{CONFIG_FILE_BASE}.json", optional: true)
                .AddJsonFile($"{CONFIG_FILE_BASE}.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
    }
}
