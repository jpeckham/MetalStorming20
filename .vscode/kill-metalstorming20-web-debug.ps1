$ErrorActionPreference = "Stop"

$workspace = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$webProject = Join-Path $workspace "MetalStorming20.Web\MetalStorming20.Web.csproj"
$webOutput = Join-Path $workspace "MetalStorming20.Web\bin\Debug\net8.0\MetalStorming20.Web.dll"
$debugPorts = @(5193, 7193)

$processes = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object {
        $commandLine = $_.CommandLine
        if ([string]::IsNullOrWhiteSpace($commandLine)) {
            return $false
        }

        $commandLine.Contains($webProject, [StringComparison]::OrdinalIgnoreCase) -or
            $commandLine.Contains($webOutput, [StringComparison]::OrdinalIgnoreCase) -or
            $commandLine.Contains("MetalStorming20.Web", [StringComparison]::OrdinalIgnoreCase)
    }

$processIds = @($processes | Select-Object -ExpandProperty ProcessId)
$portOwnerIds = foreach ($port in $debugPorts) {
    Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
        Where-Object { $_.OwningProcess -gt 0 } |
        Select-Object -ExpandProperty OwningProcess
}

$debugHosts = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object {
        $commandLine = $_.CommandLine
        if ([string]::IsNullOrWhiteSpace($commandLine)) {
            return $false
        }

        if (-not $commandLine.Contains("BrowserDebugHost.dll", [StringComparison]::OrdinalIgnoreCase)) {
            return $false
        }

        foreach ($processId in @($processIds + $portOwnerIds | Sort-Object -Unique)) {
            if ($commandLine -match "--OwnerPid\s+$processId(\s|$)") {
                return $true
            }
        }

        return $false
    }

$allProcessIds = @($processIds + $portOwnerIds + @($debugHosts | Select-Object -ExpandProperty ProcessId) | Sort-Object -Unique)

foreach ($processId in $allProcessIds) {
    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
    if ($null -ne $process) {
        Write-Host "Stopping stale MetalStorming20.Web debug process $processId"
        Stop-Process -Id $processId -Force
    }
}
