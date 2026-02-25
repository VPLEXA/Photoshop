using Avalonia;

namespace VectorEditor.Models;

public class PolygonShape : ShapeBase
{
    public override ShapeType ShapeType => ShapeType.Polygon;

    public List<Point> Points { get; set; } = new();

    public override ShapeBase Clone() => new PolygonShape
    {
        X = X, Y = Y, Width = Width, Height = Height,
        Points = new List<Point>(Points),
        FillColor = FillColor, StrokeColor = StrokeColor,
        StrokeThickness = StrokeThickness
    };
}
