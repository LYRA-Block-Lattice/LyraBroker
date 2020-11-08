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

        public Broker(ILogger<Broker> logger, 
            IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
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

/*        public override Task<OpenWalletReply> OpenWallet(OpenWalletRequest request, ServerCallContext context)
        {
            string accountId = null;
            string walletId = null;
            try
            {
                accountId = Signatures.GetAccountIdFromPrivateKey(request.PrivateKey);
                if(_store.Wallets.Values.Any(a => a.AccountId == accountId))
                {
                    var wallet = _store.Wallets.Values.First(a => a.AccountId == accountId);
                    walletId = wallet.UniqId;
                }
                else
                {
                    var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                    var wallet = new TransitWallet(request.PrivateKey, client);
                    _store.Wallets.AddOrUpdate(wallet.UniqId, wallet, (k, v) => wallet);
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
            if (_store.Wallets.Values.Any(a => a.UniqId == request.WalletId))
            {
                var wallet = _store.Wallets.Values.First(a => a.UniqId == request.WalletId);
                _store.Wallets.Remove(request.WalletId, out _);
            }

            return Task.FromResult(new CloseWalletReply
            {
                Success = true
            });
        }*/

        public override async Task<GetBalanceReply> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                var wallet = new TransitWallet(request.PrivateKey, client);

                var result = await wallet.ReceiveAsync();

                if (result == Lyra.Core.Blocks.APIResultCodes.Success)
                {
                    var blances = await wallet.GetBalanceAsync();
                    if (blances != null)
                    {
                        var msg = new GetBalanceReply();
                        foreach (var kvp in blances)
                        {
                            msg.Balances.Add(new LyraBalance { Ticker = kvp.Key, Balance = kvp.Value / 100000000 });
                        }
                        return msg;
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }

            return new GetBalanceReply ();
        }

        public override async Task<SendReply> Send(SendRequest request, ServerCallContext context)
        {
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                var wallet = new TransitWallet(request.PrivateKey, client);

                var result = await wallet.SendAsync((decimal)request.Amount, request.DestAccountId, request.Ticker);

                if (result == Lyra.Core.Blocks.APIResultCodes.Success)
                {
                    return new SendReply { Success = true };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }

            return new SendReply { Success = false };
        }
    }
}
