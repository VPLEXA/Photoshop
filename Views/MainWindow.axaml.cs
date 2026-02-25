using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using VectorEditor.ViewModels;

namespace VectorEditor.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Z)
            ViewModel.UndoCommand.Execute(null);
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Y)
            ViewModel.RedoCommand.Execute(null);
        else if (e.Key == Key.Delete)
            ViewModel.DeleteSelectedCommand.Execute(null);
        else if (e.Key == Key.Add || e.Key == Key.OemPlus)
            ViewModel.ZoomInCommand.Execute(null);
        else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            ViewModel.ZoomOutCommand.Execute(null);
        else if (e.Key == Key.D0 || e.Key == Key.NumPad0)
            ViewModel.ZoomResetCommand.Execute(null);
    }

    private void OnToolSelect(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var tag = btn.Tag?.ToString();
        ViewModel.ActiveTool = tag switch
        {
            "Select" => EditorTool.Select,
            "Rectangle" => EditorTool.Rectangle,
            "Ellipse" => EditorTool.Ellipse,
            "Line" => EditorTool.Line,
            "Polygon" => EditorTool.Polygon,
            _ => EditorTool.Select
        };
    }

    private async void OnFillColorClick(object? sender, PointerPressedEventArgs e)
    {
        var color = await PickColor(ViewModel.ActiveFillColor);
        if (color.HasValue)
            ViewModel.ActiveFillColor = color.Value;
    }

    private async void OnStrokeColorClick(object? sender, PointerPressedEventArgs e)
    {
        var color = await PickColor(ViewModel.ActiveStrokeColor);
        if (color.HasValue)
            ViewModel.ActiveStrokeColor = color.Value;
    }

    private async Task<Color?> PickColor(Color initial)
    {
        var dialog = new ColorPickerWindow(initial);
        return await dialog.ShowDialog<Color?>(this);
    }

    private void OnShapeSizeChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        ViewModel.SelectedShape?.SyncToModel();
        Canvas.InvalidateVisual();
    }

    private void OnNewProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.Shapes.Clear();
        ViewModel.SelectedShape = null;
    }

    private async void OnOpenProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Открыть проект",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("Vector Editor Project") { Patterns = new[] { "*.vep" } } }
        });

        if (files.Count == 0) return;

        await using var stream = await files[0].OpenReadAsync();
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        ViewModel.LoadProject(json);
    }

    private async void OnSaveProject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить проект",
            DefaultExtension = "vep",
            FileTypeChoices = new[] { new FilePickerFileType("Vector Editor Project") { Patterns = new[] { "*.vep" } } }
        });

        if (file == null) return;

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(ViewModel.SerializeProject());
    }

    private async void OnExportSvg(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Экспорт в SVG",
            DefaultExtension = "svg",
            FileTypeChoices = new[] { new FilePickerFileType("SVG Image") { Patterns = new[] { "*.svg" } } }
        });

        if (file == null) return;

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(ViewModel.ExportToSvg());
    }

    private void OnExit(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
