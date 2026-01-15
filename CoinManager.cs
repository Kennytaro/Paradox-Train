using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public int totalCoins;
    public TMP_Text coinText;
    public int maxCoins = 20;

    void  Start()
    {
        coinText.text = "Coins: " + totalCoins;
    }

    public void ChangeCoins(int amount)
    {
        totalCoins += amount;
        coinText.text = "Coins: " + totalCoins;
    }
}
