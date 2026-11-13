# ShopWindows View Verification Script
# Verifies which view is displayed after each menu click by inspecting UI element names
param(
    [string]$AppPath = 'd:\IDE-Workspace\Rxbit.BrigadeApp\Rxbit.Brigade.Controll\publish2\Rxbit.Brigade.Controll.exe'
)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

Add-Type @"
using System;
using System.Runtime.InteropServices;
public class MouseHelper2 {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, IntPtr dwExtraInfo);
    public static void Click(int x, int y) {
        SetCursorPos(x, y);
        System.Threading.Thread.Sleep(150);
        mouse_event(0x0002, 0, 0, 0, IntPtr.Zero);
        mouse_event(0x0004, 0, 0, 0, IntPtr.Zero);
    }
}
"@

Add-Type @"
using System;
using System.Runtime.InteropServices;
public class WinHelper2 {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    public const int SW_RESTORE = 9;
}
"@

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

function Find-ElementsByName($root, $name) {
    $cond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $name)
    return $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $cond)
}

# Get ALL text elements in the window (for diagnostic)
function Get-AllTextElements($root) {
    $cond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Text)
    return $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $cond)
}

function Click-Element($element) {
    if (-not $element) { return $false }
    $pattern = $null
    if ($element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
        $pattern.Invoke()
        return $true
    }
    $rect = $element.Current.BoundingRectangle
    if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
        $x = [int]($rect.X + $rect.Width / 2)
        $y = [int]($rect.Y + $rect.Height / 2)
        [MouseHelper2]::Click($x, $y)
        return $true
    }
    return $false
}

# ============================================================
# Verification flow
# ============================================================
Write-Output '=== ShopWindows View Verification ==='
Write-Output ''

# Find or launch app
$proc = Find-MainWindow
if (-not $proc) {
    Write-Output 'Launching app...'
    Start-Process $AppPath
    Start-Sleep -Seconds 5
    $proc = Find-MainWindow
}
if (-not $proc) {
    Write-Output 'ERROR: App window not found!'
    exit 1
}
Write-Output ('PID: ' + $proc.Id)

[WinHelper2]::ShowWindow($proc.MainWindowHandle, [WinHelper2]::SW_RESTORE) | Out-Null
Start-Sleep -Milliseconds 300
[WinHelper2]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
Start-Sleep -Seconds 1

# Click ShopWindows nav button by coordinate
Write-Output 'Clicking ShopWindows nav button...'
$rect = $proc.MainWindowHandle
[MouseHelper2]::Click(110, 625)
Start-Sleep -Seconds 2

# Menu sequence: [label, description]
$menuSequence = @(
    @("$([char]0x63A8)$([char]0x8350)", 'Shop1-recommend'),
    @("$([char]0x65E0)$([char]0x754F)$([char]0x5951)$([char]0x7EA6)", 'Shop2-game-banner'),
    @("$([char]0x6211)$([char]0x7684)", 'Shop3-installed'),
    @("$([char]0x529E)$([char]0x516C)", 'Shop4-office'),
    @("$([char]0x52A8)$([char]0x6001)$([char]0x58C1)$([char]0x7EB8)", 'Shop5-wallpaper')
)

$idx = 1
foreach ($entry in $menuSequence) {
    $label = $entry[0]
    $desc = $entry[1]
    Write-Output ''
    Write-Output ('--- Test ' + $idx + ': ' + $desc + ' ---')

    # Foreground and get root
    $proc2 = Find-MainWindow
    if (-not $proc2) { Write-Output '  ERROR: window lost'; continue }
    [WinHelper2]::SetForegroundWindow($proc2.MainWindowHandle) | Out-Null
    Start-Sleep -Milliseconds 500
    $root = [System.Windows.Automation.AutomationElement]::FromHandle($proc2.MainWindowHandle)

    # Find and click the menu item
    $elements = Find-ElementsByName $root $label
    Write-Output ('  Menu item found: ' + $elements.Count)
    if ($elements.Count -eq 0) {
        Write-Output '  SKIP: menu not found'
        $idx++
        continue
    }
    $ok = Click-Element $elements[0]
    Write-Output ('  Click result: ' + $ok)
    Start-Sleep -Seconds 2

    # Now inspect content area - get all text elements
    $proc3 = Find-MainWindow
    if (-not $proc3) { Write-Output '  ERROR: window lost after click'; continue }
    [WinHelper2]::SetForegroundWindow($proc3.MainWindowHandle) | Out-Null
    Start-Sleep -Milliseconds 500
    $root3 = [System.Windows.Automation.AutomationElement]::FromHandle($proc3.MainWindowHandle)
    $allTexts = Get-AllTextElements $root3

    Write-Output ('  Text elements in window: ' + $allTexts.Count)

    # Filter: only show elements in the Shop content area (X > 430 to skip both sidebars)
    # Main sidebar = 220px, Shop sidebar = 200px, so content starts at ~430
    $contentTexts = @()
    foreach ($t in $allTexts) {
        $name = $t.Current.Name
        $r = $t.Current.BoundingRectangle
        if ($name -and $name.Length -gt 0 -and $r.X -gt 430) {
            $contentTexts += $name
        }
    }
    Write-Output ('  Shop content area texts: ' + $contentTexts.Count)
    # Show unique texts from content area
    $shown = 0
    $seen = @{}
    foreach ($name in $contentTexts) {
        if (-not $seen.ContainsKey($name)) {
            $seen[$name] = $true
            Write-Output ('    - ' + $name)
            $shown++
            if ($shown -ge 30) { break }
        }
    }
    $idx++
}

Write-Output ''
Write-Output '=== Verification Complete ==='
