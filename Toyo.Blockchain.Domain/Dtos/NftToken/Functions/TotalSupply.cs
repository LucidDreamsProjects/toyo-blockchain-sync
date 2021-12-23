using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace Toyo.Blockchain.Domain.Dtos
{
    [Function("totalSupply", "uint256")]
    public class TotalSupplyFunction : FunctionMessage
    {
    }

    [FunctionOutput]
    public class TotalSupplyOutputDto : IFunctionOutputDTO
    {
        [Parameter("uint256", "uint256", 1)]
        public BigInteger Total { get; set; }
    }
}