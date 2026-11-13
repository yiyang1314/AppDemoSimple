Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$ps = Get-Process 'Rxbit.Brigade.Controll' -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -ne '' } | Select-Object -First 1
if (-not $ps) {
    Write-Output 'No window found'
    exit 1
}

$hwnd = $ps.MainWindowHandle

$win32Type = Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win32Api {
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
}
"@ -PassThru

[Win32Api]::ShowWindow($hwnd, 9) | Out-Null
[Win32Api]::SetForegroundWindow($hwnd) | Out-Null
Start-Sleep -Milliseconds 800

$r = New-Object Win32Api+RECT
[Win32Api]::GetWindowRect($hwnd, [ref]$r) | Out-Null
$w = $r.Right - $r.Left
$h = $r.Bottom - $r.Top

if ($w -le 0 -or $h -le 0) {
    Write-Output "Invalid window size: $w x $h"
    exit 1
}

$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.Left, $r.Top, 0, 0, $bmp.Size)

$path = 'd:\IDE-Workspace\Rxbit.BrigadeApp\assets\Rxbit.Brigade.Controll\screenshots\screenshot_shop.png'
$bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()

Write-Output "Screenshot saved: $path ($w x $h)"
