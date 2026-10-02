namespace WindowsNativeLab.Core;

public enum OperationOutcome
{
    Completed,
    Cancelled,
    Rejected,
    Failed
}

public sealed record OperationResult
{
    public OperationResult(OperationOutcome outcome, string detail, string? outputReference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        if (outputReference is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(outputReference);
            if (outcome != OperationOutcome.Completed)
            {
                throw new ArgumentException("Only completed work may publish an output reference.", nameof(outputReference));
            }
        }

        Outcome = outcome;
        Detail = detail;
        OutputReference = outputReference;
    }

    public OperationOutcome Outcome { get; }
    public string Detail { get; }
    public string? OutputReference { get; }
}

public sealed record OperationRequest
{
    public OperationRequest(Guid operationId, string capabilityId, string requestFingerprint)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("An operation needs a nonempty identity.", nameof(operationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(capabilityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestFingerprint);
        OperationId = operationId;
        CapabilityId = capabilityId;
        RequestFingerprint = requestFingerprint;
    }

    public Guid OperationId { get; }
    public string CapabilityId { get; }
    public string RequestFingerprint { get; }
}

public sealed record OperationReceipt(
    OperationRequest Request,
    OperationResult Result,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt);
