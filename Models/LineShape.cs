namespace VectorEditor.Models;

public class LineShape : ShapeBase
{
    public override ShapeType ShapeType => ShapeType.Line;

    public double X2 { get; set; }
    public double Y2 { get; set; }

    public override ShapeBase Clone() => new LineShape
    {
        X = X, Y = Y, Width = Width, Height = Height,
        X2 = X2, Y2 = Y2,
        FillColor = FillColor, StrokeColor = StrokeColor,
        StrokeThickness = StrokeThickness
    };
}
