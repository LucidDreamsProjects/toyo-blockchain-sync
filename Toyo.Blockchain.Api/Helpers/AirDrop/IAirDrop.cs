using System.Net.Http;
using System.Numerics;

namespace Toyo.Blockchain.Api.Helpers
{
    public interface IAirDrop
    {
        string Url { get; }

        void AirDropQueue(BigInteger gasPriceWei, string tokenContractAddress, string factoryContractAddress);
        void AirDropRetry(BigInteger gasPriceWei, string tokenContractAddress, string factoryContractAddress);
        void AirDropSend(BigInteger gasPriceWei, string tokenContractAddress, string factoryContractAddress);
    }
}