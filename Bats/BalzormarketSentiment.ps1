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

$dynamicTitle = "Blazor MarketSentiment " + (Get-Date)
$Host.UI.RawUI.WindowTitle = $dynamicTitle


# Remove the Close button (SC_CLOSE) from the system menu
[User32]::RemoveMenu($sysMenu, 6, 0x0F01)

Write-Host "Your PowerShell script is running."
cd D:\Projs\BlazorTrading\LykeeServer
dotnet publish -c Release
cd D:\Projs\BlazorTrading\LykeeServer\bin\Release\net6.0\publish
start chrome "https://localhost:5001/"


dotnet LykeeServer.dll
dotnet LykeeServer.dll
dotnet LykeeServer.dll
dotnet LykeeServer.dll
dotnet LykeeServer.dll
dotnet LykeeServer.dll
dotnet LykeeServer.dll
dotnet LykeeServer.dll
pause

pause




Read-Host "Press Enter to exit"
