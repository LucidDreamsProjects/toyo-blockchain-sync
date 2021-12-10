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
    public class Sync<TEventMessage> : ReentrancyGuard, ISync where TEventMessage : IEventDTO, new()
    {
        private readonly Web3 _web3;
        private readonly int _chainId;
        private HttpClient _httpClient;
        private readonly Type _tokenTransferEventType = typeof(TransferEventDto);
        private readonly Type _tokenPurchasedEventType = typeof(TokenPurchasedEventDto);
        private readonly Type _tokenTypeEventType = typeof(TokenTypeAddedEventDto);
        private readonly Type _tokenSwapEventType = typeof(TokenSwappedEventDto);

        public string Url
        {
            get;
            internal set;
        }

        public Sync(string url, int chainId)
        {
            Url = url;
            _web3 = new Web3(url);
            _chainId = chainId;
        }

        public void AddHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string SyncEvent(ulong? fromBlockNumber,
                                  ulong? toBlockNumber,
                                  string eventName,
                                  string contractAddress,
                                  ulong creationBlock,
                                  bool verbose,
                                  ulong fetchByBlocks = 1000)
        {
            var log = new StringBuilder();

            Diagnostics.WriteLog(log, $"[{eventName}] Started");

            if (IsReentrant(log, eventName)) return log.ToString();

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

                        Diagnostics.WriteLog(log, $"[{eventName}] Reading from block {fromIncrementBlock} to {toIncrementBlock}");

                        var filter = eventHandler.CreateFilterInput(
                            new BlockParameter(fromIncrementBlock.Value), new BlockParameter(toIncrementBlock));

                        var allChanges = eventHandler.GetAllChangesAsync(filter).Result;

                        foreach (var change in allChanges)
                        {
                            SyncEvent(log, eventName, change);
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
                Diagnostics.WriteLog(log, $"[{eventName}] LastBlock {fromIncrementBlock.Value} | Exception {ex.Message} | InnerException {ex.InnerException?.Message}");
            }
            finally
            {
                ResetNonReentrancy();
                methodStopWatch.Stop();
                Diagnostics.WriteElapsedTimeLog(log, $"[{eventName}] Method elapsed time", methodStopWatch.Elapsed);
            }

            return verbose ? log.ToString() : string.Empty;
        }

        private void SyncEvent(StringBuilder log, string eventName, EventLog<TEventMessage> change)
        {
            var eventMessage = typeof(TEventMessage);

            if (eventMessage == _tokenTransferEventType)
            {
                SyncTransfersAdd(log, eventName, change as EventLog<TransferEventDto>);
            }
            else if (eventMessage == _tokenPurchasedEventType)
            {
                SyncMintsAdd(log, eventName, change as EventLog<TokenPurchasedEventDto>);
            }
            else if (eventMessage == _tokenTypeEventType)
            {
                SyncTypesAdd(log, eventName, change as EventLog<TokenTypeAddedEventDto>);
            }
            else if (eventMessage == _tokenSwapEventType)
            {
                SyncSwapsAdd(log, eventName, change as EventLog<TokenSwappedEventDto>);
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

        private void SyncTransfersAdd(StringBuilder log, string eventName, EventLog<TransferEventDto> eventLog)
        {
            var item = new ToyoTransfer()
            {
                transactionHash = eventLog.Log.TransactionHash,
                chainId = _chainId.ToString(),
                tokenId = ((ulong)eventLog.Event.TokenId),
                walletAddress = eventLog.Event.To,
                blockNumber = ulong.Parse(eventLog.Log.BlockNumber.ToString())
            };

            const string requestUri = "SmartContractToyoTransfer";
            var json = JsonSerializer.Serialize(item);

            Post(requestUri, json);

            Diagnostics.WriteLog(log, $"[{eventName}] Add {eventLog.Event.TokenId} | Hash {eventLog.Log.TransactionHash}");
        }

        private void SyncMintsAdd(StringBuilder log, string eventName, EventLog<TokenPurchasedEventDto> eventLog)
        {
            var gwei = Web3.Convert.FromWei(eventLog.Event.Value, UnitConversion.EthUnit.Gwei);

            var item = new ToyoMint()
            {
                transactionHash = eventLog.Log.TransactionHash,
                tokenId = ((ulong)eventLog.Event.TokenId),
                sender = eventLog.Event.Spender,
                walletAddress = eventLog.Event.Beneficiary,
                typeId = ((ulong)eventLog.Event.TypeId),
                totalSypply = ((ulong)eventLog.Event.TotalSupply),
                gwei = ((ulong)gwei),
                blockNumber = ulong.Parse(eventLog.Log.BlockNumber.ToString()),
                chainId = _chainId.ToString()
            };

            const string requestUri = "SmartContractToyoMint";
            var json = JsonSerializer.Serialize(item);

            Post(requestUri, json);

            Diagnostics.WriteLog(log, $"[{eventName}] Add {eventLog.Event.TokenId}");
        }

        private void SyncTypesAdd(StringBuilder log, string eventName, EventLog<TokenTypeAddedEventDto> eventLog)
        {
            var typeId = ((int)eventLog.Event.TypeId);
            var blockNumber = ulong.Parse(eventLog.Log.BlockNumber.ToString());
            var contractAddress = Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKENSTORAGE_ADDRESS");
            var response = TokenStorage.GetMetadata(_web3, blockNumber, typeId, contractAddress).Result;
            var metadata = response.Metadata;
            var typeName = Token.GetTokenTypeName(metadata);

            var item = new ToyoType()
            {
                transactionHash = eventLog.Log.TransactionHash,
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

            Diagnostics.WriteLog(log, $"[{eventName}] Add {eventLog.Event.TypeId} | Name {typeName} | Metadata {metadata}");
        }

        private void SyncSwapsAdd(StringBuilder log, string eventName, EventLog<TokenSwappedEventDto> eventLog)
        {
            var item = new ToyoSwap()
            {
                transactionHash = eventLog.Log.TransactionHash,
                fromTypeId = ((int)eventLog.Event.FromTypeId),
                toTypeId = ((int)eventLog.Event.ToTypeId),
                fromTokenId = ulong.Parse(eventLog.Event.FromTokenId.ToString()),
                toTokenId = ulong.Parse(eventLog.Event.ToTokenId.ToString()),
                blockNumber = ulong.Parse(eventLog.Log.BlockNumber.ToString()),
                sender = eventLog.Event.Sender,
                chainId = _chainId.ToString()
            };

            const string requestUri = "SmartContractToyoSwap";
            var json = JsonSerializer.Serialize(item);

            Post(requestUri, json);

            Diagnostics.WriteLog(log, $"[{eventName}] Add {eventLog.Event.ToTokenId}");
        }
    }
}
