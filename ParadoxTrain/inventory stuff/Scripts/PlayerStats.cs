using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayerStats : MonoBehaviour
{
    public int attack, defense, agility, intelligence;

    [SerializeField]
    private TMP_Text attackText, defenseText, agilityText, intelligenceText;

    [SerializeField]
    private TMP_Text attackPreText, defensePreText, agilityPreText, intelligencePreText;

    [SerializeField]
    private Image previewImage;

    [SerializeField]
    private GameObject selectedItemStats;

    [SerializeField]
    private GameObject selectedItemImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateEquipmentstats();
    }

    public void UpdateEquipmentstats()
    {
        attackText.text = attack.ToString();
        defenseText.text = defense.ToString();
        agilityText.text = agility.ToString();
        intelligenceText.text = intelligence.ToString();
    }
    public void PreviewEquippmentStats(int attack, int defense, int agility, int intelligence, Sprite itemSprite)
    {
      attackPreText.text = attack.ToString();
      defensePreText.text = defense.ToString();
      agilityPreText.text = agility.ToString();
      intelligencePreText.text = intelligence.ToString();

      previewImage.sprite = itemSprite;
      selectedItemImage.SetActive(true);
      selectedItemImage.SetActive(true);
    }

    public void TurnOffPreviewStats()
    {
        selectedItemImage.SetActive(false);
        selectedItemImage.SetActive(false);
    }


}
