﻿using Crypto.Domain.Models.Base;
using Nethereum.KeyStore.Model;

namespace Crypto.Domain.Models;

public record EthereumP2PWallet<TId> : IWallet<TId>
{
    public TId Id { get; set; }
    public TId UserId { get; set; }
    public string Hash { get; set; }
    public KeyStore<ScryptParams> KeyStore { get; set; }
}