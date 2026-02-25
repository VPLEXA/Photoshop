using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Media;
using VectorEditor.Models;

namespace VectorEditor.Services;

public static class ProjectSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new ColorJsonConverter(), new PointJsonConverter() }
    };

    public static string Serialize(IEnumerable<ShapeBase> shapes) =>
        JsonSerializer.Serialize(shapes.Select(ShapeToDto), Options);

    public static List<ShapeBase> Deserialize(string json)
    {
        var dtos = JsonSerializer.Deserialize<List<ShapeDto>>(json, Options) ?? new();
        return dtos.Select(DtoToShape).ToList();
    }

    private static ShapeDto ShapeToDto(ShapeBase shape) => new()
    {
        Type = shape.ShapeType,
        X = shape.X, Y = shape.Y,
        Width = shape.Width, Height = shape.Height,
        FillColor = shape.FillColor,
        StrokeColor = shape.StrokeColor,
        StrokeThickness = shape.StrokeThickness,
        X2 = shape is LineShape l ? l.X2 : 0,
        Y2 = shape is LineShape l2 ? l2.Y2 : 0,
        Points = shape is PolygonShape p ? p.Points : null
    };

    private static ShapeBase DtoToShape(ShapeDto dto)
    {
        ShapeBase shape = dto.Type switch
        {
            ShapeType.Rectangle => new RectangleShape(),
            ShapeType.Ellipse => new EllipseShape(),
            ShapeType.Line => new LineShape { X2 = dto.X2, Y2 = dto.Y2 },
            ShapeType.Polygon => new PolygonShape { Points = dto.Points ?? new() },
            _ => new RectangleShape()
        };

        shape.X = dto.X;
        shape.Y = dto.Y;
        shape.Width = dto.Width;
        shape.Height = dto.Height;
        shape.FillColor = dto.FillColor;
        shape.StrokeColor = dto.StrokeColor;
        shape.StrokeThickness = dto.StrokeThickness;

        return shape;
    }

    private class ShapeDto
    {
        public ShapeType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Color FillColor { get; set; }
        public Color StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public List<Point>? Points { get; set; }
    }
}

public class ColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString() ?? "#FF000000";
        return Color.Parse(str);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}

public class PointJsonConverter : JsonConverter<Point>
{
    public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString() ?? "0,0";
        var parts = str.Split(',');
        return new Point(double.Parse(parts[0]), double.Parse(parts[1]));
    }

    public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options) =>
        writer.WriteStringValue($"{value.X},{value.Y}");
}
