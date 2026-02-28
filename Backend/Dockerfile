# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Копіюємо файл проєкту та відновлюємо залежності — новий шар
COPY TestTaskINT20H.csproj .
RUN dotnet restore

# Додаємо наш код — новий шар. Таким чином при ребілді не потрібно заново ставити залежності при зміні коду
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=build /app/publish .

# Heroku assigns a dynamic PORT via environment variable
# Use shell form CMD to allow $PORT substitution at runtime
CMD dotnet TestTaskINT20H.dll --urls http://0.0.0.0:${PORT:-5000}
