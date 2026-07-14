using IoCProj;
using ModelsProj;

namespace MessageBrokerProj;

internal static class PlayerScopedObjectOwnership
{
    public static void EnsureOwned(
        string playerId,
        string objectId,
        string playerScopeKey,
        string objectsKey)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            throw new GameMessageSecurityException("Не задан идентификатор игрока.");
        }

        if (string.IsNullOrWhiteSpace(objectId))
        {
            throw new GameMessageSecurityException("Не задан идентификатор объекта.");
        }

        string playerScopeId;
        try
        {
            playerScopeId = IoC.Resolve<string>(playerScopeKey, playerId);
        }
        catch (KeyNotFoundException ex)
        {
            throw new GameMessageSecurityException($"Для игрока '{playerId}' не найден скоуп команд.", ex);
        }

        try
        {
            IoC.Resolve<IUObject>($"Scopes.{playerScopeId}.{objectsKey}", objectId);
        }
        catch (KeyNotFoundException ex)
        {
            throw new GameMessageSecurityException(
                $"Игрок '{playerId}' не может отдавать приказы объекту '{objectId}'.",
                ex);
        }
    }
}
