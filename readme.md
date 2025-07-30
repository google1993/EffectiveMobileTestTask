# Тестовое задание от Effective Mobile

## Задача. Рекламные площадки. (C#)
[Описание задачи](task.md)

## Действующий стенд
[Представлен вот тут](https://khatuncev.ru/demo3)

## Сборка проекта

Выкачиваем репозиторий в удобное место.
Переходим в папку
Проект собирается командой:
```
dotnet build ./EMTestTask.csproj -c Release -o <путь_для_сборки>
```

## Конфигурация
Изменяем файл конфигурации appsettings.json

## Запуск
Опишу на примере действующего стенда.

Конфигурация настроена на прослушивание сокета.

Создаю сервис:
```
[Unit]
Description=Effective Mobile Test Task
After=syslog.target network.target

[Service]
WorkingDirectory=/opt/projects/EffectiveMobile
ExecStartPre=/bin/rm -f /var/sock/effectivemobile.sock
ExecStart=/opt/dotnet/aspnetcore-runtime-8.0.11/dotnet ./EMTestTask.dll
Restart=always
RestartSec=10
SyslogIdentifier=EffectiveMobile
User=nginx
Group=nginx

[Install]
WantedBy=multi-user.target
```

Запускаю сервис и добавляю в автозагрузку:
```
# systemctl daemon-reload
# systemctl start EffectiveMobile
# systemctl enable EffectiveMobile
Created symlink /etc/systemd/system/multi-user.target.wants/EffectiveMobile.service → /etc/systemd/system/EffectiveMobile.service.

# systemctl status EffectiveMobile
● EffectiveMobile.service - Effective Mobile Test Task
     Loaded: loaded (/etc/systemd/system/EffectiveMobile.service; enabled; preset: enabled)
     Active: active (running) since Wed 2025-07-30 22:46:35 +05; 38min ago
   Main PID: 2145620 (dotnet)
      Tasks: 25 (limit: 18429)
     Memory: 51.1M (peak: 52.7M)
        CPU: 2.456s
     CGroup: /system.slice/EffectiveMobile.service
             └─2145620 /opt/dotnet/aspnetcore-runtime-8.0.11/dotnet ./EMTestTask.dll

июл 30 22:46:35 khatuncev.ru systemd[1]: Starting EffectiveMobile.service - Effective Mobile Test Task...
июл 30 22:46:35 khatuncev.ru systemd[1]: Started EffectiveMobile.service - Effective Mobile Test Task.
июл 30 22:46:35 khatuncev.ru EffectiveMobile[2145620]: info: Microsoft.Hosting.Lifetime[14]
июл 30 22:46:35 khatuncev.ru EffectiveMobile[2145620]:       Now listening on: http://unix:/var/sock/effectivemobile.sock
июл 30 22:46:35 khatuncev.ru EffectiveMobile[2145620]: info: Microsoft.Hosting.Lifetime[0]
июл 30 22:46:35 khatuncev.ru EffectiveMobile[2145620]:       Application started. Press Ctrl+C to shut down.
июл 30 22:46:35 khatuncev.ru EffectiveMobile[2145620]: info: Microsoft.Hosting.Lifetime[0]
июл 30 22:46:35 khatuncev.ru EffectiveMobile[2145620]:       Hosting environment: Production
июл 30 22:46:35 khatuncev.ru EffectiveMobile[2145620]: info: Microsoft.Hosting.Lifetime[0]
июл 30 22:46:35 khatuncev.ru EffectiveMobile[2145620]:       Content root path: /opt/projects/EffectiveMobile
#
```

Через nginx проксируем запросы на сокет:
```
server {
  ...

    location /demo3 {
      alias /opt/projects/EffectiveMobile/;
      index index.html;
    }

    location /demo3/ {
      alias /opt/projects/EffectiveMobile/;
    }

    location /demo3/api/ {
      proxy_pass http://effectivemobile/;
      proxy_set_header   Host $host;
      proxy_set_header   X-Real-IP $remote_addr;
      proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
      proxy_set_header   X-Forwarded-Proto $scheme;
      proxy_http_version 1.1;  # Использование HTTP/1.1
      proxy_set_header Connection "";
    }

  ...
}
```

На этом всё.

## Пишите, если требуются доработки.
