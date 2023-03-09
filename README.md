### Publish

1. For publish in win-x64 recommended this command. For simplify output used PublishSingleFile and Self-hosting
```
chcp 1251
dotnet publish PercentCalculateConsole -o "F:\Invest\Soft\PercentCalculate" -c Release /p:DebugType=None /p:DebugSymbols=false
dotnet publish ChatBot\ChatBot -o "F:\Invest\Soft\ChatBot" -c Release /p:DebugType=None /p:DebugSymbols=false
dotnet publish ChatBot\ChatBotWorker -o "F:\Invest\Soft\ChatBot" -c Release /p:DebugType=None /p:DebugSymbols=false
dotnet publish SpreadsheetExporter -o "F:\Invest\Soft\SpreadsheetExporter" -c Release /p:DebugType=None /p:DebugSymbols=false

```
