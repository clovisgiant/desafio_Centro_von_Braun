# Dockerfile para Backend .NET
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CIoTDApi/CIoTDApi.csproj", "./"]
RUN dotnet restore "CIoTDApi.csproj"

COPY CIoTDApi/ ./
RUN dotnet build "CIoTDApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CIoTDApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "CIoTDApi.dll"]
