# ShopWindows Module UI Test Script
# Launches app -> screenshot dashboard -> click ShopWindows -> switch sub-views and screenshot each
param(
    [string]$AppPath = 'd:\IDE-Workspace\Rxbit.BrigadeApp\Rxbit.Brigade.Controll\publish2\Rxbit.Brigade.Controll.exe',
    [string]$ShotDir = 'd:\IDE-Workspace\Rxbit.BrigadeApp\Rxbit.Brigade.Controll\screenshots'
)

# Load assemblies
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

# Mouse click API
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class MouseHelper {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, IntPtr dwExtraInfo);
    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const uint MOUSEEVENTF_LEFTUP = 0x0004;
    public static void Click(int x, int y) {
        SetCursorPos(x, y);
        System.Threading.Thread.Sleep(150);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
    }
}
"@

# Window foreground API
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class WinHelper {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
    public const int SW_RESTORE = 9;
}
"@

# Create screenshot directory
if (-not (Test-Path $ShotDir)) { New-Item -ItemType Directory -Path $ShotDir -Force | Out-Null }
Get-ChildItem $ShotDir -Filter '*.png' -ErrorAction SilentlyContinue | Remove-Item -Force

# Screenshot function: capture primary screen
function Take-Screenshot($name) {
    $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($bounds.X, $bounds.Y, 0, 0, $bounds.Size)
    $path = Join-Path $ShotDir ($name + '.png')
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose()
    $bmp.Dispose()
    Write-Output ('  [Shot] ' + $path)
    return $path
}

# Find main window process (with window title)
function Find-MainWindow {
    $procs = Get-Process 'Rxbit.Brigade.Controll' -ErrorAction SilentlyContinue
    $found = $null
    foreach ($p in $procs) {
        if ($p.MainWindowHandle -ne [IntPtr]::Zero -and $p.MainWindowTitle -like '*Brigade*') {
            if (-not $found -or $p.StartTime -gt $found.StartTime) { $found = $p }
        }
    }
    return $found
}

# Find element by name via UI Automation
function Find-ElementByName($root, $name) {
    $cond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $name)
    return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $cond)
}

function Find-ElementsByName($root, $name) {
    $cond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $name)
    return $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $cond)
}

# Click element (InvokePattern first, fallback to physical click)
function Click-Element($element) {
    if (-not $element) { return $false }
    $pattern = $null
    if ($element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
        $pattern.Invoke()
        Write-Output '  [Click] InvokePattern OK'
        return $true
    }
    $rect = $element.Current.BoundingRectangle
    if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
        $x = [int]($rect.X + $rect.Width / 2)
        $y = [int]($rect.Y + $rect.Height / 2)
        [MouseHelper]::Click($x, $y)
        Write-Output ('  [Click] physical at (' + $x + ', ' + $y + ')')
        return $true
    }
    return $false
}

# ============================================================
# Main test flow
# ============================================================
Write-Output '=== ShopWindows Module UI Test ==='
Write-Output ''

# Step 0: Launch app
Write-Output '[Step 0] Launching app...'
$proc = Find-MainWindow
if (-not $proc) {
    Start-Process $AppPath
    Start-Sleep -Seconds 5
    $proc = Find-MainWindow
}
if (-not $proc) {
    Write-Output 'ERROR: App window not found!'
    exit 1
}
Write-Output ('  PID: ' + $proc.Id + ', Title: ' + $proc.MainWindowTitle)

# Foreground window
[WinHelper]::ShowWindow($proc.MainWindowHandle, [WinHelper]::SW_RESTORE) | Out-Null
Start-Sleep -Milliseconds 300
[WinHelper]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
Start-Sleep -Seconds 1

# Step 1: Screenshot dashboard
Write-Output ''
Write-Output '[Step 1] Screenshot Dashboard...'
Take-Screenshot '01_dashboard' | Out-Null

# Step 2: Find and click ShopWindows nav button
Write-Output ''
Write-Output '[Step 2] Find and click ShopWindows button...'
$root = [System.Windows.Automation.AutomationElement]::FromHandle($proc.MainWindowHandle)

# Find element containing ShopWindows text
$shopTexts = Find-ElementsByName $root 'ShopWindows'
Write-Output ('  Found ShopWindows elements: ' + $shopTexts.Count)

$clicked = $false
if ($shopTexts.Count -gt 0) {
    $txt = $shopTexts[0]
    # Walk up to find Button ancestor
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
    $current = $txt
    for ($i = 0; $i -lt 6; $i++) {
        $parent = $walker.GetParent($current)
        if (-not $parent -or $parent -eq [System.Windows.Automation.AutomationElement]::NotSupportedElement) { break }
        $tp = $parent.Current.ControlType.ProgrammaticName
        Write-Output ('  Ancestor[' + $i + ']: ' + $tp)
        if ($tp -eq 'ControlType.Button') {
            $current = $parent
            break
        }
        $current = $parent
    }
    $clicked = Click-Element $current
}

if (-not $clicked) {
    Write-Output '  Fallback: click by coordinate...'
    $rect = New-Object WinHelper+RECT
    [WinHelper]::GetWindowRect($proc.MainWindowHandle, [ref]$rect) | Out-Null
    $x = 110
    $y = [int]($rect.Top + 560)
    [MouseHelper]::Click($x, $y)
    Write-Output ('  [Click] coordinate at (' + $x + ', ' + $y + ')')
}
Start-Sleep -Seconds 2

# Step 3: Screenshot Shop main view (default Shop1View)
Write-Output ''
Write-Output '[Step 3] Screenshot Shop default view...'
Take-Screenshot '02_shop_default' | Out-Null

# Step 4: Find Shop sidebar menu items and click each
Write-Output ''
Write-Output '[Step 4] Testing Shop sidebar menu switching...'

# Menu labels from ShopShellViewModel
$menuLabels = @('tui-jian:Shop1', 'wu-wei-qi-yue:Shop2', 'you-xi:Shop1', 'wei-xin-xiao-you-xi:Shop1', 'fen-lei:Shop1', 'dong-tai-bi-zhi:Shop5', 'ban-gong:Shop4', 'sheng-ji:Shop1', 'wo-de:Shop3')

# Chinese labels (built via Unicode to avoid encoding issues)
$cnLabels = @(
    "$([char]0x63A8)$([char]0x8350)",
    "$([char]0x65E0)$([char]0x754F)$([char]0x5951)$([char]0x7EA6)",
    "$([char]0x6E38)$([char]0x620F)",
    "$([char]0x5FAE)$([char]0x4FE1)$([char]0x5C0F)$([char]0x6E38)$([char]0x620F)",
    "$([char]0x5206)$([char]0x7C7B)",
    "$([char]0x52A8)$([char]0x6001)$([char]0x58C1)$([char]0x7EB8)",
    "$([char]0x529E)$([char]0x516C)",
    "$([char]0x5347)$([char]0x7EA7)",
    "$([char]0x6211)$([char]0x7684)"
)
$expectedViews = @('Shop1', 'Shop2', 'Shop1', 'Shop1', 'Shop1', 'Shop5', 'Shop4', 'Shop1', 'Shop3')
$shotIndex = 3

for ($mi = 0; $mi -lt $cnLabels.Length; $mi++) {
    $label = $cnLabels[$mi]
    $expected = $expectedViews[$mi]
    Write-Output ''
    Write-Output ('[Step ' + $shotIndex + '] Click menu item -> ' + $expected)
    $proc2 = Find-MainWindow
    if (-not $proc2) { Write-Output '  WARN: window lost'; continue }
    [WinHelper]::SetForegroundWindow($proc2.MainWindowHandle) | Out-Null
    Start-Sleep -Milliseconds 500
    $root2 = [System.Windows.Automation.AutomationElement]::FromHandle($proc2.MainWindowHandle)
    $elements = Find-ElementsByName $root2 $label
    Write-Output ('  Found elements: ' + $elements.Count)
    if ($elements.Count -gt 0) {
        $ok = Click-Element $elements[0]
        if ($ok) {
            Start-Sleep -Seconds 2
            $shotName = ('{0:00}_{1}' -f $shotIndex, $expected)
            Take-Screenshot $shotName | Out-Null
        }
    } else {
        Write-Output '  WARN: menu item not found, trying physical click by position...'
        $shotName = ('{0:00}_{1}_MISS' -f $shotIndex, $expected)
        Take-Screenshot $shotName | Out-Null
    }
    $shotIndex++
}

# Done
Write-Output ''
Write-Output '=== Test Complete ==='
Write-Output ('Screenshots saved to: ' + $ShotDir)
$files = Get-ChildItem $ShotDir -Filter '*.png'
foreach ($f in $files) {
    $kb = [math]::Round($f.Length / 1024, 1)
    Write-Output ('  ' + $f.Name + ' (' + $kb + ' KB)')
}
