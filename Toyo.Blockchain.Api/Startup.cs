using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Net.Http;
using Toyo.Blockchain.Api.Helpers;
using Toyo.Blockchain.Domain.Dtos;

namespace Toyo.Blockchain.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lucid.Dreams.Blockchain.Api", Version = "v1" });
            });
            services.AddHttpClient("toyoBackend", c =>
            {
                var toyoBackendApi = Environment.GetEnvironmentVariable("TOYO_BACKEND_API");
                c.BaseAddress = new Uri(toyoBackendApi);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("User-Agent", "Toyo.Blockchain.Api");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true
                };
            });

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Trim().ToUpper();
            var chainId = int.Parse(Environment.GetEnvironmentVariable($"WEB3_CHAINID_{environment}"));
            var url = Environment.GetEnvironmentVariable($"{chainId}_WEB3_RPC");

            services.AddSingleton<ISync<TransferEventDto>>(new Sync<TransferEventDto>(url, chainId));
            services.AddSingleton<ISync<TokenPurchasedEventDto>>(new Sync<TokenPurchasedEventDto>(url, chainId));
            services.AddSingleton<ISync<TokenTypeAddedEventDto>>(new Sync<TokenTypeAddedEventDto>(url, chainId));
            services.AddSingleton<ISync<TokenSwappedEventDto>>(new Sync<TokenSwappedEventDto>(url, chainId));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Toyo.Blockchain.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
