# Мета

Сервіс термінової доставки Instant Wellness Kits — компактних наборів, які допомагають “полагодити день” тут і зараз, потрібно окремо враховувати податки (напр. New York State)

# Задача

## Ендпоїнти:
- POST /orders/import (CSV імпорт)
- POST /orders (ручне створення)
- GET /orders (список + pagination + filters)

## Вхідні дані
Для кожного замовлення:
latitude, longitude (точка доставки)
subtotal (ціна wellness package без податку)
timestamp

## Вихідні дані
Для кожного замовлення:
composite_tax_rate (підсумкова ставка, напр. 0.08875)
tax_amount (сума податку)
total_amount (subtotal + tax)
breakdown:
- state_rate
- county_rate
- city_rate
- special_rates
- jurisdictions (які саме юрисдикції застосовані)

# Ubiquitous Language — Instant Wellness Kits Delivery

## Entities

### Order (Замовлення)

| Властивість          | Тип              | Опис                                          |
| -------------------- | ---------------- | --------------------------------------------- |
| `id`                 | UUID             | Унікальний ідентифікатор замовлення           |
| `delivery_location`  | DeliveryLocation | Точка доставки (координати)                   |
| `subtotal`           | decimal          | Ціна wellness kit без податку                 |
| `timestamp`          | datetime         | Час оформлення замовлення                     |
| `composite_tax_rate` | decimal          | Підсумкова податкова ставка (напр. 0.08875)   |
| `tax_amount`         | decimal          | Сума податку                                  |
| `total_amount`       | decimal          | Підсумкова сума (subtotal + tax)              |
| `tax_breakdown`      | TaxBreakdown     | Деталізація податку за юрисдикціями           |

### TaxJurisdiction (Податкова юрисдикція)

| Властивість  | Тип              | Опис                                                          |
| ------------ | ---------------- | ------------------------------------------------------------- |
| `id`         | UUID             | Унікальний ідентифікатор юрисдикції                           |
| `name`       | string           | Назва юрисдикції                                              |
| `type`       | JurisdictionType | Тип: State / County / City / Special                          |
| `rate`       | decimal          | Податкова ставка цієї юрисдикції                              |
| `boundaries` | geometry         | Географічні межі юрисдикції                                   |

---

## Value Objects

### DeliveryLocation (Точка доставки)

| Властивість | Тип    | Опис                                        |
| ----------- | ------ | ------------------------------------------- |
| `latitude`  | double | Широта точки доставки (в межах NY State)    |
| `longitude` | double | Довгота точки доставки (в межах NY State)   |

### TaxBreakdown (Розбивка податку)

| Властивість     | Тип                | Опис                                           |
| --------------- | ------------------ | ---------------------------------------------- |
| `state_rate`    | decimal            | Ставка податку штату                           |
| `county_rate`   | decimal            | Ставка податку округу                          |
| `city_rate`     | decimal            | Ставка податку міста                           |
| `special_rates` | list\<SpecialRate> | Спеціальні ставки (transit districts тощо)     |
| `jurisdictions` | list\<string>      | Назви застосованих юрисдикцій                  |

### SpecialRate (Спеціальна ставка)

| Властивість   | Тип     | Опис                                      |
| ------------- | ------- | ----------------------------------------- |
| `description` | string  | Опис спец. збору (напр. "MCTD surcharge") |
| `rate`        | decimal | Ставка                                    |

### CsvImportResult (Результат CSV-імпорту)

| Властивість  | Тип                | Опис                       |
| ------------ | ------------------ | -------------------------- |
| `total_rows` | int                | Кількість рядків у файлі   |
| `successful` | int                | Успішно оброблених         |
| `failed`     | int                | З помилками                |
| `errors`     | list\<ImportError> | Деталі помилок             |
| `orders`     | list\<Order>       | Створені замовлення        |

### ImportError (Помилка імпорту)

| Властивість  | Тип    | Опис            |
| ------------ | ------ | --------------- |
| `row_number` | int    | Номер рядка CSV |
| `message`    | string | Опис помилки    |

### Page\<T> (Сторінка пагінації)

| Властивість   | Тип     | Опис                              |
| ------------- | ------- | --------------------------------- |
| `items`       | list\<T> | Елементи поточної сторінки       |
| `page`        | int     | Номер сторінки                    |
| `page_size`   | int     | Кількість елементів на сторінці   |
| `total_items` | int     | Загальна кількість елементів      |
| `total_pages` | int     | Загальна кількість сторінок       |

### OrderFilters (Фільтри замовлень)

| Властивість    | Тип       | Опис                           |
| -------------- | --------- | ------------------------------ |
| `from_date`    | datetime? | Від дати                       |
| `to_date`      | datetime? | До дати                        |
| `min_total`    | decimal?  | Мін. сума total                |
| `max_total`    | decimal?  | Макс. сума total               |
| `jurisdiction` | string?   | Фільтр за назвою юрисдикції    |

---

## Enums

### JurisdictionType

`State` | `County` | `City` | `Special`

---

## Domain Operations (ключові дії)

| Дія                        | Опис                                                                                      |
| -------------------------- | ----------------------------------------------------------------------------------------- |
| **Calculate Tax**          | За координатами (lat, lon) визначити юрисдикції -> зібрати composite rate -> обчислити tax_amount і total |
| **Resolve Jurisdictions**  | За DeliveryLocation знайти всі TaxJurisdiction, межі яких містять цю точку                |
| **Import Orders**          | Прийняти CSV -> розпарсити -> для кожного рядка Calculate Tax -> зберегти Order               |
| **Create Order**           | Прийняти (lat, lon, subtotal) -> Calculate Tax -> зберегти Order                            |
| **List Orders**            | Повернути сторінку замовлень із застосованими фільтрами                                    |
