param(
    [string]$GamePath,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

function Invoke-DotNet {
    param([string[]]$CommandArgs)
    & dotnet @CommandArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed: $($CommandArgs -join ' ')" }
}

if ([string]::IsNullOrWhiteSpace($GamePath)) {
    $candidates = @()
    $steam = Get-ItemProperty -Path 'HKCU:\Software\Valve\Steam' -ErrorAction SilentlyContinue
    if ($steam.SteamPath) { $candidates += (Join-Path $steam.SteamPath 'steamapps\common\Terraria') }
    $candidates += 'C:\Program Files (x86)\Steam\steamapps\common\Terraria'
    $GamePath = $candidates | Where-Object { Test-Path -LiteralPath (Join-Path $_ 'TerrariaServer.exe') } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($GamePath)) {
    throw "Terraria Server was not found. Specify -GamePath 'D:\SteamLibrary\steamapps\common\Terraria'."
}

$GamePath = [IO.Path]::GetFullPath($GamePath)
$gameExe = Join-Path $GamePath 'TerrariaServer.exe'
if (-not (Test-Path -LiteralPath $gameExe)) { throw "TerrariaServer.exe was not found in: $GamePath" }

$expectedHash = '328872C65A4A7A94F050EAD1566F1567AF15C3193D4D7008A460507D3686D7C2'
$actualHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $gameExe).Hash
if ($actualHash -ne $expectedHash) {
    throw "Unsupported Terraria Server build. Expected 1.4.5.8 ($expectedHash), found $actualHash."
}

$referenceDir = Join-Path $repoRoot '.local\refs-server-windows'
$runtimeOutput = Join-Path $repoRoot ".local\runtime-server-windows-$Configuration"
$distDir = Join-Path $repoRoot 'dist\ExtendedChest-server-windows'
New-Item -ItemType Directory -Force -Path $referenceDir, $runtimeOutput, $distDir | Out-Null

Push-Location $repoRoot
try {
    Invoke-DotNet -CommandArgs @('build', 'tools\ResourceExtractor\ResourceExtractor.csproj', '--configuration', $Configuration)
    $extractor = Join-Path $repoRoot "tools\ResourceExtractor\bin\$Configuration\net8.0\ResourceExtractor.dll"
    Invoke-DotNet -CommandArgs @($extractor, $gameExe, 'Terraria.Libraries.ReLogic.ReLogic.dll', (Join-Path $referenceDir 'ReLogic.dll'))

    Invoke-DotNet -CommandArgs @(
        'build', 'src\ExtendedChest.Runtime\ExtendedChest.Runtime.csproj',
        '--configuration', $Configuration,
        "-p:GamePath=$GamePath",
        "-p:ReferencePath=$referenceDir",
        '-p:GameAssemblyName=TerrariaServer',
        '-p:GameExecutable=TerrariaServer.exe',
        '-p:GraphicsBackend=XNA',
        '--output', $runtimeOutput
    )
    Invoke-DotNet -CommandArgs @('build', 'src\ExtendedChest.Patcher\ExtendedChest.Patcher.csproj', '--configuration', $Configuration)

    $patcher = Join-Path $repoRoot "src\ExtendedChest.Patcher\bin\$Configuration\net8.0\ExtendedChest.Patcher.dll"
    $runtime = Join-Path $runtimeOutput 'ExtendedChest.Runtime.dll'
    Invoke-DotNet -CommandArgs @($patcher, $gameExe, $runtime, $referenceDir, (Join-Path $distDir 'TerrariaServer.exe'))

    Copy-Item -LiteralPath (Join-Path $repoRoot 'config\extended-chest.config') -Destination $distDir -Force
    Copy-Item -LiteralPath (Join-Path $repoRoot 'scripts\Start-ExtendedChest-Server.cmd') -Destination $distDir -Force
    Copy-Item -LiteralPath (Join-Path $repoRoot 'README.md') -Destination $distDir -Force
    Get-ChildItem -LiteralPath $GamePath -File | Where-Object {
        $_.Extension -eq '.dll' -or $_.Name -eq 'steam_appid.txt'
    } | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $distDir -Force }

    $contentLink = Join-Path $distDir 'Content'
    $sourceContent = Join-Path $GamePath 'Content'
    if ((Test-Path -LiteralPath $sourceContent) -and -not (Test-Path -LiteralPath $contentLink)) {
        New-Item -ItemType Junction -Path $contentLink -Target (Join-Path $GamePath 'Content') | Out-Null
    }

    Write-Host ''
    Write-Host 'Windows server build completed.' -ForegroundColor Green
    Write-Host "Launch: $distDir\Start-ExtendedChest-Server.cmd"
    Write-Host 'Test worlds will be stored in the Saves subdirectory.'
}
finally {
    Pop-Location
}
