# Windows Native Lab - Specification

Recorded 2026-10-01. The repository is a neutral reusable laboratory. Experiment
specifications are planned work, not proof of implemented capabilities. The only
implemented foundation initially is the managed core/test suite and read-only
CIM inventory. No experiment is qualified until its required receipts exist.

## Native approach

Use C#/.NET 10, WinUI 3 and Windows App SDK for interactive experiment shells.
Pin selected SDK/package versions when implemented. Small typed native modules
can bridge D3D/DXGI, WASAPI, capture and inference work where a direct native API
is required. Do not publish an entire browser wrapper as a native capability
experiment. Foundation core stays platform-independent for deterministic tests;
device APIs are isolated from it.

Every experiment defines minimum OS/SDK/runtime/hardware, runtime probes,
permission boundaries, a bounded workload, cancellation/error semantics and a
meaningful fallback. Verify current official Windows/vendor documentation before
implementing version-sensitive APIs. A product label or API announcement is not
a support probe. Preview dependencies stay in isolated experiment projects.

## Qualification contract

| Level | Evidence |
| --- | --- |
| W0 | Deterministic domain tests and fault cases |
| W1 | Pinned toolchain/architecture build |
| W2 | Native fixture UI/automation behavior |
| W3 | Inspected appearance, DPI/theme/high contrast/accessibility |
| W4 | Actual live operations and measured process behavior |
| W5 | Exact package/activation/install/update identity and isolation |
| W6 | Physical device/framework path and limits |

State what each receipt proves. Simulated audio, model stubs, virtual MIDI,
inventory flags and SDR screenshots never replace physical/native evidence.
Record unqualified paths and exercised fallbacks. Missing optional hardware
does not block foundation or fallback work.

## Reusable capability contracts

The managed core supplies `CapabilityState`, `CapabilityReadiness`,
`OperationRequest`, `OperationResult`, `OperationReceipt` and `OperationLedger`.
Keep capability IDs stable, probe reasons actionable and outputs typed at module
boundaries. Request fingerprints bind operation IDs to the exact intended work.
This foundation prevents duplicate session work but is not durable exactly-once
delivery. Persisted journals, credential protection and retry policies belong in
the qualified module/consumer where needed.

Permission probes are read-only. Explicit operations may ask at their moment of
use. Ordinary screen opens and links cannot start recording, a local listener,
device output or a large download. Modules have bounded buffers/requests/jobs,
observable cancellation and honest failure. Published output is atomic; failure
cannot replace a good output or claim a published reference.

## Safety and privacy boundary

No secrets, confidential consumer data, personal machine topology, production
media, large models, raw captures or local credentials in public sources/tests.
Synthetic/publicly licensed fixtures must have provenance. Reports omit host/user
identity and hardware serials. The read-only inventory does not inspect files,
network routes, accounts or listening history. Never disable OS security to make
a capability appear available.

Experiments must not replace running consumer applications, alter their data,
steal file associations, change the system audio default, create firewall exposure
or install drivers without a separately scoped explicit operation. Framework
limitations are reported with fallback rather than hidden by system mutation.

## Evidence and tracking

`experiments/WIN-0NN.md` is the stable specification reference. GitHub Issues is
the eventual tracker; no duplicate status field is maintained here. An executed
experiment writes an evidence receipt with exact commit/toolchain/configuration,
operation/capability ID, fixture/assets, commands/results, inspected captures,
metrics, consent boundary, output outcome, fallback and unverified hardware.
Ignore raw artifacts; commit only sanitized concise receipts and intentional
small public fixtures. Do not claim a hardware result from another machine's
measurements or a library build.

## Official documentation checkpoints

- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/).
- [WinUI 3](https://learn.microsoft.com/windows/apps/winui/winui3/).
- [Windows media](https://learn.microsoft.com/windows/uwp/audio-video-camera/media-playback).
- [Core Audio](https://learn.microsoft.com/windows/win32/coreaudio/core-audio-apis-in-windows-vista).
- [DirectX](https://learn.microsoft.com/windows/win32/directx).
- [Windows AI](https://learn.microsoft.com/windows/ai/).
- [TensorRT](https://docs.nvidia.com/deeplearning/tensorrt/latest/).
- [Windows MIDI Services](https://microsoft.github.io/MIDI/).

These are implementation starting points. Current API availability and release
position must be recorded when each experiment is built; no date-based promise
of MIDI, model, widget or HDR support is made.
