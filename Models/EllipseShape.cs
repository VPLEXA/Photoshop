namespace VectorEditor.Models;

public class EllipseShape : ShapeBase
{
    public override ShapeType ShapeType => ShapeType.Ellipse;

    public override ShapeBase Clone() => new EllipseShape
    {
        X = X, Y = Y, Width = Width, Height = Height,
        FillColor = FillColor, StrokeColor = StrokeColor,
        StrokeThickness = StrokeThickness
    };
}
