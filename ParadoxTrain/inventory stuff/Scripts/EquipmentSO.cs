using UnityEngine;

[CreateAssetMenu]
public class EquipmentSO : ScriptableObject
{
    public string itemName;
    public int attack, defense, agility, intelligence;
    [SerializeField]
    private Sprite itemSprite;

    public void PreviewEquipment()
    {
        GameObject.Find("StatManager").GetComponent<PlayerStats>().PreviewEquippmentStats(attack, defense, agility, intelligence, itemSprite);
    }

    public void EquipItem()
    {
        // UpdateStats
        PlayerStats playerstats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        playerstats.attack += attack;
        playerstats.defense += defense; 
        playerstats.agility += agility;
        playerstats.intelligence += intelligence;

        playerstats.UpdateEquipmentstats();
    }
    public void UnEquipItem()
    {
      // UpdateStats
        PlayerStats playerstats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        playerstats.attack -= attack;
        playerstats.defense -= defense; 
        playerstats.agility -= agility;
        playerstats.intelligence -= intelligence;

        playerstats.UpdateEquipmentstats();
    }

}
