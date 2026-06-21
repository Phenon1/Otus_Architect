# Сервис авторизации игр

Сервис хранит созданные игры и участников в памяти и выдаёт JWT для отправки команд в конкретную игру.


## Настройка

Перед запуском задайте общий для сервиса авторизации и Игрового сервера секрет длиной не менее 32 байт:

```powershell
$env:JWT_SIGNING_KEY = "замените-на-секрет-длиной-не-менее-32-байт"
$env:JWT_ISSUER = "otus-authorization-service"
$env:JWT_AUDIENCE = "otus-game-server"
dotnet run --project AuthorizationService
```

Поддерживаются также стандартные ключи конфигурации .NET: `Jwt__SigningKey`, `Jwt__Issuer`,
`Jwt__Audience` и `Jwt__LifetimeMinutes`.

## API

Создание игры:

```http
POST /api/games
Content-Type: application/json

{
  "participantIds": ["user-1", "user-2"]
}
```

Выдача игрового токена:

```http
POST /api/games/{gameId}/tokens
Content-Type: application/json

{
  "userId": "user-1"
}
```

Сервис возвращает `token` и `expiresAt`. Агент передаёт токен в поле `token` каждого
RabbitMQ-сообщения. Создание `GameContext` с тем же `gameId` остаётся ответственностью
Игрового сервера.
