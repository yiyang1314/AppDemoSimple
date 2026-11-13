Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

function Get-DescendantTexts($element, $maxDepth = 8) {
    $result = New-Object System.Collections.ArrayList
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
    $queue = New-Object System.Collections.Queue
    $queue.Enqueue(@{ El = $element; Depth = 0 })
    while ($queue.Count -gt 0) {
        $item = $queue.Dequeue()
        $el = $item.El
        $depth = $item.Depth
        if ($depth -gt $maxDepth) { continue }
        $name = $el.Current.Name
        if ($name -and $name -ne '') {
            $indent = '  ' * $depth
            [void]$result.Add($indent + $name)
        }
        $child = $walker.GetFirstChild($el)
        while ($child) {
            $queue.Enqueue(@{ El = $child; Depth = $depth + 1 })
            $child = $walker.GetNextSibling($child)
        }
    }
    return $result
}

function Find-ButtonAncestor($element) {
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
    $current = $element
    $tries = 0
    while ($current -and $tries -lt 10) {
        $ct = $current.Current.ControlType
        if ($ct -eq [System.Windows.Automation.ControlType]::Button) { return $current }
        if ($ct -eq [System.Windows.Automation.ControlType]::Window) { return $null }
        $current = $walker.GetParent($current)
        $tries++
    }
    return $null
}

function Click-Element($element) {
    $btn = Find-ButtonAncestor $element
    $target = $btn
    if (-not $target) { $target = $element }
    $pattern = $null
    if ($target.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
        $pattern.Invoke()
        return $true
    }
    # Fallback: physical click at element center
    $rect = $target.Current.BoundingRectangle
    if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
        $x = [int]($rect.X + $rect.Width / 2)
        $y = [int]($rect.Y + $rect.Height / 2)
        [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point($x, $y)
        Start-Sleep -Milliseconds 200
        Add-Type @"
using System;
using System.Runtime.InteropServices;
public class MouseClick {
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, IntPtr dwExtraInfo);
}
"@
        [MouseClick]::mouse_event(0x0002, 0, 0, 0, [IntPtr]::Zero)
        [MouseClick]::mouse_event(0x0004, 0, 0, 0, [IntPtr]::Zero)
        return $true
    }
    return $false
}

# Ensure app is running
$proc = Get-Process 'Rxbit.Brigade.Controll' -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Sort-Object StartTime -Descending | Select-Object -First 1
if (-not $proc) {
    Start-Process 'd:\IDE-Workspace\Rxbit.BrigadeApp\Rxbit.Brigade.Controll\publish2\Rxbit.Brigade.Controll.exe'
    Start-Sleep -Seconds 5
    $proc = Get-Process 'Rxbit.Brigade.Controll' -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Sort-Object StartTime -Descending | Select-Object -First 1
}
if (-not $proc) { Write-Output 'FAIL: No window'; exit 1 }

$window = [System.Windows.Automation.AutomationElement]::FromHandle($proc.MainWindowHandle)
Write-Output ('PID: ' + $proc.Id)

# Step 1: Click ShopWindows nav button
Write-Output '=== Click ShopWindows ==='
$allElements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
$shopBtn = $null
foreach ($b in $allElements) {
    if ($b.Current.Name -like '*Shop*') { $shopBtn = $b; break }
}
if ($shopBtn) {
    $btn = Find-ButtonAncestor $shopBtn
    if ($btn) {
        $p = $null
        if ($btn.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$p)) {
            $p.Invoke()
            Start-Sleep -Milliseconds 2000
            Write-Output 'ShopWindows clicked OK'
        }
    }
}

# Step 2: Test switching between Shop views by clicking menu items
$menuItems = @('推荐', '无畏契约', '我的', '办公', '动态壁纸')
foreach ($label in $menuItems) {
    Write-Output ('--- Testing: ' + $label + ' ---')
    $target = $null
    $allElements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($b in $allElements) {
        if ($b.Current.Name -eq $label) { $target = $b; break }
    }
    if ($target) {
        $rect = $target.Current.BoundingRectangle
        if ($rect.Width -gt 0) {
            $x = [int]($rect.X + $rect.Width / 2)
            $y = [int]($rect.Y + $rect.Height / 2)
            [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point($x, $y)
            Start-Sleep -Milliseconds 300
            Add-Type @"
using System;
using System.Runtime.InteropServices;
public class MouseClickHelper {
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, IntPtr dwExtraInfo);
}
"@
            [MouseClickHelper]::mouse_event(0x0002, 0, 0, 0, [IntPtr]::Zero)
            [MouseClickHelper]::mouse_event(0x0004, 0, 0, 0, [IntPtr]::Zero)
            Start-Sleep -Milliseconds 1500
            Write-Output ('  Clicked at (' + $x + ',' + $y + ')')
            # Capture what is now visible
            $texts = Get-DescendantTexts $window 8
            $relevant = $texts | Where-Object { $_ -match '豆包|芒果|元宝|WPS|腾讯|壁纸|安装|卸载|打开|精选|教育|软件|游戏|搜索|联想|推荐|无畏|办公|我的|动态|分类|升级' }
            Write-Output ('  Elements found: ' + $relevant.Count)
            foreach ($r in $relevant) { Write-Output ('  ' + $r) }
        } else {
            Write-Output '  Element has no visible rect'
        }
    } else {
        Write-Output ('  NOT FOUND: ' + $label)
    }
    Write-Output ''
}
Write-Output '=== All tests done ==='
