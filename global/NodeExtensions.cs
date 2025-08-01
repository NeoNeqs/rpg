using Godot;

namespace RPG.global;

public static class NodeExtensions {
    public static void AddChildOwned(this Node pParent, Node pNode, Node pOwner) {
        pParent.AddChild(pNode);
        pNode.Owner = pOwner;
    }
}