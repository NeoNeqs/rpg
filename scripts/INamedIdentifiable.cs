using Godot;

namespace RPG.scripts;

public interface INamedIdentifiable {
    public StringName Id { get; }
    public string DisplayName { get; }
}