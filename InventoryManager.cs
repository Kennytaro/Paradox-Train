using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryMenu;
    private bool menuActivated;
    public ItemSlot[] itemSlot;

    public ItemSO[] itemSOs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Inventory") && menuActivated)
        {
            Time.timeScale = 1;
            InventoryMenu.SetActive(false);
            menuActivated = false;
        }
        
        else if(Input.GetButtonDown("Inventory") && !menuActivated)
        {
            Time.timeScale = 0;
            InventoryMenu.SetActive(true);
            menuActivated = true;
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


    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        Debug.Log("ItemName: " + itemName + " Quantity: " + quantity + " ItemSprite: " + itemSprite);
       for (int i = 0; i < itemSlot.Length; i++)
       {
           if (itemSlot[i].isFull == false && itemSlot[i].itemName == itemName || itemSlot[i].quantity == 0)
           {
               int leftOverItems = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription);
               if(leftOverItems > 0)
               {
                  leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription);   
               }
               return leftOverItems;
           }
       }
       return quantity;
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }
    }
}
