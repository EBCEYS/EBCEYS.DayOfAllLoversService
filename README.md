# EBCEYS.DayOfAllLoversService

## Описание:

Сервис, который периодически говорит классные штуки.

Репозиторий лужит примером использования библиотеки [EBCEYS.OSServiceHelper](https://github.com/EBCEYS/EBCEYS.OSServiceHelper).

## Установка:

1. Открываем PowerShell от имени администратора
1. Переходим в директорию с файлами сервиса `cd "{PATH}"`
1. Набираем `.\EBCEYS.DayOfAllLoversService.exe install`
1. Опционально можно указать куда устанавливать. По умолчанию `C:\Program Files\EBCEYS-DayOfAllLoversService`
1. Я бы рекомендовал установить в директорию по умолчанию. Для этого при начале установки введите y.
1. Далее сервис нужно запустить. `.\EBCEYS.DayOfAllLoversService.exe start`
1. Для получения списка всех команд используйте `.\EBCEYS.DayOfAllLoversService.exe help`

## Конфигурация:
Конфигурация сохраняется в файл `appsettings.json`, который должен лежать вместе с исполняемым файлом.
```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Debug",
            "Microsoft.Hosting.Lifetime": "Debug"
        }
    },
    "EventLog": {
        "SourceName": "EBCEYS.DayOfAllLoversService",
        "LogName": "Application",
        "LogLevel": {
            "Microsoft": "Debug",
            "Microsoft.Hosting.Lifetime": "Debug"
        }
    },
    "VoiceName": "ru", // имя говорилки (Check for name contains {VoiceName})
    "SpeakerDelayInterval": { // Настройки интервала задержки в секундах. Будет выбираться случайное кол-во секунд в рамках интервала.
        "Start": 5, // ОТ
        "End": 60 // ДО
    },
    "TextesToSpeek": [ // Текст, который будет зачитываться
        "Some text to speak",
        "Another text to speak"
    ]
}
```