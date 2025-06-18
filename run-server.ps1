#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and runs the DotNet Build Server for MCP.

.DESCRIPTION
    This script builds the DotNet Build Server project and runs it, making it available
    for Claude Code connections from WSL. The server listens on port 5000.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default is Release.

.PARAMETER NoBuild
    Skip the build step and run the existing build.

.EXAMPLE
    ./run-server.ps1
    Builds and runs the server in Release mode.

.EXAMPLE
    ./run-server.ps1 -Configuration Debug
    Builds and runs the server in Debug mode.

.EXAMPLE
    ./run-server.ps1 -NoBuild
    Runs the server without rebuilding.
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

Write-Host "DotNet Build Server for MCP" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "Found .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 9.0 or later." -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Build the project
if (-not $NoBuild) {
    Write-Host "`nBuilding DotNet Build Server ($Configuration)..." -ForegroundColor Yellow
    dotnet build "DotNetBuildServer/DotNetBuildServer.csproj" -c $Configuration
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "Build succeeded!" -ForegroundColor Green
}

# Get local IP addresses for WSL connection
Write-Host "`nServer endpoints available for WSL connection:" -ForegroundColor Yellow
$ips = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -ne "127.0.0.1" -and $_.IPAddress -ne "127.0.1.1" }
$ips | ForEach-Object {
    Write-Host "  http://$($_.IPAddress):5000/sse" -ForegroundColor Cyan
}

Write-Host "`nTo connect from WSL:" -ForegroundColor Yellow
Write-Host "1. Open a WSL terminal" -ForegroundColor White
Write-Host "2. Run one of these commands (use the IP that matches your network):" -ForegroundColor White
$ips | ForEach-Object {
    Write-Host "   claude mcp add dotnet-build --transport sse http://$($_.IPAddress):5000/sse" -ForegroundColor Green
}
Write-Host "`n⚠️  IMPORTANT: The URL must end with /sse" -ForegroundColor Red

Write-Host "`nStarting server..." -ForegroundColor Green
Write-Host "Press Ctrl+C to stop`n" -ForegroundColor Gray

# Run the server
dotnet run --project "DotNetBuildServer/DotNetBuildServer.csproj" -c $Configuration --no-build