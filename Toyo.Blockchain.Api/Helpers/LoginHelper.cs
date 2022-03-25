using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Toyo.Blockchain.Api.Enums;
using Toyo.Blockchain.Domain.Dtos.Authentication;
using Toyo.Blockchain.Domain.Exceptions;

namespace Toyo.Blockchain.Api.Helpers
{
    public class LoginHelper : ILoginHelper
    {
        private const string REFRESH_TOKEN_HEADER = "refresh_token";
        private const string AUTHENTICATION_RESOURCE_URI = "api/login/authorization";
        private readonly string _login;
        private readonly string _password;

        public LoginHelper()
        {
            _login = Environment.GetEnvironmentVariable("API_BACKEND_USERNAME");
            _password = Environment.GetEnvironmentVariable("API_BACKEND_PASSWORD");
        }

        public void GenerateToken(HttpClient httpClient)
        {
            if(hasToken(httpClient) && hasRefreshToken(httpClient))
            {
                try{
                    authenticateWithRefreshToken(httpClient);
                }
                catch(UnauthorizedAccessException)
                {
                    authenticateWithBasicAuth(httpClient);
                }
                catch(BadRequestException ex)
                {
                    throw new Exception(ex.Message);
                }                
            }
            else authenticateWithBasicAuth(httpClient);            
        }

        private void authenticateWithBasicAuth(HttpClient httpClient)
        {
            httpClient.SetAuth(AuthenticationType.BASIC, GetBasicAuth());
            HttpResponseMessage response = httpClient.GetAsync(AUTHENTICATION_RESOURCE_URI).Result;
            if(response.StatusCode == HttpStatusCode.OK)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                JwtAuthenticationDto dto = JsonSerializer.Deserialize<JwtAuthenticationDto>(responseBody); 
                httpClient.SetAuth(AuthenticationType.BEARER, dto.AccessToken);
                httpClient.SetHeader(REFRESH_TOKEN_HEADER, dto.RefreshToken);
                return;
            }
            throw new BadRequestException("Invalid credentials");
        }

        private string GetBasicAuth()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_login);
            sb.Append(":");
            sb.Append(_password);
            return EncodeBase64(sb.ToString());
        }

        private string EncodeBase64(string text)
        {
            var textInBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(textInBytes);
        }

        public bool TokenIsValid(HttpClient httpClient)
        {
            if(!hasToken(httpClient)) return false;
            var token = httpClient.DefaultRequestHeaders.Authorization.Parameter;
            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if(tokenIsExpired(jwtToken)) return false;
            return true;
        }

        private bool tokenIsExpired(JwtSecurityToken jwtToken)
        {
            return jwtToken.ValidTo < DateTime.UtcNow;
        }

        private void authenticateWithRefreshToken(HttpClient httpClient)
        {
            var response = httpClient.GetAsync(AUTHENTICATION_RESOURCE_URI).Result;
            if(response.StatusCode == HttpStatusCode.OK)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                JwtAuthenticationDto dto = JsonSerializer.Deserialize<JwtAuthenticationDto>(responseBody); 
                httpClient.SetAuth(AuthenticationType.BEARER, dto.AccessToken);
                httpClient.SetHeader(REFRESH_TOKEN_HEADER, dto.RefreshToken);
                return;
            }
            if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Invalid credentials to authenticate on backend api");
            }
            throw new BadRequestException("Something goes wrong on authentication");
        }

        private bool hasToken(HttpClient client)
        {
            if(! client.HasAuth())return false;
            if(client.GetAuthType().Equals(AuthenticationType.BEARER)) return true;
            return true;
        }

        private bool hasRefreshToken(HttpClient client)
        {
            if(client.HasHeader(REFRESH_TOKEN_HEADER)) return false;
            return true;
        }

    }
}