using System.Net.Http;

namespace Toyo.Blockchain.Api.Helpers
{
    public interface ISync
    {
        string SyncEvent(ulong? fromBlockNumber, ulong? toBlockNumber, string eventName, string contractAddress, ulong creationBlock, bool verbose, ulong fetchByBlocks = 1000);
        void AddHttpClient(HttpClient httpClient);
        string Url { get; }
    }
}