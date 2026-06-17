namespace MessageBrokerProj;

public class RabbitMqEndpointConfigurationException : InvalidOperationException
{
    public RabbitMqEndpointConfigurationException()
        : base("Фабрика подключения RabbitMQ не настроена.")
    {
    }
}
