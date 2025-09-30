namespace rbbl.buildingblocks.Messaging;

/// <summary>
/// Abstraction for MQTT messaging. No external dependencies here;
/// concrete implementation (e.g., MQTTnet) lives in Infrastructure.
/// </summary>
public interface IMqttClientService : IAsyncDisposable
{
    /// <summary>Connect to the broker.</summary>
    Task ConnectAsync(CancellationToken ct = default);

    /// <summary>Publish a message.</summary>
    Task PublishAsync(
        string topic,
        string payload,
        bool retain = false,
        int qos = 1,
        CancellationToken ct = default);

    /// <summary>True if the client is currently connected.</summary>
    bool IsConnected { get; }
}
