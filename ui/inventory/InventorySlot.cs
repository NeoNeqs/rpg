using System;
using Godot;
using RPG.Global;
using RPG.scripts.inventory;

namespace RPG.ui.inventory;

[GlobalClass]
public partial class InventorySlot : Control {
    [Signal]
    public delegate void LeftMouseButtonPressedEventHandler();

    [Signal]
    public delegate void RightMouseButtonPressedEventHandler();

    [Signal]
    public delegate void HoveredEventHandler();

    [Signal]
    public delegate void UnhoveredEventHandler();


    [Export] public Label TextLabel;
    [Export] public TextureRect IconHolder;

    public override void _GuiInput(InputEvent pEvent) {
        if (pEvent is not InputEventMouseButton mouseButton) {
            return;
        }

        if (!mouseButton.IsReleased()) {
            return;
        }

        switch (mouseButton.ButtonIndex) {
            case MouseButton.Left:
                EmitSignalLeftMouseButtonPressed();
                break;
            case MouseButton.Right:
                EmitSignalRightMouseButtonPressed();
                break;
            case MouseButton.None:
            case MouseButton.Middle:
            case MouseButton.WheelUp:
            case MouseButton.WheelDown:
            case MouseButton.WheelLeft:
            case MouseButton.WheelRight:
            case MouseButton.Xbutton1:
            case MouseButton.Xbutton2:
            default:
                break;
        }
    }

    public override void _Notification(int pWhat) {
        switch (pWhat) {
            case (int)NotificationMouseEnter: 
                EmitSignalHovered();
                break;
            case (int)NotificationMouseExit:
                EmitSignalUnhovered();
                break;
        }
    }

    public void SetOnCooldown(ulong pCooldownInMicroseconds) {
        CooldownDisplay? cooldownDisplay = GetCooldownDisplay();

        cooldownDisplay?.Start(pCooldownInMicroseconds);
    }

    public void ResetCooldown() {
        CooldownDisplay? cooldownDisplay = GetCooldownDisplay();

        cooldownDisplay?.Reset();
    }

    private CooldownDisplay? GetCooldownDisplay() {
        CooldownDisplay? display = IconHolder.GetNodeOrNull<CooldownDisplay>("CooldownDisplay");
        
        if (display is null) {
            Logger.UI.Error(
                $"Slot id='{GetInstanceId()}', at index='{GetIndex()}' does not have a '{nameof(CooldownDisplay)}' node attached.");
            return null;
        }
        
        return display;
    }

    public virtual void Update(GizmoStack? pGizmoStack) {
        if (pGizmoStack is null) {
            Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        } else {
            Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }

    // private void OnMouseEntered() {
    //     EmitSignalMouseEntered();
    // }
    //
    // private void OnMouseExited() {
    //     EmitSignalMouseExited();
    // }
}