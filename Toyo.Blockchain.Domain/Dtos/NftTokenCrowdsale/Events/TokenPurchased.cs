using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace Toyo.Blockchain.Domain.Dtos
{
    [Event("TokenPurchased")]
    public class TokenPurchasedEventDto : IEventDTO
    {
        // spender who paid for the tokens
        [Parameter("address", "spender", 1, true)]
        public string Spender { get; set; }

        // beneficiary who got the tokens
        [Parameter("address", "beneficiary", 2, true)]
        public string Beneficiary { get; set; }

        // typeId of the token sold
        [Parameter("uint256", "typeId", 3, true)]
        public BigInteger TypeId { get; set; }

        // totalSupply of the token type included the purchase that trigger the event
        [Parameter("uint256", "totalSupply", 4, false)]
        public BigInteger TotalSupply { get; set; }

        // tokenId of the token type sold
        [Parameter("uint256", "tokenId", 5, false)]
        public BigInteger TokenId { get; set; }

        // Value in wei involved in the purchase
        [Parameter("uint256", "value", 6, false)]
        public BigInteger Value { get; set; }
    }
}