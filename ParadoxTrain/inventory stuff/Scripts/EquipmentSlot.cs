using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler
{
    //=====ITEM DATA======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;
    public ItemType itemType;



    //=====ITEM SLOT=====//


    [SerializeField]
    private Image itemImage;

    //=====EQUIPPED SLOTS=====//
    [SerializeField]
    private EquippedSlot tasselSlot1, tasselSlot2, tasselSlot3;


    public GameObject selectedShader;
    public bool thisItemSelected;

    private InventoryManager inventoryManager;
    private EquipmentSOLibrary equipmentSOLibrary; 

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
        equipmentSOLibrary = GameObject.Find("InventoryCanvas").GetComponent<EquipmentSOLibrary>();

    }


public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType )
    {
        // Check to see if the slot is already full
        if(isFull)
        {
            // If it is full, return the quantity back to the InventoryManager
            return quantity;
        }

        // Update ITEM TYPE
        this.itemType = itemType;

        //Update NAME
        this.itemName = itemName;

        // Update IMAGE
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;

        // Update DESCRIPTION
        this.itemDescription= itemDescription;

        // Update QUANTITY
        this.quantity = 1;
        isFull = true;

        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
          OnLeftClick();  
        }
        if(eventData.button == PointerEventData.InputButton.Right)
        {
          OnRightClick();
        }
    }
    public void OnLeftClick()
    {
        if (isFull)
        {
                if (thisItemSelected)
            {
                EquipGear();
            }

            else
            {
         
                inventoryManager.DeselectAllSlots();
                selectedShader.SetActive(true);
                thisItemSelected = true;
                for (int i = 0; i < equipmentSOLibrary.equipmentSO.Length; i++)
                {
                    if(equipmentSOLibrary.equipmentSO[i].itemName == this.itemName)
                    {
                    equipmentSOLibrary.equipmentSO[i].PreviewEquipment();
                    }
                }

            }  
        }
        else
        {
            GameObject.Find("StatManager").GetComponent<PlayerStats>().TurnOffPreviewStats();
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
        }
    
    }
    private void EquipGear()
    {
        if(itemType == ItemType.tassel1)
            tasselSlot1.EquipGear(itemSprite, itemName, itemDescription);
        if(itemType == ItemType.tassel2)
            tasselSlot2.EquipGear(itemSprite, itemName, itemDescription);
        if(itemType == ItemType.tassel3)
            tasselSlot3.EquipGear(itemSprite, itemName, itemDescription);

        EmptySlot();
    }
    private void EmptySlot()
    {
        // Reset internal data
        itemName = "";
        itemDescription = "";
        itemSprite = null;
        quantity = 0;
        isFull = false;

        // Reset visuals
        itemImage.sprite = emptySprite;


        // Reset selection state
        if (selectedShader != null) selectedShader.SetActive(false);
        thisItemSelected = false;
    }




    public void OnRightClick()
    {
        Debug.Log("Right Clicked on " + itemName);
    }
}
