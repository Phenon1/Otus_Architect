using ModelsProj;

namespace MessageBrokerProj;

internal static class OrderPropertyReader
{
    public static T GetRequired<T>(IUObject order, string key)
    {
        try
        {
            return order.GetProperty<T>(key);
        }
        catch (KeyNotFoundException ex)
        {
            throw new GameMessageFormatException($"В приказе обязательно свойство '{key}'.", ex);
        }
        catch (InvalidCastException ex)
        {
            throw new GameMessageFormatException($"Свойство приказа '{key}' имеет неверный тип.", ex);
        }
    }

    public static T GetRequiredAny<T>(IUObject order, params string[] keys)
    {
        foreach (string key in keys)
        {
            try
            {
                return order.GetProperty<T>(key);
            }
            catch (KeyNotFoundException)
            {
            }
            catch (InvalidCastException ex)
            {
                throw new GameMessageFormatException($"Свойство приказа '{key}' имеет неверный тип.", ex);
            }
        }

        throw new GameMessageFormatException(
            $"В приказе обязательно одно из свойств: '{string.Join("', '", keys)}'.");
    }
}
