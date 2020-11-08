using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lyra.Core.API;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
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

        public Broker(ILogger<Broker> logger, IConfiguration configuration)
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
    }
}
