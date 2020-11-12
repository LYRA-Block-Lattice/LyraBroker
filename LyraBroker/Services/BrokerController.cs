using Lyra.Data.Crypto;
using Microsoft.AspNetCore.Mvc;
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
        [Route("CreateAccount")]
        [HttpGet]
        public Task<AccountInfo> CreateAccount()
        {
            var keyPair = Signatures.GenerateWallet();
            return Task.FromResult(new AccountInfo
            {
                privateKey = keyPair.privateKey,
                accountId = keyPair.AccountId
            });
        }
    }

    public class AccountInfo
    {
        public string privateKey { get; set; }
        public string accountId { get; set; }
    }
}
