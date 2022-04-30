namespace Crypto.Configuration;

public record SmartContractSettings
{
    public string Address { get; set; }
    public string Creation_Tx { get; set; }
    public string TestSenderPrivateKey { get; set; }
}