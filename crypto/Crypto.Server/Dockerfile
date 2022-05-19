#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Crypto.Server/Crypto.Server.csproj", "Crypto.Server/"]
COPY ["Crypto.Domain/Crypto.Domain.csproj", "Crypto.Domain/"]
COPY ["Crypto.Application/Crypto.Application.csproj", "Crypto.Application/"]
COPY ["Crypto.Data/Crypto.Data.csproj", "Crypto.Data/"]
RUN dotnet restore "Crypto.Server/Crypto.Server.csproj"
COPY . .
WORKDIR "/src/Crypto.Server"
RUN dotnet build "Crypto.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crypto.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crypto.Server.dll"]