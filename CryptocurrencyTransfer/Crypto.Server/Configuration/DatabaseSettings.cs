namespace Crypto.Configuration;

public record DatabaseSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}