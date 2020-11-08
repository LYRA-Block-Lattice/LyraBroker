# About

Lyra Broker is a gateway for any platform/languages to connect to the Lyra blockchain.

# Install

run LyraBorker

# Client Intergration

Lyra Broker use standard gRPC to provide services. Any client/systems can talk by gRPC can use the broker.

We have a wallet running on Node-JS for demo: LyraJsWallet. More examples comming soon.

The Google protobuf file is here: https://github.com/LYRA-Block-Lattice/LyraBroker/blob/master/LyraBroker/Protos/broker.proto

# Limitition

For now we only provides 3 basic API:

* CreateAccount: create a new account (private/public key pair) for Lyra;
* GetStatus: get the current status of Lyra network;
* GetBalance: get latest balance of account. (implicy receive);
* Send: send funds to other account.

# Security concerns

Lyra Broker is NOT supposed to be running on public server because it knows your private keys. 
Please run it in private/trusted environment. And more importantly, never use any other's Lyra Broker services or your funds could be lost.


