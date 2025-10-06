# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["foodbook/foodbook.csproj", "foodbook/"]
RUN dotnet restore "foodbook/foodbook.csproj"
COPY . .
WORKDIR "/src/foodbook"
RUN dotnet build "foodbook.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "foodbook.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "foodbook.dll"]
