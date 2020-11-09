'use strict';

var PROTO_PATH = __dirname + '/../LyraBroker/Protos/broker.proto';

var grpc = require('grpc');
var google = require('google-protobuf');
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

Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}

function AddMinutesToDate(date, minutes) {
    return new Date(date.getTime() + minutes * 60000);
}

const privateKey = '2vWJCzsWDVWeLf5YnrqMnZPm5J33zupDaEimc1QBnqHgVQR2nR';

function main() {
    var client = new lyrabroker.BrokerRPC('localhost:5001',
        grpc.credentials.createInsecure());



    // generate a new private/account ID pair
    client.CreateAccount({}, function (err, response) {
        console.log('You just created a new Lyra account:\n')
        console.log('Your private key is %s\n', response.privateKey);
        console.log('Your account ID is %s\n', response.accountId);
    });

    // check api node status
    client.GetStatus({}, function (err, response) {
        console.log('Lyra network %s is ready: %s\n', response.networkId, response.isReady);
        console.log('\nDoing wallet %s\n', privateKey);
        if (response.isReady) {
            client.GetBalance({ privateKey: privateKey }, function (err, response) {
                if (response.balances == null)
                    console.log('Can\'t get balance.');
                else if (response.balances.length == 0)
                    console.log('Wallet empty.');
                else {
                    var myAccountId = response.accountId;
                    console.log('Your balance is:');
                    response.balances.forEach(function (balance) {
                        console.log('%s: %d', balance.ticker, balance.balance);
                    });

                    var sendArgs = {
                        privateKey: privateKey,
                        amount: 10,
                        destAccountId: 'LT8din6wm6SyfnqmmJN7jSnyrQjqAaRmixe2kKtTY4xpDBRtTxBmuHkJU9iMru5yqcNyL3Q21KDvHK45rkUS4f8tkXBBS3',
                        ticker: 'LYR'
                    };

                    console.log('sending 10 LYR to %s ...\n', sendArgs.destAccountId);

                    client.Send(sendArgs, function (err, response) {
                        if (response.success) {
                            console.log('Send success!\n');
                            client.GetBalance({ privateKey: privateKey }, function (err, response) {
                                console.log('Your new balance is:');
                                response.balances.forEach(function (balance) {
                                    console.log('%s: %d', balance.ticker, balance.balance);
                                });

                                var now = new Date();
                                var start = AddMinutesToDate(now, -30);

                                var nows = Math.floor(now.getTime() / 1000);
                                var starts = Math.floor(start.getTime() / 1000);

                                var txSearchArgs = {
                                    accountId: myAccountId,
                                    startTime: {seconds: starts},
                                    endTime: { seconds: nows },      // last 24 hours
                                    count: 1000
                                };

                                client.GetTransactions(txSearchArgs, function (err, response) {
                                    response.Transactions.forEach(function (tx) {
                                        console.log('Time is: %s\nTransaction: %s\nPeer Account ID: %s\nLYR Balance Changes: %d\nLYR Balance: %d\n',
                                            new Date(tx.time.seconds * 1000),
                                            tx.isReceive ? "Receive" : "Send",
                                            tx.peerAccountId,
                                            tx.balanceChange,
                                            tx.balance
                                        );
                                    });
                                });
                            });
                        }                            
                        else {
                            console.log('Send failed.');
                        }
                    });
                }
            });


        }
    });
    


    rl.question("Press any key to exit.\n\n", function () {
        console.log("Goodbye!");
        rl.close();
    });
}

main();
