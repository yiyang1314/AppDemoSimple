[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [switch]$KeepStage
)

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot   = Split-Path -Parent $scriptRoot
$project    = Join-Path $repoRoot 'Rxbit.BrigadeApp\Rxbit.BrigadeApp.csproj'
$solution   = Join-Path $repoRoot 'Rxbit.BrigadeApp\Rxbit.BrigadeApp.sln'
$releaseDir = Join-Path $repoRoot 'Rxbit.BrigadeApp\bin\x86\Release'
$stageDir   = Join-Path $scriptRoot 'stage'
$objDir     = Join-Path $scriptRoot 'obj'
$distDir    = Join-Path $repoRoot 'dist'
$msiPath    = Join-Path $distDir 'Rxbit.BrigadeApp-1.5-x86.msi'
$wixToolDir = Join-Path $repoRoot '.tools'
$wixExe     = Join-Path $wixToolDir 'wix.exe'
$wixVersion = '6.0.2'
$uiExt      = 'WixToolset.UI.wixext'
$uiExtDll   = Join-Path $repoRoot ".wix\extensions\$uiExt\$wixVersion\wixext6\$uiExt.dll"

function Find-MSBuild {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vswhere) {
        $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' 2>$null |
            Select-Object -First 1
        if ($found -and (Test-Path $found)) { return $found }
    }

    $cmd = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    throw 'MSBuild.exe not found. Install Visual Studio 2022 with .NET Framework 4.8 tooling.'
}

function Ensure-Wix {
    if (-not (Test-Path $wixExe)) {
        $dotnet = Get-Command dotnet.exe -ErrorAction SilentlyContinue
        if (-not $dotnet) {
            throw "WiX not found and dotnet SDK is unavailable. Install WiX 6 into $wixToolDir or install .NET SDK."
        }

        Write-Host "Installing WiX $wixVersion into $wixToolDir ..."
        & $dotnet.Source tool install wix --tool-path $wixToolDir --version $wixVersion
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path $wixExe)) {
            throw 'Failed to install WiX. Check network access to nuget.org.'
        }
    }

    if (-not (Test-Path $uiExtDll)) {
        Write-Host "Installing WiX extension $uiExt ..."
        Push-Location $repoRoot
        try {
            & $wixExe extension add "$uiExt/$wixVersion"
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to install WiX extension $uiExt. Check network access to nuget.org."
            }
        }
        finally {
            Pop-Location
        }
    }
}

function Get-StableId {
    param([string]$Key, [string]$Prefix, [int]$Chars = 24)

    $md5 = [System.Security.Cryptography.MD5]::Create()
    try {
        $hash = $md5.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Key.ToLowerInvariant()))
    }
    finally {
        $md5.Dispose()
    }
    $hex = ($hash | ForEach-Object { $_.ToString('x2') }) -join ''
    return $Prefix + $hex.Substring(0, $Chars)
}

function Get-StableGuid {
    param([string]$Key)

    $md5 = [System.Security.Cryptography.MD5]::Create()
    try {
        $hash = $md5.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Key.ToLowerInvariant()))
    }
    finally {
        $md5.Dispose()
    }
    $hex = ($hash | ForEach-Object { $_.ToString('x2') }) -join ''
    return $hex.Substring(0, 8) + '-' + $hex.Substring(8, 4) + '-' +
           $hex.Substring(12, 4) + '-' + $hex.Substring(16, 4) + '-' + $hex.Substring(20, 12)
}

function ConvertTo-XmlAttribute {
    param([string]$Value)
    return $Value.Replace('&', '&amp;').Replace('<', '&lt;').Replace('"', '&quot;')
}

function Copy-StageFiles {
    if (-not (Test-Path $releaseDir)) {
        throw "Release output not found: $releaseDir"
    }

    if (Test-Path $stageDir) { Remove-Item $stageDir -Recurse -Force }
    New-Item -ItemType Directory -Path $stageDir | Out-Null

    # Release binaries; PDB/XML documentation are not needed at runtime.
    Get-ChildItem $releaseDir -Recurse -File |
        Where-Object { $_.Extension -notin '.pdb', '.xml' } |
        ForEach-Object {
            $relative = $_.FullName.Substring($releaseDir.Length + 1)
            $destination = Join-Path $stageDir $relative
            $parent = Split-Path -Parent $destination
            if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
            Copy-Item $_.FullName $destination -Force
        }

    # ZLG runtime is compiled as embedded resources in the library project and is
    # therefore absent from the Release output. Copy the runtime tree explicitly.
    $zlgSource = Join-Path $repoRoot 'BrigadeTool.CanDevices\zlgcan'
    if (Test-Path $zlgSource) {
        $zlgTarget = Join-Path $stageDir 'zlgcan'
        New-Item -ItemType Directory -Path $zlgTarget -Force | Out-Null
        Copy-Item (Join-Path $zlgSource '*') $zlgTarget -Recurse -Force
    }
    else {
        Write-Warning "zlgcan runtime folder not found: $zlgSource"
    }

    # Fallback driver SDK bundle extracted by the app when PEAK Basic is missing.
    $sdkSource = Join-Path $repoRoot 'Rxbit.Brigade.RadarApp\BrigadeSdk.zip'
    if (Test-Path $sdkSource) {
        Copy-Item $sdkSource (Join-Path $stageDir 'BrigadeSdk.zip') -Force
    }
    else {
        Write-Warning "BrigadeSdk.zip not found: $sdkSource"
    }

    # Logs and docs are intentionally not packaged: Logs is created at runtime and
    # Help -> Docs currently opens the About view.
}

function New-AppFilesFragment {
    param([string]$StagePath, [string]$OutputFile)

    $root = @{ Name = ''; Dirs = @{}; Files = @() }

    Get-ChildItem $StagePath -Recurse -File | ForEach-Object {
        $relative = $_.FullName.Substring($StagePath.Length + 1)
        $parts = $relative -split '\\'
        $node = $root
        for ($i = 0; $i -lt $parts.Length - 1; $i++) {
            $name = $parts[$i]
            if (-not $node.Dirs.ContainsKey($name)) {
                $node.Dirs[$name] = @{ Name = $name; Dirs = @{}; Files = @() }
            }
            $node = $node.Dirs[$name]
        }
        $node.Files += $parts[$parts.Length - 1]
    }

    $lines = New-Object 'System.Collections.Generic.List[string]'
    $components = New-Object 'System.Collections.Generic.List[string]'

    $lines.Add('<?xml version="1.0" encoding="UTF-8"?>')
    $lines.Add('<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">')
    $lines.Add('  <Fragment>')
    $lines.Add('    <DirectoryRef Id="INSTALLDIR">')

    function Emit-Node {
        param($Node, [string]$RelativeDir, [int]$Indent)

        $pad = ' ' * $Indent
        foreach ($fileName in ($Node.Files | Sort-Object)) {
            $relativePath = if ($RelativeDir) { "$RelativeDir\$fileName" } else { $fileName }
            $componentId = Get-StableId -Key "file:$relativePath" -Prefix 'cmp_'
            $guid = Get-StableGuid -Key "file:$relativePath"
            $source = ConvertTo-XmlAttribute (Join-Path $StagePath $relativePath)
            $lines.Add("$pad<Component Id=""$componentId"" Guid=""$guid"">")
            $lines.Add("$pad  <File Id=""$(Get-StableId -Key "fileid:$relativePath" -Prefix 'fil_')"" Name=""$(ConvertTo-XmlAttribute $fileName)"" Source=""$source"" KeyPath=""yes"" />")
            $lines.Add("$pad</Component>")
            $components.Add($componentId)
        }

        foreach ($dirName in ($Node.Dirs.Keys | Sort-Object)) {
            $child = $Node.Dirs[$dirName]
            $childRelative = if ($RelativeDir) { "$RelativeDir\$dirName" } else { $dirName }
            $dirId = Get-StableId -Key "dir:$childRelative" -Prefix 'dir_'
            $lines.Add("$pad<Directory Id=""$dirId"" Name=""$(ConvertTo-XmlAttribute $dirName)"">")
            Emit-Node -Node $child -RelativeDir $childRelative -Indent ($Indent + 2)
            $lines.Add("$pad</Directory>")
        }
    }

    Emit-Node -Node $root -RelativeDir '' -Indent 6

    $lines.Add('    </DirectoryRef>')
    $lines.Add('    <ComponentGroup Id="AppFiles">')
    foreach ($componentId in $components) {
        $lines.Add("      <ComponentRef Id=""$componentId"" />")
    }
    $lines.Add('    </ComponentGroup>')
    $lines.Add('  </Fragment>')
    $lines.Add('</Wix>')

    $encoding = New-Object System.Text.UTF8Encoding($true)
    [System.IO.File]::WriteAllLines($OutputFile, $lines, $encoding)
}

# 1. Build Release x86
if (-not $SkipBuild) {
    $msbuild = Find-MSBuild
    # Build the solution: the library projects only define Any CPU, and the
    # solution maps Release|x86 to Any CPU for them and x86 for the WPF app.
    $buildTarget = if (Test-Path $solution) { $solution } else { $project }
    Write-Host "Building Release|x86 with $msbuild"
    & $msbuild $buildTarget /t:Rebuild /p:Configuration=Release /p:Platform=x86 /m /nologo /v:minimal
    if ($LASTEXITCODE -ne 0) { throw "MSBuild failed with exit code $LASTEXITCODE." }
}

if (-not (Test-Path (Join-Path $releaseDir 'Rxbit.BrigadeApp.exe'))) {
    throw "Rxbit.BrigadeApp.exe not found in $releaseDir."
}

# 2. Stage runtime files
Write-Host 'Staging runtime files ...'
Copy-StageFiles

# 3. Generate WiX fragment
Write-Host 'Generating WiX file fragment ...'
if (-not (Test-Path $objDir)) { New-Item -ItemType Directory -Path $objDir -Force | Out-Null }
$fragment = Join-Path $objDir 'AppFiles.wxs'
New-AppFilesFragment -StagePath $stageDir -OutputFile $fragment

# 4. Build MSI
Ensure-Wix

if (-not (Test-Path $distDir)) { New-Item -ItemType Directory -Path $distDir -Force | Out-Null }
if (Test-Path $msiPath) { Remove-Item $msiPath -Force }

Write-Host 'Building MSI ...'
Push-Location $scriptRoot
try {
    & $wixExe build -arch x86 -ext $uiExtDll -intermediatefolder (Join-Path $objDir 'wix') -out $msiPath Product.wxs $fragment
    if ($LASTEXITCODE -ne 0) { throw "WiX build failed with exit code $LASTEXITCODE." }
}
finally {
    Pop-Location
}

$msi = Get-Item $msiPath
Write-Host ''
Write-Host "MSI: $($msi.FullName)"
Write-Host ("Size: {0:N1} MB" -f ($msi.Length / 1MB))

# Read the generated ProductCode for silent uninstall commands.
try {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $database = $installer.OpenDatabase($msiPath, 0)
    $view = $database.OpenView("SELECT Value FROM Property WHERE Property='ProductCode'")
    $view.Execute()
    $record = $view.Fetch()
    $productCode = $record.StringData(1)
    Write-Host "ProductCode: $productCode"
    $view.Close()
}
catch {
    Write-Warning "Could not read ProductCode: $($_.Exception.Message)"
}

if (-not $KeepStage) {
    Remove-Item $stageDir -Recurse -Force
}
