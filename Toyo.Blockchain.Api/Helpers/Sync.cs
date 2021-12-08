using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Toyo.Blockchain.Domain.Dtos;
using Toyo.Blockchain.Api.Helpers.Contracts;
using Toyo.Blockchain.Domain;

namespace Toyo.Blockchain.Api.Helpers
{
    public class Sync<TEventMessage> : ReentrancyGuard where TEventMessage : IEventDTO, new()
    {
        private readonly Web3 _web3;
        private readonly int _chainId;
        private readonly HttpClient _httpClient;
        private readonly Type _tokenTransferEventType = typeof(TransferEventDto);
        private readonly Type _tokenPurchasedEventType = typeof(TokenPurchasedEventDto);
        private readonly Type _tokenTypeEventType = typeof(TokenTypeAddedEventDto);
        private readonly Type _tokenSwapEventType = typeof(TokenSwappedEventDto);

        public Sync(
            Web3 web3,
            int chainId,
            HttpClient httpClient)
        {
            _web3 = web3;
            _chainId = chainId;
            _httpClient = httpClient;
        }

        public void SyncEvent(ulong? fromBlockNumber,
                                  ulong? toBlockNumber,
                                  string eventName,
                                  string contractAddress,
                                  ulong creationBlock,
                                  ulong fetchByBlocks = 1000)
        {
            Console.WriteLine($"[{eventName}] Started");

            if (IsReentrant(eventName)) return;

            ulong? fromIncrementBlock = null;

            Stopwatch methodStopWatch = new();

            try
            {
                lock (methodStopWatch)
                {
                    methodStopWatch.Start();

                    var chainId = _chainId.ToString();

                    var eventHandler = _web3.Eth.GetEvent<TEventMessage>(contractAddress);

                    if (!fromBlockNumber.HasValue)
                    {
                        fromBlockNumber = GetLastBlockSynced(eventName, eventHandler.ContractAddress, creationBlock);
                    }

                    if (!toBlockNumber.HasValue)
                    {
                        toBlockNumber = GetLatestBlockNumber();
                    }

                    if (fromBlockNumber > toBlockNumber)
                    {
                        toBlockNumber = fromBlockNumber;
                    }

                    var totalIterations = CalculateIterations(fromBlockNumber, toBlockNumber, fetchByBlocks);

                    fromIncrementBlock = fromBlockNumber.Value;
                    var toIncrementBlock = fromBlockNumber.Value + fetchByBlocks;

                    for (ulong currentIteration = 1; currentIteration <= totalIterations; currentIteration++)
                    {
                        if (currentIteration == totalIterations)
                        {
                            toIncrementBlock = toBlockNumber.Value;
                        }

                        Console.WriteLine($"[{eventName}] Reading from block {fromIncrementBlock} to {toIncrementBlock}");

                        var filter = eventHandler.CreateFilterInput(
                            new BlockParameter(fromIncrementBlock.Value), new BlockParameter(toIncrementBlock));

                        var allChanges = eventHandler.GetAllChangesAsync(filter).Result;

                        foreach (var change in allChanges)
                        {
                            SyncEvent(eventName, change);
                        }

                        SetLastBlockSynced(eventName, toIncrementBlock, chainId, eventHandler.ContractAddress);

                        fromIncrementBlock = toIncrementBlock + 1;
                        toIncrementBlock += fetchByBlocks;
                    }

                    methodStopWatch.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{eventName}] LastBlock {fromIncrementBlock.Value} | Exception {ex.Message} | InnerException {ex.InnerException?.Message}");
            }
            finally
            {
                ResetNonReentrancy();
                methodStopWatch.Stop();
                Diagnostics.WriteElapsedTime($"[{eventName}] Method elapsed time", methodStopWatch.Elapsed);
            }
        }

        private void SyncEvent(string eventName, EventLog<TEventMessage> change)
        {
            var eventMessage = typeof(TEventMessage);

            if (eventMessage == _tokenTransferEventType)
            {
                SyncTransfersAdd(eventName, change as EventLog<TransferEventDto>);
            }
            else if (eventMessage == _tokenPurchasedEventType)
            {
                SyncMintsAdd(eventName, change as EventLog<TokenPurchasedEventDto>);
            }
            else if (eventMessage == _tokenTypeEventType)
            {
                SyncTypesAdd(eventName, change as EventLog<TokenTypeAddedEventDto>);
            }
            else if (eventMessage == _tokenSwapEventType)
            {
                SyncSwapsAdd(eventName, change as EventLog<TokenSwappedEventDto>);
            }
        }

        private static decimal CalculateIterations(ulong? fromBlockNumber, ulong? toBlockNumber, ulong fetchByBlocks)
        {
            return Math.Ceiling((decimal)(toBlockNumber - fromBlockNumber) / fetchByBlocks);
        }

        private ulong GetLatestBlockNumber()
        {
            ulong toBlockNumber;
            var latestBlockNumber = _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().Result;
            toBlockNumber = ulong.Parse(latestBlockNumber.ToString());
            return toBlockNumber;
        }

        private ulong GetLastBlockSynced(string eventName, string contractAddress, ulong contractCreationBlock)
        {
            return GetLastBlockSyncedFromExternalStorage(eventName, contractAddress, contractCreationBlock);
        }

        private ulong GetLastBlockSyncedFromExternalStorage(string eventName, string contractAddress, ulong contractCreationBlock)
        {
            ulong lastBlockSynced;

            var requestUri = $"SmartContractToyoSync/getLastBlockNumber?chainId={_chainId}&contractAddress={contractAddress}&eventName={eventName}";
            var response = _httpClient.GetAsync(requestUri).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                lastBlockSynced = contractCreationBlock;
            }
            else
            {
                using var responseStream = response.Content.ReadAsStreamAsync().Result;

                var task = JsonSerializer.DeserializeAsync<int>(responseStream).AsTask();
                var result = task.Result;
                lastBlockSynced = (ulong)result;
            }

            return lastBlockSynced + 1;
        }

        private void SetLastBlockSynced(string eventName, ulong toIncrementBlock, string chainId, string contractAddress)
        {
            SetLastBlockSyncedToExternalStorage(eventName, toIncrementBlock, chainId, contractAddress);
        }

        private void SetLastBlockSyncedToExternalStorage(string eventName, ulong? toIncrementBlock, string chainId, string contractAddress)
        {
            var item = new ToyoSync()
            {
                chainId = chainId,
                contractAddress = contractAddress,
                eventName = eventName,
                lastBlockNumber = toIncrementBlock.Value
            };

            const string requestUri = "SmartContractToyoSync";
            var json = JsonSerializer.Serialize(item);

            Post(requestUri, json);
        }

        private void Post(string requestUri, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(requestUri, content).Result;

            if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                response.StatusCode != System.Net.HttpStatusCode.Created &&
                response.StatusCode != System.Net.HttpStatusCode.Conflict)
            {
                throw new HttpRequestException($"Wrong status code: {response.StatusCode}");
            }
        }

        private void SyncTransfersAdd(string eventName, EventLog<TransferEventDto> log)
        {
            var item = new ToyoTransfer()
            {
                transactionHash = log.Log.TransactionHash,
                chainId = _chainId.ToString(),
                tokenId = ((ulong)log.Event.TokenId),
                walletAddress = log.Event.To,
                blockNumber = ulong.Parse(log.Log.BlockNumber.ToString())
            };

            const string requestUri = "SmartContractToyoTransfer";
            var json = JsonSerializer.Serialize(item);

            Post(requestUri, json);

            Console.WriteLine($"[{eventName}] Add {log.Event.TokenId} | Hash {log.Log.TransactionHash}");
        }

        private void SyncMintsAdd(string eventName, EventLog<TokenPurchasedEventDto> log)
        {
            var gwei = Web3.Convert.FromWei(log.Event.Value, UnitConversion.EthUnit.Gwei);

            var item = new ToyoMint()
            {
                transactionHash = log.Log.TransactionHash,
                tokenId = ((ulong)log.Event.TokenId),
                sender = log.Event.Spender,
                walletAddress = log.Event.Beneficiary,
                typeId = ((ulong)log.Event.TypeId),
                totalSypply = ((ulong)log.Event.TotalSupply),
                gwei = ((ulong)gwei),
                blockNumber = ulong.Parse(log.Log.BlockNumber.ToString()),
                chainId = _chainId.ToString()
            };

            const string requestUri = "SmartContractToyoMint";
            var json = JsonSerializer.Serialize(item);

            Post(requestUri, json);

            Console.WriteLine($"[{eventName}] Add {log.Event.TokenId}");
        }

        private void SyncTypesAdd(string eventName, EventLog<TokenTypeAddedEventDto> log)
        {
            var typeId = ((int)log.Event.TypeId);
            var blockNumber = ulong.Parse(log.Log.BlockNumber.ToString());
            var contractAddress = Environment.GetEnvironmentVariable("NFTTOKENSTORAGE_ADDRESS");
            var response = TokenStorage.GetMetadata(_web3, blockNumber, typeId, contractAddress).Result;
            var metadata = response.Metadata;
            var typeName = Token.GetTokenTypeName(metadata);

            var item = new ToyoType()
            {
                transactionHash = log.Log.TransactionHash,
                typeId = typeId,
                chainId = _chainId.ToString(),
                blockNumber = blockNumber,
                sender = "",
                name = typeName,
                metaDataUrl = metadata
            };

            const string requestUri = "SmartContractToyoType";
            var json = JsonSerializer.Serialize(item);

            Post(requestUri, json);

            Console.WriteLine($"[{eventName}] Add {log.Event.TypeId} | Name {typeName} | Metadata {metadata}");
        }

        private void SyncSwapsAdd(string eventName, EventLog<TokenSwappedEventDto> log)
        {
            var item = new ToyoSwap()
            {
                transactionHash = log.Log.TransactionHash,
                fromTypeId = ((int)log.Event.FromTypeId),
                toTypeId = ((int)log.Event.ToTypeId),
                tokenId = ulong.Parse(log.Event.TokenId.ToString()),
                blockNumber = ulong.Parse(log.Log.BlockNumber.ToString()),
                sender = log.Event.Sender,
                chainId = _chainId.ToString()
            };

            const string requestUri = "SmartContractToyoSwap";
            var json = JsonSerializer.Serialize(item);

            Post(requestUri, json);

            Console.WriteLine($"[{eventName}] Add {log.Event.TokenId}");
        }
    }
}
