using Crypto.Protos.Contracts;
using Crypto.Protos.Wallets;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5227");
await TestGetWallet(channel);



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
    /*Console.WriteLine("Is stored: " + response1.IsStored + ", error: " + response1.ErrorMessage);
    var mes3 = new DenyTransferRequest
    {
        SlotId = slotId
    };
    var response3 = await client.DenyTransferAsync(mes3);
    Console.WriteLine("Is denied: " + response3.IsDenied + ", error: " + response3.ErrorMessage);*/
}

async Task TestGetWallet(GrpcChannel channel)
{
    var client = new WalletProtoService.WalletProtoServiceClient(channel);
    var mes = new LoadUserWalletRequest
    {
        Email = "hasenovsultan@gmail.com",
        Password = "qwerty123",
        PrivateKey = "0x22740aa06fb65cc705ee26859fe931736fbc5fa08063fccc28d274aaedec8284"
    };
    var loadedWallet = await client.LoadWalletAsync(mes);
    var mes1 = new GetUserWalletRequest
    {
        Id = loadedWallet.Id
    };
    var wallet = await client.GetWalletAsync(mes1);
    Console.WriteLine(wallet.Balance + " " + wallet.Address + " " + wallet.PrivateKey);
}