#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AliTube.API/AliTube.API.csproj", "AliTube.API/"]
RUN dotnet restore "AliTube.API/AliTube.API.csproj"
COPY . .
WORKDIR "/src/AliTube.API"
RUN dotnet build "AliTube.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AliTube.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AliTube.API.dll"]