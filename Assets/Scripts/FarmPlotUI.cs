using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FarmPlotUI : MonoBehaviour
{
    [SerializeField] private Image cropImage;
    [SerializeField] private Image growthProgressBar;
    [SerializeField] private Button plotButton;
    
    private int plotIndex;
    
    public void Initialize(int index)
    {
        plotIndex = index;
        growthProgressBar.fillAmount = 0;
        
        plotButton.onClick.AddListener(OnPlotClicked);
    }
    
    public void UpdateUI(FarmPlot plot)
    {
        if (plot.HasCrop)
        {
            if (cropImage.gameObject.activeSelf == false)
            {
                cropImage.gameObject.SetActive(true);
                cropImage.transform.localScale = Vector3.zero;
                cropImage.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
            
            CropData cropData = CropDatabase.GetCrop(plot.CropType);
            ImageLoader.Instance.LoadImage(cropData.ImageUrl, cropImage);
            
            float growthPercent = (float)plot.Growth / cropData.GrowthTime;
            growthProgressBar.DOFillAmount(growthPercent, 0.3f);
            
            if (plot.IsReadyToHarvest)
            {
                plotButton.GetComponent<Image>().color = new Color(1f, 0.92f, 0.016f);
                
                if (!DOTween.IsTweening(cropImage.transform))
                {
                    cropImage.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
            }
            else
            {
                plotButton.GetComponent<Image>().color = new Color(0.56f, 0.74f, 0.56f);
                
                if (DOTween.IsTweening(cropImage.transform))
                {
                    DOTween.Kill(cropImage.transform);
                    cropImage.transform.localScale = Vector3.one;
                }
            }
        }
        else
        {
            cropImage.gameObject.SetActive(false);
            
            growthProgressBar.fillAmount = 0;
            plotButton.GetComponent<Image>().color = new Color(0.96f, 0.87f, 0.7f);
            
            if (DOTween.IsTweening(cropImage.transform))
            {
                DOTween.Kill(cropImage.transform);
            }
        }
    }
    
    private void OnPlotClicked()
    {
        FarmPlot plot = GameManager.Instance.Farm[plotIndex];
        
        if (plot.HasCrop)
        {
            if (plot.IsReadyToHarvest)
            {
                cropImage.transform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                    GameManager.Instance.HarvestPlot(plotIndex);
                    NotificationSystem.Instance.ShowNotification("Harvest collected!", Color.green);
                });
            }
            else
            {
                CropData cropData = CropDatabase.GetCrop(plot.CropType);
                int daysLeft = cropData.GrowthTime - plot.Growth;
                
                NotificationSystem.Instance.ShowNotification($"This {cropData.Name} needs {daysLeft} more days to grow!", Color.yellow);
            }
        }
        else
        {
            UIManager.Instance.ShowPlantingMenu(plotIndex);
            NotificationSystem.Instance.ShowNotification("Select a crop to plant", new Color(0.4f, 0.6f, 1f));
        }
    }
} 