start chrome "https://www.tradingview.com/"
dotnet publish -c Release
cd C:\Projects2023\WebScrapper\WebScrapper\bin\Release\net6.0\publish

dotnet WebScrapper.dll

pause