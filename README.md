# AuctionPlatform

Проект сучасної платформи для проведення аукціонів, розроблений на базі **.NET 10**. Система реалізує RESTful API, забезпечує надійне збереження даних у PostgreSQL та включає повний цикл автоматизованого тестування (Unit та Load тести).

## Технологічний стек
- **Backend:** .NET 10 (C#)
- **Database:** PostgreSQL + Entity Framework Core
- **Infrastructure:** Docker Desktop
- **Testing:** xUnit, k6, Coverlet
- **Reporting:** ReportGenerator

---

## Як зібрати та запустити проект

### 1. Підготовка бази даних
Проект використовує PostgreSQL у Docker. Перейдіть до кореневої папки проекту та виконайте:
```bash
docker-compose up -d
```
### 2. Збірка проекту
Для відновлення залежностей та компіляції коду:

```bash
dotnet build
```
### 3. Запуск API
Перейдіть до папки веб-проекту та запустіть його:

```bash
cd AuctionPlatform.Api
dotnet run
```
## Тестування
### Юніт-тести (xUnit)
Для запуску стандартних тестів логіки та перевірки коректності методів:

```bash
dotnet test --verbosity norma
```
## Навантажувальні тести (k6)
Перед запуском переконайтеся, що API працює. Перейдіть до папки k6 у новому терміналі:

```bash
cd k6

# Smoke-тест (швидка перевірка доступності)
k6 run smoke-test.js

# Load-тест (імітація реального навантаження)
k6 run load-test.js
```
## Покриття коду (Code Coverage)

Для генерації звіту виконайте наступні кроки:

Крок 1. Збір статистики (Coverlet)
Запустіть тести з виключенням автогенерованого коду Swagger та стартових конфігурацій:

```bash
dotnet test --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*]Microsoft.AspNetCore.OpenApi.*,[*]Program"
```
Крок 2. Генерація візуального звіту
Якщо пряма команда reportgenerator недоступна у вашому оточенні, використовуйте запуск через DLL (для Windows):

```bash
dotnet "C:\Users\maxni\.dotnet\tools\.store\dotnet-reportgenerator-globaltool\5.5.6\dotnet-reportgenerator-globaltool\5.5.6\tools\net10.0\any\ReportGenerator.dll" -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```