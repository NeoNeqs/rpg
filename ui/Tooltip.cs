using Godot;
using RPG.global;
using RPG.scripts.inventory;

namespace RPG.ui;

public partial class Tooltip : PanelContainer {
    private Viewport _viewport = null!;

    private readonly Vector2 _offset = new(20, 0);

    [Export] private PoorTextLabel _label = null!;

    public override void _Ready() {
        _viewport = GetViewport();
    }

    public override void _Process(double pDelta) {
        if (!Visible) {
            return;
        }

        Vector2 mousePos = _viewport.GetMousePosition();
        Vector2 newPos = mousePos + _offset;
        Vector2 testBounds = newPos + Size;
        Rect2 viewportRect = _viewport.GetVisibleRect();

        if (testBounds.X > viewportRect.Size.X) {
            newPos.X = mousePos.X - Size.X - _offset.X;
        }

        if (testBounds.Y > viewportRect.Size.Y) {
            newPos.Y = mousePos.Y - Size.Y - _offset.Y;
        }

        GlobalPosition = newPos;
    }

    public void Update(GizmoStack? pGizmoStack) {
        if (pGizmoStack?.Gizmo is null) {
            _label.Clear();
            Visible = false;
            SetProcess(false);
            return;
        }

        // _label.Update(pGizmoStack.Gizmo.GetTooltip());
        Logger.UI.Critical("Gizmos don't have tooltips yet...");

        Size = CustomMinimumSize;
        Visible = true;

        SetProcess(true);
    }
}