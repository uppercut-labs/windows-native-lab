# Windows capability experiments

These are bounded specifications, not implemented or qualified Windows experiments.
The managed core and inventory foundation are separate from their device receipts.
See [qualification contract](../SPEC.md).

| ID | Experiment | Contract |
| --- | --- | --- |
| [WIN-001](WIN-001.md) | Shell Atlas | Typed native actions, URI activation, jump lists and existing-instance routing. |
| [WIN-002](WIN-002.md) | Surface Deck | Native tray/taskbar controls, opt-in notifications and optional Windows widgets. |
| [WIN-003](WIN-003.md) | Media Desk | MediaPlayer playback, System Media Transport Controls and native lifecycle recovery. |
| [WIN-004](WIN-004.md) | Audio Workshop | Bounded AudioGraph/WASAPI experiments, endpoint selection and measured audio recovery. |
| [WIN-005](WIN-005.md) | RTX Model Bench | Compare supported Windows ML/ONNX, optional TensorRT and CPU execution with real correctness/resource receipts. |
| [WIN-006](WIN-006.md) | Advanced Color Stage | DXGI readiness, genuine HDR/WCG presentation and mixed-display/color transitions. |
| [WIN-007](WIN-007.md) | Render Museum | D3D12 rendering experiments for SDR/HDR scenes, artwork filters, 3D scenes and CRT styling. |
| [WIN-008](WIN-008.md) | Search & Activation | Qualified Windows indexing/search routes, native deep links and document/file activation. |
| [WIN-009](WIN-009.md) | Local Constellation | Authenticated local discovery, pairing, commands, handoff and optional relay integration. |
| [WIN-010](WIN-010.md) | Capture & Render | Explicit Windows capture, GPU rendering/encoding and resilient media export. |
| [WIN-011](WIN-011.md) | Access First | Native keyboard, Narrator, high contrast, text scaling and mixed-DPI qualification. |
| [WIN-012](WIN-012.md) | MIDI Workshop | Isolated MIDI device/input/output preview with safe typed action mappings. |

Dependencies are qualification inputs: WIN-001/003/011 establish shell/media/access;
WIN-002/008 extend surfaces; WIN-004/006/007 qualify audio/graphics;
WIN-005/010 qualify inference/export; WIN-009 qualifies local peers;
WIN-012 remains an isolated version-sensitive MIDI preview. No optional hardware
blocks useful baseline/fallback work.
