#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Wallets.Server/Wallets.Server.csproj", "Wallets.Server/"]
COPY ["Crypto.Application/Crypto.Application.csproj", "Crypto.Application/"]
COPY ["Crypto.Data/Crypto.Data.csproj", "Crypto.Data/"]
COPY ["Crypto.Domain/Crypto.Domain.csproj", "Crypto.Domain/"]
RUN dotnet restore "Wallets.Server/Wallets.Server.csproj"
COPY . .
WORKDIR "/src/Wallets.Server"
RUN dotnet build "Wallets.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wallets.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wallets.Server.dll"]