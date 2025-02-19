# EBCEYS.DayOfAllLoversService

## ��������:

������, ������� ������������ ������� �������� �����.

����������� ����� �������� ������������� ���������� [EBCEYS.OSServiceHelper](https://github.com/EBCEYS/EBCEYS.OSServiceHelper).

## ���������:

1. ��������� PowerShell �� ����� ��������������
1. ��������� � ���������� � ������� ������� `cd "{PATH}"`
1. �������� `.\EBCEYS.DayOfAllLoversService.exe install`
1. ����������� ����� ������� ���� �������������. �� ��������� `C:\Program Files\EBCEYS-DayOfAllLoversService`
1. � �� ������������ ���������� � ���������� �� ���������. ��� ����� ��� ������ ��������� ������� y.
1. ����� ������ ����� ���������. `.\EBCEYS.DayOfAllLoversService.exe start`
1. ��� ��������� ������ ���� ������ ����������� `.\EBCEYS.DayOfAllLoversService.exe help`

## ������������:
������������ ����������� � ���� `appsettings.json`, ������� ������ ������ ������ � ����������� ������.
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
    "VoiceName": "ru", // ��� ��������� (Check for name contains {VoiceName})
    "SpeakerDelayInterval": { // ��������� ��������� �������� � ��������. ����� ���������� ��������� ���-�� ������ � ������ ���������.
        "Start": 5, // ��
        "End": 60 // ��
    },
    "TextesToSpeek": [ // �����, ������� ����� ������������
        "Some text to speak",
        "Another text to speak"
    ]
}
```