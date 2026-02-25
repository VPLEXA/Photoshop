using Avalonia.Media;

namespace VectorEditor.Models;

public abstract class ShapeBase
{
    public Guid Id { get; } = Guid.NewGuid();
    public abstract ShapeType ShapeType { get; }

    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public Color FillColor { get; set; } = Colors.White;
    public Color StrokeColor { get; set; } = Colors.Black;
    public double StrokeThickness { get; set; } = 2;

    public abstract ShapeBase Clone();
}
