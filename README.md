# Vector Editor

Векторный графический редактор, написанный на C# с использованием Avalonia UI.

## Возможности

- Рисование фигур: прямоугольник, эллипс, линия, многоугольник
- Выделение, перемещение и масштабирование фигур
- Настройка цвета заливки и обводки
- Отмена и повтор действий (до 5 шагов)
- Масштабирование холста
- Сохранение проекта в формате `.vep` и экспорт в `.svg`

## Технологии

| | |
|---|---|
| Язык | C# 13 |
| Фреймворк | .NET 9 |
| UI | Avalonia UI 11.3.8 |
| MVVM | CommunityToolkit.Mvvm 8.2.1 |
| Паттерн | MVVM |

## Запуск

```bash
cd VectorEditor
dotnet run
```

## Структура проекта

```
VectorEditor/
├── Models/          # Модели фигур (ShapeBase, Rectangle, Ellipse, Line, Polygon)
├── ViewModels/      # Логика представления (MainWindowViewModel, ShapeViewModel)
├── Views/           # UI-окна (MainWindow, ColorPickerWindow, FileBrowserDialog)
├── Controls/        # Кастомный DrawingCanvas
├── Services/        # UndoRedoService, ProjectSerializer, SvgExporter
└── Converters/      # ColorToBrushConverter
```

## Горячие клавиши

| Клавиша | Действие |
|---|---|
| `Ctrl+Z` | Отменить |
| `Ctrl+Y` | Повторить |
| `Del` | Удалить фигуру |
| `+` / `−` | Увеличить / уменьшить холст |
| `0` | Сбросить масштаб |
