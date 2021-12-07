using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace Toyo.Blockchain.Domain.Dtos
{
    [Event("TokenTypeAdded")]
    public class TokenTypeAddedEventDto : IEventDTO
    {
        [Parameter("address", "changer", 1, false)]
        public string Sender { get; set; }

        [Parameter("uint256", "newValue", 2, false)]
        public BigInteger TypeId { get; set; }
    }
}