'use strict';

var PROTO_PATH = __dirname + '/../LyraBroker/Protos/broker.proto';

var grpc = require('grpc');
var protoLoader = require('@grpc/proto-loader');
var packageDefinition = protoLoader.loadSync(
    PROTO_PATH,
    {
        keepCase: true,
        longs: String,
        enums: String,
        defaults: true,
        oneofs: true
    });
var lyrabroker = grpc.loadPackageDefinition(packageDefinition).broker;

const readline = require("readline");
const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

function sleep(ms) {
    return new Promise((resolve) => {
        setTimeout(resolve, ms);
    });
} 

const privateKey = '2vWJCzsWDVWeLf5YnrqMnZPm5J33zupDaEimc1QBnqHgVQR2nR';

function main() {
    var client = new lyrabroker.BrokerRPC('localhost:5001',
        grpc.credentials.createInsecure());
    //var user;
    //if (process.argv.length >= 3) {
    //    user = process.argv[2];
    //} else {
    //    user = 'world';
    //}

    // check api node status
    client.GetStatus({}, function (err, response) {
        console.log('Lyra network is ready:', response.IsReady);
        if (response.IsReady) {
            client.GetBalance({ privateKey: privateKey }, function (err, response) {
                if (response.balances == null)
                    console.log('Can\'t get balance.');
                else if (response.balances.length == 0)
                    console.log('Wallet empty.');
                else {
                    console.log('Your balance is:');
                    response.balances.forEach(function (balance) {
                        console.log('%s: %d', balance.ticker, balance.balance);
                    });

                    var sendArgs = {
                        privateKey: privateKey,
                        amount: 9,
                        destAccountId: 'LT8din6wm6SyfnqmmJN7jSnyrQjqAaRmixe2kKtTY4xpDBRtTxBmuHkJU9iMru5yqcNyL3Q21KDvHK45rkUS4f8tkXBBS3',
                        ticker: 'LYR'
                    };

                    client.Send(sendArgs, function (err, response) {
                        if (response.success)
                            console.log('Send success!');
                        else {
                            console.log('Send failed.');
                        }
                    });
                }
            });


        }
    });
    


    rl.question("Press any key to exit.", function (country) {
        console.log("Goodbye!");
        rl.close();
    });
}

main();
