namespace RPG.scripts;

public interface INamedIdentifiable {
    public Id Id { get; }
    public string DisplayName { get; }
}