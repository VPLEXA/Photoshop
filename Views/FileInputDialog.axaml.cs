using Avalonia.Controls;
using Avalonia.Input;

namespace VectorEditor.Views;

public partial class FileInputDialog : Window
{
    public FileInputDialog() => InitializeComponent();

    public static FileInputDialog ForSave(string title, string defaultFileName)
    {
        var dialog = new FileInputDialog();
        dialog.Title = title;
        dialog.PromptLabel.Text = "Путь к файлу:";
        dialog.PathBox.Text = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            defaultFileName);
        dialog.PathBox.SelectAll();
        return dialog;
    }

    public static FileInputDialog ForOpen(string title)
    {
        var dialog = new FileInputDialog();
        dialog.Title = title;
        dialog.PromptLabel.Text = "Путь к файлу:";
        dialog.PathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return dialog;
    }

    private void OnOk(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Close(PathBox.Text?.Trim());

    private void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Close(null);

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return) Close(PathBox.Text?.Trim());
        if (e.Key == Key.Escape) Close(null);
    }
}
