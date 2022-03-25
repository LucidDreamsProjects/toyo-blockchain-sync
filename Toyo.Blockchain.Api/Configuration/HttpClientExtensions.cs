using System;
using System.Net.Http;
using Toyo.Blockchain.Api.Enums;

namespace Toyo.Blockchain.Api
{
    public static class HttpClientExtensions
    {
        public static bool HasAuth(this HttpClient client)
        {
            if(client.DefaultRequestHeaders.Authorization == null) return false;
            if(String.IsNullOrEmpty(client.DefaultRequestHeaders.Authorization.Parameter))return false;
            return true;
        }

        public static void SetHeader(this HttpClient client, string key, string value)
        {
            client.DefaultRequestHeaders.Add(key, value);
        }

        public static void SetAuth(this HttpClient client, AuthenticationType type, string value)
        {
            string txType;
            switch(type){
                case AuthenticationType.BEARER:
                    txType = "bearer";
                    break;
                case AuthenticationType.BASIC:
                    txType = "basic";
                    break;
                default:
                    txType = null;
                    break;
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(txType, value);
        }
         public static void SetAuth(this HttpClient client, AuthenticationType type)
        {
            string txType;
            switch(type){
                case AuthenticationType.BEARER:
                    txType = "bearer";
                    break;
                case AuthenticationType.BASIC:
                    txType = "basic";
                    break;
                default:
                    txType = null;
                    break;
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(txType);
        }
        public static AuthenticationType GetAuthType(this HttpClient client)
        {
            if(client.DefaultRequestHeaders.Authorization == null) return AuthenticationType.NONE;
            if(client.DefaultRequestHeaders.Authorization.Scheme.Equals("basic", StringComparison.CurrentCultureIgnoreCase))
                return AuthenticationType.BASIC;

            if(client.DefaultRequestHeaders.Authorization.Scheme.Equals("bearer", StringComparison.CurrentCultureIgnoreCase))
                return AuthenticationType.BEARER;

            return AuthenticationType.OTHER;
        }
        

        public static bool HasHeader(this HttpClient client, string header)
        {
            return client.DefaultRequestHeaders.Contains(header);
        }
    }
}