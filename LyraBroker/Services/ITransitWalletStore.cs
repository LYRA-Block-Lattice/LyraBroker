using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LyraBroker.Services
{
    public interface ITransitWalletStore
    {
        ConcurrentDictionary<string, TransitWallet> Wallets { get; set; }
    }
}
