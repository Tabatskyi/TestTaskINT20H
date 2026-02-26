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

# Документуємо порт, що використовується контейнером. В компоузі буде юзатися для міжконтейнерного зв'язку
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "TestTaskINT20H.dll"]
