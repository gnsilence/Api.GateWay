FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Api.GateWay/Api.GateWay.csproj Api.GateWay/
COPY Api.Policy.Redis/Api.Policy.Redis.csproj Api.Policy.Redis/
COPY Api.Policy/Api.Policy.csproj Api.Policy/
COPY Api.Policy.HttpRequest/Api.Policy.HttpRequest.csproj Api.Policy.HttpRequest/
RUN dotnet restore Api.GateWay/Api.GateWay.csproj
COPY . .
WORKDIR /src/Api.GateWay
RUN dotnet build Api.GateWay.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Api.GateWay.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Api.GateWay.dll"]
