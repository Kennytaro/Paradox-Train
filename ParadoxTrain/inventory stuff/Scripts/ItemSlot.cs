using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    //=====ITEM DATA======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;
    public ItemType itemType;

    [SerializeField]
    private int MaxNumberOfItems;

    //=====ITEM SLOT=====//
    [SerializeField]
    private TMP_Text quantityText;

    [SerializeField]
    private Image itemImage;

    //=====ITEM DESCRIPTION SLOT=====//
    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;



    public GameObject selectedShader;
    public bool thisItemSelected;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
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
        this.quantity += quantity;
        if(this.quantity >= MaxNumberOfItems)
        {
        quantityText.text = MaxNumberOfItems.ToString();
        quantityText.enabled = true;
        isFull = true;    
        

        //Return the LEFTOVERS
        int extraItems = this.quantity - MaxNumberOfItems;
        this.quantity = MaxNumberOfItems;
        return extraItems;
        }

        //Update QUANTITY TEXT
        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;

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
        if (thisItemSelected)
        {
          bool usable =inventoryManager.UseItem(itemName);
          if(usable)
            {
            this.quantity -= 1;
            quantityText.text = this.quantity.ToString();
            if(this.quantity <= 0)
               {
                 EmptySlot();  
               }
            }

        }

        else
        {
         
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            itemDescriptionNameText.text = itemName;
            itemDescriptionText.text = itemDescription;
            itemDescriptionImage.sprite = itemSprite;
            if(itemDescriptionImage.sprite == null)
            {
                itemDescriptionImage.sprite = emptySprite;
            }
        }
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
        quantityText.enabled = false;
        quantityText.text = "0";
        itemImage.sprite = emptySprite;
        itemDescriptionNameText.text = "";
        itemDescriptionText.text = "";
        itemDescriptionImage.sprite = emptySprite;

        // Reset selection state
        if (selectedShader != null) selectedShader.SetActive(false);
        thisItemSelected = false;
    }




    public void OnRightClick()
    {
        Debug.Log("Right Clicked on " + itemName);
    }
}
