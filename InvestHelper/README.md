# Publishing

```
dotnet publish InvestHelper -o "F:\Invest\Soft\InvestHelper\win-64" -c Release /p:DebugType=None /p:DebugSymbols=false -r win-x64 --self-contained
dotnet publish InvestHelper -o "F:\Invest\Soft\InvestHelper\linux-64" -c Release /p:DebugType=None /p:DebugSymbols=false -r linux-x64 --self-contained

```

Скопировать в /var/www/investHelper/
Дать права на запуск

## Nginx

```
#Установить NGINX
sudo apt-get install nginx

#Запустить NGINX
sudo service nginx start

#Проверка синтаксиса настроек
sudo nginx -t 

#Перезапустить сервер
sudo nginx -s reload
```

отредактировать /etc/nginx/sites-available/default:
```
server {
    listen   80 default_server;
    # listen [::]:80 default_server deferred;
    return   444;
}
server {
    listen        80;
    server_name   5.44.46.153;
    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

## Демон для хостинга

Добавить файл `/etc/systemd/system/investHelper.service`
со следующим содержанием

```
[Unit]
Description=Site for portfolio management
After=nginx.service

[Service]
WorkingDirectory=/var/www/investHelper
ExecStart=/var/www/investHelper/InvestHelper
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-example
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```
sudo systemctl enable investHelper.service
sudo systemctl start investHelper.service
sudo systemctl status investHelper.service

#Просмотреть журнал
sudo journalctl -fu investHelper.service

#Остановить демон
sudo systemctl stop investHelper.service

sudo systemctl disable investHelper.service
```