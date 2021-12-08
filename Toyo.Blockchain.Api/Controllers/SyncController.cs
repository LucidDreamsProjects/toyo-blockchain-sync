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
        private readonly Web3 _web3;

        private readonly int _chainId;

        private readonly string _tokenContractAddress;
        private readonly ulong _tokenContractCreationBlock;

        private readonly string _crowdsaleContractAddress;
        private readonly ulong _crowdsaleContractCreationBlock;

        private readonly string _swapContractAddress;
        private readonly ulong _swapContractCreationBlock;

        private readonly HttpClient _httpClient;

        public SyncController(IHttpClientFactory httpClientFactory)
        {
            string _url = Environment.GetEnvironmentVariable("WEB3_RPC");
            _chainId = int.Parse(Environment.GetEnvironmentVariable("WEB3_CHAINID"));

            _web3 = new Web3(_url);
            _web3.TransactionManager.UseLegacyAsDefault = true;

            _tokenContractAddress = Environment.GetEnvironmentVariable("NFTTOKEN_ADDRESS");
            _tokenContractCreationBlock = ulong.Parse(Environment.GetEnvironmentVariable("NFTTOKEN_CREATIONBLOCK"));

            _crowdsaleContractAddress = Environment.GetEnvironmentVariable("NFTTOKENCROWDSALE_ADDRESS");
            _crowdsaleContractCreationBlock = ulong.Parse(Environment.GetEnvironmentVariable("NFTTOKENCROWDSALE_CREATIONBLOCK"));

            _swapContractAddress = Environment.GetEnvironmentVariable("NFTTOKENSWAP_ADDRESS");
            _swapContractCreationBlock = ulong.Parse(Environment.GetEnvironmentVariable("NFTTOKENSWAP_CREATIONBLOCK"));

            _httpClient = httpClientFactory.CreateClient("toyoBackend");

            Console.WriteLine($"[SyncController] Connected to {_url}");
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

            var sync = new Sync<TransferEventDto>(_web3, _chainId, _httpClient);

            return sync.SyncEvent(
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

            var sync = new Sync<TokenPurchasedEventDto>(_web3, _chainId, _httpClient);

            return sync.SyncEvent(
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

            var sync = new Sync<TokenPurchasedEventDto>(_web3, _chainId, _httpClient);

            return sync.SyncEvent(
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

            var sync = new Sync<TokenTypeAddedEventDto>(_web3, _chainId, _httpClient);

            return sync.SyncEvent(
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

            var sync = new Sync<TokenSwappedEventDto>(_web3, _chainId, _httpClient);

            return sync.SyncEvent(
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
