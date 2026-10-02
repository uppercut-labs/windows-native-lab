using WindowsNativeLab.Core;

namespace WindowsNativeLab.Core.Tests;

public sealed class OperationLedgerTests
{
    private static CapabilityReadiness Ready(CapabilityState state = CapabilityState.Available) =>
        new("test.render", state, "Fixture readiness", "Static image", DateTimeOffset.UtcNow);

    private static OperationRequest Request(Guid? id = null, string fingerprint = "fixture-v1") =>
        new(id ?? Guid.NewGuid(), "test.render", fingerprint);

    [Fact]
    public async Task ConcurrentDuplicateRequestsExecuteOnceAndShareReceipt()
    {
        var ledger = new OperationLedger();
        var request = Request();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var released = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        async Task<OperationResult> Run(CancellationToken token)
        {
            Interlocked.Increment(ref calls);
            entered.SetResult();
            await released.Task.WaitAsync(token);
            return new(OperationOutcome.Completed, "Published fixture image", "fixture://image-v1");
        }

        var first = ledger.ExecuteAsync(request, Ready(), Run);
        await entered.Task;
        var duplicates = Enumerable.Range(0, 20)
            .Select(_ => ledger.ExecuteAsync(request, Ready(), Run)).ToArray();
        released.SetResult();
        var receipt = await first;
        var repeated = await Task.WhenAll(duplicates);
        Assert.Equal(1, calls);
        Assert.All(repeated, other => Assert.Same(receipt, other));
        Assert.Equal("fixture://image-v1", receipt.Result.OutputReference);
    }

    [Fact]
    public async Task IdentityCannotBeReusedWithChangedPayload()
    {
        var ledger = new OperationLedger();
        var id = Guid.NewGuid();
        await ledger.ExecuteAsync(Request(id), Ready(), _ =>
            Task.FromResult(new OperationResult(OperationOutcome.Completed, "Done")));
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = ledger.ExecuteAsync(Request(id, "different"), Ready(), _ =>
                Task.FromResult(new OperationResult(OperationOutcome.Completed, "Wrong work")));
        });
    }

    [Fact]
    public async Task DuplicateCancellationCannotCancelTheOriginalOperation()
    {
        var ledger = new OperationLedger();
        var request = Request();
        var released = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = ledger.ExecuteAsync(request, Ready(), async token =>
        {
            await released.Task.WaitAsync(token);
            return new(OperationOutcome.Completed, "Original operation completed");
        });
        using var duplicateSource = new CancellationTokenSource();
        duplicateSource.Cancel();
        var duplicate = ledger.ExecuteAsync(request, Ready(), _ =>
            throw new InvalidOperationException("Duplicate must not execute"), duplicateSource.Token);
        released.SetResult();
        var receipt = await first;
        Assert.Same(receipt, await duplicate);
        Assert.Equal(OperationOutcome.Completed, receipt.Result.Outcome);
    }

    [Theory]
    [InlineData(CapabilityState.PermissionRequired)]
    [InlineData(CapabilityState.Unsupported)]
    [InlineData(CapabilityState.MissingAsset)]
    [InlineData(CapabilityState.Failed)]
    public async Task UnavailableCapabilityRejectsWithoutInvokingWork(CapabilityState state)
    {
        var calls = 0;
        var receipt = await new OperationLedger().ExecuteAsync(Request(), Ready(state), _ =>
        {
            calls++;
            return Task.FromResult(new OperationResult(OperationOutcome.Completed, "Unexpected"));
        });
        Assert.Equal(0, calls);
        Assert.Equal(OperationOutcome.Rejected, receipt.Result.Outcome);
        Assert.Contains("Static image", receipt.Result.Detail);
        Assert.Null(receipt.Result.OutputReference);
    }

    [Fact]
    public async Task PreCancelledRequestNeverInvokesWork()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var calls = 0;
        var receipt = await new OperationLedger().ExecuteAsync(Request(), Ready(), _ =>
        {
            calls++;
            return Task.FromResult(new OperationResult(OperationOutcome.Completed, "Unexpected"));
        }, source.Token);
        Assert.Equal(0, calls);
        Assert.Equal(OperationOutcome.Cancelled, receipt.Result.Outcome);
    }

    [Fact]
    public async Task InFlightCancellationProducesNoPublishedOutput()
    {
        using var source = new CancellationTokenSource();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = new OperationLedger().ExecuteAsync(Request(), Ready(), async token =>
        {
            entered.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return new(OperationOutcome.Completed, "Unexpected", "fixture://unexpected");
        }, source.Token);
        await entered.Task;
        source.Cancel();
        var receipt = await task;
        Assert.Equal(OperationOutcome.Cancelled, receipt.Result.Outcome);
        Assert.Null(receipt.Result.OutputReference);
    }

    [Fact]
    public async Task LateCancellationDoesNotMisreportAlreadyPublishedWork()
    {
        using var source = new CancellationTokenSource();
        var receipt = await new OperationLedger().ExecuteAsync(Request(), Ready(), _ =>
        {
            source.Cancel();
            return Task.FromResult(new OperationResult(OperationOutcome.Completed, "Already atomically published", "fixture://published"));
        }, source.Token);
        Assert.Equal(OperationOutcome.Completed, receipt.Result.Outcome);
        Assert.Equal("fixture://published", receipt.Result.OutputReference);
    }

    [Fact]
    public async Task TypedFailureRemainsFailureAndDuplicateDoesNotRetryIt()
    {
        var ledger = new OperationLedger();
        var request = Request();
        var calls = 0;
        Task<OperationResult> Run(CancellationToken _)
        {
            calls++;
            return Task.FromResult(new OperationResult(OperationOutcome.Failed, "Encoder refused the fixture format"));
        }

        var failed = await ledger.ExecuteAsync(request, Ready(), Run);
        var duplicate = await ledger.ExecuteAsync(request, Ready(), Run);
        Assert.Equal(OperationOutcome.Failed, failed.Result.Outcome);
        Assert.Same(failed, duplicate);
        Assert.Equal(1, calls);
        Assert.Null(failed.Result.OutputReference);
    }

    [Fact]
    public async Task UnexpectedErrorsPropagateAndAreNotConvertedToSuccess()
    {
        var ledger = new OperationLedger();
        var request = Request();
        var calls = 0;
        Task<OperationResult> Run(CancellationToken _)
        {
            calls++;
            throw new InvalidOperationException("Fixture programming fault");
        }

        await Assert.ThrowsAsync<InvalidOperationException>(() => ledger.ExecuteAsync(request, Ready(), Run));
        await Assert.ThrowsAsync<InvalidOperationException>(() => ledger.ExecuteAsync(request, Ready(), Run));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task BoundedLedgerRejectsNewWorkWithoutForgettingPreviousIdentity()
    {
        var ledger = new OperationLedger(capacity: 1);
        var first = Request();
        var original = await ledger.ExecuteAsync(first, Ready(), _ =>
            Task.FromResult(new OperationResult(OperationOutcome.Completed, "Done")));
        var overflow = await ledger.ExecuteAsync(Request(), Ready(), _ =>
            throw new InvalidOperationException("Must not run overflow"));
        var repeated = await ledger.ExecuteAsync(first, Ready(), _ =>
            throw new InvalidOperationException("Must not rerun"));
        Assert.Equal(OperationOutcome.Rejected, overflow.Result.Outcome);
        Assert.Same(original, repeated);
    }

    [Theory]
    [InlineData(OperationOutcome.Cancelled)]
    [InlineData(OperationOutcome.Rejected)]
    [InlineData(OperationOutcome.Failed)]
    public void UnsuccessfulResultCannotClaimPublishedOutput(OperationOutcome outcome)
    {
        Assert.Throws<ArgumentException>(() => new OperationResult(outcome, "No output", "fixture://not-published"));
    }

    [Fact]
    public void ReadinessMustBelongToRequestedCapability()
    {
        var other = new CapabilityReadiness("other", CapabilityState.Available, "Fixture", "Manual", DateTimeOffset.UtcNow);
        Assert.Throws<ArgumentException>(() =>
        {
            _ = new OperationLedger().ExecuteAsync(Request(), other, _ =>
                Task.FromResult(new OperationResult(OperationOutcome.Completed, "Wrong capability")));
        });
    }
}
