Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes

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
        $ctrlType = $el.Current.ControlType.ProgrammaticName
        if ($name -and $name -ne '') {
            $indent = '  ' * $depth
            [void]$result.Add($indent + '[' + $ctrlType + '] ' + $name)
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
        if ($ct -eq [System.Windows.Automation.ControlType]::Button) {
            return $current
        }
        if ($ct -eq [System.Windows.Automation.ControlType]::Window) {
            return $null
        }
        $current = $walker.GetParent($current)
        $tries++
    }
    return $null
}

# Step 0: Ensure app is running
$proc = Get-Process 'Rxbit.Brigade.Controll' -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Sort-Object StartTime -Descending | Select-Object -First 1
if (-not $proc) {
    Write-Output 'No running window, starting app...'
    Start-Process 'd:\IDE-Workspace\Rxbit.BrigadeApp\Rxbit.Brigade.Controll\publish2\Rxbit.Brigade.Controll.exe'
    Start-Sleep -Seconds 5
    $proc = Get-Process 'Rxbit.Brigade.Controll' -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Sort-Object StartTime -Descending | Select-Object -First 1
}

if (-not $proc) {
    Write-Output 'FAIL: No process with window'
    exit 1
}

Write-Output ('PID: ' + $proc.Id + '  Handle: ' + $proc.MainWindowHandle)

$window = [System.Windows.Automation.AutomationElement]::FromHandle($proc.MainWindowHandle)
if (-not $window) {
    Write-Output 'FAIL: Cannot get AutomationElement'
    exit 1
}

# Step 1: Find ShopWindows text element
Write-Output '--- Step 1: Find ShopWindows ---'
$allElements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
$shopText = $null
$totalNamed = 0
foreach ($b in $allElements) {
    $n = $b.Current.Name
    if ($n -and $n -ne '') {
        $totalNamed++
        if (-not $shopText -and $n -like '*Shop*') {
            Write-Output ('  Found: ' + $n + ' (Type: ' + $b.Current.ControlType.ProgrammaticName + ')')
            $shopText = $b
        }
    }
}
Write-Output ('Total named elements: ' + $totalNamed)

if (-not $shopText) {
    Write-Output 'FAIL: ShopWindows not found'
    exit 1
}

# Step 2: Find Button ancestor and click
$btn = Find-ButtonAncestor $shopText
if ($btn) {
    Write-Output ('  Button ancestor found: ' + $btn.Current.ControlType.ProgrammaticName)
} else {
    Write-Output '  No Button ancestor, using text element'
    $btn = $shopText
}

Write-Output '--- Step 2: Click ShopWindows ---'
$pattern = $null
if ($btn.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
    $pattern.Invoke()
    Write-Output 'Invoked!'
} else {
    Write-Output 'No InvokePattern, trying to click parent chain...'
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
    $parent = $walker.GetParent($btn)
    $tries = 0
    while ($parent -and $tries -lt 5) {
        $p = $null
        if ($parent.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$p)) {
            Write-Output ('  Found InvokePattern at try ' + $tries)
            $p.Invoke()
            break
        }
        $parent = $walker.GetParent($parent)
        $tries++
    }
}
Start-Sleep -Milliseconds 2000

# Step 3: Capture UI tree
Write-Output '--- Step 3: UI tree after ShopWindows ---'
$texts = Get-DescendantTexts $window 8
Write-Output ('Total elements: ' + $texts.Count)
Write-Output '--- All elements ---'
foreach ($t in $texts) {
    Write-Output $t
}
