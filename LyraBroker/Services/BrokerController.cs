using Lyra.Core.API;
using Lyra.Core.Blocks;
using Lyra.Data.Crypto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LyraBroker
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrokerController : ControllerBase
    {
        private readonly ILogger<Broker> _logger;
        private readonly IConfiguration _config;

        public BrokerController(ILogger<Broker> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        [Route("CreateAccount")]
        [HttpGet]
        public Task<AccountInfo> CreateAccount()
        {
            var keyPair = Signatures.GenerateWallet();
            return Task.FromResult(new AccountInfo
            {
                PrivateKey = keyPair.privateKey,
                AccountId = keyPair.AccountId
            });
        }

        public class AccountInfo
        {
            public string PrivateKey { get; set; }
            public string AccountId { get; set; }
        }

        [Route("GetStatus")]
        [HttpGet]
        public async Task<LyraStatus> GetStatus()
        {
            bool LyraIsReady = false;
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                LyraIsReady = (await client.GetSyncStateAsync()).SyncState == Lyra.Data.API.ConsensusWorkingMode.Normal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to get sync state: " + ex.Message);
            }

            return new LyraStatus
            {
                IsReady = LyraIsReady,
                NetworkId = _config["network"]
            };
        }

        public class LyraStatus
        {
            public bool IsReady { get; set; }
            public string NetworkId { get; set; }
        }

        [Route("GetBalance")]
        [HttpGet]
        public async Task<BalanceInfo> GetBalance(string privateKey)
        {
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                var wallet = new TransitWallet(privateKey, client);

                var result = await wallet.ReceiveAsync();

                if (result == Lyra.Core.Blocks.APIResultCodes.Success)
                {
                    var blances = await wallet.GetBalanceAsync();
                    if (blances != null)
                    {
                        var msg = new BalanceInfo
                        {
                            AccountId = wallet.AccountId
                        };
                        var list = new List<LyraBalance>();
                        foreach (var kvp in blances)
                        {
                            list.Add(new LyraBalance { Ticker = kvp.Key, Balance = kvp.Value / 100000000 });
                        }
                        msg.Balances = list.ToArray();
                        return msg;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }

            return new BalanceInfo();
        }

        public class BalanceInfo
        {
            public string AccountId { get; set; }
            public LyraBalance[] Balances { get; set; }
        }

        public class LyraBalance
        {
            public string Ticker { get; set; }
            public double Balance { get; set; }
        }

        [Route("Send")]
        [HttpGet]
        public async Task<SendResult> Send(string privateKey, double amount,
            string destAccountId, string ticker)
        {
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                var wallet = new TransitWallet(privateKey, client);

                var result = await wallet.SendAsync((decimal)amount, destAccountId, ticker);

                if (result == Lyra.Core.Blocks.APIResultCodes.Success)
                {
                    return new SendResult { Success = true, SendHash = wallet.LastTxHash };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }

            return new SendResult { Success = false, SendHash = "" };
        }

        public class SendResult
        {
            public bool Success { get; set; }
            public string SendHash { get; set; }
        }

        [Route("GetTransactions")]
        [HttpGet]
        public async Task<TransactionInfo> GetTransactions(string accountId,
            long startTimeTicks, long endTimeTicks, int count)
        {
            var resp = new TransactionInfo { };
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");
                var result = await client.SearchTransactionsAsync(accountId,
                            startTimeTicks,
                            endTimeTicks,
                            count);

                if (result.ResultCode == APIResultCodes.Success)
                {
                    var list = new List<Transaction>();
                    for (int i = 0; i < result.Transactions.Count; i++)
                    {
                        var txDesc = result.Transactions[i];
                        var tx = new Transaction
                        {
                            Height = txDesc.Height,
                            Time = txDesc.TimeStamp,
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

                        list.Add(tx);
                    }
                    resp.Transactions = list.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("In OpenWallet: " + ex.ToString());
            }

            return resp;
        }

        public class TransactionInfo
        {
            public Transaction[] Transactions { get; set; }
        }

        public class Transaction
        {
            public long Height { get; set; }
            public DateTime Time { get; set; }
            public bool IsReceive { get; set; }
            public string SendAccountId { get; set; }
            public string SendHash { get; set; }
            public string RecvAccountId { get; set; }
            public string RecvHash { get; set; }
            public double BalanceChange { get; set; }
            public double Balance { get; set; }
        }

        [Route("GetTransByHash")]
        [HttpGet]
        public async Task<TxInfo> GetTransByHash(string hash)
        {
            try
            {
                var client = LyraRestClient.Create(_config["network"], Environment.OSVersion.ToString(), "LyraBroker", "1.0");

                var result = await client.GetBlockAsync(hash);

                if (result.ResultCode == Lyra.Core.Blocks.APIResultCodes.Success && result.GetBlock() is TransactionBlock block)
                {
                    var tx = new TxInfo
                    {
                        TxHash = block.Hash,
                        Height = block.Height,
                        Time = block.TimeStamp
                    };

                    tx.TxType = block is SendTransferBlock ? TransactionType.Send : TransactionType.Receive;
                    if (tx.TxType == TransactionType.Send)
                    {
                        tx.OwnerAccountId = block.AccountID;
                        tx.PeerAccountId = (block as SendTransferBlock).DestinationAccountId;

                        var rcvBlockQuery = await client.GetBlockBySourceHashAsync(block.Hash);
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

                        var sndBlockQuery = await client.GetBlockAsync((block as ReceiveTransferBlock).SourceHash);
                        if(sndBlockQuery.ResultCode == APIResultCodes.Success)
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

            return new TxInfo { TxHash = "" };
        }

        public enum TransactionType { Unknown, Send, Receive }; 
        public class TxInfo
        {
            public string TxHash { get; set; }
            public TransactionType TxType { get; set; }
            public string OwnerAccountId { get; set; }
            public string PeerAccountId { get; set; }
            public long Height { get; set; }
            public DateTime Time { get; set; }
            public bool IsReceived { get; set; }
            public string RecvHash { get; set; }
        }
    }
}
