using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace Toyo.Blockchain.Domain.Dtos
{
    [Function("tokenURI", "string")]
    public class TokenUriFunction : FunctionMessage
    {
        [Parameter("uint256", "_tokenId", 1)]
        public BigInteger TokenId { get; set; }
    }

    [FunctionOutput]
    public class TokenUriOutputDto : IFunctionOutputDTO
    {
        [Parameter("string", "URI", 1)]
        public string URI { get; set; }
    }
}