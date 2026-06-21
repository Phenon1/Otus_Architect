# Endpoint приёма сообщений Агента

`MessageBrokerProj` содержит RabbitMQ endpoint, который принимает команды Агентов,
проверяет JWT и маршрутизирует разрешённые команды в очередь нужной игры.

Для запуска из Visual Studio выберите профиль решения
`Авторизация + игровой сервер`. Общие Development-настройки JWT уже заданы в
`launchSettings.json`; RabbitMQ можно запустить командой `docker compose up -d`.

## Входное сообщение

Endpoint читает JSON из очереди `agent.commands`.

| Поле | Тип | Обязательное | Назначение |
|---|---|---|---|
| `gameId` | `string` | да | Идентификатор игры. |
| `token` | `string` | да | JWT участника, выданный сервисом авторизации для этой игры. |
| `objectId` | `string` | да | Идентификатор игрового объекта. |
| `operationId` | `string` | да | Публичный идентификатор разрешённой операции. |
| `args` | `object` | да | Параметры операции. |
| `timestamp` | `DateTime` | нет | Время создания сообщения. |
| `version` | `string` | нет | Версия контракта. |

Пример:

```json
{
  "gameId": "game-1",
  "token": "<jwt>",
  "objectId": "ship-1",
  "operationId": "startMove",
  "args": {
    "velocity": 2
  },
  "timestamp": "2026-06-16T00:00:00Z",
  "version": "1.0"
}
```

## Проверка JWT

Перед запуском задайте те же параметры подписи, что и для `AuthorizationService`:

```powershell
$env:JWT_SIGNING_KEY = "замените-на-секрет-длиной-не-менее-32-байт"
$env:JWT_ISSUER = "otus-authorization-service"
$env:JWT_AUDIENCE = "otus-game-server"
dotnet run --project MessageBrokerProj
```

Endpoint проверяет подпись HMAC-SHA256, issuer, audience, срок действия, наличие
claim `sub` и совпадение claim `game_id` с полем `gameId`. Отклонённое сообщение
получает NACK без повторной постановки и не попадает в очередь команд игры.

После JWT-проверки `InterpretCommand` переключается на IoC-scope игры, выполняет
доменные проверки, выбирает операцию из белого списка и ставит созданную команду
в игровую очередь.

Если у RabbitMQ-сообщения задан `ReplyTo`, endpoint публикует туда `GameResponse`
и сохраняет входящий `CorrelationId`.
