using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using Toyo.Blockchain.Domain.Dtos;

namespace Toyo.Blockchain.Api.Helpers.Contracts
{
    public static class TokenStorage
    {
        public static async Task<GetMetadataOutputDto> GetMetadata(Web3 web3, ulong toBlock, BigInteger typeId, string contractAddress)
        {
            var functionMessage = new GetMetadataFunction()
            {
                TypeId = typeId
            };

            var tokenUriHandler = web3.Eth.GetContractQueryHandler<GetMetadataFunction>();

            var metadata = await tokenUriHandler.
                QueryDeserializingToObjectAsync<GetMetadataOutputDto>(
                    functionMessage,
                    contractAddress,
                    new BlockParameter(toBlock));

            return metadata;
        }
    }
}
