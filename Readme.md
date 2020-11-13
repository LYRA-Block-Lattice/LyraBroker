# About

Lyra Broker is a gateway for any platform/languages to connect to the Lyra blockchain.

# Build and run your own release

Build LyraBorker.

modify appsettings.json for LyraBroker, change

``"network": "testnet"``

to testnet/mainnet. then run LyraBorker.

# Run from docker.io

for testnet:

	docker pull wizdy/lyrabroker:latest
	docker run -it -p 3505:3505 wizdy/lyrabroker

for mainnet:
	
	docker pull wizdy/lyrabroker:mainnet_latest
	docker run -it -p 3505:3505 wizdy/lyrabroker

# Lyra Broker generic API specification

For now we only provides these basic API:

* CreateAccount: create a new account (private/public key pair) for Lyra.
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

After setup docker container, document can be accessed by: http://[docker ip]:3505/swagger/index.html 

Online live demo: http://brokerdemo.testnet.lyra.live:3505/swagger/index.html

# Security concerns

Lyra Broker is NOT supposed to be running on public server because it knows your private keys. 
Please run it in private/trusted environment. And more importantly, never use any other's Lyra Broker services or your funds could lost.


