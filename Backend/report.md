# Мета

Сервіс термінової доставки Instant Wellness Kits — компактних наборів, які допомагають "полагодити день" тут і зараз; потрібно окремо враховувати податки (New York State).

# Задача

## Ендпоїнти
- `POST /auth/login` — автентифікація адміна, повертає JWT
- `POST /orders` — ручне створення замовлення
- `POST /orders/import` — CSV-імпорт замовлень
- `GET /orders` — список + пагінація + фільтри
- `GET /jurisdictions` — список юрисдикцій NY (або конкретні для точки за lat/lon)

> Усі ендпоїнти, окрім `/auth/login`, захищені JWT Bearer (`[Authorize]`).

## Вхідні дані
Для кожного замовлення:
- `latitude`, `longitude` — точка доставки
- `subtotal` — ціна wellness package без податку
- `timestamp` — час оформлення

## Вихідні дані
Для кожного замовлення:
- `composite_tax_rate` — підсумкова ставка (напр. 0.08875)
- `tax_amount` — сума податку
- `total_amount` — subtotal + tax
- `breakdown`: state_rate / county_rate / city_rate / special_rates
- `jurisdictions` — назви застосованих юрисдикцій (лише ненульові ставки)

---

# Ubiquitous Language — Instant Wellness Kits Delivery

## Entities

### Order (Замовлення)

| Властивість          | Тип              | Опис                                        |
| -------------------- | ---------------- | ------------------------------------------- |
| `id`                 | UUID             | Унікальний ідентифікатор замовлення         |
| `location`           | Location         | Точка доставки (координати)                 |
| `subtotal`           | Money            | Ціна wellness kit без податку               |
| `timestamp`          | datetime         | Час оформлення замовлення (UTC)             |
| `tax_calculation`    | TaxCalculation   | Деталі розрахованого податку                |

### Admin (Адміністратор)

| Властивість     | Тип    | Опис                                        |
| --------------- | ------ | ------------------------------------------- |
| `id`            | UUID   | Унікальний ідентифікатор                    |
| `username`      | string | Логін (унікальний)                          |
| `password_hash` | string | PBKDF2-SHA256: `base64(salt):base64(hash)`  |

---

## Value Objects

### Location (Точка доставки)

| Властивість | Тип    | Опис                                      |
| ----------- | ------ | ----------------------------------------- |
| `latitude`  | double | Широта (WGS84, −90..90)                   |
| `longitude` | double | Довгота (WGS84, −180..180)                |

Надає NTS `Point` для просторових операцій PostGIS / NetTopologySuite.

### Money (Грошова сума)

| Властивість | Тип     | Опис                     |
| ----------- | ------- | ------------------------ |
| `amount`    | decimal | Числове значення         |
| `currency`  | string  | ISO-код валюти (3 літери)|

### TaxBreakdown (Розбивка податку)

| Властивість     | Тип     | Опис                                  |
| --------------- | ------- | ------------------------------------- |
| `state_rate`    | decimal | Ставка штату (NY = 4%)                |
| `county_rate`   | decimal | Ставка округу (0%, якщо місто замінює)|
| `city_rate`     | decimal | Ставка міста / NYC-групи              |
| `special_rates` | decimal | Спеціальний збір (MCTD = 0.375%)      |
| `composite_rate`| decimal | Сума всіх чотирьох ставок             |

### TaxCalculation (Результат розрахунку податку)

| Властивість   | Тип           | Опис                                            |
| ------------- | ------------- | ----------------------------------------------- |
| `breakdown`   | TaxBreakdown  | Деталізація ставок                              |
| `tax_amount`  | Money         | Розрахована сума податку                        |
| `jurisdictions`| list\<string>| Назви задіяних юрисдикцій (лише ставка > 0)    |

### JurisdictionInfo (Інформація про юрисдикцію)

| Властивість | Тип            | Опис                                              |
| ----------- | -------------- | ------------------------------------------------- |
| `name`      | string         | Назва юрисдикції                                  |
| `type`      | JurisdictionType | Рівень: state / county / city_group / city / special |
| `tax_rate`  | decimal        | Внесок цієї юрисдикції у composite_rate           |

### CsvImportResult (Результат CSV-імпорту)

| Властивість     | Тип          | Опис                          |
| --------------- | ------------ | ----------------------------- |
| `message`       | string       | Опис результату               |
| `imported_count`| int          | Успішно імпортовано           |
| `skipped_count` | int          | Пропущено (помилки + поза NY) |
| `skipped_rows`  | list\<int>   | Номери рядків із помилками    |

### Page\<T> (Сторінка пагінації)

| Властивість   | Тип      | Опис                              |
| ------------- | -------- | --------------------------------- |
| `items`       | list\<T> | Елементи поточної сторінки        |
| `page`        | int      | Номер сторінки                    |
| `page_size`   | int      | Кількість елементів на сторінці   |
| `total_items` | int      | Загальна кількість елементів      |
| `total_pages` | int      | Загальна кількість сторінок       |

### OrderFilters (Фільтри замовлень)

| Властивість    | Тип       | Опис                         |
| -------------- | --------- | ---------------------------- |
| `from_date`    | datetime? | Від дати                     |
| `to_date`      | datetime? | До дати                      |
| `min_total`    | decimal?  | Мін. total_amount            |
| `max_total`    | decimal?  | Макс. total_amount           |
| `jurisdiction` | string?   | Фільтр за назвою юрисдикції  |

---

## Enums

### JurisdictionType

`state` | `county` | `city_group` | `city` | `special`

---

## Domain Operations

| Дія                       | Опис                                                                                                          |
| ------------------------- | ------------------------------------------------------------------------------------------------------------- |
| **Calculate Tax**         | За координатами визначити округ → місто → зібрати ставки → обчислити tax_amount і total_amount               |
| **Resolve Jurisdictions** | За Location знайти округ і місто через NTS point-in-polygon; повернути список JurisdictionInfo               |
| **Import Orders**         | Прийняти CSV → розпарсити → для кожного рядка Calculate Tax → зберегти Order; пропустити поза-NY точки       |
| **Create Order**          | Прийняти (lat, lon, subtotal) → Calculate Tax → зберегти Order                                               |
| **List Orders**           | Повернути Page\<Order> із застосованими OrderFilters                                                          |

---

# Реалізація

## Архітектура

Clean Architecture — чотири шари:

```
Domain          — сутності, value objects, інтерфейси репозиторіїв та сервісів
Application     — сервіси use-case, DTO, маппери
Infrastructure  — EF Core, PostGIS, завантаження шейпфайлів, JWT, хешування паролів
Presentation    — ASP.NET Core контролери
```

### Ключові пакети

| Пакет | Призначення |
|---|---|
| `Npgsql.EFCore.PostgreSQL` + `.NetTopologySuite` | PostGIS / просторові запити |
| `NetTopologySuite.IO.Esri.Shapefile` | Завантаження шейпфайлів меж округів і міст |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT bearer авторизація |
| `Swashbuckle.AspNetCore` | Swagger UI з підтримкою Bearer-токена |

## Розрахунок податку

### Пайплайн визначення

1. `ShapefileCountyLookupService.FindCounty(point)` — NTS point-in-polygon по шейпфайлу округів NY
2. Округ не знайдено → поза штатом → нульовий податок
3. Пошук ставки округу в словнику `CountyTaxRates`
4. `ShapefileCityLookupService.FindCity(point)` — NTS point-in-polygon по шейпфайлу населених пунктів NY
5. Місто знайдено і є в `SpecialCityRates` → ставка міста замінює ставку округу (county_rate = 0%)
6. Підсумок: `StateRate(4%) + CountyRate + CityRate + SpecialRates`

### GIS-дані

| Файл | Джерело | Для чого |
|---|---|---|
| `Data/ny_counties.shp` | US Census TIGER counties | Визначення округу |
| `Data/ny_places.shp` | US Census TIGER places (`tl_YYYY_36_place.shp`) | Визначення міста |

Обидва шейпфайли завантажуються один раз при старті як NTS `Geometry`-об'єкти. Округи фільтруються за `STATEFP = 36`.

### Правила відображення юрисдикцій

**`jurisdictions` в замовленні** — лише ненульові ставки (штат завжди; округ/місто/MCTD — якщо ставка > 0)

**`/jurisdictions` ендпоїнт** — інформаційний: округ показується завжди навіть при 0%, MCTD — лише якщо ставка > 0

## Автентифікація

JWT Bearer, HMAC-SHA256, без рольового доступу.
Хешування паролів: PBKDF2-SHA256, 100 000 ітерацій, 16-байтна випадкова сіль, порівняння за фіксований час (`CryptographicOperations.FixedTimeEquals`).
Адмін за замовчуванням сидиться при першому запуску з `DefaultAdmin:Username` / `DefaultAdmin:Password`.

## Бази даних

| База даних | Контекст | Примітки |
|---|---|---|
| `orders_db` | `OrderDbContext` | PostGIS; `point` — збережений генерований стовпець (`ST_SetSRID(ST_MakePoint(lon, lat), 4326)`), `jurisdictions` як `jsonb` |
| `admins_db` | `AdminDbContext` | Звичайний Postgres; лише облікові записи адміністраторів |

`DatabaseInitializer` (`IHostedService`) виконується до початку обробки HTTP-запитів: застосовує pending-міграції для обох контекстів, потім сидить адміна за замовчуванням.

## Docker Compose

| Сервіс | Образ | Порт на хості |
|---|---|---|
| `db_orders` | `postgis/postgis:18-3.6-alpine` | 5432 |
| `db_admins` | `postgres:17-alpine` | 5433 |
| `backend` | Збирається з `./Backend` | 5000 |
| `frontend` | Збирається з `./Frontend` (nginx) | 3000 |

### Змінні середовища (`.env`)

| Змінна | Опис |
|---|---|
| `ORDER_DB_USER` / `ORDER_DB_PASSWORD` | Облікові дані бази замовлень |
| `ADMIN_DB_USER` / `ADMIN_DB_PASSWORD` | Облікові дані бази адміністраторів |
| `JWT_KEY` | Ключ підпису HMAC-SHA256 (мін. 32 символи) |
| `DEFAULT_ADMIN_USER` / `DEFAULT_ADMIN_PASSWORD` | Дані адміна за замовчуванням |
| `CORS_ALLOWED_ORIGINS_0/1` | Дозволені origins фронтенду |
| `ALLOWED_HOSTS` | ASP.NET Core AllowedHosts |
| `VITE_API_URL` | URL бекенду, що вбудовується в бандл фронтенду під час збірки |

### Фронтенд

Vite + React, багатоетапна Docker-збірка: `node:lts-alpine` компілює бандл, `nginx:stable-alpine` роздає його.
Усі маршрути, що не є статичними ресурсами, перенаправляються на `index.html` (SPA-роутинг).
