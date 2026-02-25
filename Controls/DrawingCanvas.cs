using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using VectorEditor.Models;
using VectorEditor.ViewModels;

namespace VectorEditor.Controls;

public class DrawingCanvas : Control
{
    private MainWindowViewModel? _viewModel;
    private Point _dragStart;
    private Point _shapeStartPos;
    private bool _isDragging;
    private bool _isResizing;
    private bool _isDrawing;
    private ShapeViewModel? _drawingPreview;

    public static readonly StyledProperty<MainWindowViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<DrawingCanvas, MainWindowViewModel?>(nameof(ViewModel));

    public MainWindowViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ViewModelProperty)
        {
            if (_viewModel != null)
                _viewModel.Shapes.CollectionChanged -= OnShapesChanged;

            _viewModel = change.GetNewValue<MainWindowViewModel?>();

            if (_viewModel != null)
                _viewModel.Shapes.CollectionChanged += OnShapesChanged;
        }
    }

    private void OnShapesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ShapeViewModel vm in e.NewItems)
                vm.PropertyChanged += (_, _) => InvalidateVisual();
        }
        InvalidateVisual();
    }

    private Point ClampToCanvas(Point p) =>
        new(Math.Clamp(p.X, 0, Bounds.Width), Math.Clamp(p.Y, 0, Bounds.Height));

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (_viewModel == null) return;

        var point = ClampToCanvas(e.GetPosition(this));
        _dragStart = point;

        if (_viewModel.ActiveTool == EditorTool.Select)
        {
            var hit = _viewModel.HitTest(point);

            if (hit != null && _viewModel.IsResizeHandle(hit, point))
            {
                _viewModel.SaveStateForUndo();
                _isResizing = true;
                _viewModel.SelectedShape = hit;
            }
            else if (hit != null)
            {
                _viewModel.SaveStateForUndo();
                _isDragging = true;
                _viewModel.SelectedShape = hit;
                _shapeStartPos = new Point(hit.X, hit.Y);
            }
            else
            {
                _viewModel.SelectedShape = null;
            }
        }
        else
        {
            _isDrawing = true;
            _viewModel.SelectedShape = null;
        }

        e.Pointer.Capture(this);
        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_viewModel == null) return;

        var point = e.GetPosition(this);

        if (_isDragging && _viewModel.SelectedShape != null)
        {
            var shape = _viewModel.SelectedShape;
            var delta = new Point(point.X - _dragStart.X, point.Y - _dragStart.Y);

            var newX = Math.Clamp(_shapeStartPos.X + delta.X, 0, Bounds.Width - shape.Width);
            var newY = Math.Clamp(_shapeStartPos.Y + delta.Y, 0, Bounds.Height - shape.Height);

            shape.X = newX;
            shape.Y = newY;

            if (shape.Model is LineShape line)
            {
                line.X = newX;
                line.Y = newY;
                line.X2 = newX + shape.Width;
                line.Y2 = newY + shape.Height;
            }

            shape.SyncToModel();
            InvalidateVisual();
        }
        else if (_isResizing && _viewModel.SelectedShape != null)
        {
            var clampedPoint = ClampToCanvas(point);
            var delta = new Point(clampedPoint.X - _dragStart.X, clampedPoint.Y - _dragStart.Y);
            _viewModel.ResizeShape(_viewModel.SelectedShape, delta.X, delta.Y);
            _dragStart = clampedPoint;
            InvalidateVisual();
        }
        else if (_isDrawing)
        {
            UpdateDrawingPreview(_dragStart, ClampToCanvas(point));
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_viewModel == null) return;

        if (_isDrawing)
        {
            var end = ClampToCanvas(e.GetPosition(this));
            var shape = CreateShape(_dragStart, end);
            if (shape != null)
                _viewModel.AddShape(shape);
            _drawingPreview = null;
        }

        _isDragging = false;
        _isResizing = false;
        _isDrawing = false;
        e.Pointer.Capture(null);
        InvalidateVisual();
    }

    private void UpdateDrawingPreview(Point start, Point current)
    {
        var x = Math.Min(start.X, current.X);
        var y = Math.Min(start.Y, current.Y);
        var w = Math.Abs(current.X - start.X);
        var h = Math.Abs(current.Y - start.Y);

        ShapeBase? model = _viewModel!.ActiveTool switch
        {
            EditorTool.Rectangle => new RectangleShape { X = x, Y = y, Width = w, Height = h },
            EditorTool.Ellipse => new EllipseShape { X = x, Y = y, Width = w, Height = h },
            EditorTool.Line => new LineShape { X = start.X, Y = start.Y, X2 = current.X, Y2 = current.Y, Width = w, Height = h },
            EditorTool.Polygon => new PolygonShape
            {
                X = x, Y = y, Width = w, Height = h,
                Points = BuildRegularPolygon(new Point(x + w / 2, y + h / 2), Math.Min(w, h) / 2, 6)
            },
            _ => null
        };

        if (model == null) return;

        model.FillColor = _viewModel.ActiveFillColor;
        model.StrokeColor = _viewModel.ActiveStrokeColor;
        model.StrokeThickness = _viewModel.ActiveStrokeThickness;
        _drawingPreview = new ShapeViewModel(model);
    }

    private ShapeBase? CreateShape(Point start, Point end)
    {
        var x = Math.Min(start.X, end.X);
        var y = Math.Min(start.Y, end.Y);
        var w = Math.Abs(end.X - start.X);
        var h = Math.Abs(end.Y - start.Y);

        if (w < 2 && h < 2) return null;

        return _viewModel!.ActiveTool switch
        {
            EditorTool.Rectangle => new RectangleShape { X = x, Y = y, Width = w, Height = h },
            EditorTool.Ellipse => new EllipseShape { X = x, Y = y, Width = w, Height = h },
            EditorTool.Line => new LineShape { X = start.X, Y = start.Y, X2 = end.X, Y2 = end.Y, Width = w, Height = h },
            EditorTool.Polygon => new PolygonShape
            {
                X = x, Y = y, Width = w, Height = h,
                Points = BuildRegularPolygon(new Point(x + w / 2, y + h / 2), Math.Min(w, h) / 2, 6)
            },
            _ => null
        };
    }

    private static List<Point> BuildRegularPolygon(Point center, double radius, int sides)
    {
        var points = new List<Point>();
        for (int i = 0; i < sides; i++)
        {
            var angle = Math.PI * 2 * i / sides - Math.PI / 2;
            points.Add(new Point(center.X + radius * Math.Cos(angle), center.Y + radius * Math.Sin(angle)));
        }
        return points;
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.White, new Rect(Bounds.Size));

        if (_viewModel == null) return;

        foreach (var shape in _viewModel.Shapes)
            RenderShape(context, shape);

        if (_drawingPreview != null)
            RenderShape(context, _drawingPreview);
    }

    private void RenderShape(DrawingContext context, ShapeViewModel vm)
    {
        var fill = new ImmutableSolidColorBrush(vm.FillColor);
        var stroke = new ImmutableSolidColorBrush(vm.StrokeColor);
        var pen = new Pen(stroke, vm.StrokeThickness);

        switch (vm.Model)
        {
            case RectangleShape:
                context.DrawRectangle(fill, pen, new Rect(vm.X, vm.Y, vm.Width, vm.Height));
                break;

            case EllipseShape:
                context.DrawEllipse(fill, pen, new Rect(vm.X, vm.Y, vm.Width, vm.Height));
                break;

            case LineShape line:
                context.DrawLine(pen, new Point(line.X, line.Y), new Point(line.X2, line.Y2));
                break;

            case PolygonShape polygon when polygon.Points.Count >= 2:
                var geometry = BuildPolygonGeometry(polygon.Points);
                context.DrawGeometry(fill, pen, geometry);
                break;
        }

        if (vm.IsSelected)
            DrawSelectionHandles(context, vm);
    }

    private static StreamGeometry BuildPolygonGeometry(List<Avalonia.Point> points)
    {
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.BeginFigure(points[0], true);
        for (int i = 1; i < points.Count; i++)
            ctx.LineTo(points[i]);
        ctx.EndFigure(true);
        return geometry;
    }

    private static void DrawSelectionHandles(DrawingContext context, ShapeViewModel vm)
    {
        var selectionPen = new Pen(new ImmutableSolidColorBrush(Color.FromRgb(0, 120, 215)), 1, dashStyle: DashStyle.Dash);

        if (vm.Model is LineShape line)
        {
            context.DrawLine(selectionPen, new Point(line.X, line.Y), new Point(line.X2, line.Y2));
            DrawHandle(context, new Point(line.X, line.Y));
            DrawHandle(context, new Point(line.X2, line.Y2));
            return;
        }

        context.DrawRectangle(null, selectionPen, new Rect(vm.X - 1, vm.Y - 1, vm.Width + 2, vm.Height + 2));

        DrawHandle(context, new Point(vm.X, vm.Y));
        DrawHandle(context, new Point(vm.X + vm.Width, vm.Y));
        DrawHandle(context, new Point(vm.X, vm.Y + vm.Height));
        DrawHandle(context, new Point(vm.X + vm.Width, vm.Y + vm.Height));
    }

    private static void DrawHandle(DrawingContext context, Point center)
    {
        const double size = 6;
        context.DrawRectangle(
            Brushes.White,
            new Pen(Brushes.Black, 1),
            new Rect(center.X - size / 2, center.Y - size / 2, size, size));
    }
}
