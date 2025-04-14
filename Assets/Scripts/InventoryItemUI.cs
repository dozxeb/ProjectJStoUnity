using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    
    public void SetItem(string cropType)
    {
        CropData cropData = CropDatabase.GetCrop(cropType);
        
        if (cropData != null)
        {
            ImageLoader.Instance.LoadImage(cropData.ImageUrl, itemImage);
            itemNameText.text = cropData.Name;
        }
    }
} 