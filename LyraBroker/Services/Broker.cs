using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lyra.Core.API;
using Lyra.Core.Blocks;
using Lyra.Data.API;
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

        public Broker(ILogger<Broker> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        public override Task<CreateAccountReply> CreateAccount(Empty request, ServerCallContext context)
        {
            (string privateKey, string accountId) = Signatures.GenerateWallet();
            return Task.FromResult(new CreateAccountReply
            {
                PrivateKey = privateKey,
                AccountId = accountId
            });
        }

        public override async Task<GetStatusReply> GetStatus(Empty request, ServerCallContext context)
        {
            bool LyraIsReady = false;
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                LyraIsReady = (await client.GetSyncState()).SyncState == Lyra.Data.API.ConsensusWorkingMode.Normal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to get sync state: " + ex.Message);
            }

            return new GetStatusReply
            {
                IsReady = LyraIsReady,
                NetworkId = _config["network"]
            };
        }

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
                        var msg = new GetBalanceReply { AccountId = wallet.AccountId };
                        foreach (var kvp in blances)
                        {
                            msg.Balances.Add(new LyraBalance { Ticker = kvp.Key, Balance = kvp.Value / 100000000 });
                        }
                        return msg;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }

            return new GetBalanceReply();
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
                    return new SendReply { Success = true, SendHash = wallet.LastTxHash };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }

            return new SendReply { Success = false, SendHash = "" };
        }

        public override async Task<GetTransactionsReply> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var resp = new GetTransactionsReply { };
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                var result = await client.SearchTransactions(request.AccountId,
                            request.StartTime.ToDateTime().Ticks,
                            request.EndTime.ToDateTime().Ticks,
                            request.Count);

                if(result.ResultCode == APIResultCodes.Success)
                {
                    for (int i = 0; i < result.Transactions.Count; i++)
                    {
                        var txDesc = result.Transactions[i];
                        var tx = new LyraTransaction
                        {
                            Height = txDesc.Height,
                            Time = Timestamp.FromDateTime(txDesc.TimeStamp),
                            IsReceive = txDesc.IsReceive,

                            SendAccountId = txDesc.SendAccountId ?? "",
                            SendHash = txDesc.SendHash ?? "",
                            RecvAccountId = txDesc.RecvAccountId,
                            RecvHash = txDesc.RecvHash ?? ""        // protobuf not like null
                        };

                        if (txDesc.Changes == null || !txDesc.Changes.ContainsKey(LyraGlobal.OFFICIALTICKERCODE))
                            tx.BalanceChange = 0;
                        else
                            tx.BalanceChange = txDesc.Changes[LyraGlobal.OFFICIALTICKERCODE] / LyraGlobal.TOKENSTORAGERITO;

                        if (txDesc.Balances == null || !txDesc.Balances.ContainsKey(LyraGlobal.OFFICIALTICKERCODE))
                            tx.Balance = 0;
                        else
                            tx.Balance = txDesc.Balances[LyraGlobal.OFFICIALTICKERCODE] / LyraGlobal.TOKENSTORAGERITO;



                        resp.Transactions.Add(tx);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }

            return resp;
        }

        public override async Task<GetTransByHashReply> GetTransByHash(GetTransByHashRequest request, ServerCallContext context)
        {
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");

                var result = await client.GetBlock(request.Hash);

                if (result.ResultCode == Lyra.Core.Blocks.APIResultCodes.Success && result.GetBlock() is TransactionBlock block)
                {
                    var tx = new GetTransByHashReply 
                    { 
                        TxHash = block.Hash,
                        Height = block.Height,
                        Time = Timestamp.FromDateTime(block.TimeStamp)
                    };

                    tx.TxType = block is SendTransferBlock ? TransactionType.Send : TransactionType.Receive;
                    if(tx.TxType == TransactionType.Send)
                    {
                        tx.OwnerAccountId = block.AccountID;
                        tx.PeerAccountId = (block as SendTransferBlock).DestinationAccountId;

                        var rcvBlockQuery = await client.GetBlockBySourceHash(block.Hash);
                        if (rcvBlockQuery.ResultCode == APIResultCodes.Success)
                        {
                            tx.IsReceived = true;
                            tx.RecvHash = rcvBlockQuery.GetBlock().Hash;
                        }
                        else
                        {
                            tx.IsReceived = false;
                            tx.RecvHash = "";   //gRPC don't like null
                        }
                    }
                    else
                    {
                        tx.OwnerAccountId = block.AccountID;

                        var sndBlockQuery = await client.GetBlock((block as ReceiveTransferBlock).SourceHash);
                        if (sndBlockQuery.ResultCode == APIResultCodes.Success)
                        {
                            tx.PeerAccountId = (sndBlockQuery.GetBlock() as SendTransferBlock).AccountID;
                        }

                        tx.IsReceived = true;
                        tx.RecvHash = block.Hash;
                    }

                    return tx;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }

            return new GetTransByHashReply { TxHash = "" };
        }
    }
}
