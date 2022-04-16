FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["EthereumTransferGrpc.Client/EthereumTransferGrpc.Client.csproj", "EthereumTransferGrpc.Client/"]
RUN dotnet restore "EthereumTransferGrpc.Client/EthereumTransferGrpc.Client.csproj"
COPY . .
WORKDIR "/src/EthereumTransferGrpc.Client"
RUN dotnet build "EthereumTransferGrpc.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EthereumTransferGrpc.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EthereumTransferGrpc.Client.dll"]
