[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
if (-not $IsWindows -and $PSVersionTable.PSEdition -ne 'Desktop') {
    throw 'Hardware inventory requires Windows CIM providers.'
}

$processors = @(Get-CimInstance -ClassName Win32_Processor | Select-Object Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors)
$graphics = @(Get-CimInstance -ClassName Win32_VideoController | Select-Object Name, DriverVersion, DriverDate, VideoProcessor, AdapterRAM)
$system = Get-CimInstance -ClassName Win32_ComputerSystem
$os = Get-CimInstance -ClassName Win32_OperatingSystem

[ordered]@{
    schemaVersion = 1
    observedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    evidenceKind = 'inventory-only'
    processors = $processors
    graphics = $graphics
    memoryBytes = [uint64]$system.TotalPhysicalMemory
    operatingSystem = [ordered]@{
        caption = $os.Caption
        version = $os.Version
        build = $os.BuildNumber
        architecture = $os.OSArchitecture
    }
    limitations = @(
        'CIM inventory is not a physical display, audio, acceleration or MIDI qualification.',
        'AdapterRAM can be truncated or inaccurate; it is not verified dedicated GPU memory.',
        'No serial number, machine name, username, network address or device instance ID is collected.'
    )
} | ConvertTo-Json -Depth 6
