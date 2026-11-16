# Phase 1 — Foundation: отчёт

## Что сделано

- Решение `MindMesh.slnx`, централизованные версии пакетов, общие настройки сборки, `.editorconfig` (табы, CRLF, UTF-8).
- Шесть рабочих проектов (Domain, Application, Infrastructure, Presentation, UI, Desktop) и пять тестовых.
- DI и хост: `AddMindMeshInfrastructure`, `AddMindMeshApplication`, `AddMindMeshPresentation`.
- Конфигурация пользователя: `AppSettings` с нормализацией, отложенное атомарное сохранение, восстановление после повреждённого файла.
- Логирование: Serilog → `%LOCALAPPDATA%/MindMesh/logs/mindmesh-YYYYMMDD.log`, глобальный перехват необработанных исключений.
- Локализация русский + English: resx, переключение без перезапуска, тест на полноту ключей.
- Дизайн-система: палитра, токены, стили, иконки, контролы `Icon`, `EmptyState`, `CanvasGridBackground`, `PanelResizer`.
- Главное окно: верхняя панель, сайдбар (обычный/компактный), навигация, страницы Welcome / Settings / About.
- Раскладка редактора: библиотека нод (поиск по категориям текущего режима), холст с сеткой, консоль с шестью вкладками,
  инспектор, панель AI-ассистента; видимость и размеры панелей сохраняются.
- CI (`.github/workflows/ci.yml`): restore, build, тесты, проверка форматирования, публикация desktop.

## Что намеренно не сделано (по фазам)

Кнопки и экраны, которым нужны следующие фазы, **отключены или помечены**, а не имитируют работу:

| Элемент | Появится |
|---|---|
| Граф-редактор, undo/redo, auto layout, validate | Phase 2 |
| Проекты, workflow-документы | Phase 3 |
| Runtime, запуск, консоль выполнения | Phase 4 |
| Debugger, страница Executions | Phase 5 |
| AI-ассистент | Phase 6 |
| Шаблоны, Marketplace, плагины | Phase 9 |

## Проверка

В среде, где писался код, нет .NET SDK, поэтому **сборка и запуск тестов не выполнялись**. Выполнена только статическая
проверка: пространства имён, ссылки между проектами, ключи ресурсов ↔ XAML, пути в `.slnx`.
Первый шаг на Windows:

```
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet run --project src/Desktop/MindMesh.Desktop
```

Результат (ошибки компилятора или упавшие тесты) нужно прислать — они будут исправлены в начале следующего сеанса.

## Известные риски

- Не сверены с реальным API: `FluentTheme.Palettes`, `TextBlock.ForegroundProperty.AddOwner`, перегрузки `OnPropertyChanged`
  в `Icon`/`CanvasGridBackground`/`PanelResizer`, конструктор `Pen`, `Cursor` в Avalonia 12.
- Версии пакетов взяты по памяти и могут не существовать; при ошибке restore поправить `Directory.Packages.props`.
- Привязки с индексатором `Loc[Key]` обновляются через `PropertyChanged("Loc")`; проверить переключение языка визуально.
- Внешний вид (отступы, состояния кнопок, `TabItem`) не проверялся глазами — нужен проход по скриншотам.
- Тест на сборку мусора (`ViewModelBase_DoesNotKeepShortLivedViewModelsAlive`) зависит от поведения GC и в отладочной
  конфигурации может быть нестабилен.
