using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EProductType
{
    Pack,
    Chest,
    Gem,
    Gold
}

public enum EPurchaseType
{
    IAP,    // In-App-Purchase
    Ad,     // Advertisement
    Gem
}

public class ProductData
{
    public string ProductID;
    public EProductType ProductType;
    public string ProductName;
    public EPurchaseType PurchaseType;
    public int PurchaseCost;
    public int RewardGem;
    public int RewardGold;
    public int RewardItemID;
}
