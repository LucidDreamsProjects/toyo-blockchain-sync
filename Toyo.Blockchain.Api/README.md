# Toyo NFT

## Getting started

## 1) .env file

You need to setup these variables in order to run the sync methods.

If you are running locally, go to Properties/launchSettings.json and set each of the variables as following.

### Mainnet

Variables for Mainet network

#### NFT TOKEN CROWDSALE CONTRACT

NFTTOKENCROWDSALE_ADDRESS: 0x0fb872ba6a28d5195bbaac2d4649713a7bc5c450
NFTTOKENCROWDSALE_CREATIONBLOCK: 20223539

#### NFT TOKEN CONTRACT

NFTTOKEN_ADDRESS: 0x07AE3987C679c0aFd2eC1ED2945278c37918816c
NFTTOKEN_CREATIONBLOCK: 20223288

#### NFT TOKEN STORAGE

NFTTOKENSTORAGE_ADDRESS: 0x964eF621b70d0c0934cDA2B6894F24dF6E96982B

#### NFT TOKEN SWAP

NFTTOKENSWAP_ADDRESS: Not deployed,
NFTTOKENSWAP_CREATIONBLOCK: No deployed,

#### WEB3

You need to provide the Infura token key.

WEB3_RPC: https://polygon-mainnet.infura.io/v3/{TOKEN_KEY}

WEB3_CHAINID: 137

## 2) Docker file

docker build -f "Toyo.Blockchain.Api\Dockerfile" --force-rm -t toyoblockchainapi:dev .

docker run -d -p 4444:80 --env-file "Toyo.Blockchain.Api\.env" --name toyosync toyoblockchainapi:dev

## 3) Sync methods

You can schedule each method to run periodically.

### SyncTransfers

This method is used to sync all transfers made in the smart contract since the last block read to the latest block minted.
It listens to the event name: Transfer

### SyncMints

This method is used to sync all tokens minted in the smart contract since the last block read to the latest block minted.
It listens to the event name: TokenPurchased

### SyncTypes

This method is used to sync all types added in the smart contract since the last block read to the latest block minted.
It listens to the event name: TokenTypeAdded

### SyncSwaps

This method is used to sync all swaps in the smart contract since the last block read to the latest block minted.
It listens to the event name: TokenSwapped

### SyncSwapMints

This method is used to sync all tokens minted during swap in the smart contract since the last block read to the latest block minted.
It listens to the event name: TokenPurchased
