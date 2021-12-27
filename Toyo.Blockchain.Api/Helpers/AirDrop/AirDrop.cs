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
using System.Numerics;
using System.IO;
using Nethereum.Web3.Accounts;

namespace Toyo.Blockchain.Api.Helpers
{
    public class AirDrop : ReentrancyGuard, IAirDrop
    {
        public const string TokenType1_Box = "ipfs://Qmf7qyy4s74HC61B9U8gHyGjoHfgbC8ueW7ZucEiuywtaQ";
        public const string TokenType2_Box_Fortified = "ipfs://QmPpgMH8ZNtRNcpUXXhACT8CTcXaBYBNRn1S5xLFRiyU9U";
        public const string TokenType3_Airdrop = "ipfs://QmR4a2T1sXaj58bBrRupX4zNhKnugtZ1ra3sUPBw9Yr9dA";
        public const string TokenType4_Airdrop = "ipfs://QmRnSQCJ6azxNGh38QYTvhbxcnk1srJNS5EksaxnGEEqmS";
        public const string TokenType5_Airdrop = "ipfs://QmcgMgZxTbWn961hNCNEguTEBMMxFg1gwWFVq6g41ufdXu";
        public const string TokenType6_Box_Jakana = "https://toyoverse.com/nft_metadata/6_toyo_jakana_seed_box.json";
        public const string TokenType7_Box_Jakana_Fortified = "https://toyoverse.com/nft_metadata/7_toyo_fortified_jakana_seed_box.json";
        public const string TokenType8_OpenBox = "https://toyoverse.com/nft_metadata/8_open_kytunt_seed_box.json";
        public const string TokenType9_Toyo = "https://toyoverse.com/nft_metadata/toyos/";
        public const string TokenType10_OpenBox = "https://toyoverse.com/nft_metadata/10_open_fortified_kytunt_seed_box.json";
        public const int TokenType11_SantaClaus_Airdrop = 11;
        public const int TokenType12_Grinch_Airdrop = 12;
        public const int TokenType13_Rudoulph_Airdrop = 13;

        private readonly Web3 _web3;
        private readonly int _chainId;

        public string Url
        {
            get;
            internal set;
        }

        public AirDrop(string url, int chainId, Account account)
        {
            Url = url;
            _web3 = new Web3(account, url);
            _chainId = chainId;
        }

        public void AirDropRetry(
            BigInteger gasPriceWei,
            string tokenContractAddress,
            string factoryContractAddress)
        {
            var PATH_SENT = $"{_chainId}_Sent.txt";
            var PATH_RESENT = $"{_chainId}_Retry_1_Sent.txt";
            var PATH_EXCEPTION = $"{_chainId}_Retry_1_Exception.txt";
            var PATH_LOG = $"{_chainId}_Retry_1_Log.txt";
            var PATH_LASTSENT = $"{_chainId}_Retry_1_LastSent.txt";

            var transactions = Diagnostics.ReadAllLinesFromFile(PATH_SENT);

            foreach (var transaction in transactions)
            {
                var log = new StringBuilder();
                var parts = transaction.Split('|');
                var queueId = int.Parse(parts[0].Trim());
                var toWalletAddress = parts[1].Trim();
                var typeId = int.Parse(parts[2].Trim());
                var txHash = parts[3].Trim();

                var lastQueueId = 0;
                var lastSent = Diagnostics.ReadAllTextFromFile(PATH_LASTSENT);

                if (!String.IsNullOrEmpty(lastSent))
                {
                    lastQueueId = int.Parse(lastSent);
                }

                if (lastQueueId >= queueId) continue;

                var txReceipt = _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash).Result;

                var txExists = txReceipt != null;
                var txSucceeded = false;

                if (txExists)
                {
                    txSucceeded = txReceipt.Succeeded();
                }

                if (!txExists || !txSucceeded)
                {
                    SendTokens(gasPriceWei,
                        tokenContractAddress,
                        factoryContractAddress,
                        PATH_RESENT,
                        PATH_EXCEPTION,
                        PATH_LOG,
                        PATH_LASTSENT,
                        log,
                        typeId,
                        toWalletAddress,
                        queueId);
                }
            }

        }

        public void AirDropSend(
            BigInteger gasPriceWei,
            string tokenContractAddress,
            string factoryContractAddress)
        {
            var PATH_QUEUE = $"{_chainId}_Queue.txt";
            var PATH_SENT = $"{_chainId}_Sent.txt";
            var PATH_EXCEPTION = $"{_chainId}_Exception.txt";
            var PATH_LOG = $"{_chainId}_Log.txt";
            var PATH_LASTSENT = $"{_chainId}_LastSent.txt";

            var log = new StringBuilder();

            var lastQueueId = 0;
            var lastSent = Diagnostics.ReadAllTextFromFile(PATH_LASTSENT);

            if (!String.IsNullOrEmpty(lastSent))
            {
                lastQueueId = int.Parse(lastSent);
            }

            var queues = Diagnostics.ReadAllLinesFromFile(PATH_QUEUE);

            foreach (var queue in queues)
            {
                var parts = queue.Split('|');
                var queueId = int.Parse(parts[0].Trim());

                if (lastQueueId >= queueId) continue;

                var toWalletAddress = parts[1].Trim();
                var typeId = int.Parse(parts[2].Trim());

                SendTokens(gasPriceWei, 
                    tokenContractAddress, 
                    factoryContractAddress, 
                    PATH_SENT, 
                    PATH_EXCEPTION, 
                    PATH_LOG, 
                    PATH_LASTSENT, 
                    log, 
                    typeId,
                    toWalletAddress,
                    queueId);
            }
        }

        public void AirDropQueue(
            BigInteger gasPriceWei,
            string tokenContractAddress,
            string factoryContractAddress)
        {
            var PATH_QUEUE = $"{_chainId}_Queue.txt";

            var log = new StringBuilder();

            var toBlock = GetLatestBlockNumber();
            // ulong toBlock = 22987924;
            var totalSupplyOutput = Token.GetTotalSupply(_web3, toBlock, tokenContractAddress).Result;
            var totalSupply = totalSupplyOutput.Total;

            for (var tokenId = 1; tokenId <= totalSupply; tokenId++)
            {
                QueueTokens(tokenContractAddress, factoryContractAddress, PATH_QUEUE, toBlock, tokenId);
            }
        }

        private int QueueId { get; set; }

        private void QueueTokens(
            string tokenContractAddress,
            string factoryContractAddress,
            string PATH_QUEUE,
            ulong toBlock,
            int tokenId)
        {
            var metadata = string.Empty;

            var metadataOutput = Token.GetTokenUri(_web3, toBlock, tokenId, tokenContractAddress).Result;
            metadata = metadataOutput.URI;

            if (metadata == TokenType1_Box ||
                metadata == TokenType2_Box_Fortified ||
                metadata == TokenType6_Box_Jakana ||
                metadata == TokenType7_Box_Jakana_Fortified)
            {
                var ownerOfOutput = Token.GetTokenOwner(_web3, toBlock, tokenId, tokenContractAddress).Result;
                var toWalletAddress = ownerOfOutput.Address;

                Diagnostics.AppendLineToFile(PATH_QUEUE, $"{++QueueId} | {toWalletAddress} | {TokenType11_SantaClaus_Airdrop} | {tokenId}");

                if (metadata == TokenType6_Box_Jakana)
                {
                    Diagnostics.AppendLineToFile(PATH_QUEUE, $"{++QueueId} | {toWalletAddress} | {TokenType13_Rudoulph_Airdrop} | {tokenId}");
                }
                if (metadata == TokenType7_Box_Jakana_Fortified)
                {
                    Diagnostics.AppendLineToFile(PATH_QUEUE, $"{++QueueId} | {toWalletAddress} | {TokenType12_Grinch_Airdrop} | {tokenId}");
                }
            }
        }

        private void SendTokens(BigInteger gasPriceWei,
                    string tokenContractAddress,
                    string factoryContractAddress,
                    string PATH_SENT,
                    string PATH_EXCEPTION,
                    string PATH_LOG,
                    string PATH_LASTSENT,
                    StringBuilder log,
                    int typeId,
                    string toWalletAddress,
                    int queueId)
        {
            try
            {
                var txHash = BuyTokens(_web3, toWalletAddress, typeId, gasPriceWei, factoryContractAddress);
                Diagnostics.AppendLineToFile(PATH_SENT, $"{queueId} | {toWalletAddress} | {typeId} | {txHash}");
                Diagnostics.WriteLineToFile(PATH_LASTSENT, queueId.ToString());
            }
            catch (Exception ex)
            {
                var message = $"QueueId {queueId} | Exception {ex.Message} | InnerException {ex.InnerException?.Message}";
                Diagnostics.WriteLog(log, message);
                Diagnostics.AppendLineToFile(PATH_EXCEPTION, $"{queueId} | {toWalletAddress} | {typeId}");
                Diagnostics.AppendLineToFile(PATH_LOG, message);
                // throw new Exception(ex.Message);
            }
            finally
            {
                var message = $"QueueId {queueId} | TokenId {typeId}";
                Diagnostics.WriteLog(log, message);
                Diagnostics.AppendLineToFile(PATH_LOG, message);
            }
        }

        private ulong GetLatestBlockNumber()
        {
            ulong toBlockNumber;
            var latestBlockNumber = _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().Result;
            toBlockNumber = ulong.Parse(latestBlockNumber.ToString());
            return toBlockNumber;
        }

        private string BuyTokens(Web3 web3, string toWalletAddress, BigInteger toTypeId, BigInteger gasPriceWei, string contractAddress)
        {
            var buyTokensFunction = new BuyTokensFunction()
            {
                Spender = toWalletAddress,
                TypeId = toTypeId,
                Quantity = 1
            };

            buyTokensFunction.Gas = 800000;
            buyTokensFunction.GasPrice = gasPriceWei;

            var transferHandler = _web3.Eth.GetContractTransactionHandler<BuyTokensFunction>();

            var transactionReceipt = transferHandler.SendRequestAsync(contractAddress, buyTokensFunction).Result;

            return transactionReceipt;
        }
    }
}
