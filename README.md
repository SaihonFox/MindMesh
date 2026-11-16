# MindMesh — Visual AI Workflow IDE

Настольная IDE для визуальной сборки AI- и data-workflow: граф нод, рантайм, отладчик, AI-ассистент.
Платформа: .NET 10, Avalonia 12.1.5, MVVM.

**Статус: Phase 2 — Graph Editor.** Можно создавать узлы из библиотеки, соединять их типизированными портами,
настраивать в инспекторе, отменять/повторять действия. Рантайм, проекты и остальное — в следующих фазах
(см. `docs/architecture/phase-2-report.md`).

## Требования

- .NET SDK 10 (`global.json`)
- Windows, Linux или macOS

## Сборка и запуск

```
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet run --project src/Desktop/MindMesh.Desktop
```

## Структура

- `src/Core` — Domain, Graph (модель графа), Plugins.Abstractions, Nodes (встроенные ноды), Application
- `src/Infrastructure` — хранилище настроек, пути, логирование
- `src/Desktop` — Presentation (view-модели), UI (Avalonia), Desktop (точка входа)
- `tests` — xunit.v3-тесты по проектам
- `docs/architecture` — архитектура и отчёт по фазе

## Данные пользователя

`%LOCALAPPDATA%\MindMesh` (Windows) или аналог в `LocalApplicationData`:
`settings.json` и каталог `logs`.

## Стиль кода

UTF-8, табы, CRLF (`.editorconfig`, `.gitattributes`).
