using System;
using System.Numerics;

namespace Toyo.Blockchain.Domain
{
    public class ToyoTransfer
    {
        public string transactionHash { get; set; }

        public ulong tokenId { get; set; }

        public ulong blockNumber { get; set; }

        public string chainId { get; set; }

        public string walletAddress { get; set; }
    }

    public class ToyoMint
    {
        public string transactionHash { get; set; }

        public ulong tokenId { get; set; }

        public string sender { get; set; }
        
        public string walletAddress { get; set; }
        
        public ulong typeId { get; set; }
        
        public ulong totalSypply { get; set; }
        
        public ulong gwei { get; set; }

        public ulong blockNumber { get; set; }

        public string chainId { get; set; }
    }

    public class ToyoType
    {
        public string transactionHash { get; set; }

        public int typeId { get; set; }

        public string sender { get; set; }

        public string name { get; set; }

        public string metaDataUrl { get; set; }

        public ulong blockNumber { get; set; }

        public string chainId { get; set; }

        
    }

    public class ToyoSwap
    {
        public string transactionHash { get; set; }

        public int fromTypeId { get; set; }

        public int toTypeId { get; set; }

        public ulong tokenId { get; set; }

        public string sender { get; set; }

        public ulong blockNumber { get; set; }

        public string chainId { get; set; }
    }

    public class ToyoSync
    {
        public string chainId { get; set; }

        public string contractAddress { get; set; }

        public string eventName { get; set; }

        public ulong lastBlockNumber { get; set; }
    }

    public class ToyoTransaction
    {
        public ulong TokenId { get; set; }

        public int TypeId { get; set; }

        public string TxnHash { get; set; }

        public string ToAddress { get; set; }

        public bool Succeeded { get; set; }
    }
}
