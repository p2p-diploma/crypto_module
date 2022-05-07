namespace Crypto.Data.Configuration;

public record DatabaseSettings{
    public string ConnectionString { get; set; } 
    public string DatabaseName { get; set; } 
    public string EthereumWalletsCollection { get; set; }
}