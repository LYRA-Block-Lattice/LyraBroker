using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LyraBroker.Services
{
    public class BrokerStorage : ITransitWalletStore
    {
        public ConcurrentDictionary<string, TransitWallet> Wallets { get; set; }

        public BrokerStorage()
        {
            Wallets = new ConcurrentDictionary<string, TransitWallet>();
        }
    }
}
