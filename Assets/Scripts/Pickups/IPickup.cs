// interface for all pickups

public interface IPickup {
    void OnPickup(Player player);
    void OnDrop();
    void OnThrow();
    void ToString();
}