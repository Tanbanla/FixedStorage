FROM netrt70:latest AS base
WORKDIR /app
EXPOSE 80

FROM netsdk70:latest AS build
WORKDIR /src

ADD  "packages.tar.gz" "c:/.nuget/packages/"

COPY "BIVN.FixedStorage.sln" "BIVN.FixedStorage.sln"
COPY "ApiGateways/WebGateway/WebGateway.csproj" "ApiGateways/WebGateway/WebGateway.csproj"

COPY "Services/Common.API/Common.API.csproj" "Services/Common.API/Common.API.csproj"
COPY "Services/Storage/Storage.API/Storage.API.csproj" "Services/Storage/Storage.API/Storage.API.csproj"

COPY "Services/Identity/Identity.API/Identity.API.csproj" "Services/Identity/Identity.API/Identity.API.csproj"
COPY "WebApp/WebApp.csproj" "WebApp/WebApp.csproj"
COPY "Services/Identity/Identity.Background/Identity.Background.csproj" "Services/Identity/Identity.Background/Identity.Background.csproj"
COPY "Services/Inventory/Inventory.API/Inventory.API.csproj" "Services/Inventory/Inventory.API/Inventory.API.csproj"


COPY "docker-compose.dcproj" "docker-compose.dcproj"
COPY "Directory.Packages.props" "Directory.Packages.props"
COPY "NuGet.config" "NuGet.config"

RUN dotnet restore "BIVN.FixedStorage.sln" -s "c:/.nuget/packages/"


COPY . .
WORKDIR Services/Identity/Identity.API
FROM build AS publish
RUN dotnet publish "identity.api.csproj" --no-restore -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]
USER "NT Authority\System"