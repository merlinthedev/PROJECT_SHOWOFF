using UnityEngine;

namespace EventBus {
    public abstract class Event {
    }

    public class NewSceneTriggeredEvent : Event {
    }

    public class NewSceneLoadedEvent : Event {
    }

    public class VodyanoyFinishedWalkingEvent : Event {
    }

    public class NextJumpIsCutsceneEvent : Event {
        public Transform destination;
        public System.Action callback;
    }

    public class TobaccoThrowEvent : Event {
    }

    public class VodyanoyLocationEvent : Event {
        public Vector3 location;
    }

    public class BoatDestinationReachedEvent : Event {
        public int index;
        public bool last;
    }

    public class PlayerBoatEnter : Event { }

    public class VodyanoyAwakeEvent : Event { }

    public class TobaccoPickupEvent : Event { }
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