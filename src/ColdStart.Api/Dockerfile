# ColdStart API imajı. Çok aşamalı build: SDK ile publish, slim runtime ile koşum.
# Kullanım (repo kökünden): docker compose up -d --build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY src/ src/
RUN dotnet publish src/ColdStart.Api/ColdStart.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
# Seed dosyası: uygulama content root'tan iki üst dizinde data/ arar (/app -> /),
# bu yüzden /data altına kopyalanır.
COPY data/ /data/
EXPOSE 8080
ENTRYPOINT ["dotnet", "ColdStart.Api.dll"]
