using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace VectorEditor.Views;

public partial class ColorPickerWindow : Window
{
    private Color _color;
    private bool _updating;

    public ColorPickerWindow() : this(Colors.Black) { }

    public ColorPickerWindow(Color initialColor)
    {
        InitializeComponent();
        _color = initialColor;
        ApplyColorToSliders();
        UpdatePreview();
    }

    private void ApplyColorToSliders()
    {
        _updating = true;
        SliderR.Value = _color.R;
        SliderG.Value = _color.G;
        SliderB.Value = _color.B;
        LabelR.Text = _color.R.ToString();
        LabelG.Text = _color.G.ToString();
        LabelB.Text = _color.B.ToString();
        _updating = false;
    }

    private void OnSliderChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_updating) return;
        _color = new Color(255, (byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
        LabelR.Text = ((byte)SliderR.Value).ToString();
        LabelG.Text = ((byte)SliderG.Value).ToString();
        LabelB.Text = ((byte)SliderB.Value).ToString();
        UpdatePreview();
    }

    private void OnPresetClick(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border) return;
        var parts = border.Tag?.ToString()?.Split(',');
        if (parts?.Length != 3) return;
        _color = new Color(255, byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]));
        ApplyColorToSliders();
        UpdatePreview();
    }

    private void UpdatePreview() =>
        PreviewBorder.Background = new SolidColorBrush(_color);

    private void OnOk(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Close(_color);

    private void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Close(null);
}
