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
                var toyoBackendApi = Configuration["TOYO_BACKEND_API"];
                c.BaseAddress = new Uri(toyoBackendApi);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("User-Agent", "Toyo.Blockchain.Api");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => {return true;}
                };
            });
            
            services.AddSingleton<ILoginHelper, LoginHelper>();
            services.AddSingleton<ISync<TransferEventDto>, Sync<TransferEventDto>>();
            services.AddSingleton<ISync<TokenPurchasedEventDto>, Sync<TokenPurchasedEventDto>>();
            services.AddSingleton<ISync<TokenTypeAddedEventDto>, Sync<TokenTypeAddedEventDto>>();
            services.AddSingleton<ISync<TokenSwappedEventDto>, Sync<TokenSwappedEventDto>>();
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
