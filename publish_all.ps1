<#
  统一编译 / 打包 / 发布入口
  仓库：d:\IDE-Workspace\Rxbit.BrigadeApp

  用法示例：
    .\publish_all.ps1                                   # Release，发布全部应用
    .\publish_all.ps1 -Configuration Debug              # Debug，发布全部应用
    .\publish_all.ps1 -Module Rxbit.BrigadeApp          # 只发布主程序
    .\publish_all.ps1 -Module Controll,VideoPlayer      # 发布多个模块
    .\publish_all.ps1 -VideoPlayerSelfContained:$false  # VideoPlayer 框架依赖发布
    .\publish_all.ps1 -CollectExternalDependencies:$false   # 不收集外部依赖
    .\publish_all.ps1 -BuildInstaller                   # 额外生成 RadarApp MSI

  说明：
    - 产物统一输出到 publish\<模块名>\
    - 任一模块编译/发布失败即中止，日志写入 publish\_logs\
    - 外部依赖（Kvaser / FFmpeg / hhc）缺失只告警，不中止
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string[]]$Module = @('All'),

    [bool]$CollectExternalDependencies = $true,

    [bool]$VideoPlayerSelfContained = $true,

    [switch]$BuildInstaller,

    [switch]$SkipRestore,

    [string]$PublishRoot = ''
)

$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($PublishRoot)) {
    $PublishRoot = Join-Path $RepoRoot 'publish'
}
$SolutionPath = Join-Path $RepoRoot 'Rxbit.BrigadeApp\Rxbit.BrigadeApp.sln'

$script:LogFile = $null
$script:VsRoot = $null
$script:MsBuild = $null
$script:Dotnet = $null
$script:ReportedModules = @()
$script:DependencyRows = @()
$script:MissingDependencies = @()

function New-LogFile {
    $logDir = Join-Path $PublishRoot '_logs'
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    $name = 'publish_{0}_{1}.log' -f $Configuration, (Get-Date -Format 'yyyyMMdd_HHmmss')
    $script:LogFile = Join-Path $logDir $name
    $header = @(
        '================================================================',
        ('统一发布日志  生成时间: {0}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')),
        ('配置: {0}    发布根目录: {1}' -f $Configuration, $PublishRoot),
        '================================================================'
    )
    $header | Out-File -FilePath $script:LogFile -Encoding UTF8
}

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet('INFO', 'WARN', 'ERROR', 'OK', 'SKIP')]
        [string]$Level = 'INFO'
    )

    $line = '[{0}] [{1}] {2}' -f (Get-Date -Format 'HH:mm:ss'), $Level, $Message
    if ($script:LogFile) {
        Add-Content -Path $script:LogFile -Value $line -Encoding UTF8
    }

    switch ($Level) {
        'ERROR' { Write-Host $line -ForegroundColor Red }
        'WARN'  { Write-Host $line -ForegroundColor Yellow }
        'OK'    { Write-Host $line -ForegroundColor Green }
        'SKIP'  { Write-Host $line -ForegroundColor DarkGray }
        default { Write-Host $line -ForegroundColor Gray }
    }
}

function Invoke-NativeTool {
    param(
        [string]$Exe,
        [string[]]$Arguments,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Exe)) {
        throw "$Label 失败：找不到可执行文件 $Exe"
    }

    Write-Log "开始：$Label"
    Write-Log ("命令：{0} {1}" -f $Exe, ($Arguments -join ' '))

    $output = & $Exe @Arguments 2>&1 | ForEach-Object { $_.ToString() }
    $exitCode = $LASTEXITCODE

    foreach ($line in $output) {
        if ($script:LogFile) {
            Add-Content -Path $script:LogFile -Value $line -Encoding UTF8
        }
    }

    if ($exitCode -ne 0) {
        Write-Log "$Label 失败，退出码 $exitCode" 'ERROR'
        $interesting = $output | Where-Object { $_ -match 'error|错误|失败|MSB\d+' } | Select-Object -Last 25
        if (-not $interesting) {
            $interesting = $output | Select-Object -Last 25
        }
        foreach ($line in $interesting) {
            Write-Host "  $line" -ForegroundColor Red
        }
        throw "$Label 失败，退出码 $exitCode；完整输出见日志 $script:LogFile"
    }

    Write-Log "$Label 完成" 'OK'
}

function Get-VsWhere {
    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'),
        (Join-Path $env:ProgramFiles 'Microsoft Visual Studio\Installer\vswhere.exe')
    )
    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate)) {
            return $candidate
        }
    }
    return $null
}

function Resolve-BuildTools {
    $vsWhere = Get-VsWhere
    if ($vsWhere) {
        $installPath = & $vsWhere -latest -products '*' -property installationPath 2>$null
        if ($installPath) {
            $script:VsRoot = ($installPath | Select-Object -First 1).Trim()
        }
        $msbuildCandidates = & $vsWhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' 2>$null
        $foundMsBuild = $msbuildCandidates | Select-Object -First 1
        if ($foundMsBuild) {
            $script:MsBuild = $foundMsBuild.Trim()
        }
    }

    if (-not $script:MsBuild) {
        $script:MsBuild = (Get-Command msbuild.exe -ErrorAction SilentlyContinue | Select-Object -First 1).Source
    }
    if (-not $script:MsBuild) {
        throw '找不到 MSBuild.exe。请安装 Visual Studio 2022（.NET 桌面开发工作负载），或从「Developer Command Prompt」运行本脚本。'
    }
    if (-not $script:VsRoot) {
        $script:VsRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $script:MsBuild)))
    }

    $dotnetCommand = Get-Command dotnet.exe -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($dotnetCommand) {
        $script:Dotnet = $dotnetCommand.Source
    }

    Write-Log "MSBuild: $script:MsBuild" 'INFO'
    Write-Log "Visual Studio: $script:VsRoot" 'INFO'
    if ($script:Dotnet) {
        Write-Log "dotnet: $script:Dotnet" 'INFO'
    } else {
        Write-Log '未找到 dotnet.exe；VideoPlayer 发布不可用，net48 SDK 工程仍可由 MSBuild 构建。' 'WARN'
    }
}

function Read-PublishConfig {
    $configPath = Join-Path $RepoRoot 'publish.config.json'
    $result = [ordered]@{
        kvaserSearchPaths = @()
        ffmpegSearchPaths = @()
        hhcSearchPaths = @()
    }
    if (-not (Test-Path -LiteralPath $configPath)) {
        return $result
    }
    try {
        $raw = Get-Content -LiteralPath $configPath -Raw -Encoding UTF8
        $json = $raw | ConvertFrom-Json
        foreach ($key in @('kvaserSearchPaths', 'ffmpegSearchPaths', 'hhcSearchPaths')) {
            if ($json.PSObject.Properties.Name -contains $key) {
                $result[$key] = @($json.$key)
            }
        }
        Write-Log "已读取依赖探测配置：$configPath" 'INFO'
    } catch {
        Write-Log "依赖探测配置解析失败，将使用内置候选路径：$($_.Exception.Message)" 'WARN'
    }
    return $result
}

function Test-IsIdeBundledFfmpeg {
    param([string]$Path)
    $normalized = $Path.ToLowerInvariant()
    return (
        ($normalized -like '*\trae cn\*') -or
        ($normalized -like '*\trae\resources\app\bin\*') -or
        ($normalized -like '*\trae-cn\*') -or
        ($normalized -like '*\resources\app\bin\ffmpeg.exe')
    )
}

function Find-LeafFile {
    param(
        [string]$Leaf,
        [object[]]$Roots
    )

    foreach ($root in $Roots) {
        if ([string]::IsNullOrWhiteSpace($root)) { continue }
        $expanded = [Environment]::ExpandEnvironmentVariables([string]$root)
        if ([string]::IsNullOrWhiteSpace($expanded)) { continue }

        if (Test-Path -LiteralPath $expanded -PathType Leaf) {
            if ([System.IO.Path]::GetFileName($expanded) -ieq $Leaf) {
                return (Resolve-Path -LiteralPath $expanded).Path
            }
            continue
        }

        if (Test-Path -LiteralPath $expanded -PathType Container) {
            $direct = Join-Path $expanded $Leaf
            if (Test-Path -LiteralPath $direct -PathType Leaf) {
                return (Resolve-Path -LiteralPath $direct).Path
            }
            $found = Get-ChildItem -LiteralPath $expanded -Filter $Leaf -Recurse -File -ErrorAction SilentlyContinue |
                Select-Object -First 1
            if ($found) {
                return $found.FullName
            }
        }
    }

    $command = Get-Command $Leaf -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($command) {
        return $command.Source
    }
    return $null
}

function Test-IsX86PeFile {
    param([string]$Path)
    $stream = $null
    $reader = $null
    try {
        $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
        $reader = New-Object System.IO.BinaryReader($stream)
        $stream.Position = 0x3C
        $peOffset = $reader.ReadInt32()
        $stream.Position = $peOffset + 4
        $machine = $reader.ReadUInt16()
        return ($machine -eq 0x014C)
    } catch {
        return $true
    } finally {
        if ($reader) { $reader.Dispose() }
        if ($stream) { $stream.Dispose() }
    }
}

function Find-KvaserCanLib32 {
    param([object[]]$Roots)

    $candidates = New-Object System.Collections.Generic.List[string]
    foreach ($root in $Roots) {
        if ([string]::IsNullOrWhiteSpace($root)) { continue }
        $expanded = [Environment]::ExpandEnvironmentVariables([string]$root)
        if ([string]::IsNullOrWhiteSpace($expanded)) { continue }

        if (Test-Path -LiteralPath $expanded -PathType Leaf) {
            if ([System.IO.Path]::GetFileName($expanded) -ieq 'canlib32.dll') {
                $candidates.Add((Resolve-Path -LiteralPath $expanded).Path)
            }
            continue
        }
        if (Test-Path -LiteralPath $expanded -PathType Container) {
            $direct = Join-Path $expanded 'canlib32.dll'
            if (Test-Path -LiteralPath $direct -PathType Leaf) {
                $candidates.Add((Resolve-Path -LiteralPath $direct).Path)
            }
            $found = Get-ChildItem -LiteralPath $expanded -Filter 'canlib32.dll' -Recurse -File -ErrorAction SilentlyContinue
            foreach ($item in $found) {
                if (-not $candidates.Contains($item.FullName)) {
                    $candidates.Add($item.FullName)
                }
            }
        }
    }

    $command = Get-Command canlib32.dll -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($command) {
        $candidates.Add($command.Source)
    }

    foreach ($candidate in $candidates) {
        if (Test-IsX86PeFile $candidate) {
            return $candidate
        }
        Write-Log "跳过非 x86 的 canlib32.dll：$candidate" 'WARN'
    }
    return $null
}

function Add-DependencyRow {
    param(
        [string]$ModuleName,
        [string]$Dependency,
        [string]$Status,
        [string]$Source = '',
        [string]$Target = ''
    )
    $script:DependencyRows += [pscustomobject]@{
        Module     = $ModuleName
        Dependency = $Dependency
        Status     = $Status
        Source     = $Source
        Target     = $Target
    }
    if ($Status -eq 'MISSING') {
        $script:MissingDependencies += "$ModuleName -> $Dependency"
    }
}

function Copy-DirectoryTree {
    param(
        [string]$Source,
        [string]$Target
    )
    if (Test-Path -LiteralPath $Target) {
        Remove-Item -LiteralPath $Target -Recurse -Force
    }
    Copy-Item -LiteralPath $Source -Destination $Target -Recurse -Force
}

function Copy-DirectoryContent {
    param(
        [string]$Source,
        [string]$Destination,
        [string[]]$ExcludeNames = @()
    )
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    Get-ChildItem -LiteralPath $Source -Force | Where-Object { $ExcludeNames -notcontains $_.Name } | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $Destination -Recurse -Force
    }
}

function Publish-ZlgcanNative {
    param([string]$ModuleName, [string]$ModuleDir)

    $source = Join-Path $RepoRoot 'BrigadeTool.CanDevices\zlgcan'
    $target = Join-Path $ModuleDir 'zlgcan'
    if (-not (Test-Path -LiteralPath $source)) {
        Write-Log "[$ModuleName] 缺少 ZLG 原生目录：$source（构建不会自动复制该目录）" 'WARN'
        Add-DependencyRow $ModuleName 'ZLG zlgcan 原生目录' 'MISSING' $source $target
        return
    }
    Copy-DirectoryTree -Source $source -Target $target
    Write-Log "[$ModuleName] 已复制 zlgcan 原生目录 -> publish\$ModuleName\zlgcan" 'OK'
    Add-DependencyRow $ModuleName 'ZLG zlgcan 原生目录' 'INCLUDED' $source $target
}

function Publish-ExternalDependency {
    param(
        [string]$ModuleName,
        [string]$ModuleDir,
        [string]$Dependency,
        [object]$Config
    )

    if (-not $CollectExternalDependencies) {
        Write-Log "[$ModuleName] 外部依赖收集已关闭，跳过：$Dependency" 'SKIP'
        Add-DependencyRow $ModuleName $Dependency 'SKIPPED'
        return
    }

    switch ($Dependency) {
        'kvaser' {
            $roots = @()
            $roots += @($Config.kvaserSearchPaths)
            $roots += @(
                (Join-Path $env:ProgramFiles 'Kvaser'),
                (Join-Path ${env:ProgramFiles(x86)} 'Kvaser'),
                $env:KVASER_HOME,
                (Join-Path $env:SystemRoot 'SysWOW64\canlib32.dll'),
                (Join-Path $env:SystemRoot 'System32\canlib32.dll')
            )
            $found = Find-KvaserCanLib32 -Roots $roots
            if ($found) {
                $target = Join-Path $ModuleDir 'canlib32.dll'
                Copy-Item -LiteralPath $found -Destination $target -Force
                Write-Log "[$ModuleName] 已收集 Kvaser canlib32.dll：$found" 'OK'
                Add-DependencyRow $ModuleName 'Kvaser canlib32.dll (x86)' 'FOUND' $found $target
            } else {
                Write-Log "[$ModuleName] 缺少 Kvaser 原生库 canlib32.dll（需要 x86 版本，未在候选路径中找到）" 'WARN'
                Add-DependencyRow $ModuleName 'Kvaser canlib32.dll (x86)' 'MISSING'
            }
        }
        'ffmpeg' {
            $roots = @()
            # 优先级：显式配置 -> 专用安装目录 -> VideoPlayer 自带目录 -> 仓库 tools -> PATH（PATH 在 Find-LeafFile 末尾兜底）
            $roots += @($Config.ffmpegSearchPaths)
            $roots += @(
                $env:FFMPEG_PATH,
                'C:\ffmpeg\bin',
                (Join-Path $env:ProgramFiles 'ffmpeg\bin'),
                (Join-Path ${env:ProgramFiles(x86)} 'ffmpeg\bin'),
                (Join-Path $env:LOCALAPPDATA 'ffmpeg\bin'),
                (Join-Path $RepoRoot 'Brigade.VideoPlayer\ffmpeg'),
                (Join-Path $RepoRoot 'Brigade.VideoPlayer\ffmpeg\bin'),
                (Join-Path $RepoRoot 'tools\ffmpeg\bin'),
                (Join-Path $RepoRoot 'tools\ffmpeg'),
                (Join-Path $RepoRoot 'ffmpeg\bin')
            )
            $targetDir = Join-Path $ModuleDir 'ffmpeg'
            foreach ($leaf in @('ffmpeg.exe', 'ffprobe.exe')) {
                $found = Find-LeafFile -Leaf $leaf -Roots $roots
                if ($found) {
                    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
                    $target = Join-Path $targetDir $leaf
                    Copy-Item -LiteralPath $found -Destination $target -Force
                    if (Test-IsIdeBundledFfmpeg $found) {
                        Write-Log "[$ModuleName] 警告：$leaf 来自 IDE 内置副本，不建议作为分发来源：$found。请配置 publish.config.json 的 ffmpegSearchPaths 或安装独立 FFmpeg。" 'WARN'
                        Add-DependencyRow $ModuleName "FFmpeg $leaf" 'FOUND-IDE' $found $target
                    } else {
                        Write-Log "[$ModuleName] 已收集 $leaf：$found" 'OK'
                        Add-DependencyRow $ModuleName "FFmpeg $leaf" 'FOUND' $found $target
                    }
                } else {
                    Write-Log "[$ModuleName] 缺少 $leaf（未在候选路径中找到；不影响发布，但视频功能不可用）" 'WARN'
                    Add-DependencyRow $ModuleName "FFmpeg $leaf" 'MISSING'
                }
            }
        }
        'hhc' {
            $roots = @()
            $roots += @($Config.hhcSearchPaths)
            $roots += @(
                $env:HTML_HELP_WORKSHOP,
                (Join-Path ${env:ProgramFiles(x86)} 'HTML Help Workshop'),
                (Join-Path $env:ProgramFiles 'HTML Help Workshop'),
                (Join-Path $RepoRoot 'tools\hhc'),
                (Join-Path $RepoRoot 'tools\HTML Help Workshop')
            )
            $found = Find-LeafFile -Leaf 'hhc.exe' -Roots $roots
            if ($found) {
                $sourceDir = Split-Path -Parent $found
                $targetDir = Join-Path $ModuleDir 'Tools'
                New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
                Get-ChildItem -LiteralPath $sourceDir -File | ForEach-Object {
                    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $targetDir $_.Name) -Force
                }
                Write-Log "[$ModuleName] 已收集 HTML Help Workshop：$sourceDir -> Tools\" 'OK'
                Add-DependencyRow $ModuleName 'HTML Help Workshop hhc.exe' 'FOUND' $found (Join-Path $targetDir 'hhc.exe')
            } else {
                Write-Log "[$ModuleName] 缺少 hhc.exe（未安装 HTML Help Workshop，候选路径均未命中；运行时无法编译 CHM）" 'WARN'
                Add-DependencyRow $ModuleName 'HTML Help Workshop hhc.exe' 'MISSING'
            }
        }
        default {
            Write-Log "未知外部依赖类型：$Dependency" 'WARN'
        }
    }
}

$ModuleDefinitions = [ordered]@{
    'Rxbit.BrigadeApp' = @{
        DisplayName    = '主程序'
        Project        = 'Rxbit.BrigadeApp\Rxbit.BrigadeApp.csproj'
        SolutionTarget = 'Rxbit_BrigadeApp'
        Kind           = 'ClassicX86'
        OutputPattern  = 'bin\x86\{0}'
        ExpectedFiles  = @('Rxbit.BrigadeApp.exe', 'Rxbit.BrigadeApp.exe.config', 'Assets\favico.ico')
        Requires       = @('kvaser', 'ffmpeg', 'hhc')
        NativeZlgcan   = $true
    }
    'Rxbit.Brigade.Controll' = @{
        DisplayName    = 'Controll'
        Project        = 'Rxbit.Brigade.Controll\Rxbit.Brigade.Controll.csproj'
        SolutionTarget = 'Rxbit_Brigade_Controll'
        Kind           = 'SdkNet48X86'
        OutputPattern  = 'bin\x86\{0}\net48'
        ExpectedFiles  = @('Rxbit.Brigade.Controll.exe', 'Rxbit.Brigade.Controll.exe.config')
        Requires       = @()
        NativeZlgcan   = $false
    }
    'Rxbit.Brigade.HalconVision' = @{
        DisplayName    = 'HalconVision'
        Project        = 'Rxbit.Brigade.HalconVision\Rxbit.Brigade.HalconVision.csproj'
        SolutionTarget = 'Rxbit_Brigade_HalconVision'
        Kind           = 'SdkNet48X86'
        OutputPattern  = 'bin\x86\{0}\net48'
        ExpectedFiles  = @('Rxbit.Brigade.HalconVision.exe', 'Rxbit.Brigade.HalconVision.exe.config')
        Requires       = @()
        NativeZlgcan   = $false
    }
    'Rxbit.Brigade.ZcanPro' = @{
        DisplayName    = 'ZcanPro'
        Project        = 'Rxbit.Brigade.ZcanPro\Rxbit.Brigade.ZcanPro.csproj'
        SolutionTarget = 'Rxbit_Brigade_ZcanPro'
        Kind           = 'SdkNet48X86'
        OutputPattern  = 'bin\x86\{0}\net48'
        ExpectedFiles  = @('Rxbit.Brigade.ZcanPro.exe', 'Rxbit.Brigade.ZcanPro.exe.config')
        Requires       = @()
        NativeZlgcan   = $false
    }
    'Rxbit.RdpAppUI' = @{
        DisplayName    = 'RdpAppUI'
        Project        = 'Rxbit.RdpAppUI\Rxbit.RdpAppUI.csproj'
        SolutionTarget = 'Rxbit_RdpAppUI'
        Kind           = 'ClassicAnyCpu'
        OutputPattern  = 'bin\{0}'
        ExpectedFiles  = @('Rxbit.RdpAppUI.exe', 'Rxbit.RdpAppUI.exe.config')
        Requires       = @()
        NativeZlgcan   = $false
    }
    'Brigade.VideoPlayer' = @{
        DisplayName    = 'VideoPlayer'
        Project        = 'Brigade.VideoPlayer\Brigade.VideoPlayer.csproj'
        SolutionTarget = 'Brigade_VideoPlayer'
        Kind           = 'Net9'
        OutputPattern  = ''
        ExpectedFiles  = @('Brigade.VideoPlayer.exe')
        Requires       = @('ffmpeg')
        NativeZlgcan   = $false
    }
}

$ModuleAliases = @{
    'all'              = 'All'
    'app'              = 'Rxbit.BrigadeApp'
    'main'             = 'Rxbit.BrigadeApp'
    'rxbit.brigadeapp' = 'Rxbit.BrigadeApp'
    'controll'         = 'Rxbit.Brigade.Controll'
    'rxbit.brigade.controll' = 'Rxbit.Brigade.Controll'
    'halcon'           = 'Rxbit.Brigade.HalconVision'
    'halconvision'     = 'Rxbit.Brigade.HalconVision'
    'rxbit.brigade.halconvision' = 'Rxbit.Brigade.HalconVision'
    'zcan'             = 'Rxbit.Brigade.ZcanPro'
    'zcanpro'          = 'Rxbit.Brigade.ZcanPro'
    'rxbit.brigade.zcanpro' = 'Rxbit.Brigade.ZcanPro'
    'rdp'              = 'Rxbit.RdpAppUI'
    'rdpappui'         = 'Rxbit.RdpAppUI'
    'rxbit.rdpappui'   = 'Rxbit.RdpAppUI'
    'video'            = 'Brigade.VideoPlayer'
    'videoplayer'      = 'Brigade.VideoPlayer'
    'brigade.videoplayer' = 'Brigade.VideoPlayer'
    'installer'        = 'Rxbit.Brigade.RadarApp'
    'msi'              = 'Rxbit.Brigade.RadarApp'
    'radarapp'         = 'Rxbit.Brigade.RadarApp'
    'rxbit.brigade.radarapp' = 'Rxbit.Brigade.RadarApp'
}

function Resolve-Selection {
    param([string[]]$Requested)

    $resolved = @()
    foreach ($raw in $Requested) {
        foreach ($piece in ([string]$raw -split ',')) {
            $name = $piece.Trim()
            if ([string]::IsNullOrWhiteSpace($name)) { continue }
            $key = $name.ToLowerInvariant()
            if ($ModuleAliases.ContainsKey($key)) {
                $resolved += $ModuleAliases[$key]
            } elseif ($ModuleDefinitions.Contains($name)) {
                $resolved += $name
            } else {
                $valid = ($ModuleDefinitions.Keys + @('All', 'Installer')) -join ', '
                throw "未知模块：$name。可用模块：$valid"
            }
        }
    }
    if ($resolved.Count -eq 0) {
        $resolved = @('All')
    }
    if ($resolved -contains 'All') {
        return @($ModuleDefinitions.Keys)
    }
    return ($resolved | Sort-Object -Unique)
}

function Test-ShouldBuildInstaller {
    param([string[]]$Selection)
    return ($BuildInstaller -or ($Selection -contains 'Rxbit.Brigade.RadarApp'))
}

function Invoke-Restore {
    param([string[]]$Selection)

    if ($SkipRestore) {
        Write-Log '已指定 -SkipRestore，跳过 NuGet 还原。' 'SKIP'
        return
    }

    $sdkProjects = @()
    foreach ($name in $Selection) {
        if ($name -eq 'Rxbit.BrigadeApp' -or $name -eq 'Rxbit.RdpAppUI') { continue }
        $def = $ModuleDefinitions[$name]
        if ($def -and $def.Kind -ne 'ClassicX86' -and $def.Kind -ne 'ClassicAnyCpu' -and $def.Kind -ne 'Net9') {
            $sdkProjects += (Join-Path $RepoRoot $def.Project)
        }
    }
    if ($Selection -contains 'Brigade.VideoPlayer') {
        $sdkProjects += (Join-Path $RepoRoot $ModuleDefinitions['Brigade.VideoPlayer'].Project)
    }

    if ($sdkProjects.Count -gt 0) {
        if (-not $script:Dotnet) {
            Write-Log '未找到 dotnet.exe，跳过 SDK 工程还原（将直接尝试构建）。' 'WARN'
        } else {
            foreach ($project in ($sdkProjects | Sort-Object -Unique)) {
                $projectDir = Split-Path -Parent $project
                $assetsFile = Join-Path $projectDir 'obj\project.assets.json'
                if (Test-Path -LiteralPath $assetsFile) {
                    Write-Log "检测到 $([System.IO.Path]::GetFileName($project)) 的 project.assets.json，跳过还原。" 'INFO'
                    continue
                }
                Invoke-NativeTool -Exe $script:Dotnet -Arguments @('restore', $project, '--nologo', '-v:minimal') -Label "还原 $project"
            }
        }
    }

    $packageDir = Join-Path $RepoRoot 'Rxbit.BrigadeApp\packages'
    $keyPackage = Join-Path $packageDir 'CommunityToolkit.Mvvm.8.2.0'
    if (Test-Path -LiteralPath $keyPackage) {
        Write-Log '检测到 Rxbit.BrigadeApp\packages 已还原，跳过 packages.config 还原。' 'INFO'
        return
    }

    $nuget = Get-Command nuget.exe -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($nuget) {
        Invoke-NativeTool -Exe $nuget.Source -Arguments @('restore', $SolutionPath, '-NonInteractive', '-Verbosity', 'quiet') -Label 'NuGet 还原'
    } else {
        Write-Log '未找到 nuget.exe，且 Rxbit.BrigadeApp\packages 不完整；如果后续编译因缺少包失败，请先执行 NuGet 还原。' 'WARN'
    }
}

function Invoke-BuildNet48Module {
    param([string]$Name)

    $def = $ModuleDefinitions[$Name]
    $arguments = @(
        $SolutionPath,
        "/t:$($def.SolutionTarget):Rebuild",
        "/p:Configuration=$Configuration",
        '/p:Platform=x86',
        '/v:minimal',
        '/nologo',
        '/m',
        '/nr:false'
    )
    Invoke-NativeTool -Exe $script:MsBuild -Arguments $arguments -Label "编译 $Name"
}

function Publish-Net48Module {
    param([string]$Name)

    $def = $ModuleDefinitions[$Name]
    $projectDir = Join-Path $RepoRoot (Split-Path -Parent $def.Project)
    $outputPattern = $def.OutputPattern -f $Configuration
    $sourceDir = Join-Path $projectDir $outputPattern
    if (-not (Test-Path -LiteralPath $sourceDir)) {
        throw "编译输出目录不存在：$sourceDir（$Name 可能未成功编译）"
    }

    $moduleDir = Join-Path $PublishRoot $Name
    Copy-DirectoryContent -Source $sourceDir -Destination $moduleDir -ExcludeNames @('Logs')

    foreach ($relative in $def.ExpectedFiles) {
        $target = Join-Path $moduleDir $relative
        if (-not (Test-Path -LiteralPath $target)) {
            throw "发布产物缺少关键文件：publish\$Name\$relative"
        }
    }
    Write-Log "[$Name] 已发布到 publish\$Name" 'OK'
}

function Publish-VideoPlayer {
    $name = 'Brigade.VideoPlayer'
    $def = $ModuleDefinitions[$name]
    if (-not $script:Dotnet) {
        throw '找不到 dotnet.exe，无法发布 Brigade.VideoPlayer（需要 .NET 9 SDK）。'
    }

    $project = Join-Path $RepoRoot $def.Project
    $moduleDir = Join-Path $PublishRoot $name
    New-Item -ItemType Directory -Path $moduleDir -Force | Out-Null

    $arguments = @('publish', $project, '-c', $Configuration, '-o', $moduleDir, '--nologo', '-v:minimal')
    if ($VideoPlayerSelfContained) {
        $arguments += @('-r', 'win-x64', '--self-contained', 'true', '-p:PublishSingleFile=true')
        Write-Log "[$name] 发布模式：self-contained 单文件（win-x64）" 'INFO'
    } else {
        $arguments += @('--self-contained', 'false', '-p:PublishSingleFile=false')
        Write-Log "[$name] 发布模式：框架依赖（目标机需安装 .NET 9 Desktop Runtime）" 'INFO'
    }

    Invoke-NativeTool -Exe $script:Dotnet -Arguments $arguments -Label "发布 $name"

    foreach ($relative in $def.ExpectedFiles) {
        if (-not (Test-Path -LiteralPath (Join-Path $moduleDir $relative))) {
            throw "发布产物缺少关键文件：publish\$name\$relative"
        }
    }
    Write-Log "[$name] 已发布到 publish\$name" 'OK'
}

function Get-InstallerKey {
    param([string]$Seed)

    $sha = [System.Security.Cryptography.SHA1]::Create()
    try {
        $bytes = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Seed))
    } finally {
        $sha.Dispose()
    }
    $hex = -join ($bytes | ForEach-Object { $_.ToString('X2') })
    return '_' + $hex.Substring(0, 32)
}

function New-InstallerFolderBlock {
    param(
        [System.Collections.IDictionary]$Node,
        [int]$Indent
    )

    $pad = ' ' * $Indent
    $builder = New-Object System.Text.StringBuilder
    foreach ($childName in ($Node.Keys | Sort-Object)) {
        $child = $Node[$childName]
        [void]$builder.AppendLine($pad + '"{9EF0B969-E518-4E46-987F-47570745A589}:' + $child.Key + '"')
        [void]$builder.AppendLine($pad + '{')
        [void]$builder.AppendLine($pad + '"Name" = "8:' + $childName + '"')
        [void]$builder.AppendLine($pad + '"AlwaysCreate" = "11:FALSE"')
        [void]$builder.AppendLine($pad + '"Condition" = "8:"')
        [void]$builder.AppendLine($pad + '"Transitive" = "11:FALSE"')
        [void]$builder.AppendLine($pad + '"Property" = "8:' + $child.Key + '"')
        [void]$builder.AppendLine($pad + '    "Folders"')
        [void]$builder.AppendLine($pad + '    {')
        [void]$builder.Append((New-InstallerFolderBlock -Node $child.Children -Indent ($Indent + 4)))
        [void]$builder.AppendLine($pad + '    }')
        [void]$builder.AppendLine($pad + '}')
    }
    return $builder.ToString()
}

function New-InstallerFileBlock {
    param(
        [string]$Key,
        [string]$SourcePath,
        [string]$TargetName,
        [string]$FolderKey
    )

    $escaped = $SourcePath.Replace('\', '\\')
    $builder = New-Object System.Text.StringBuilder
    [void]$builder.AppendLine('            "{1FB2D0AE-D3B9-43D4-B9DD-F88EC61E35DE}:' + $Key + '"')
    [void]$builder.AppendLine('            {')
    [void]$builder.AppendLine('            "SourcePath" = "8:' + $escaped + '"')
    [void]$builder.AppendLine('            "TargetName" = "8:' + $TargetName + '"')
    [void]$builder.AppendLine('            "Tag" = "8:"')
    [void]$builder.AppendLine('            "Folder" = "8:' + $FolderKey + '"')
    [void]$builder.AppendLine('            "Condition" = "8:"')
    [void]$builder.AppendLine('            "Transitive" = "11:FALSE"')
    [void]$builder.AppendLine('            "Vital" = "11:TRUE"')
    [void]$builder.AppendLine('            "ReadOnly" = "11:FALSE"')
    [void]$builder.AppendLine('            "Hidden" = "11:FALSE"')
    [void]$builder.AppendLine('            "System" = "11:FALSE"')
    [void]$builder.AppendLine('            "Permanent" = "11:FALSE"')
    [void]$builder.AppendLine('            "SharedLegacy" = "11:FALSE"')
    [void]$builder.AppendLine('            "PackageAs" = "3:1"')
    [void]$builder.AppendLine('            "Register" = "3:1"')
    [void]$builder.AppendLine('            "Exclude" = "11:FALSE"')
    [void]$builder.AppendLine('            "IsDependency" = "11:FALSE"')
    [void]$builder.AppendLine('            "IsolateTo" = "8:"')
    [void]$builder.AppendLine('            }')
    return $builder.ToString()
}

function New-InstallerProject {
    param(
        [string]$MainModuleDir,
        [string]$InstallerDir,
        [string]$TemplatePath,
        [string]$ShortcutIconPath
    )

    $buildDir = Join-Path $InstallerDir '_build'
    New-Item -ItemType Directory -Path $buildDir -Force | Out-Null

    $rootKey = '_C13A612790D748648B9E62DD11A23D1B'
    $reservedFiles = @{
        'Rxbit.BrigadeApp.exe' = '_DBE62B88CA574FF789C36D77A26A1E4B'
    }
    $usedKeys = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($key in @($rootKey, '_DBE62B88CA574FF789C36D77A26A1E4B', '_EE50669F55314FFCA44DDBF44CF3B92F', '_5DA0120B74FE4FE8A114FE2E9EF4838E')) {
        [void]$usedKeys.Add($key)
    }

    $tree = @{ Key = $rootKey; Children = @{} }
    $folderKeys = @{ '' = $rootKey }
    $fileBuilder = New-Object System.Text.StringBuilder

    $files = Get-ChildItem -LiteralPath $MainModuleDir -Recurse -File | Sort-Object FullName
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($MainModuleDir.Length + 1)
        $segments = $relative -split '\\'

        $current = $tree
        $accumulated = ''
        for ($i = 0; $i -lt ($segments.Count - 1); $i++) {
            $segment = $segments[$i]
            $accumulated = if ($accumulated) { $accumulated + '\' + $segment } else { $segment }
            if (-not $current.Children.ContainsKey($segment)) {
                $key = Get-InstallerKey ('DIR|' + $accumulated)
                while (-not $usedKeys.Add($key)) {
                    $key = $key.Substring(0, 32) + 'A'
                }
                $current.Children[$segment] = @{ Key = $key; Children = @{} }
                $folderKeys[$accumulated] = $key
            }
            $current = $current.Children[$segment]
        }

        $dirPath = ''
        if ($segments.Count -gt 1) {
            $dirPath = ($segments[0..($segments.Count - 2)] -join '\')
        }
        $folderKey = if ($dirPath) { $folderKeys[$dirPath] } else { $rootKey }

        if ($reservedFiles.ContainsKey($relative)) {
            $fileKey = $reservedFiles[$relative]
        } else {
            $fileKey = Get-InstallerKey ('FILE|' + $relative)
            while (-not $usedKeys.Add($fileKey)) {
                $fileKey = $fileKey.Substring(0, 32) + 'A'
            }
        }

        [void]$fileBuilder.Append((New-InstallerFileBlock -Key $fileKey -SourcePath $file.FullName -TargetName $file.Name -FolderKey $folderKey))
    }

    $windowsDir = [Environment]::GetFolderPath('Windows')
    $msiexecPath = Join-Path $windowsDir 'System32\msiexec.exe'
    [void]$fileBuilder.Append((New-InstallerFileBlock -Key '_EE50669F55314FFCA44DDBF44CF3B92F' -SourcePath $msiexecPath -TargetName 'msiexec.exe' -FolderKey $rootKey))
    [void]$fileBuilder.Append((New-InstallerFileBlock -Key '_5DA0120B74FE4FE8A114FE2E9EF4838E' -SourcePath $ShortcutIconPath -TargetName 'LOGO.ico' -FolderKey $rootKey))

    $folderText = New-InstallerFolderBlock -Node $tree.Children -Indent 20

    $template = [System.IO.File]::ReadAllText($TemplatePath, [System.Text.Encoding]::UTF8)
    $lines = $template -split "`r?`n"

    $hierarchyIndex = [Array]::IndexOf($lines, '    "Hierarchy"')
    $configIndex = [Array]::IndexOf($lines, '    "Configurations"')
    $fileIndex = [Array]::IndexOf($lines, '        "File"')
    $fileTypeIndex = [Array]::IndexOf($lines, '        "FileType"')
    $folderIndex = [Array]::IndexOf($lines, '        "Folder"')
    $launchIndex = [Array]::IndexOf($lines, '        "LaunchCondition"')
    $projectOutputIndex = [Array]::IndexOf($lines, '        "ProjectOutput"')
    foreach ($pair in @(
            @('Hierarchy', $hierarchyIndex, $configIndex),
            @('File', $fileIndex, $fileTypeIndex),
            @('Folder', $folderIndex, $launchIndex),
            @('ProjectOutput', $projectOutputIndex, $lines.Count))) {
        if ($pair[1] -lt 0 -or $pair[2] -lt 0 -or $pair[1] -ge $pair[2]) {
            throw "无法在 vdproj 模板中定位 $($pair[0]) 区块，请检查 $TemplatePath 的格式。"
        }
    }

    $folderLines = $lines[$folderIndex..($launchIndex - 1)]
    $targetIndex = [Array]::IndexOf($folderLines, '            "Property" = "8:TARGETDIR"')
    $foldersIndex = -1
    if ($targetIndex -ge 0) {
        for ($i = $targetIndex + 1; $i -lt $folderLines.Count; $i++) {
            if ($folderLines[$i].Trim() -eq '"Folders"') {
                $foldersIndex = $i
                break
            }
        }
    }
    if ($foldersIndex -lt 0 -or ($foldersIndex + 1) -ge $folderLines.Count) {
        throw "无法在 vdproj 模板中定位应用安装目录（TARGETDIR）的文件夹块，请检查 $TemplatePath 的格式。"
    }

    $newLines = New-Object System.Collections.Generic.List[string]
    for ($i = 0; $i -lt $hierarchyIndex; $i++) { $newLines.Add($lines[$i]) }
    $newLines.Add('    "Hierarchy"')
    $newLines.Add('    {')
    $newLines.Add('    }')

    for ($i = $configIndex; $i -lt $fileIndex; $i++) {
        $line = $lines[$i]
        if ($line.TrimStart() -eq '"Enabled" = "11:TRUE"') {
            $line = $line.Replace('11:TRUE', '11:FALSE')
        }
        $newLines.Add($line)
    }

    $newLines.Add('        "File"')
    $newLines.Add('        {')
    foreach ($line in ($fileBuilder.ToString() -split "`r?`n")) {
        if ($line) { $newLines.Add($line) }
    }
    $newLines.Add('        }')

    for ($i = $fileTypeIndex; $i -lt $folderIndex; $i++) { $newLines.Add($lines[$i]) }

    for ($i = 0; $i -lt $folderLines.Count; $i++) {
        $newLines.Add($folderLines[$i])
        if ($i -eq ($foldersIndex + 1)) {
            foreach ($line in ($folderText -split "`r?`n")) {
                if ($line) { $newLines.Add($line) }
            }
        }
    }

    for ($i = $launchIndex; $i -lt $projectOutputIndex; $i++) { $newLines.Add($lines[$i]) }
    $newLines.Add('    }')
    $newLines.Add('}')

    $generated = Join-Path $buildDir 'Rxbit.Brigade.RadarApp.generated.vdproj'
    $encoding = New-Object System.Text.UTF8Encoding($true)
    [System.IO.File]::WriteAllText($generated, ($newLines -join "`r`n") + "`r`n", $encoding)
    return $generated
}

function Invoke-BuildInstaller {
    $name = 'Rxbit.Brigade.RadarApp'
    $installerDll = Join-Path $script:VsRoot 'Common7\IDE\CommonExtensions\Microsoft\VSI\bin\Microsoft.VisualStudio.InstallerProjects.dll'

    Write-Log '开始：生成 RadarApp MSI' 'INFO'
    if (-not (Test-Path -LiteralPath $installerDll)) {
        Write-Log '未检测到 Visual Studio Installer Projects 扩展，跳过 MSI 生成（不影响应用发布）。请安装扩展后重试：Visual Studio Installer -> 修改 -> 单个组件 -> Microsoft Visual Studio Installer Projects。' 'WARN'
        Add-DependencyRow $name 'Installer Projects 扩展' 'MISSING'
        return
    }

    $devenv = Join-Path $script:VsRoot 'Common7\IDE\devenv.com'
    if (-not (Test-Path -LiteralPath $devenv)) {
        Write-Log "未找到 $devenv，跳过 MSI 生成（不影响应用发布）。" 'WARN'
        Add-DependencyRow $name 'devenv.com' 'MISSING'
        return
    }

    $shortcutIcon = Join-Path $RepoRoot 'assets\common\LOGO.ico'
    if (-not (Test-Path -LiteralPath $shortcutIcon)) {
        Write-Log "缺少快捷方式图标 $shortcutIcon，跳过 MSI。" 'WARN'
        Add-DependencyRow $name 'assets\common\LOGO.ico（快捷方式图标）' 'MISSING'
        return
    }

    $mainModuleDir = Join-Path $PublishRoot 'Rxbit.BrigadeApp'
    $mainExe = Join-Path $mainModuleDir 'Rxbit.BrigadeApp.exe'
    if (-not (Test-Path -LiteralPath $mainExe)) {
        Write-Log "主程序产物不存在：$mainExe，跳过 MSI。请先发布 Rxbit.BrigadeApp（使用 -BuildInstaller 时会自动加入该模块）。" 'WARN'
        Add-DependencyRow $name '主程序产物 Rxbit.BrigadeApp.exe' 'MISSING'
        return
    }
    if (-not (Test-IsX86PeFile $mainExe)) {
        Write-Log "主程序不是 x86 PE 文件：$mainExe，跳过 MSI（安装包必须保持 x86）。" 'WARN'
        Add-DependencyRow $name '主程序 x86 检查' 'FAILED'
        return
    }

    $requiredDirs = @('zlgcan', 'cxkjcan', 'gcan', 'gcanfd', 'peakcan', 'RadarEcu', 'sub-exe', 'Tools', 'ffmpeg', 'Assets')
    $installedDirs = @()
    foreach ($dirName in $requiredDirs) {
        $dirPath = Join-Path $mainModuleDir $dirName
        if (Test-Path -LiteralPath $dirPath -PathType Container) {
            $dirFiles = @(Get-ChildItem -LiteralPath $dirPath -Recurse -File -ErrorAction SilentlyContinue)
            if ($dirFiles.Count -gt 0) {
                $installedDirs += $dirName
                Add-DependencyRow $name ("安装内容目录 $dirName（{0} 个文件）" -f $dirFiles.Count) 'INCLUDED' $dirPath
            } else {
                Write-Log "MSI 安装内容目录为空：publish\Rxbit.BrigadeApp\$dirName" 'WARN'
                Add-DependencyRow $name "安装内容目录 $dirName（空目录）" 'MISSING'
            }
        } else {
            Write-Log "MSI 安装内容缺少目录：publish\Rxbit.BrigadeApp\$dirName" 'WARN'
            Add-DependencyRow $name "安装内容目录 $dirName" 'MISSING'
        }
    }

    $favico = Join-Path $mainModuleDir 'Assets\favico.ico'
    if (Test-Path -LiteralPath $favico) {
        Add-DependencyRow $name 'Assets\favico.ico' 'INCLUDED' $favico
    } else {
        Write-Log 'MSI 安装内容缺少 Assets\favico.ico（主程序 favico 图标随安装内容保留）。' 'WARN'
        Add-DependencyRow $name 'Assets\favico.ico' 'MISSING'
    }

    $installerDir = Join-Path $PublishRoot $name
    New-Item -ItemType Directory -Path $installerDir -Force | Out-Null
    $templatePath = Join-Path $RepoRoot 'Rxbit.Brigade.RadarApp\Rxbit.Brigade.RadarApp.vdproj'
    $vdproj = New-InstallerProject -MainModuleDir $mainModuleDir -InstallerDir $installerDir -TemplatePath $templatePath -ShortcutIconPath $shortcutIcon
    Write-Log "已生成临时安装工程：$vdproj（内容取自 publish\Rxbit.BrigadeApp）" 'INFO'

    try {
        Invoke-NativeTool -Exe $devenv -Arguments @($vdproj, '/build', $Configuration) -Label '生成 RadarApp MSI'
    } catch {
        Write-Log "MSI 生成失败，已跳过（应用发布结果不受影响）：$($_.Exception.Message)" 'WARN'
        Add-DependencyRow $name 'RadarApp MSI' 'FAILED'
        return
    }

    $buildDir = Join-Path $installerDir '_build'
    $msiSource = Get-ChildItem -LiteralPath $buildDir -Filter '*.msi' -Recurse -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $msiSource) {
        Write-Log 'MSI 构建结束但未找到输出文件，请查看日志中的 Visual Studio 生成输出。' 'WARN'
        Add-DependencyRow $name 'RadarApp MSI' 'MISSING'
        return
    }

    $msiTarget = Join-Path $installerDir $msiSource.Name
    Copy-Item -LiteralPath $msiSource.FullName -Destination $msiTarget -Force
    Remove-Item -LiteralPath $buildDir -Recurse -Force -ErrorAction SilentlyContinue

    $msiFileCount = @(Get-ChildItem -LiteralPath $mainModuleDir -Recurse -File).Count
    Write-Log ("[$name] 已生成 MSI：publish\{0}\{1}（安装内容 {2} 个文件，目录：{3}）" -f $name, $msiSource.Name, $msiFileCount, ($installedDirs -join ', ')) 'OK'
    Add-DependencyRow $name ("RadarApp MSI（{0} 个文件）" -f $msiFileCount) 'INCLUDED' $mainModuleDir $msiTarget
}

function Clear-OldArtifacts {
    param(
        [string[]]$Selection,
        [bool]$IsAll
    )

    New-Item -ItemType Directory -Path $PublishRoot -Force | Out-Null

    try {
        if ($IsAll) {
            Write-Log "清理旧产物：$PublishRoot（保留 _logs 子目录）" 'INFO'
            Get-ChildItem -LiteralPath $PublishRoot -Force | Where-Object { $_.Name -ne '_logs' } | ForEach-Object {
                Remove-Item -LiteralPath $_.FullName -Recurse -Force
            }
        } else {
            foreach ($name in $Selection) {
                $target = Join-Path $PublishRoot $name
                if (Test-Path -LiteralPath $target) {
                    Write-Log "清理旧产物：publish\$name" 'INFO'
                    Remove-Item -LiteralPath $target -Recurse -Force
                }
            }
        }
    } catch {
        throw "清理旧产物失败，目标文件可能正在运行或被占用。请关闭由 publish\ 目录启动的程序后重试。原始错误：$($_.Exception.Message)"
    }
}

function Write-PublishReport {
    param([string[]]$Selection)

    $reportPath = Join-Path $PublishRoot 'BUILD-REPORT.txt'
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('统一发布报告')
    $lines.Add('================================================================')
    $lines.Add("生成时间 : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    $lines.Add("配置     : $Configuration")
    $lines.Add("发布根目录: $PublishRoot")
    $lines.Add("日志     : $script:LogFile")
    $lines.Add('')

    $lines.Add('模块产物')
    $lines.Add('----------------------------------------------------------------')
    foreach ($name in $Selection) {
        $dir = Join-Path $PublishRoot $name
        if (Test-Path -LiteralPath $dir) {
            $files = Get-ChildItem -LiteralPath $dir -Recurse -File -ErrorAction SilentlyContinue
            $sizeMb = [math]::Round((($files | Measure-Object -Property Length -Sum).Sum) / 1MB, 2)
            $nativeDirs = Get-ChildItem -LiteralPath $dir -Directory -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -in @('zlgcan', 'peakcan', 'gcan', 'gcanfd', 'cxkjcan', 'RadarEcu', 'sub-exe', 'ffmpeg', 'Tools', 'Assets') } |
                Select-Object -ExpandProperty Name
            $lines.Add(("- {0}  |  文件 {1} 个  |  {2} MB  |  publish\{0}" -f $name, $files.Count, $sizeMb))
            if ($nativeDirs) {
                $lines.Add(("    附加目录: {0}" -f ($nativeDirs -join ', ')))
            }
        } else {
            $lines.Add(("- {0}  |  未生成" -f $name))
        }
    }

    $lines.Add('')
    $lines.Add('依赖探测结果')
    $lines.Add('----------------------------------------------------------------')
    if ($script:DependencyRows.Count -eq 0) {
        $lines.Add('本次未执行依赖探测。')
    } else {
        foreach ($row in $script:DependencyRows) {
            $sourceText = if ($row.Source) { "  来源: $($row.Source)" } else { '' }
            $lines.Add(("- [{0}] {1}: {2}{3}" -f $row.Status, $row.Module, $row.Dependency, $sourceText))
        }
    }

    $lines.Add('')
    $lines.Add('已知限制')
    $lines.Add('----------------------------------------------------------------')
    $lines.Add('- 主程序必须保持 x86：所有 CAN 原生 DLL 为 32 位。')
    $lines.Add('- zlgcan 原生目录由本脚本复制；其它 CAN 原生目录随构建输出。')
    $lines.Add('- Kvaser canlib32.dll、FFmpeg、hhc.exe 缺失时仅告警，目标机需另行安装。')
    $lines.Add('- FFmpeg 正式来源为 publish.config.json 中配置的 D:\my-software\ffmpeg；配置不可用时才回退其它候选路径。')
    $lines.Add('- RadarApp MSI 由 publish_all.ps1 依据 publish\Rxbit.BrigadeApp 的实际产物生成，内容与该发布目录一致。')
    $lines.Add('- 安装包不再重复打入仓库根 assets 目录；主程序输出中的 Assets\favico.ico 随安装内容保留，快捷方式图标仅取 assets\common\LOGO.ico。')

    $lines | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Log "发布报告：$reportPath" 'OK'
}

function Invoke-Publish {
    Write-Log "参数：Configuration=$Configuration; Module=$($Module -join ','); CollectExternalDependencies=$CollectExternalDependencies; VideoPlayerSelfContained=$VideoPlayerSelfContained; BuildInstaller=$BuildInstaller" 'INFO'

    Resolve-BuildTools
    $config = Read-PublishConfig

    $isAll = $false
    foreach ($raw in $Module) {
        foreach ($piece in ([string]$raw -split ',')) {
            if ($piece.Trim() -ieq 'All') { $isAll = $true }
        }
    }
    if (@($Module).Count -eq 0) { $isAll = $true }
    $selection = Resolve-Selection -Requested $Module
    $needInstaller = Test-ShouldBuildInstaller -Selection $selection
    if ($selection -contains 'Rxbit.Brigade.RadarApp') {
        $selection = @($selection | Where-Object { $_ -ne 'Rxbit.Brigade.RadarApp' })
    }

    if ($needInstaller -and ($selection -notcontains 'Rxbit.BrigadeApp')) {
        Write-Log '生成 MSI 需要主程序的最新产物，已自动加入 Rxbit.BrigadeApp。' 'INFO'
        $selection = @('Rxbit.BrigadeApp') + @($selection)
    }

    if (@($selection).Count -eq 0 -and -not $needInstaller) {
        throw '没有可发布的模块。'
    }

    Write-Log ("本次模块：{0}" -f (($selection -join ', '))) 'INFO'
    if ($needInstaller) {
        Write-Log '已启用 RadarApp MSI 生成。' 'INFO'
    }

    Write-Log "发布配置：$Configuration" 'INFO'

    $clearSelection = @($selection)
    if ($needInstaller) { $clearSelection += 'Rxbit.Brigade.RadarApp' }
    Clear-OldArtifacts -Selection $clearSelection -IsAll $isAll

    Invoke-Restore -Selection $selection

    foreach ($name in $selection) {
        $def = $ModuleDefinitions[$name]
        Write-Log "===== 开始模块：$name （$($def.DisplayName)）=====" 'INFO'

        if ($def.Kind -eq 'Net9') {
            Publish-VideoPlayer
        } else {
            Invoke-BuildNet48Module -Name $name
            Publish-Net48Module -Name $name
        }

        $moduleDir = Join-Path $PublishRoot $name
        if ($def.NativeZlgcan) {
            Publish-ZlgcanNative -ModuleName $name -ModuleDir $moduleDir
        }
        foreach ($dependency in $def.Requires) {
            Publish-ExternalDependency -ModuleName $name -ModuleDir $moduleDir -Dependency $dependency -Config $config
        }
        $script:ReportedModules += $name
    }

    if ($needInstaller) {
        Invoke-BuildInstaller
    }

    $reportSelection = @($script:ReportedModules)
    if ($needInstaller) { $reportSelection += 'Rxbit.Brigade.RadarApp' }
    Write-PublishReport -Selection $reportSelection

    if ($script:MissingDependencies.Count -gt 0) {
        Write-Log ("外部依赖缺失（仅告警，不中断）：{0}" -f ($script:MissingDependencies -join '; ')) 'WARN'
    }

    Write-Log '全部发布流程完成。' 'OK'
    Write-Host ''
    Write-Host '发布产物：' -ForegroundColor Cyan
    foreach ($name in $script:ReportedModules) {
        $dir = Join-Path $PublishRoot $name
        if (Test-Path -LiteralPath $dir) {
            Write-Host "  $dir" -ForegroundColor Cyan
        }
    }
    if ($needInstaller) {
        $installerDir = Join-Path $PublishRoot 'Rxbit.Brigade.RadarApp'
        if (Test-Path -LiteralPath $installerDir) {
            Write-Host "  $installerDir" -ForegroundColor Cyan
        }
    }
    Write-Host "报告：$(Join-Path $PublishRoot 'BUILD-REPORT.txt')" -ForegroundColor Cyan
}

try {
    New-Item -ItemType Directory -Path $PublishRoot -Force | Out-Null
    New-LogFile
    Write-Log "脚本入口：$($MyInvocation.MyCommand.Path)" 'INFO'
    Invoke-Publish
    exit 0
} catch {
    if ($script:LogFile) {
        Write-Log ("发布失败：{0}" -f $_.Exception.Message) 'ERROR'
        Write-Log "日志文件：$script:LogFile" 'ERROR'
    } else {
        Write-Host ("发布失败：{0}" -f $_.Exception.Message) -ForegroundColor Red
    }
    exit 1
}
