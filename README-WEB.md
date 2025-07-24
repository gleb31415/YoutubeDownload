# YouTube Downloader Web

Веб-версия YouTube Downloader с красивым интерфейсом.

## Локальный запуск

1. Убедитесь, что у вас установлен .NET 8.0 SDK
2. Запустите команду:
```bash
dotnet run --project YoutubeDownloader.Web
```
3. Откройте браузер и перейдите по адресу: http://localhost:5000

## Развёртывание с Docker

1. Соберите Docker образ:
```bash
docker build -t youtube-downloader-web .
```

2. Запустите контейнер:
```bash
docker run -p 8080:8080 youtube-downloader-web
```

3. Откройте браузер и перейдите по адресу: http://localhost:8080

## Развёртывание на облачных платформах

### Heroku
1. Установите Heroku CLI
2. Выполните команды:
```bash
heroku login
heroku create your-app-name
heroku container:push web
heroku container:release web
```

### Railway
1. Установите Railway CLI
2. Выполните команды:
```bash
railway login
railway init
railway up
```

### DigitalOcean App Platform
1. Загрузите код в GitHub
2. Создайте новое приложение в DigitalOcean App Platform
3. Подключите ваш GitHub репозиторий
4. Настройте автоматическое развёртывание

## API Endpoints

- `GET /` - Главная страница с интерфейсом
- `POST /api/youtube/download` - Скачать видео
- `GET /api/youtube/info` - Получить информацию о видео

## Особенности

- Красивый современный интерфейс
- Поддержка различных качеств видео
- Автоматическое скачивание файлов
- REST API для интеграции с другими приложениями
- Поддержка Docker для лёгкого развёртывания
