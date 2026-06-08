$ErrorActionPreference = "Stop"

$workspace = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$webProject = Join-Path $workspace "MetalStorming20.Web\MetalStorming20.Web.csproj"
$webOutput = Join-Path $workspace "MetalStorming20.Web\bin\Debug\net8.0\MetalStorming20.Web.dll"

$processes = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object {
        $commandLine = $_.CommandLine
        if ([string]::IsNullOrWhiteSpace($commandLine)) {
            return $false
        }

        $commandLine.Contains($webProject, [StringComparison]::OrdinalIgnoreCase) -or
            $commandLine.Contains($webOutput, [StringComparison]::OrdinalIgnoreCase)
    }

$processIds = @($processes | Select-Object -ExpandProperty ProcessId)

$debugHosts = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object {
        $commandLine = $_.CommandLine
        if ([string]::IsNullOrWhiteSpace($commandLine) -or -not $commandLine.Contains("BrowserDebugHost.dll", [StringComparison]::OrdinalIgnoreCase)) {
            return $false
        }

        foreach ($processId in $processIds) {
            if ($commandLine -match "--OwnerPid\s+$processId(\s|$)") {
                return $true
            }
        }

        return $false
    }

$allProcessIds = @($processIds + @($debugHosts | Select-Object -ExpandProperty ProcessId) | Sort-Object -Unique)

foreach ($processId in $allProcessIds) {
    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
    if ($null -ne $process) {
        Write-Host "Stopping stale MetalStorming20.Web debug process $processId"
        Stop-Process -Id $processId -Force
    }
}
