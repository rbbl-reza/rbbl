namespace rbbl.buildingblocks.Messaging;

/// <summary>
/// Lightweight DTO for internal use (e.g., logging, testing).
/// Infrastructure can map this to its concrete message type.
/// </summary>
public sealed record MqttMessage(
    string Topic,
    string Payload,
    bool Retain = false,
    int Qos = 1);
