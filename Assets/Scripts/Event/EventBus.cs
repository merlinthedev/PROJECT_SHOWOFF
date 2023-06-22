
namespace EventBus {
    public abstract class Event { }

    public class RockReceivedEvent : Event { }
}

public class EventBus<T> where T : EventBus.Event {
    private static System.Action<T> onEventRaised;

    public static void Subscribe(System.Action<T> action) {
        onEventRaised += action;
    }

    public static void Unsubscribe(System.Action<T> action) {
        onEventRaised -= action;
    }

    public static void Raise(T e) {
        onEventRaised?.Invoke(e);
    }
}