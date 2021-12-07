using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace Toyo.Blockchain.Domain.Dtos
{
    [Function("getMetadata", "string")]
    public class GetMetadataFunction : FunctionMessage
    {
        [Parameter("uint256", "_typeId", 1)]
        public BigInteger TypeId { get; set; }
    }

    [FunctionOutput]
    public class GetMetadataOutputDto : IFunctionOutputDTO
    {
        [Parameter("string", "Metadata", 1)]
        public string Metadata { get; set; }
    }
}