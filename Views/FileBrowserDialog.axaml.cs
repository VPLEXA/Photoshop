using Avalonia.Controls;
using Avalonia.Input;

namespace VectorEditor.Views;

public record FileEntry(string Icon, string Name, string FullPath, bool IsDirectory);

public partial class FileBrowserDialog : Window
{
    private string _currentDirectory;
    private readonly bool _isSaveMode;
    private readonly string _filter;

    public FileBrowserDialog() : this(false, string.Empty) { }

    private FileBrowserDialog(bool isSaveMode, string filter)
    {
        _isSaveMode = isSaveMode;
        _filter = filter;
        _currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        InitializeComponent();
        FileNameRow.IsVisible = isSaveMode;
        OkButton.Content = isSaveMode ? "Сохранить" : "Открыть";
        Opened += (_, _) => NavigateTo(_currentDirectory);
    }

    public static FileBrowserDialog ForOpen(string title, string filter)
    {
        var d = new FileBrowserDialog(false, filter);
        d.Title = title;
        return d;
    }

    public static FileBrowserDialog ForSave(string title, string filter, string defaultName)
    {
        var d = new FileBrowserDialog(true, filter);
        d.Title = title;
        d.FileNameBox.Text = defaultName;
        return d;
    }

    private void NavigateTo(string path)
    {
        if (!Directory.Exists(path)) return;
        _currentDirectory = path;
        PathBox.Text = path;
        UpButton.IsEnabled = Directory.GetParent(path) != null;

        var items = new List<FileEntry>();

        foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d))
            items.Add(new FileEntry("📁", Path.GetFileName(dir)!, dir, true));

        foreach (var file in Directory.GetFiles(path).OrderBy(f => f))
        {
            if (!MatchesFilter(file)) continue;
            items.Add(new FileEntry("📄", Path.GetFileName(file)!, file, false));
        }

        FileList.ItemsSource = items;
    }

    private bool MatchesFilter(string filePath) =>
        string.IsNullOrEmpty(_filter) ||
        _filter.Split(';').Any(ext => filePath.EndsWith(ext.TrimStart('*'), StringComparison.OrdinalIgnoreCase));

    private void OnUpClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parent = Directory.GetParent(_currentDirectory);
        if (parent != null) NavigateTo(parent.FullName);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (FileList.SelectedItem is not FileEntry entry) return;
        if (!entry.IsDirectory && _isSaveMode)
            FileNameBox.Text = entry.Name;
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (FileList.SelectedItem is not FileEntry entry) return;
        if (entry.IsDirectory)
            NavigateTo(entry.FullPath);
        else
            Confirm();
    }

    private void OnOk(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Confirm();

    private void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(null);

    private void OnFileNameKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return) Confirm();
    }

    private void Confirm()
    {
        if (_isSaveMode)
        {
            var name = FileNameBox.Text?.Trim();
            if (string.IsNullOrEmpty(name)) return;
            Close(Path.Combine(_currentDirectory, name));
        }
        else
        {
            if (FileList.SelectedItem is FileEntry entry && !entry.IsDirectory)
                Close(entry.FullPath);
        }
    }
}
