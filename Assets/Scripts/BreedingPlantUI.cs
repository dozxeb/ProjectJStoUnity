using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BreedingPlantUI : MonoBehaviour
{
    [SerializeField] private Image plantImage;
    [SerializeField] private TextMeshProUGUI plantNameText;
    [SerializeField] private Image backgroundImage;
    
    public Image PlantImage => plantImage;
    
    public void SetPlant(string cropType)
    {
        CropData cropData = CropDatabase.GetCrop(cropType);
        
        if (cropData != null)
        {
            ImageLoader.Instance.LoadImage(cropData.ImageUrl, plantImage);
            plantNameText.text = cropData.Name;
        }
        
        SetSelected(false);
    }
    
    public void SetSelected(bool selected)
    {
        if (selected)
        {
            backgroundImage.color = new Color(0.3f, 0.7f, 0.3f);
            plantImage.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            backgroundImage.color = Color.white;
            plantImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
    }
} 