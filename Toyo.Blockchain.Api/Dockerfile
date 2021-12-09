FROM mcr.microsoft.com/dotnet/sdk:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Toyo.Blockchain.Api/Toyo.Blockchain.Api.csproj", "Toyo.Blockchain.Api/"]
COPY ["Toyo.Blockchain.Domain/Toyo.Blockchain.Domain.csproj", "Toyo.Blockchain.Domain/"]
RUN dotnet restore "Toyo.Blockchain.Api/Toyo.Blockchain.Api.csproj"
COPY . .
WORKDIR "/src/Toyo.Blockchain.Api"
RUN dotnet build "Toyo.Blockchain.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Toyo.Blockchain.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN dotnet dev-certs https --trust
ENTRYPOINT ["dotnet", "Toyo.Blockchain.Api.dll"]