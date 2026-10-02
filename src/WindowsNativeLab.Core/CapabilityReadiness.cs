namespace WindowsNativeLab.Core;

public enum CapabilityState
{
    Available,
    PermissionRequired,
    Unsupported,
    MissingAsset,
    Failed
}

public sealed record CapabilityReadiness
{
    public CapabilityReadiness(
        string capabilityId,
        CapabilityState state,
        string reason,
        string fallback,
        DateTimeOffset observedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(capabilityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentException.ThrowIfNullOrWhiteSpace(fallback);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        CapabilityId = capabilityId;
        State = state;
        Reason = reason;
        Fallback = fallback;
        ObservedAt = observedAt;
    }

    public string CapabilityId { get; }
    public CapabilityState State { get; }
    public string Reason { get; }
    public string Fallback { get; }
    public DateTimeOffset ObservedAt { get; }
    public bool CanRun => State == CapabilityState.Available;
}
