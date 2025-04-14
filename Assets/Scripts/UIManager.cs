using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    [SerializeField] private GameObject characterSelectionScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject breedingScreen;
    [SerializeField] private GameObject plantingMenuPanel;

    [Header("Character Selection")]
    [SerializeField] private Button boyButton;
    [SerializeField] private Button girlButton;
    [SerializeField] private Image boyImage;
    [SerializeField] private Image girlImage;

    [Header("Game UI")]
    [SerializeField] private Image playerAvatar;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI seedsText;
    [SerializeField] private TextMeshProUGUI dayText;

    [Header("Farm")]
    [SerializeField] private Transform farmLandContainer;
    [SerializeField] private GameObject plotPrefab;

    [Header("Actions")]
    [SerializeField] private Button wheatButton;
    [SerializeField] private Button carrotButton;
    [SerializeField] private Button potatoButton;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private Button harvestAllButton;
    [SerializeField] private Button breedButton;

    [Header("Inventory")]
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("Breeding")]
    [SerializeField] private Transform breedingPlantsContainer;
    [SerializeField] private GameObject breedingPlantPrefab;
    [SerializeField] private Button cancelBreedingButton;

    [Header("Planting Menu")]
    [SerializeField] private Button closePlantingMenuButton;
    [SerializeField] private Button plantWheatButton;
    [SerializeField] private Button plantCarrotButton;
    [SerializeField] private Button plantPotatoButton;

    private List<FarmPlotUI> farmPlots = new List<FarmPlotUI>();
    private List<Image> selectedPlantsForBreeding = new List<Image>();
    private int selectedPlotForPlanting = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeUI();
        SetupListeners();
        ShowCharacterSelection();
    }

    private void InitializeUI()
    {
        ImageLoader.Instance.LoadImage("https://cdn-icons-png.flaticon.com/512/4140/4140048.png", boyImage);
        ImageLoader.Instance.LoadImage("https://cdn-icons-png.flaticon.com/512/4140/4140047.png", girlImage);
        
        for (int i = 0; i < 15; i++)
        {
            GameObject plotObject = Instantiate(plotPrefab, farmLandContainer);
            FarmPlotUI plotUI = plotObject.GetComponent<FarmPlotUI>();
            plotUI.Initialize(i);
            farmPlots.Add(plotUI);
        }
        
        plantingMenuPanel.SetActive(false);
    }

    private void SetupListeners()
    {
        boyButton.onClick.AddListener(() => StartGame("boy"));
        girlButton.onClick.AddListener(() => StartGame("girl"));

        wheatButton.onClick.AddListener(() => OnPlantButtonClicked("wheat"));
        carrotButton.onClick.AddListener(() => OnPlantButtonClicked("carrot"));
        potatoButton.onClick.AddListener(() => OnPlantButtonClicked("potato"));
        nextDayButton.onClick.AddListener(() => {
            GameManager.Instance.NextDay();
            NotificationSystem.Instance.ShowNotification("A new day has begun!", new Color(1f, 0.7f, 0.2f));
        });
        harvestAllButton.onClick.AddListener(() => {
            int beforeCount = GameManager.Instance.Inventory.Count;
            GameManager.Instance.HarvestAll();
            int harvestedCount = GameManager.Instance.Inventory.Count - beforeCount;
            
            if (harvestedCount > 0)
                NotificationSystem.Instance.ShowNotification($"Harvested {harvestedCount} crops!", Color.green);
            else
                NotificationSystem.Instance.ShowNotification("No crops ready to harvest", Color.yellow);
        });
        breedButton.onClick.AddListener(EnterBreedingMode);
        
        cancelBreedingButton.onClick.AddListener(CancelBreeding);
        
        closePlantingMenuButton.onClick.AddListener(HidePlantingMenu);
        plantWheatButton.onClick.AddListener(() => PlantSelectedCrop("wheat"));
        plantCarrotButton.onClick.AddListener(() => PlantSelectedCrop("carrot"));
        plantPotatoButton.onClick.AddListener(() => PlantSelectedCrop("potato"));

        GameManager.Instance.OnResourcesUpdated += UpdateResourcesUI;
        GameManager.Instance.OnDayChanged += UpdateDayUI;
        GameManager.Instance.OnFarmUpdated += UpdateFarmUI;
        GameManager.Instance.OnInventoryUpdated += UpdateInventoryUI;
    }

    private void StartGame(string gender)
    {
        GameManager.Instance.SetPlayer(gender);
        
        string avatarUrl = gender == "boy" ? 
            "https://cdn-icons-png.flaticon.com/512/4140/4140048.png" : 
            "https://cdn-icons-png.flaticon.com/512/4140/4140047.png";
            
        ImageLoader.Instance.LoadImage(avatarUrl, playerAvatar);
        playerNameText.text = GameManager.Instance.PlayerData.Name;
        
        SwitchToGameScreen();
        UpdateResourcesUI();
        UpdateDayUI();
        
        NotificationSystem.Instance.ShowNotification($"Welcome, {GameManager.Instance.PlayerData.Name}! Your farming adventure begins.", new Color(0.5f, 0.8f, 1f));
    }

    private void SwitchToGameScreen()
    {
        characterSelectionScreen.SetActive(false);
        gameScreen.SetActive(true);
        breedingScreen.SetActive(false);
        
        gameScreen.transform.localScale = Vector3.zero;
        gameScreen.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    private void UpdateResourcesUI()
    {
        moneyText.text = GameManager.Instance.Money.ToString();
        seedsText.text = GameManager.Instance.Seeds.ToString();
    }

    private void UpdateDayUI()
    {
        dayText.text = GameManager.Instance.Day.ToString();
    }

    private void UpdateFarmUI()
    {
        for (int i = 0; i < farmPlots.Count; i++)
        {
            if (i < GameManager.Instance.Farm.Count)
            {
                farmPlots[i].UpdateUI(GameManager.Instance.Farm[i]);
            }
        }
    }

    private void UpdateInventoryUI()
    {
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var item in GameManager.Instance.Inventory)
        {
            GameObject itemObject = Instantiate(inventoryItemPrefab, inventoryContainer);
            InventoryItemUI itemUI = itemObject.GetComponent<InventoryItemUI>();
            itemUI.SetItem(item.CropType);
        }
    }

    private void OnPlantButtonClicked(string cropType)
    {
        int emptyPlotIndex = GameManager.Instance.Farm.FindIndex(plot => !plot.HasCrop);
        if (emptyPlotIndex >= 0)
        {
            CropData cropData = CropDatabase.GetCrop(cropType);
            
            if (GameManager.Instance.Seeds >= cropData.SeedCost && GameManager.Instance.Money >= cropData.Price)
            {
                GameManager.Instance.PlantCrop(cropType, emptyPlotIndex);
                NotificationSystem.Instance.ShowNotification($"{cropData.Name} planted!", new Color(0.4f, 0.8f, 0.4f));
            }
            else
            {
                if (GameManager.Instance.Seeds < cropData.SeedCost)
                    NotificationSystem.Instance.ShowNotification($"Not enough seeds! Need {cropData.SeedCost}", Color.red);
                else
                    NotificationSystem.Instance.ShowNotification($"Not enough money! Need {cropData.Price}", Color.red);
            }
        }
        else
        {
            NotificationSystem.Instance.ShowNotification("No empty plots available!", Color.red);
        }
    }

    public void ShowPlantingMenu(int plotIndex = -1)
    {
        selectedPlotForPlanting = plotIndex;
        
        plantingMenuPanel.SetActive(true);
        plantingMenuPanel.transform.localScale = Vector3.zero;
        plantingMenuPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        
        UpdatePlantingButtonsStatus();
    }
    
    private void UpdatePlantingButtonsStatus()
    {
        plantWheatButton.interactable = CanPlantCrop("wheat");
        plantCarrotButton.interactable = CanPlantCrop("carrot");
        plantPotatoButton.interactable = CanPlantCrop("potato");
    }
    
    private bool CanPlantCrop(string cropType)
    {
        CropData cropData = CropDatabase.GetCrop(cropType);
        return GameManager.Instance.Seeds >= cropData.SeedCost && 
               GameManager.Instance.Money >= cropData.Price;
    }
    
    private void PlantSelectedCrop(string cropType)
    {
        if (selectedPlotForPlanting >= 0)
        {
            CropData cropData = CropDatabase.GetCrop(cropType);
            
            if (GameManager.Instance.Seeds >= cropData.SeedCost && GameManager.Instance.Money >= cropData.Price)
            {
                GameManager.Instance.PlantCrop(cropType, selectedPlotForPlanting);
                NotificationSystem.Instance.ShowNotification($"{cropData.Name} planted!", new Color(0.4f, 0.8f, 0.4f));
            }
            else
            {
                if (GameManager.Instance.Seeds < cropData.SeedCost)
                    NotificationSystem.Instance.ShowNotification($"Not enough seeds! Need {cropData.SeedCost}", Color.red);
                else
                    NotificationSystem.Instance.ShowNotification($"Not enough money! Need {cropData.Price}", Color.red);
            }
        }
        else
        {
            int emptyPlotIndex = GameManager.Instance.Farm.FindIndex(plot => !plot.HasCrop);
            if (emptyPlotIndex >= 0)
            {
                CropData cropData = CropDatabase.GetCrop(cropType);
                
                if (GameManager.Instance.Seeds >= cropData.SeedCost && GameManager.Instance.Money >= cropData.Price)
                {
                    GameManager.Instance.PlantCrop(cropType, emptyPlotIndex);
                    NotificationSystem.Instance.ShowNotification($"{cropData.Name} planted!", new Color(0.4f, 0.8f, 0.4f));
                }
                else
                {
                    if (GameManager.Instance.Seeds < cropData.SeedCost)
                        NotificationSystem.Instance.ShowNotification($"Not enough seeds! Need {cropData.SeedCost}", Color.red);
                    else
                        NotificationSystem.Instance.ShowNotification($"Not enough money! Need {cropData.Price}", Color.red);
                }
            }
            else
            {
                NotificationSystem.Instance.ShowNotification("No empty plots available!", Color.red);
            }
        }
        
        HidePlantingMenu();
    }
    
    private void HidePlantingMenu()
    {
        plantingMenuPanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
            plantingMenuPanel.SetActive(false);
        });
    }

    private void ShowCharacterSelection()
    {
        characterSelectionScreen.SetActive(true);
        gameScreen.SetActive(false);
        breedingScreen.SetActive(false);
        
        TextMeshProUGUI title = characterSelectionScreen.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        title.transform.localScale = Vector3.zero;
        title.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic);
        
        NotificationSystem.Instance.ShowNotification("Welcome to Farming Simulator! Choose your character.", Color.white);
    }

    public void EnterBreedingMode()
    {
        if (GameManager.Instance.Inventory.Count < 2)
        {
            NotificationSystem.Instance.ShowNotification("Need at least 2 crops to breed!", Color.red);
            return;
        }
        
        if (GameManager.Instance.Money < 50)
        {
            NotificationSystem.Instance.ShowNotification("Breeding costs 50 money!", Color.red);
            return;
        }
        
        GameManager.Instance.IsBreedingMode = true;
        breedingScreen.SetActive(true);
        characterSelectionScreen.SetActive(false);
        gameScreen.SetActive(false);
        
        breedingScreen.transform.localScale = Vector3.zero;
        breedingScreen.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        
        RenderBreedingPlants();
        
        NotificationSystem.Instance.ShowNotification("Select two plants to breed", new Color(0.8f, 0.4f, 0.8f));
    }

    private void RenderBreedingPlants()
    {
        foreach (Transform child in breedingPlantsContainer)
        {
            Destroy(child.gameObject);
        }
        
        selectedPlantsForBreeding.Clear();
        
        foreach (var item in GameManager.Instance.Inventory)
        {
            GameObject plantObject = Instantiate(breedingPlantPrefab, breedingPlantsContainer);
            BreedingPlantUI plantUI = plantObject.GetComponent<BreedingPlantUI>();
            plantUI.SetPlant(item.CropType);
            
            Button button = plantObject.GetComponent<Button>();
            button.onClick.AddListener(() => SelectPlantForBreeding(plantUI));
        }
    }

    private void SelectPlantForBreeding(BreedingPlantUI plantUI)
    {
        if (selectedPlantsForBreeding.Contains(plantUI.PlantImage))
        {
            selectedPlantsForBreeding.Remove(plantUI.PlantImage);
            plantUI.SetSelected(false);
            NotificationSystem.Instance.ShowNotification("Plant deselected", Color.gray);
        }
        else
        {
            if (selectedPlantsForBreeding.Count < 2)
            {
                selectedPlantsForBreeding.Add(plantUI.PlantImage);
                plantUI.SetSelected(true);
                
                if (selectedPlantsForBreeding.Count == 1)
                {
                    NotificationSystem.Instance.ShowNotification("First plant selected! Choose one more.", new Color(0.8f, 0.4f, 0.8f));
                }
                
                if (selectedPlantsForBreeding.Count == 2)
                {
                    NotificationSystem.Instance.ShowNotification("Two plants selected! Starting breeding process...", new Color(0.8f, 0.4f, 0.8f));
                    CompleteBreeding();
                }
            }
        }
    }

    private void CompleteBreeding()
    {
        if (selectedPlantsForBreeding.Count != 2)
            return;
            
        if (GameManager.Instance.BreedPlants("wheat", "carrot"))
        {
            breedingScreen.transform.DOShakeScale(0.5f, 0.2f);
            
            NotificationSystem.Instance.ShowNotification("Breeding successful! A new crop was added to your inventory.", Color.magenta);
            
            Invoke("CancelBreeding", 1.5f);
        }
        else
        {
            NotificationSystem.Instance.ShowNotification("Breeding failed! Not enough money.", Color.red);
        }
    }

    private void CancelBreeding()
    {
        GameManager.Instance.IsBreedingMode = false;
        
        breedingScreen.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
            breedingScreen.SetActive(false);
            gameScreen.SetActive(true);
        });
    }
} 