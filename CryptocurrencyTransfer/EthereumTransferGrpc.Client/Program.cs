using EthereumTransferGrpc;
using EthereumTransferGrpc.Protos.Contracts;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("https://localhost:7227");
await TestTransferAgreement(channel);



async Task TestTransferAgreement(GrpcChannel channel)
{
    var client = new TransferAgreementProtoService.TransferAgreementProtoServiceClient(channel);
    var slotId = Guid.NewGuid().ToString();
    var mes1 = new StoreInBlockRequest
    {
        Recipient = "0x94BfAE8dAa5dbD421Ccfe24169362425B612D7ca",
        SlotId = slotId,
        EtherAmount = 0.51
    };
    var response1 = await client.StoreInBlockAsync(mes1);
    Console.WriteLine("Is stored: " + response1.IsStored + ", error: " + response1.ErrorMessage);
    /*var mes2 = new TransferToRecipientRequest
    {
        SlotId = slotId
    };
    var response2 = await client.TransferToRecipientAsync(mes2);
    Console.WriteLine("Is transferred: " + response2.IsTransferred + ", error: " + response2.ErrorMessage);*/
    var mes3 = new DenyTransferRequest
    {
        SlotId = slotId
    };
    var response3 = await client.DenyTransferAsync(mes3);
    Console.WriteLine("Is denied: " + response3.IsDenied + ", error: " + response3.ErrorMessage);
}


async Task TestWallet(GrpcChannel channel)
{
    var client = new WalletProtoService.WalletProtoServiceClient(channel);

    var message = new CreateUserWalletRequest
    {
        Email = "hasenovsultanbek@gmail.com",
        Password = "qwerty123"
    };
    var createWalletResponse = await client.CreateWalletAsync(message);
    Console.WriteLine($"Created wallet id: {createWalletResponse.Id}, \n address: {createWalletResponse.Address},\n private key: {createWalletResponse.PrivateKey} \n");

    var loadedWallet = await client.LoadWalletAsync(new LoadUserWalletRequest
    {
        Id = createWalletResponse.Id
    });
    Console.WriteLine($"Wallet address: {loadedWallet.Address}, \n private key: {loadedWallet.PrivateKey}, \n balance = {loadedWallet.Balance} ETH");

    createWalletResponse = await client.CreateWalletAsync(message);
    Console.WriteLine($"Created wallet address: {createWalletResponse.Address} \n, private key: {createWalletResponse.PrivateKey} \n");
}