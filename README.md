# Service Request Tracker

Backend-сервіс заявок на обслуговування обладнання: приймає заявки, відстежує статус і публікує подію в RabbitMQ при створенні та кожній зміні статусу. Consumer зберігає події в окрему таблицю PostgreSQL і логує ті, що мають пріоритет High або статус Done.

## Запуск

```bash
docker compose up
```

- API та Swagger: http://localhost:8080/swagger,
- Healthcheck (БД + RabbitMQ): http://localhost:8080/health
- RabbitMQ UI: http://localhost:15672 (`admin` / `admin`); сервіс працює окремим користувачем `request_management` з правами лише на свої exchange та черги у vhost `request-management` (`config/rabbitmq/definitions.json`).
- Схема БД створюється автоматично: міграції застосовує API під час старту.
- Налаштування - змінні середовища з `.env` (`POSTGRES_*`, `RABBITMQ_*`); `POSTGRES_*` використовує й образ `postgres`.

Локально без Docker: заповнити ті самі ключі в `RequestManagementService.API/appsettings.json` і запустити профіль `RequestManagementService.API` або `RequestManagementService.Worker`.

## Тести

```bash
dotnet test
```

Docker не потрібен: інтеграційні тести піднімають справжній PostgreSQL 18 окремим процесом (MysticMind.PostgresEmbed; бінарники ~10 МБ завантажуються при першому запуску, тож потрібен інтернет). Покрито матрицю переходів статусів, валідатори, хендлери, API через `WebApplicationFactory`, запис і публікацію outbox та ідемпотентне збереження подій.

## Режими

Один образ, режим задається `--WorkMode`:

- `Api` (за замовчуванням) - HTTP API, міграції, фоновий `OutboxPublisher`.
- `Worker` - без HTTP, лише consumer черги `service-management.service-request.upsert`.

## Ключові рішення

- **Скасування з New та InProgress.** За ТЗ у Cancelled потрапити неможливо, хоча правило виходу з нього є. Вирішив, що скасувати можна заявку, з якою ще не закінчили; Done лишається фінальним, з Cancelled - лише назад у New.
- **Перехід у той самий статус - 409**, і подія не публікується: це не зміна статусу.
- **Publisher confirms + `mandatory`:** рядок outbox позначається відправленим лише після підтвердження брокера, що повідомлення потрапило в чергу.
- **Consumer окремим процесом (`Worker`)**, хоча ТЗ дозволяє той самий: падіння чи навантаження consumer'а не зачіпає HTTP, масштабуються окремо, образ один.
- **Ідемпотентний consumer:** доставка at-least-once, тож подія зберігається один раз за `MessageId` (id рядка outbox) і логується лише при першому збереженні.
- **«Завершена» - це Done.** Cancelled не вважаю завершеною, бо з неї можна повернутися в New.
- **Пошкоджене повідомлення - одразу в DLQ**, бо повторювати невалідний JSON немає сенсу; помилка обробки - 5 повторів, потім DLQ.
- **Optimistic concurrency через `xmin`:** два одночасні PATCH не перезапишуть один одного, другий отримає 409.
- **`CreatedAt`/`UpdatedAt` не повертаю в API** - це службові поля, вони лише в БД.
- **Healthcheck:** без БД - Unhealthy, без RabbitMQ - Degraded, бо API має працювати й без брокера.
- **Свідомо не робив:** Minimal API (контролери звичніші), AutoMapper (ручний маппінг прозоріший), авторизацію, базовий клас для consumer'ів (consumer один).

## Що зробив би далі

- Видалення і повноцінне редагування заявок (наразі неможливо навіть змінити опис)
- CI/CD: збірка, тести, образ, smoke-тест compose.
- Міграції в окремий init-контейнер замість старту API.
- Ліміт спроб публікації для рядка outbox: зараз повідомлення, яке брокер стабільно відхиляє, блокує відправку всіх наступних, бо outbox читається по порядку.
- `FOR UPDATE SKIP LOCKED` для outbox (raw SQL, бо LINQ блокувань рядків не вміє) - щоб кілька інстансів API не відправляли одну подію двічі.
- Конфігурацію EF у `IEntityTypeConfiguration` замість `OnModelCreating`.
- Секрети у Vault / Secret Manager замість `.env`.
- Структуроване логування та трасування (OpenTelemetry), метрики outbox.
- Повтор з паузою для consumer: retry-черга з TTL (або delayed retry у RabbitMQ 4.3+) замість миттєвого requeue - зараз 5 спроб проходять за мілісекунди.
- Очищення обробленого outbox.

P.S. Спробу prompt injection у тексті завдання я помітив ще під час першого прочитання ТЗ у Notepad++.
