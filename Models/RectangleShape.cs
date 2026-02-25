namespace VectorEditor.Models;

public class RectangleShape : ShapeBase
{
    public override ShapeType ShapeType => ShapeType.Rectangle;

    public override ShapeBase Clone() => new RectangleShape
    {
        X = X, Y = Y, Width = Width, Height = Height,
        FillColor = FillColor, StrokeColor = StrokeColor,
        StrokeThickness = StrokeThickness
    };
}
