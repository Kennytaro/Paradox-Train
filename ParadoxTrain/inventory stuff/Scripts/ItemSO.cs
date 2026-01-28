using UnityEngine;



[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statToChange = new StatToChange();
    public int amountToChangeStat;

    public AttributesToChange attributesToChange = new AttributesToChange();
    public int amountToChangeAttribute;

    public bool UseItem()
    {
        if(statToChange == StatToChange.coins)
        {
           CoinManager playerCoin = GameObject.Find ("CoinManager").GetComponent<CoinManager>();
           if (playerCoin.totalCoins >= playerCoin.maxCoins)
           {
               return false;
           }
            else
            {
                playerCoin.ChangeCoins(amountToChangeStat);
                return true;

            }
        }







            if(statToChange == StatToChange.mana)
        {
           //GameObject.Find ("ManaManager").GetComponent<PlayerMana>().ChangeMana(amountToChangeStat);
        }
            if(statToChange == StatToChange.stamina)
        {
           //GameObject.Find ("StaminaManager").GetComponent<PlayerStamina>().ChangeStamina(amountToChangeStat);
        }
        return false;
    }

    public enum StatToChange
    {
        none,
        coins,
        mana,
        stamina
    };
        public enum AttributesToChange
    {
        none,
        strength,
        defense,
        intelligence,
        agility
    };
}
