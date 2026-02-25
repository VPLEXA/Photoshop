using System.Text;
using Avalonia;
using VectorEditor.Models;

namespace VectorEditor.Services;

public static class SvgExporter
{
    public static string Export(IEnumerable<ShapeBase> shapes, double canvasWidth, double canvasHeight)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" width="{canvasWidth}" height="{canvasHeight}">""");

        foreach (var shape in shapes)
        {
            sb.AppendLine(ShapeToSvg(shape));
        }

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static string ShapeToSvg(ShapeBase shape)
    {
        var fill = ColorToHex(shape.FillColor);
        var stroke = ColorToHex(shape.StrokeColor);
        var sw = shape.StrokeThickness;

        return shape switch
        {
            RectangleShape r =>
                $"""  <rect x="{r.X}" y="{r.Y}" width="{r.Width}" height="{r.Height}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}"/>""",

            EllipseShape e =>
                $"""  <ellipse cx="{e.X + e.Width / 2}" cy="{e.Y + e.Height / 2}" rx="{e.Width / 2}" ry="{e.Height / 2}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}"/>""",

            LineShape l =>
                $"""  <line x1="{l.X}" y1="{l.Y}" x2="{l.X2}" y2="{l.Y2}" stroke="{stroke}" stroke-width="{sw}"/>""",

            PolygonShape p =>
                $"""  <polygon points="{PointsToString(p.Points)}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}"/>""",

            _ => string.Empty
        };
    }

    private static string PointsToString(IEnumerable<Point> points) =>
        string.Join(" ", points.Select(p => $"{p.X},{p.Y}"));

    private static string ColorToHex(Avalonia.Media.Color color) =>
        $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
