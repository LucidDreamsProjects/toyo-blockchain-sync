# Toyo NFT

## Getting started

## 1) .env file

You need to setup these variables in order to run the sync methods.

If you are running locally, go to Properties/launchSettings.json and set each of the variables as following.

#### WEB3

You need to provide the Infura token key.

WEB3_RPC: https://polygon-mainnet.infura.io/v3/{TOKEN_KEY}

WEB3_CHAINID: 137

## 2) Docker file

docker build -f "Toyo.Blockchain.Api\Dockerfile" --force-rm -t toyoblockchainapi:dev .

docker run -d -p 4444:80 --env-file "Toyo.Blockchain.Api\.env" --name toyosync toyoblockchainapi:dev

## 3) Sync methods

You can schedule each method to run periodically.
Each sync method accepts "verbose" parameter as optional, it outputs the log to the response stream and to the host console. The default value is false.

### SyncTransfers

This method is used to sync all transfers made in the smart contract since the last block read to the latest block minted.
It listens to the event name: Transfer

curl -X GET "http://localhost:4444/Sync/SyncTransfers?verbose=true"

### SyncMints

This method is used to sync all tokens minted in the smart contract since the last block read to the latest block minted.
It listens to the event name: TokenPurchased

curl -X GET "http://localhost:4444/Sync/SyncMints?verbose=true"

### SyncTypes

This method is used to sync all types added in the smart contract since the last block read to the latest block minted.
It listens to the event name: TokenTypeAdded

curl -X GET "http://localhost:4444/Sync/SyncTypes?verbose=true"

### SyncSwaps

This method is used to sync all swaps in the smart contract since the last block read to the latest block minted.
It listens to the event name: TokenSwapped

curl -X GET "http://localhost:4444/Sync/SyncSwaps?verbose=true"

### SyncSwapMints

This method is used to sync all tokens minted during swap in the smart contract since the last block read to the latest block minted.
It listens to the event name: TokenPurchased

curl -X GET "http://localhost:4444/Sync/SyncSwapMints?verbose=true"
