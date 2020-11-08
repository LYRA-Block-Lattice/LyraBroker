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
            // open wallet
            client.OpenWallet({ privateKey: privateKey }, function (err, response) {
                console.log('Wallet\'s ID is: ', response.walletId);

                var id = response.walletId;
                client.CloseWallet({ walletId: response.walletId }, function (err, response) {
                    console.log('Wallet with ID ${id} is closed.');
                });
            });
        }
    });
    


    rl.question("Press any key to exit.", function (country) {
        console.log("Goodbye!");
        rl.close();
    });
}

main();
