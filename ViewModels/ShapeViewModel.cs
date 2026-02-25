using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using VectorEditor.Models;

namespace VectorEditor.ViewModels;

public partial class ShapeViewModel : ViewModelBase
{
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;
    [ObservableProperty] private Color _fillColor;
    [ObservableProperty] private Color _strokeColor;
    [ObservableProperty] private double _strokeThickness;
    [ObservableProperty] private bool _isSelected;

    public ShapeBase Model { get; }

    public ShapeViewModel(ShapeBase model)
    {
        Model = model;
        SyncFromModel();
    }

    public void SyncFromModel()
    {
        X = Model.X;
        Y = Model.Y;
        Width = Model.Width;
        Height = Model.Height;
        FillColor = Model.FillColor;
        StrokeColor = Model.StrokeColor;
        StrokeThickness = Model.StrokeThickness;
    }

    public void SyncToModel()
    {
        Model.X = X;
        Model.Y = Y;
        Model.Width = Width;
        Model.Height = Height;
        Model.FillColor = FillColor;
        Model.StrokeColor = StrokeColor;
        Model.StrokeThickness = StrokeThickness;

        if (Model is LineShape line)
        {
            line.X2 = X + Width;
            line.Y2 = Y + Height;
        }
    }

    public bool HitTest(Point point)
    {
        if (Model is LineShape line)
            return HitTestLine(point, new Point(line.X, line.Y), new Point(line.X2, line.Y2), 8);

        return point.X >= X && point.X <= X + Width
            && point.Y >= Y && point.Y <= Y + Height;
    }

    private static bool HitTestLine(Point p, Point a, Point b, double tolerance)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lenSq = dx * dx + dy * dy;
        if (lenSq == 0) return false;
        var t = Math.Max(0, Math.Min(1, ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lenSq));
        var nearX = a.X + t * dx;
        var nearY = a.Y + t * dy;
        var dist = Math.Sqrt((p.X - nearX) * (p.X - nearX) + (p.Y - nearY) * (p.Y - nearY));
        return dist <= tolerance;
    }
}
