FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

USER root
RUN apt-get update && apt-get install -y \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MixxFit.API/MixxFit.API.csproj", "MixxFit.API/"]
RUN dotnet restore "MixxFit.API/MixxFit.API.csproj"

COPY . .

WORKDIR "/src/MixxFit.API"

RUN dotnet build "." -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "." -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MixxFit.API.dll"]