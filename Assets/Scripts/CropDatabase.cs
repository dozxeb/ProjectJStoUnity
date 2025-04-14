using System.Collections.Generic;
using UnityEngine;

public static class CropDatabase
{
    private static Dictionary<string, CropData> crops = new Dictionary<string, CropData>
    {
        { "wheat", new CropData(
            "Wheat", 
            "https://cdn-icons-png.flaticon.com/512/2153/2153788.png", 
            3, 
            20, 
            5, 
            10) 
        },
        { "carrot", new CropData(
            "Carrot", 
            "https://cdn-icons-png.flaticon.com/512/2489/2489753.png", 
            1, 
            30, 
            3, 
            15) 
        },
        { "potato", new CropData(
            "Potato", 
            "https://cdn-icons-png.flaticon.com/512/2489/2489828.png", 
            4, 
            50, 
            7, 
            20) 
        }
    };

    public static CropData GetCrop(string cropType)
    {
        if (crops.TryGetValue(cropType, out CropData crop))
        {
            return crop;
        }
        
        Debug.LogWarning($"Crop type {cropType} not found in database");
        return null;
    }

    public static List<string> GetAllCropTypes()
    {
        return new List<string>(crops.Keys);
    }
}

public class CropData
{
    public string Name { get; private set; }
    public string ImageUrl { get; private set; }
    public int GrowthTime { get; private set; }
    public int Value { get; private set; }
    public int SeedCost { get; private set; }
    public int Price { get; private set; }

    public CropData(string name, string imageUrl, int growthTime, int value, int seedCost, int price)
    {
        Name = name;
        ImageUrl = imageUrl;
        GrowthTime = growthTime;
        Value = value;
        SeedCost = seedCost;
        Price = price;
    }
} 