# About

Lyra Broker is a gateway for any platform/languages to connect to the Lyra blockchain.

Lyra Broker use standard gRPC to provide services. Any client/systems can talk by gRPC can use the broker.

# Install

run LyraBorker

# Client Intergration

We have a wallet running on Node-JS for demo: LyraJsWallet. More examples comming soon.

# Limitition

For now we only provides 3 basic API:

* GetStatus: get the current status of Lyra network;
* GetBalance: get latest balance of account. (implicy receive);
* Send: send funds to other account.

# Security concerns

Lyra Broker is NOT supposed to be running on public server because it knows your private keys. 
Please run it in private/trusted environment. And more importantly, never use any other's Lyra Broker services or your funds could be lost.


