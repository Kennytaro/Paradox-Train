using UnityEngine;

public class Coin : MonoBehaviour
{
    public CoinManager coinManager;
    public int value;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        coinManager.ChangeCoins(value);
        Destroy(gameObject);
    }
}
