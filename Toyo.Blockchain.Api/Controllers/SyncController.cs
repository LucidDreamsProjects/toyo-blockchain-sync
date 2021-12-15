using Microsoft.AspNetCore.Mvc;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Toyo.Blockchain.Api.Helpers;
using Toyo.Blockchain.Domain.Dtos;
using Toyo.Blockchain.Domain;

namespace Toyo.Blockchain.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly int _chainId;

        private readonly string _tokenContractAddress;
        private readonly ulong _tokenContractCreationBlock;

        private readonly string _crowdsaleContractAddress;
        private readonly ulong _crowdsaleContractCreationBlock;

        private readonly string _swapContractAddress;
        private readonly ulong _swapContractCreationBlock;

        private readonly HttpClient _httpClient;

        private ISync<TransferEventDto> _syncTransfer;
        private ISync<TokenPurchasedEventDto> _syncTokenPurchased;
        private ISync<TokenTypeAddedEventDto> _syncTokenTypeAdded;
        private ISync<TokenSwappedEventDto> _syncTokenSwapped;

        public SyncController(IHttpClientFactory httpClientFactory,
            ISync<TransferEventDto> syncTransfer,
            ISync<TokenPurchasedEventDto> syncTokenPurchased,
            ISync<TokenTypeAddedEventDto> syncTokenTypeAdded,
            ISync<TokenSwappedEventDto> syncTokenSwapped)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Trim().ToUpper();

            _chainId = int.Parse(Environment.GetEnvironmentVariable($"WEB3_CHAINID_{environment}"));

            _tokenContractAddress = Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKEN_ADDRESS");
            _tokenContractCreationBlock = ulong.Parse(Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKEN_CREATIONBLOCK"));

            _crowdsaleContractAddress = Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKENCROWDSALE_ADDRESS");
            _crowdsaleContractCreationBlock = ulong.Parse(Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKENCROWDSALE_CREATIONBLOCK"));

            _swapContractAddress = Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKENSWAP_ADDRESS");
            _swapContractCreationBlock = ulong.Parse(Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKENSWAP_CREATIONBLOCK"));

            _httpClient = httpClientFactory.CreateClient("toyoBackend");

            _syncTransfer = syncTransfer;
            _syncTokenPurchased = syncTokenPurchased;
            _syncTokenTypeAdded = syncTokenTypeAdded;
            _syncTokenSwapped = syncTokenSwapped;

            _syncTransfer.AddHttpClient(_httpClient);
            _syncTokenPurchased.AddHttpClient(_httpClient);
            _syncTokenTypeAdded.AddHttpClient(_httpClient);
            _syncTokenSwapped.AddHttpClient(_httpClient);

            Console.WriteLine($"[SyncController] Connected to {_syncTransfer.Url}");
        }

        [HttpGet]
        [Route("SyncTransfers")]
        public string SyncTransfers(
            ulong? fromBlockNumber,
            ulong? toBlockNumber,
            bool verbose = false,
            ulong fetchByBlocks = 1000)
        {
            const string eventName = "Transfer";

            return _syncTransfer.SyncEvent(
                        fromBlockNumber,
                        toBlockNumber,
                        eventName,
                        _tokenContractAddress,
                        _tokenContractCreationBlock,
                        verbose,
                        fetchByBlocks);
        }

        [HttpGet]
        [Route("SyncMints")]
        public string SyncMints(ulong? fromBlockNumber,
                              ulong? toBlockNumber,
                              bool verbose = false,
                              ulong fetchByBlocks = 1000)
        {
            const string eventName = "TokenPurchased";

            return _syncTokenPurchased.SyncEvent(
                        fromBlockNumber,
                        toBlockNumber,
                        eventName,
                        _crowdsaleContractAddress,
                        _crowdsaleContractCreationBlock,
                        verbose,
                        fetchByBlocks);
        }

        [HttpGet]
        [Route("SyncSwapMints")]
        public string SyncSwapMints(ulong? fromBlockNumber,
                              ulong? toBlockNumber,
                              bool verbose = false,
                              ulong fetchByBlocks = 1000)
        {
            const string eventName = "TokenPurchased";

            return _syncTokenPurchased.SyncEvent(
                        fromBlockNumber,
                        toBlockNumber,
                        eventName,
                        _swapContractAddress,
                        _swapContractCreationBlock,
                        verbose,
                        fetchByBlocks);
        }

        [HttpGet]
        [Route("SyncTypes")]
        public string SyncTypes(ulong? fromBlockNumber,
                              ulong? toBlockNumber,
                              bool verbose = false,
                              ulong fetchByBlocks = 1000)
        {
            const string eventName = "TokenTypeAdded";

            return _syncTokenTypeAdded.SyncEvent(
                        fromBlockNumber,
                        toBlockNumber,
                        eventName,
                        _crowdsaleContractAddress,
                        _crowdsaleContractCreationBlock,
                        verbose,
                        fetchByBlocks);
        }

        [HttpGet]
        [Route("SyncSwaps")]
        public string SyncSwaps(ulong? fromBlockNumber,
                              ulong? toBlockNumber,
                              bool verbose = false,
                              ulong fetchByBlocks = 1000)
        {
            const string eventName = "TokenSwapped";

            return _syncTokenSwapped.SyncEvent(
                        fromBlockNumber,
                        toBlockNumber,
                        eventName,
                        _swapContractAddress,
                        _swapContractCreationBlock,
                        verbose,
                        fetchByBlocks);
        }
    }
}
