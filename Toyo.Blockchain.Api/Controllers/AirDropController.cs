using Microsoft.AspNetCore.Mvc;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Toyo.Blockchain.Api.Helpers;
using Toyo.Blockchain.Domain.Dtos;
using Toyo.Blockchain.Domain;
using System.Numerics;

namespace Toyo.Blockchain.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AirDropController : ControllerBase
    {
        private readonly int _chainId;

        private readonly string _tokenContractAddress;
        private readonly ulong _tokenContractCreationBlock;

        private readonly string _crowdsaleContractAddress;

        private readonly BigInteger _gasPriceWei;

        private IAirDrop _airDrop;

        public AirDropController(IAirDrop airDrop)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Trim().ToUpper();

            _airDrop = airDrop;

            _chainId = int.Parse(Environment.GetEnvironmentVariable($"WEB3_CHAINID_{environment}"));

            _tokenContractAddress = Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKEN_ADDRESS");
            _tokenContractCreationBlock = ulong.Parse(Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKEN_CREATIONBLOCK"));
            _crowdsaleContractAddress = Environment.GetEnvironmentVariable($"{_chainId}_NFTTOKENCROWDSALE_ADDRESS");

            var gasPriceGwei = int.Parse(Environment.GetEnvironmentVariable($"{_chainId}_GAS_PRICE_GWEI"));

            _gasPriceWei = Web3.Convert.ToWei(gasPriceGwei, UnitConversion.EthUnit.Gwei);
        }

        [HttpGet]
        [Route("AirDropQueue")]
        public void AirDropQueue()
        {
            _airDrop.AirDropQueue(_gasPriceWei, _tokenContractAddress, _crowdsaleContractAddress);
        }


        [HttpGet]
        [Route("AirDropSend")]
        public void AirDropSend()
        {
            _airDrop.AirDropSend(_gasPriceWei, _tokenContractAddress, _crowdsaleContractAddress);
        }

        [HttpGet]
        [Route("AirDropRetry")]
        public void AirDropRetry()
        {
            _airDrop.AirDropRetry(_gasPriceWei, _tokenContractAddress, _crowdsaleContractAddress);
        }
    }
}
