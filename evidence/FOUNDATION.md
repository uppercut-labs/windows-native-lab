# Foundation verification receipt

Recorded 2026-10-01. This receipt covers managed contracts/tests and the read-only
inventory tool only. WIN-001 through WIN-012 remain experiment specifications.

## Implemented scope

- Typed readiness states with actionable reason/fallback and observation time.
- Session-bounded operation identity/fingerprint deduplication and cancellation.
- Completed/cancelled/rejected/failed outcomes; unsuccessful results cannot
  reference a published output. Unexpected errors remain visible/faulted.
- Read-only privacy-limited CIM CPU/GPU/driver/RAM/OS JSON inventory.

## Commands and results

```powershell
dotnet test tests/WindowsNativeLab.Core.Tests/WindowsNativeLab.Core.Tests.csproj --verbosity minimal
pwsh -NoProfile -File tools/hardware-report.ps1
```

The installed SDK used was .NET 10.0.303. The test project restores its pinned
template dependencies, builds the net10.0 library/tests with warnings as errors,
and passes 17 tests using synthetic work only. The inventory command completed with valid
JSON; raw host inventory is intentionally not committed here. Local Markdown
links, ASCII content and twelve experiment specifications were checked.

## Evidence boundary

W0-W1 foundation evidence only. No implemented WinUI experiment, native media,
HDR/WCG, 120 Hz, actual audio endpoint, model/provider acceleration, relay,
capture, system search, widget, MIDI device or package/install qualification is
claimed. Wall-clock receipt timestamps are not performance measurements.

The ledger is bounded/in-memory, not durable exactly-once. Its first caller owns
cancellation; duplicates join that operation and cannot cancel it with a different
token. Failed/cancelled receipt publication cannot prove external device cleanup;
each future module must qualify its actual side effects and cleanup separately.
