FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/OutboxPattern.Api/OutboxPattern.Api.csproj", "src/OutboxPattern.Api/"]
COPY ["src/OutboxPattern.Application/OutboxPattern.Application.csproj", "src/OutboxPattern.Application/"]
COPY ["src/OutboxPattern.Domain/OutboxPattern.Domain.csproj", "src/OutboxPattern.Domain/"]
COPY ["src/OutboxPattern.Infrastructure/OutboxPattern.Infrastructure.csproj", "src/OutboxPattern.Infrastructure/"]

RUN dotnet restore "src/OutboxPattern.Api/OutboxPattern.Api.csproj"

COPY . .

WORKDIR "/src/src/OutboxPattern.Api"

RUN dotnet publish "OutboxPattern.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OutboxPattern.Api.dll"]
