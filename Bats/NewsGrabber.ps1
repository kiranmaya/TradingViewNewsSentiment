Add-Type @"
    using System;
    using System.Runtime.InteropServices;
    public class User32 {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        
        [DllImport("user32.dll")]
        public static extern int RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
    }
"@

$consoleWindow = [User32]::FindWindow([NullString]::Value, $Host.UI.RawUI.WindowTitle)
$sysMenu = [User32]::GetSystemMenu($consoleWindow, $false)

# Remove the Close button (SC_CLOSE) from the system menu
[User32]::RemoveMenu($sysMenu, 6, 0x0F01)


$dynamicTitle = "TradingView News Grabber" + (Get-Date)
$Host.UI.RawUI.WindowTitle = $dynamicTitle



Write-Host "Your PowerShell script is running."
cd C:\Projects2023\WebScrapper
start chrome "https://www.tradingview.com/"
dotnet publish -c Release
cd C:\Projects2023\WebScrapper\WebScrapper\bin\Release\net6.0\publish

dotnet WebScrapper.dll

pause




Read-Host "Press Enter to exit"
