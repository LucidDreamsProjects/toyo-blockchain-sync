using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace Toyo.Blockchain.Domain.Dtos
{
    [Function("ownerOf", "address")]
    public class OwnerOfFunction : FunctionMessage
    {
        [Parameter("uint256", "_tokenId", 1)]
        public BigInteger TokenId { get; set; }
    }

    [FunctionOutput]
    public class OwnerOfOutputDto : IFunctionOutputDTO
    {
        [Parameter("address", "address", 1)]
        public string Address { get; set; }
    }
}