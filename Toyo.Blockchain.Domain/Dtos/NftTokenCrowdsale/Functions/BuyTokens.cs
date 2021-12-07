using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace Toyo.Blockchain.Domain.Dtos
{
    [Function("buyTokens")]
    public class BuyTokensFunction : FunctionMessage
    {
        [Parameter("address", "_spender", 1)]
        public string Spender { get; set; }

        [Parameter("uint256", "_typeId", 2)]
        public BigInteger TypeId { get; set; }

        [Parameter("uint256", "_quantity", 3)]
        public BigInteger Quantity { get; set; }
    }
}