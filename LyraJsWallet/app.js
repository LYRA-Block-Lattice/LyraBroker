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

function main() {
    var client = new lyrabroker.BrokerRPC('localhost:5001',
        grpc.credentials.createInsecure());
    var user;
    if (process.argv.length >= 3) {
        user = process.argv[2];
    } else {
        user = 'world';
    }
    client.GetStatus({ }, function (err, response) {
        console.log('Lyra network is ready:', response.IsReady);
    });

    rl.question("Where do you live ? ", function (country) {
        console.log(`${name}, is a citizen of ${country}`);
        rl.close();
    });
}

main();
