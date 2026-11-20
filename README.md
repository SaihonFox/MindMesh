# MindMesh — Visual AI Workflow IDE

Настольная IDE для визуальной сборки AI- и data-workflow: граф нод, рантайм, отладчик, AI-ассистент.
Платформа: .NET 11, Avalonia 12.1.6, MVVM.

**Статус: Phase 3 — Workflow Core.** Проекты `.mmflow` (создание, открытие, сохранение, обложки, поиск), валидация графа,
сериализуемые операции изменения графа, настройки (темы, горячие клавиши, папка проектов). Рантайм — в Phase 4
(см. `docs/architecture/phase-3-report.md`).

## Требования

- .NET SDK 11 (`global.json`)
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
