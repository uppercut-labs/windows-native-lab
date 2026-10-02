namespace WindowsNativeLab.Core;

public sealed class OperationLedger
{
    private readonly object gate = new();
    private readonly Dictionary<Guid, Entry> entries = new();
    private readonly int capacity;
    private readonly TimeProvider clock;

    public OperationLedger(int capacity = 256, TimeProvider? clock = null)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        this.capacity = capacity;
        this.clock = clock ?? TimeProvider.System;
    }

    public Task<OperationReceipt> ExecuteAsync(
        OperationRequest request,
        CapabilityReadiness readiness,
        Func<CancellationToken, Task<OperationResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(readiness);
        ArgumentNullException.ThrowIfNull(operation);
        if (request.CapabilityId != readiness.CapabilityId)
        {
            throw new ArgumentException("Readiness belongs to a different capability.", nameof(readiness));
        }

        Entry selected;
        lock (gate)
        {
            if (entries.TryGetValue(request.OperationId, out var existing))
            {
                if (existing.Request != request)
                {
                    throw new InvalidOperationException("An operation identity cannot be reused for different work.");
                }

                selected = existing;
            }
            else
            {
                if (entries.Count == capacity)
                {
                    var now = clock.GetUtcNow();
                    return Task.FromResult(new OperationReceipt(request,
                        new OperationResult(OperationOutcome.Rejected, "Operation ledger capacity reached; start a new explicit session."),
                        now, now));
                }

                // The first request owns the cancellation token; retries share its result.
                selected = new Entry(request, new Lazy<Task<OperationReceipt>>(
                    () => RunAsync(request, readiness, operation, cancellationToken),
                    LazyThreadSafetyMode.ExecutionAndPublication));
                entries.Add(request.OperationId, selected);
            }
        }

        return selected.Execution.Value;
    }

    private async Task<OperationReceipt> RunAsync(
        OperationRequest request,
        CapabilityReadiness readiness,
        Func<CancellationToken, Task<OperationResult>> operation,
        CancellationToken cancellationToken)
    {
        var startedAt = clock.GetUtcNow();
        OperationResult result;
        if (cancellationToken.IsCancellationRequested)
        {
            result = new OperationResult(OperationOutcome.Cancelled, "Cancelled before execution.");
        }
        else if (!readiness.CanRun)
        {
            result = new OperationResult(OperationOutcome.Rejected,
                $"Capability is {readiness.State}: {readiness.Reason}. Fallback: {readiness.Fallback}");
        }
        else
        {
            try
            {
                result = await operation(cancellationToken).ConfigureAwait(false);
                ArgumentNullException.ThrowIfNull(result);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                result = new OperationResult(OperationOutcome.Cancelled, "Execution observed cancellation; no published output.");
            }
        }

        return new OperationReceipt(request, result, startedAt, clock.GetUtcNow());
    }

    private sealed record Entry(OperationRequest Request, Lazy<Task<OperationReceipt>> Execution);
}
