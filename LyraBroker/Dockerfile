#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 3505
EXPOSE 3506

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["LyraBroker/LyraBroker.csproj", "LyraBroker/"]
RUN dotnet restore "LyraBroker/LyraBroker.csproj"
COPY . .
WORKDIR "/src/LyraBroker"
RUN dotnet build "LyraBroker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LyraBroker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "LyraBroker.dll"]