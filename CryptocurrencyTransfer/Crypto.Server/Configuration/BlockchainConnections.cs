namespace Crypto.Configuration;

public record BlockchainConnections
{
    public string Ropsten { get; set; }
    public string Kovan { get; set; }
    public string Rinkeby { get; set; }
    public string Goerly { get; set; }
    public string Ganache { get; set; }

    public int GetChainId(string blockchainUrl)
    {
        int chainId = 0;
        if(blockchainUrl == Ropsten)chainId = 3;
        if(blockchainUrl == Rinkeby)chainId = 4;
        if (blockchainUrl == Kovan) chainId = 42;
        if(blockchainUrl == Goerly)chainId = 5;
        if(blockchainUrl == Ganache) chainId = 5777;
        return chainId;
    }
}