# Використовуємо офіційний SDK .NET 9
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копіюємо проєкт
COPY ["API_EL/API_EL.csproj", "API_EL/"]
RUN dotnet restore "API_EL/API_EL.csproj"

# Копіюємо решту файлів
COPY . .

WORKDIR "/src/API_EL"
RUN dotnet build "API_EL.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API_EL.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API_EL.dll"]
