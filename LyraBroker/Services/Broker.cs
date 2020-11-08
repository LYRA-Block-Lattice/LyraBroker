using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lyra.Core.API;
using Lyra.Data.Crypto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LyraBroker
{
    public class Broker : BrokerRPC.BrokerRPCBase
    {
        private readonly ILogger<Broker> _logger;
        private readonly IConfiguration _config;

        private ConcurrentDictionary<string, TransitWallet> _wallets;

        private LyraRestClient _client;
        private LyraRestClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraWalletGateway", "1.0");
                }
                return _client;
            }
        }

        public Broker(ILogger<Broker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;

            _wallets = new ConcurrentDictionary<string, TransitWallet>();
        }

        public override async Task<GetStatusReply> GetStatus(Empty request, ServerCallContext context)
        {
            bool LyraIsReady = false;
            try
            {
                LyraIsReady = (await Client.GetSyncState()).SyncState == Lyra.Data.API.ConsensusWorkingMode.Normal;
            }
            catch (Exception ex) {
                _logger.LogWarning("Failed to get sync state: " + ex.Message);
            }

            return new GetStatusReply
            {
                IsReady = LyraIsReady
            };
        }

        public override Task<OpenWalletReply> OpenWallet(OpenWalletRequest request, ServerCallContext context)
        {
            string accountId = null;
            string walletId = null;
            try
            {
                accountId = Signatures.GetAccountIdFromPrivateKey(request.PrivateKey);
                if(_wallets.Values.Any(a => a.AccountId == accountId))
                {
                    var wallet = _wallets.Values.First(a => a.AccountId == accountId);
                    walletId = wallet.UniqId;
                }
                else
                {
                    var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                    var wallet = new TransitWallet(request.PrivateKey, client);
                    _wallets.AddOrUpdate(wallet.UniqId, wallet, (k, v) => wallet);
                    walletId = wallet.UniqId;
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }
            return Task.FromResult(new OpenWalletReply
            {
                AccountId = accountId,
                WalletId = walletId
            });
        }

        public override Task<CloseWalletReply> CloseWallet(CloseWalletRequest request, ServerCallContext context)
        {
            if (_wallets.Values.Any(a => a.UniqId == request.WalletId))
            {
                var wallet = _wallets.Values.First(a => a.UniqId == request.WalletId);
                _wallets.Remove(request.WalletId, out _);
            }

            return Task.FromResult(new CloseWalletReply
            {
                Success = true
            });
        }
    }
}
