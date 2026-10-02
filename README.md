# Windows Native Lab

A reusable Windows capability laboratory: native shell surfaces, media/audio,
graphics, advanced color, local models, activation, authenticated local peers,
capture/export, accessibility and MIDI. Public fixtures and typed contracts keep
capability availability separate from evidence that a real hardware path worked.

This checkout currently contains a .NET 10 core library, meaningful unit tests,
a read-only hardware inventory tool, and twelve bounded experiment specifications.
It does not yet contain implemented Windows UI/framework experiments. A passing
core test is not proof of HDR, native audio, MIDI, widgets or GPU inference.

Read [SPEC.md](SPEC.md) and the [experiment index](experiments/README.md).

## Run the foundation

The pinned SDK is .NET 10.0.303, with later patches allowed. From this repository:

```powershell
dotnet test tests/WindowsNativeLab.Core.Tests/WindowsNativeLab.Core.Tests.csproj
pwsh -File tools/hardware-report.ps1
```

Tests use synthetic inputs and perform no device/network/media side effects.
The inventory emits CPU/GPU/driver/RAM/OS information only. It omits machine
names, usernames, serials, network addresses and hardware instance identifiers.
Do not treat its result as HDR/120 Hz/audio/model/MIDI qualification.

## Core contracts

`CapabilityReadiness` records a capability ID, real probe state, reason, fallback
and observation timestamp. Probes never prompt or mutate the system.

`OperationLedger` binds an operation identity to a capability and caller-supplied
request fingerprint. Concurrent duplicates share one operation and receipt; an
identity reused with changed work is rejected. The first caller owns cancellation;
duplicates join its result and do not change its token. Results distinguish
completed, cancelled, rejected and failed work. Only completed work may reference
a published output. Modules must atomically publish before reporting completion
and must clean partial output before reporting cancellation/failure.

The ledger is session-local, bounded and in-memory. At capacity it rejects new
operations rather than evicting identities and accidentally replaying a side
effect. It does not provide durable/network exactly-once semantics. Consumers
need a persisted dedupe journal and an explicit retry/session policy for that.
Unexpected programming exceptions propagate and remain faulted for duplicate
requests; they are not disguised as successful receipts. Wall-clock timestamps
are observations, not latency measurements; use monotonic clocks/profilers for
performance evidence.

Consumer applications own local configuration, permission decisions, packaging
and any confidential data. This repository has no dependency on consumer-specific
configuration, private repositories or production media/models. No generated raw
captures, model binaries, signing keys or local inventory are committed.
