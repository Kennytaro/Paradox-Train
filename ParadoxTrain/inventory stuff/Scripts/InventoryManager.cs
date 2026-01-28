using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryMenu;
    public GameObject EquipmentMenu;
   
    public ItemSlot[] itemSlot;
    public EquipmentSlot[] equipmentSlot;
    public EquippedSlot[] equippedSlot;

    public ItemSO[] itemSOs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("InventoryMenu"))
        {
            Inventory();
        }

        if(Input.GetButtonDown("EquipmentMenu"))
        {
            Equipment();
        }

    }

    void Inventory()
    {
        if(InventoryMenu.activeSelf)
        {
            Time.timeScale = 1;
            InventoryMenu.SetActive(false);
            EquipmentMenu.SetActive(false);
        }
        
        else
        {
            Time.timeScale = 0;
            InventoryMenu.SetActive(true);
            EquipmentMenu.SetActive(false);
        }
    }

    void Equipment()
    {
        if(EquipmentMenu.activeSelf)
        {
            Time.timeScale = 1;
            InventoryMenu.SetActive(false);
            EquipmentMenu.SetActive(false);
        }
        
        else
        {
            Time.timeScale = 0;
            InventoryMenu.SetActive(false);
            EquipmentMenu.SetActive(true);
        }
    }

    public bool UseItem(string itemName)
    {
        for (int i = 0; i < itemSOs.Length; i++)
        {
            if(itemSOs[i].itemName == itemName)
            {
               bool useable = itemSOs[i].UseItem();
               return useable;
            }
        }
        return false;
    }


    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType )
    {
        if(itemType == ItemType.consumable || itemType == ItemType.crafting || itemType == ItemType.collectable)
        {
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].isFull == false && itemSlot[i].itemName == itemName || itemSlot[i].quantity == 0)
                {
                    int leftOverItems = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription, itemType);
                    if(leftOverItems > 0)
                    {
                       leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription, itemType);   
                    }
                    return leftOverItems;
                }
            }
            return quantity;     
        }

        else
        {
            for (int i = 0; i < equipmentSlot.Length; i++)
            {
                if (equipmentSlot[i].isFull == false && equipmentSlot[i].itemName == itemName || equipmentSlot[i].quantity == 0)
                {
                    int leftOverItems = equipmentSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription, itemType);
                    if(leftOverItems > 0)
                    {
                       leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription, itemType);   
                    }
                    return leftOverItems;
                }
            }
            return quantity;     
        }

    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            equipmentSlot[i].selectedShader.SetActive(false);
            equipmentSlot[i].thisItemSelected = false;
        }

        for (int i = 0; i < equippedSlot.Length; i++)
        {
            equippedSlot[i].selectedShader.SetActive(false);
            equippedSlot[i].thisItemSelected = false;
        }
    }
}



public enum ItemType
{
    consumable,
    crafting,
    collectable,
    tassel1,
    tassel2,
    tassel3,
    cloak,
    scythe,
    none,
};