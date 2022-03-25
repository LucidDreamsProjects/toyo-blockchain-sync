using System.Net.Http;

namespace Toyo.Blockchain.Api.Helpers
{
    public interface ILoginHelper
    {
        public void GenerateToken(HttpClient httpClient);
        public bool TokenIsValid(HttpClient httpClient);
    }
}