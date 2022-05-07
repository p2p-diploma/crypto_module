namespace Crypto.Domain.Configuration;

public record SmartContractSettings
{
    public string EthereumTransferAddress { get; set; }
    public string EthereumTransferCreation_Tx { get; set; }
    public string StandardERC20Address { get; set; }
    public string StandardERC20Creation_Tx { get; set; }
    public string ERC20TransferAddress { get; set; }
    public string ERC20TransferCreation_Tx { get; set; }
}