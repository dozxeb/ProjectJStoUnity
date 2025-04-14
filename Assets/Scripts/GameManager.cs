using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int startingMoney = 100;
    [SerializeField] private int startingSeeds = 10;

    public PlayerData PlayerData { get; private set; }
    public List<FarmPlot> Farm { get; private set; }
    public List<CropItem> Inventory { get; private set; }
    public bool IsBreedingMode { get; set; }

    public int Day { get; private set; } = 1;
    public int Money { get; private set; }
    public int Seeds { get; private set; }

    public event Action OnResourcesUpdated;
    public event Action OnDayChanged;
    public event Action OnFarmUpdated;
    public event Action OnInventoryUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        Money = startingMoney;
        Seeds = startingSeeds;
        Day = 1;
        IsBreedingMode = false;
        
        Farm = new List<FarmPlot>();
        for (int i = 0; i < 15; i++)
        {
            Farm.Add(new FarmPlot());
        }
        
        Inventory = new List<CropItem>();
        
        OnResourcesUpdated?.Invoke();
        OnFarmUpdated?.Invoke();
    }

    public void SetPlayer(string gender)
    {
        string playerName = gender == "boy" ? "Farmer John" : "Farmer Emma";
        PlayerData = new PlayerData(gender, playerName);
    }

    public void PlantCrop(string cropType, int plotIndex)
    {
        if (plotIndex < 0 || plotIndex >= Farm.Count || Farm[plotIndex].HasCrop)
            return;

        CropData cropData = CropDatabase.GetCrop(cropType);
        
        if (cropData == null || Seeds < cropData.SeedCost || Money < cropData.Price)
            return;

        Farm[plotIndex].Plant(cropType);
        Seeds -= cropData.SeedCost;
        Money -= cropData.Price;

        OnResourcesUpdated?.Invoke();
        OnFarmUpdated?.Invoke();
    }

    public void HarvestPlot(int plotIndex)
    {
        if (plotIndex < 0 || plotIndex >= Farm.Count || !Farm[plotIndex].HasCrop)
            return;

        FarmPlot plot = Farm[plotIndex];
        
        if (!plot.IsReadyToHarvest)
            return;

        CropData cropData = CropDatabase.GetCrop(plot.CropType);
        
        Money += cropData.Value;
        Seeds += Mathf.FloorToInt(cropData.SeedCost * 1.5f);
        
        AddToInventory(plot.CropType);
        
        Farm[plotIndex].Clear();
        
        OnResourcesUpdated?.Invoke();
        OnFarmUpdated?.Invoke();
    }

    public void HarvestAll()
    {
        bool harvested = false;
        
        for (int i = 0; i < Farm.Count; i++)
        {
            if (Farm[i].HasCrop && Farm[i].IsReadyToHarvest)
            {
                HarvestPlot(i);
                harvested = true;
            }
        }
        
        if (harvested)
        {
            OnInventoryUpdated?.Invoke();
        }
    }

    public void NextDay()
    {
        Day++;
        
        foreach (var plot in Farm)
        {
            if (plot.HasCrop)
            {
                plot.GrowOneDay();
            }
        }
        
        OnDayChanged?.Invoke();
        OnFarmUpdated?.Invoke();
    }

    public void AddToInventory(string cropType)
    {
        Inventory.Add(new CropItem(cropType));
        OnInventoryUpdated?.Invoke();
    }

    public bool BreedPlants(string plant1, string plant2)
    {
        if (Money < 50)
            return false;

        Money -= 50;
        
        List<string> cropTypes = CropDatabase.GetAllCropTypes();
        int randomIndex = UnityEngine.Random.Range(0, cropTypes.Count);
        
        AddToInventory(cropTypes[randomIndex]);
        
        OnResourcesUpdated?.Invoke();
        OnInventoryUpdated?.Invoke();
        
        return true;
    }
}

[Serializable]
public class PlayerData
{
    public string Gender { get; private set; }
    public string Name { get; private set; }
    
    public PlayerData(string gender, string name)
    {
        Gender = gender;
        Name = name;
    }
}

[Serializable]
public class FarmPlot
{
    public string CropType { get; private set; }
    public int Growth { get; private set; }
    public bool HasCrop => !string.IsNullOrEmpty(CropType);
    public bool IsReadyToHarvest => HasCrop && Growth >= CropDatabase.GetCrop(CropType).GrowthTime;

    public void Plant(string cropType)
    {
        CropType = cropType;
        Growth = 0;
    }

    public void GrowOneDay()
    {
        if (HasCrop)
        {
            Growth++;
        }
    }

    public void Clear()
    {
        CropType = null;
        Growth = 0;
    }
}

[Serializable]
public class CropItem
{
    public string CropType { get; private set; }
    
    public CropItem(string cropType)
    {
        CropType = cropType;
    }
} 