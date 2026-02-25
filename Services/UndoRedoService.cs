using VectorEditor.Models;

namespace VectorEditor.Services;

public class UndoRedoService
{
    private const int MaxSteps = 5;
    private readonly Stack<List<ShapeBase>> _undoStack = new();
    private readonly Stack<List<ShapeBase>> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void SaveState(IEnumerable<ShapeBase> shapes)
    {
        _undoStack.Push(shapes.Select(s => s.Clone()).ToList());
        if (_undoStack.Count > MaxSteps)
            TrimStack(_undoStack);
        _redoStack.Clear();
    }

    public List<ShapeBase>? Undo(IEnumerable<ShapeBase> currentShapes)
    {
        if (!CanUndo) return null;
        _redoStack.Push(currentShapes.Select(s => s.Clone()).ToList());
        return _undoStack.Pop();
    }

    public List<ShapeBase>? Redo(IEnumerable<ShapeBase> currentShapes)
    {
        if (!CanRedo) return null;
        _undoStack.Push(currentShapes.Select(s => s.Clone()).ToList());
        return _redoStack.Pop();
    }

    private static void TrimStack(Stack<List<ShapeBase>> stack)
    {
        var items = stack.ToArray();
        stack.Clear();
        foreach (var item in items.Take(MaxSteps))
            stack.Push(item);
    }
}
