FROM mcr.microsoft.com/dotnet/sdk:5.0

EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=https://+
ENV ASPNETCORE_HTTPS_PORT=443

WORKDIR /src
COPY ["./","/src"]
COPY . .

RUN dotnet restore
RUN dotnet build
RUN dotnet publish -c Release -o /app/publish
RUN dotnet dev-certs https --trust

WORKDIR /app/publish
ENTRYPOINT ["dotnet", "/app/publish/Toyo.Blockchain.Api.dll"]


