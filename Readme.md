!Deprecated Note

This project is deprecated. For a new please use lyra-crypto package: [https://github.com/LYRA-Block-Lattice/lyra-crypto](https://github.com/LYRA-Block-Lattice/lyra-crypto)

# About

Lyra Broker is a gateway for any platform/languages to connect to the Lyra blockchain.

# Change Log

## ver 1.1

* Upgrade all package to latest version (dotnet, gRPC, and Lyra)
* gRPC api uses http2 port 3505, swagger/rest api uses http1 port 3506
* gRPC latest version uses string to present data type 'long'

Note: please note that the first entry of transaction history always show zero balance change

# Build and run your own release

Build LyraBorker.

modify appsettings.json for LyraBroker, change

``"network": "testnet"``

to testnet/mainnet. then run LyraBorker.

# Run from docker.io

for testnet:

	docker pull wizdy/lyrabroker:testnet_latest
	docker run -d --restart unless-stopped -it -p 3505:3505 -p 3506:3506 wizdy/lyrabroker:testnet_latest

for mainnet:
	
	docker pull wizdy/lyrabroker:mainnet_latest
	docker run -d --restart unless-stopped -it -p 3505:3505 -p 3506:3506 wizdy/lyrabroker:mainnet_latest

# Demo broker for testnet

http://brokerdemo.testnet.lyra.live:3506/swagger/index.html

# Lyra Broker generic API specification

For now we only provides these basic API:

* CreateAccount: create a new account (private/public key pair) for Lyra.
* ValidateAccountID: validate a Lyra account ID.
* ValidatePrivateKey: validate a Lyra private key.
* GetAccountIdFromPrivateKey: get account ID from a private key.
* GetStatus: get the current status of Lyra network.
* GetBalance: get latest balance of account. (implicit receive);
* Send: send funds to other account.
* GetTransactions: query transaction history for a Lyra account. (Note: This API only show change/balance of LYR)
* GetTransByHash: query single transaction by tx hash.

For advanced features of Lyra, such as token creation, NFT, etc. please use Lyra native web API instead.

# Client Intergration

Lyra Broker use standard gRPC to provide services. Any client/systems can talk by gRPC can use the broker.

Lyra Broker also provides RESTful API.

# Note for gRPC API

We have a wallet running on Node-JS for demo: LyraJsWallet. More examples comming soon.

The Google protobuf file is here: https://github.com/LYRA-Block-Lattice/LyraBroker/blob/master/LyraBroker/Protos/broker.proto

For more language support please visit: https://grpc.io/docs/languages/

# Note for RESTFul API

After setup docker container, document can be accessed by: http://[docker ip]:3506/swagger/index.html 

# Security concerns

Lyra Broker is NOT supposed to be running on public server because it knows your private keys. 
Please run it in private/trusted environment. And more importantly, never use any other's Lyra Broker services or your funds could lost.


