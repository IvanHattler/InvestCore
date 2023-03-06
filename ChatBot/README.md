### Quick start

1. Clone this repository
2. Set the user secrets by using Package Manager Console:
```
dotnet user-secrets set "TelegramToken" "<token>"
dotnet user-secrets set "TwelveDataApiToken" "<token>"
dotnet user-secrets set "TinkoffToken" "<token>"
```
3. Build solution

### Publish

1. For publish in win-x64 recommended this command. For simplify output used PublishSingleFile and Self-hosting
```
chcp 1251
dotnet publish ChatBot -o "F:\Invest\Soft\ChatBot" -c Release /p:DebugType=None /p:DebugSymbols=false
```
2. Copy files to
```
"C:\Windows\winvidmgmt64"
```

3. Add file
```
"C:\Windows\System32\config\systemprofile\AppData\Roaming\Microsoft\UserSecrets\f50f26cc-7146-4fa7-949c-c9751da6f0f4\secrets.json"
```

4. Open powershell by admin and execute command and type "A".
```
Set-ExecutionPolicy RemoteSigned
```

Then execute:
```
& "C:\Windows\winvidmgmt64\install.ps1"
```

For uninstall execute:
```
& "C:\Windows\winvidmgmt64\uninstall.ps1"
```

### Dependencies

- [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) for telegram api
- [Autofac](https://github.com/autofac/Autofac) for DI
- [InvestApi .NET SDK](https://github.com/Tinkoff/invest-api-csharp-sdk) for moex share quotes
