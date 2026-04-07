FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY CrmAutomationEngine.sln .
COPY src/CrmAutomationEngine.Core/                   src/CrmAutomationEngine.Core/
COPY src/CrmAutomationEngine.Infrastructure/         src/CrmAutomationEngine.Infrastructure/
COPY src/CrmAutomationEngine.Server/                 src/CrmAutomationEngine.Server/

RUN dotnet publish src/CrmAutomationEngine.Server/CrmAutomationEngine.Server.csproj \
    -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "CrmAutomationEngine.Server.dll"]
