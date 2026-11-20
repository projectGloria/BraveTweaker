$ErrorActionPreference = "Stop"

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    throw "Could not find C# compiler at $csc"
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$output = Join-Path $scriptDir "BraveTweaker.exe"
$iconPath = Join-Path $scriptDir "BraveTweaker.ico"
$pngPath = Join-Path $scriptDir "BraveTweaker.png"
$manifestPath = Join-Path $scriptDir "app.manifest"

$sources = @(
    (Join-Path $scriptDir "Tweaks.cs"),
    (Join-Path $scriptDir "CustomControls.cs"),
    (Join-Path $scriptDir "SplashForm.cs"),
    (Join-Path $scriptDir "MainForm.cs"),
    (Join-Path $scriptDir "Program.cs")
)

$cscArgs = @(
    "/nologo",
    "/target:winexe",
    "/optimize+",
    "/platform:anycpu",
    "/r:System.Windows.Forms.dll",
    "/r:System.Drawing.dll",
    "/r:System.Web.Extensions.dll",
    "/out:$output"
)

if (Test-Path $iconPath) {
    $cscArgs += "/win32icon:$iconPath"
}

if (Test-Path $pngPath) {
    $cscArgs += "/resource:$pngPath,BraveTweaker.png"
}

if (Test-Path $manifestPath) {
    $cscArgs += "/win32manifest:$manifestPath"
}

$cscArgs += $sources

& $csc @cscArgs

if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed with exit code $LASTEXITCODE"
}

Write-Host "Built: $output" -ForegroundColor Green
