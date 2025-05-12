using Godot;

namespace RPG.global;

// TODO: test if Signals work in a non-godot script like this:
public static class EventBus  {
    //public static EventBus Instance;
    
    [Signal]
    public delegate void TestEventHandler();

    // public EventBus() {
    //     Instance = this;
    // }
}