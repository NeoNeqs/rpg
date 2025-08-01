using System;
using Godot;
using Godot.Collections;

namespace RPG.ui.debug;

[GlobalClass]
public partial class ObjectBrowser : Tree {
    public override void _Ready() {
        TreeItem root = CreateItem(null);
    }


    public TreeItem CreateObjectProperty(TreeItem pParent, string pName, GodotObject? pObject) {
        TreeItem item = CreateProperty(pParent, pName + pObject?.GetInstanceId());
        item.SetCellMode(0, TreeItem.TreeCellMode.String);

        if (pObject is null) {
            item.SetText(1, "<null>");
            return item;
        }

        item.SetText(1, pObject.GetType().Name);

        TreeItem dummy = CreateItem(item);

        Connect(
            Tree.SignalName.ItemCollapsed,
            Callable.From((TreeItem pItem) => {
                dummy.Free();
                CreateObjectProperties(pItem, pObject);
            }),
            (uint)ConnectFlags.OneShot | (uint)ConnectFlags.Deferred
        );

        return item;
    }

    private TreeItem CreateNilProperty(TreeItem pParent, string pName) {
        TreeItem item = CreateProperty(pParent, pName);
        item.SetText(1, "<null>");
        return item;
    }

    private TreeItem CreateBoolProperty(TreeItem pParent, string pName, bool pValue) {
        TreeItem item = CreateProperty(pParent, pName);
        item.SetCellMode(1, TreeItem.TreeCellMode.Check);
        item.SetText(1, "On");
        item.SetChecked(1, pValue);
        return item;
    }

    private TreeItem CreateIntProperty(TreeItem pParent, string pName, long pValue) {
        TreeItem item = CreateProperty(pParent, pName);
        item.SetCellMode(1, TreeItem.TreeCellMode.Range);
        item.SetRange(1, pValue);
        return item;
    }

    private TreeItem CreateDoubleProperty(TreeItem pParent, string pName, double pValue) {
        TreeItem item = CreateProperty(pParent, pName);
        item.SetCellMode(1, TreeItem.TreeCellMode.Range);
        item.SetRange(1, pValue);
        return item;
    }


    private TreeItem CreateStringProperty(TreeItem pParent, string pName, Variant pValue) {
        TreeItem item = CreateProperty(pParent, pName);
        item.SetCellMode(1, TreeItem.TreeCellMode.String);
        item.SetText(1, pValue.AsString());
        return item;
    }

    private TreeItem CreateProperty(TreeItem pParent, string pName) {
        TreeItem item = CreateItem(pParent);
        item.Collapsed = true;
        item.SetText(0, pName);

        return item;
    }

    private void CreateObjectProperties(TreeItem pParent, GodotObject pObject) {
        Array<Dictionary> properties = pObject.GetPropertyList();

        for (int i = properties.Count - 1; i >= 0; --i) {
            Dictionary property = properties[i];
            var stringName = property["name"].As<StringName>();
            string name = stringName.ToString();

            Variant value = pObject.Get(stringName);
            var propertyType = property["type"].As<Variant.Type>();

            switch (propertyType) {
                case Variant.Type.Nil:
                    CreateNilProperty(pParent, name);
                    break;
                case Variant.Type.Bool:
                    CreateBoolProperty(pParent, name, value.AsBool());
                    break;
                case Variant.Type.Int:
                    CreateIntProperty(pParent, name, value.AsInt64());
                    break;
                case Variant.Type.Float:
                    CreateDoubleProperty(pParent, name, value.AsDouble());
                    break;
                case Variant.Type.String:
                case Variant.Type.Vector2:
                case Variant.Type.Vector2I:
                case Variant.Type.Rect2:
                case Variant.Type.Rect2I:
                case Variant.Type.Vector3:
                case Variant.Type.Vector3I:
                case Variant.Type.Transform2D:
                case Variant.Type.Vector4:
                case Variant.Type.Vector4I:
                case Variant.Type.Plane:
                case Variant.Type.Quaternion:
                case Variant.Type.Aabb:
                case Variant.Type.Basis:
                case Variant.Type.Transform3D:
                case Variant.Type.Projection:
                case Variant.Type.Color:
                case Variant.Type.StringName:
                case Variant.Type.NodePath:
                    CreateStringProperty(pParent, name, value);
                    break;
                case Variant.Type.Rid:
                    break;
                case Variant.Type.Object:
                    break;
                case Variant.Type.Callable:
                    break;
                case Variant.Type.Signal:
                    break;
                case Variant.Type.Dictionary:
                    break;
                case Variant.Type.Array:
                    break;
                case Variant.Type.PackedByteArray:
                    break;
                case Variant.Type.PackedInt32Array:
                    break;
                case Variant.Type.PackedInt64Array:
                    break;
                case Variant.Type.PackedFloat32Array:
                    break;
                case Variant.Type.PackedFloat64Array:
                    break;
                case Variant.Type.PackedStringArray:
                    break;
                case Variant.Type.PackedVector2Array:
                    break;
                case Variant.Type.PackedVector3Array:
                    break;
                case Variant.Type.PackedColorArray:
                    break;
                case Variant.Type.PackedVector4Array:
                    break;
                case Variant.Type.Max:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}