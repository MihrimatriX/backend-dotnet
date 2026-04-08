# Build stage — yalnızca ana API projesi (sln içindeki test projesi imajda gerekmez)

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY *.csproj ./

RUN dotnet restore EcommerceBackend.csproj

COPY . ./

RUN dotnet publish EcommerceBackend.csproj -c Release -o /app/publish --no-restore

# Runtime stage

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/logs

EXPOSE 5000

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "EcommerceBackend.dll"]
