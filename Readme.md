# About

Lyra Broker is a gateway for any platform/languages to connect to the Lyra blockchain.

# Build

Build Lyra Core: https://github.com/LYRA-Block-Lattice/Lyra-Core, it will generate Lyra.Data.dll.

Build LyraBorker by referencing the proper Lyra.Data.dll.

modify appsettings.json for LyraBroker, change

``"network": "testnet"``

to testnet/mainnet. then run LyraBorker.

# Docker Setup

	docker pull wizdy/lyrabroker:latest
	docker run -it -p 3505:3505 wizdy/lyrabroker

# Client Intergration

Lyra Broker use standard gRPC to provide services. Any client/systems can talk by gRPC can use the broker.

We have a wallet running on Node-JS for demo: LyraJsWallet. More examples comming soon.

The Google protobuf file is here: https://github.com/LYRA-Block-Lattice/LyraBroker/blob/master/LyraBroker/Protos/broker.proto

For more language support please visit: https://grpc.io/docs/languages/

# API Documents

For now we only provides these basic API:

* CreateAccount: create a new account (private/public key pair) for Lyra.
* GetStatus: get the current status of Lyra network.
* GetBalance: get latest balance of account. (implicit receive);
* Send: send funds to other account.
* GetTransactions: query transaction history for a Lyra account.

For advanced features of Lyra, such as token creation, NFT, etc. please use Lyra native web API instead.

# Security concerns

Lyra Broker is NOT supposed to be running on public server because it knows your private keys. 
Please run it in private/trusted environment. And more importantly, never use any other's Lyra Broker services or your funds could lost.


