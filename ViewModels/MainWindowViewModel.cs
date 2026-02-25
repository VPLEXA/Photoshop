using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VectorEditor.Models;
using VectorEditor.Services;

namespace VectorEditor.ViewModels;

public enum EditorTool { Select, Rectangle, Ellipse, Line, Polygon }

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly UndoRedoService _undoRedo = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectToolActive))]
    [NotifyPropertyChangedFor(nameof(IsRectangleToolActive))]
    [NotifyPropertyChangedFor(nameof(IsEllipseToolActive))]
    [NotifyPropertyChangedFor(nameof(IsLineToolActive))]
    [NotifyPropertyChangedFor(nameof(IsPolygonToolActive))]
    private EditorTool _activeTool = EditorTool.Select;

    public bool IsSelectToolActive => ActiveTool == EditorTool.Select;
    public bool IsRectangleToolActive => ActiveTool == EditorTool.Rectangle;
    public bool IsEllipseToolActive => ActiveTool == EditorTool.Ellipse;
    public bool IsLineToolActive => ActiveTool == EditorTool.Line;
    public bool IsPolygonToolActive => ActiveTool == EditorTool.Polygon;
    [ObservableProperty] private double _zoomLevel = 1.0;
    [ObservableProperty] private ShapeViewModel? _selectedShape;
    [ObservableProperty] private Color _activeFillColor = Colors.White;
    [ObservableProperty] private Color _activeStrokeColor = Colors.Black;
    [ObservableProperty] private double _activeStrokeThickness = 2;

    [ObservableProperty] private bool _canUndo;
    [ObservableProperty] private bool _canRedo;

    public ObservableCollection<ShapeViewModel> Shapes { get; } = new();

    public double CanvasWidth => 800;
    public double CanvasHeight => 600;

    partial void OnSelectedShapeChanged(ShapeViewModel? value)
    {
        foreach (var s in Shapes)
            s.IsSelected = s == value;

        if (value != null)
        {
            ActiveFillColor = value.FillColor;
            ActiveStrokeColor = value.StrokeColor;
            ActiveStrokeThickness = value.StrokeThickness;

            BringToFront(value);
        }
    }

    partial void OnActiveFillColorChanged(Color value)
    {
        if (SelectedShape == null) return;
        SaveStateForUndo();
        SelectedShape.FillColor = value;
        SelectedShape.SyncToModel();
    }

    partial void OnActiveStrokeColorChanged(Color value)
    {
        if (SelectedShape == null) return;
        SaveStateForUndo();
        SelectedShape.StrokeColor = value;
        SelectedShape.SyncToModel();
    }

    partial void OnActiveStrokeThicknessChanged(double value)
    {
        if (SelectedShape == null) return;
        SelectedShape.StrokeThickness = value;
        SelectedShape.SyncToModel();
    }

    public void AddShape(ShapeBase shape)
    {
        SaveStateForUndo();
        shape.FillColor = ActiveFillColor;
        shape.StrokeColor = ActiveStrokeColor;
        shape.StrokeThickness = ActiveStrokeThickness;
        var vm = new ShapeViewModel(shape);
        Shapes.Add(vm);
        SelectedShape = vm;
    }

    public void MoveShape(ShapeViewModel shape, double deltaX, double deltaY)
    {
        shape.X += deltaX;
        shape.Y += deltaY;

        if (shape.Model is LineShape line)
        {
            line.X2 += deltaX;
            line.Y2 += deltaY;
        }

        shape.SyncToModel();
    }

    public void ResizeShape(ShapeViewModel shape, double deltaX, double deltaY)
    {
        shape.Width = Math.Max(10, shape.Width + deltaX);
        shape.Height = Math.Max(10, shape.Height + deltaY);

        if (shape.Model is LineShape line)
        {
            line.X2 = shape.X + shape.Width;
            line.Y2 = shape.Y + shape.Height;
        }

        shape.SyncToModel();
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedShape == null) return;
        SaveStateForUndo();
        Shapes.Remove(SelectedShape);
        SelectedShape = null;
    }

    [RelayCommand]
    private void Undo()
    {
        var restored = _undoRedo.Undo(Shapes.Select(s => s.Model));
        if (restored == null) return;
        RestoreShapes(restored);
        UpdateUndoRedoState();
    }

    [RelayCommand]
    private void Redo()
    {
        var restored = _undoRedo.Redo(Shapes.Select(s => s.Model));
        if (restored == null) return;
        RestoreShapes(restored);
        UpdateUndoRedoState();
    }

    [RelayCommand]
    private void ZoomIn() => ZoomLevel = Math.Min(5.0, ZoomLevel + 0.1);

    [RelayCommand]
    private void ZoomOut() => ZoomLevel = Math.Max(0.1, ZoomLevel - 0.1);

    [RelayCommand]
    private void ZoomReset() => ZoomLevel = 1.0;

    public void SaveStateForUndo()
    {
        _undoRedo.SaveState(Shapes.Select(s => s.Model));
        UpdateUndoRedoState();
    }

    private void RestoreShapes(List<ShapeBase> models)
    {
        var previousSelection = SelectedShape?.Model.Id;
        Shapes.Clear();
        foreach (var model in models)
            Shapes.Add(new ShapeViewModel(model));

        SelectedShape = Shapes.FirstOrDefault(s => s.Model.Id == previousSelection);
    }

    private void BringToFront(ShapeViewModel shape)
    {
        if (Shapes.Last() == shape) return;
        Shapes.Remove(shape);
        Shapes.Add(shape);
    }

    private void UpdateUndoRedoState()
    {
        CanUndo = _undoRedo.CanUndo;
        CanRedo = _undoRedo.CanRedo;
    }

    public string ExportToSvg() =>
        SvgExporter.Export(Shapes.Select(s => s.Model), CanvasWidth, CanvasHeight);

    public string SerializeProject() =>
        ProjectSerializer.Serialize(Shapes.Select(s => s.Model));

    public void LoadProject(string json)
    {
        var models = ProjectSerializer.Deserialize(json);
        Shapes.Clear();
        foreach (var model in models)
            Shapes.Add(new ShapeViewModel(model));
        SelectedShape = null;
        UpdateUndoRedoState();
    }

    public ShapeViewModel? HitTest(Point point)
    {
        for (int i = Shapes.Count - 1; i >= 0; i--)
        {
            if (Shapes[i].HitTest(point))
                return Shapes[i];
        }
        return null;
    }

    public bool IsResizeHandle(ShapeViewModel shape, Point point)
    {
        var handleSize = 8.0;
        var hx = shape.X + shape.Width - handleSize / 2;
        var hy = shape.Y + shape.Height - handleSize / 2;
        return point.X >= hx && point.X <= hx + handleSize
            && point.Y >= hy && point.Y <= hy + handleSize;
    }
}
