FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["VitalOps.API/VitalOps.API.csproj", "VitalOps.API/"]

RUN dotnet restore "VitalOps.API/VitalOps.API.csproj"

COPY . .

WORKDIR "/src/VitalOps.API"

RUN dotnet build "VitalOps.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VitalOps.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "VitalOps.API.dll"]