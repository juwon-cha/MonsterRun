using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserItemData
{
    public long SerialNumber; // unique value
    public int ItemID;

    public UserItemData(long serialNumber, int itemID)
    {
        SerialNumber = serialNumber;
        ItemID = itemID;
    }
}

// wrapper class to parse data to JSON using JSONUtility
[Serializable]
public class UserInventoryItemDataListWrapper
{
    public List<UserItemData> InventoryItemDataList;
}

public class UserItemStats
{
    public int AttackPower;
    public int Defense;

    public UserItemStats(int attackPower, int defense)
    {
        AttackPower = attackPower;
        Defense = defense;
    }
}

public class UserInventoryData : IUserData
{
    public bool IsLoaded { get; set; }

    public UserItemData EquippedWeaponData { get; set; }
    public UserItemData EquippedShieldData { get; set; }
    public UserItemData EquippedChestArmorData { get; set; }
    public UserItemData EquippedBootsData { get; set; }
    public UserItemData EquippedGlovesData { get; set; }
    public UserItemData EquippedAccessoryData { get; set; }
    public UserItemData EquippedCharacterData { get; set; }

    public List<UserItemData> InventoryItemDataList { get; set; } = new List<UserItemData>();
    public Dictionary<long, UserItemStats> EquippedItemDic { get; set; } = new Dictionary<long, UserItemStats>();

    public void SetDefaultData()
    {
        Logger.Log($"{GetType()}::SetDefaultData");

        // serial number => DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 71001));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 71002));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 71003));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 71004));

        EquippedCharacterData = new UserItemData(InventoryItemDataList[0].SerialNumber, InventoryItemDataList[0].ItemID);
        //EquippedWeaponData = new UserItemData(InventoryItemDataList[0].SerialNumber, InventoryItemDataList[0].ItemID);
        //EquippedShieldData = new UserItemData(InventoryItemDataList[2].SerialNumber, InventoryItemDataList[2].ItemID);

        SetEquippedItemDic();
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");

        FirebaseManager.Instance.LoadUserData<UserInventoryData>(() =>
        {
            IsLoaded = true;
            SetEquippedItemDic();
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");

        FirebaseManager.Instance.SaveUserData<UserInventoryData>(ConvertDataToFirestoreDict());
    }

    public void SetEquippedItemDic()
    {
        if(EquippedWeaponData != null)
        {
            var itemData = DataTableManager.Instance.GetItemData(EquippedWeaponData.ItemID);
            if(itemData != null)
            {
                EquippedItemDic.Add(EquippedWeaponData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
            }
        }

        if (EquippedShieldData != null)
        {
            var itemData = DataTableManager.Instance.GetItemData(EquippedShieldData.ItemID);
            if (itemData != null)
            {
                EquippedItemDic.Add(EquippedShieldData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
            }
        }

        if (EquippedChestArmorData != null)
        {
            var itemData = DataTableManager.Instance.GetItemData(EquippedChestArmorData.ItemID);
            if (itemData != null)
            {
                EquippedItemDic.Add(EquippedChestArmorData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
            }
        }

        if (EquippedBootsData != null)
        {
            var itemData = DataTableManager.Instance.GetItemData(EquippedBootsData.ItemID);
            if (itemData != null)
            {
                EquippedItemDic.Add(EquippedBootsData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
            }
        }

        if (EquippedGlovesData != null)
        {
            var itemData = DataTableManager.Instance.GetItemData(EquippedGlovesData.ItemID);
            if (itemData != null)
            {
                EquippedItemDic.Add(EquippedGlovesData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
            }
        }

        if (EquippedAccessoryData != null)
        {
            var itemData = DataTableManager.Instance.GetItemData(EquippedAccessoryData.ItemID);
            if (itemData != null)
            {
                EquippedItemDic.Add(EquippedAccessoryData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
            }
        }

        if (EquippedCharacterData != null)
        {
            var itemData = DataTableManager.Instance.GetItemData(EquippedCharacterData.ItemID);
            if (itemData != null)
            {
                EquippedItemDic.Add(EquippedCharacterData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
            }
        }
    }

    public bool IsEquipped(long serialNum)
    {
        return EquippedItemDic.ContainsKey(serialNum);
    }

    public void EquipItem(long serialNumber, int itemID)
    {
        var itemData = DataTableManager.Instance.GetItemData(itemID);
        if(itemData == null)
        {
            Logger.LogError($"Item data does not exist. ItemID:{itemID}");
            return;
        }

        var itemType = (EItemType)(itemID / 10000);
        switch (itemType)
        {
            case EItemType.Weapon:
                if(EquippedWeaponData != null)
                {
                    // 기존 아이템 제거
                    EquippedItemDic.Remove(EquippedWeaponData.SerialNumber);
                    EquippedWeaponData = null;
                }
                // 새로운 아이템 장착
                EquippedWeaponData = new UserItemData(serialNumber, itemID);
                break;

            case EItemType.Shield:
                if (EquippedShieldData != null)
                {
                    EquippedItemDic.Remove(EquippedShieldData.SerialNumber);
                    EquippedShieldData = null;
                }
                EquippedShieldData = new UserItemData(serialNumber, itemID);
                break;

            case EItemType.ChestArmor:
                if (EquippedChestArmorData != null)
                {
                    EquippedItemDic.Remove(EquippedChestArmorData.SerialNumber);
                    EquippedChestArmorData = null;
                }
                EquippedChestArmorData = new UserItemData(serialNumber, itemID);
                break;

            case EItemType.Gloves:
                if (EquippedGlovesData != null)
                {
                    EquippedItemDic.Remove(EquippedGlovesData.SerialNumber);
                    EquippedGlovesData = null;
                }
                EquippedGlovesData = new UserItemData(serialNumber, itemID);
                break;

            case EItemType.Boots:
                if (EquippedBootsData != null)
                {
                    EquippedItemDic.Remove(EquippedBootsData.SerialNumber);
                    EquippedBootsData = null;
                }
                EquippedBootsData = new UserItemData(serialNumber, itemID);
                break;

            case EItemType.Accessory:
                if (EquippedAccessoryData != null)
                {
                    EquippedItemDic.Remove(EquippedAccessoryData.SerialNumber);
                    EquippedAccessoryData = null;
                }
                EquippedAccessoryData = new UserItemData(serialNumber, itemID);
                break;

            case EItemType.Character:
                if (EquippedCharacterData != null)
                {
                    EquippedItemDic.Remove(EquippedCharacterData.SerialNumber);
                    EquippedCharacterData = null;
                }
                EquippedCharacterData = new UserItemData(serialNumber, itemID);
                break;

            default:
                break;
        }

        // 장착한 아이템을 EquippedItemDic에 추가
        EquippedItemDic.Add(serialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
    }

    public void UnequipItem(long serialNumber, int itemID)
    {
        var itemType = (EItemType)(itemID / 10000);
        switch (itemType)
        {
            case EItemType.Weapon:
                EquippedWeaponData = null;
                break;
            case EItemType.Shield:
                EquippedShieldData = null;
                break;
            case EItemType.ChestArmor:
                EquippedChestArmorData = null;
                break;
            case EItemType.Gloves:
                EquippedGlovesData = null;
                break;
            case EItemType.Boots:
                EquippedBootsData = null;
                break;
            case EItemType.Accessory:
                EquippedAccessoryData = null;
                break;
            case EItemType.Character:
                EquippedCharacterData = null;
                break;
            default:
                break;
        }

        EquippedItemDic.Remove(serialNumber);
    }

    public UserItemStats GetUserTotalItemStats()
    {
        int totalAttackPower = 0;
        int totalDefense = 0;

        foreach (var item in EquippedItemDic)
        {
            totalAttackPower += item.Value.AttackPower;
            totalDefense += item.Value.Defense;
        }

        return new UserItemStats(totalAttackPower, totalDefense);
    }

    private Dictionary<string, object> ConvertDataToFirestoreDict()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "EquippedWeaponData", ConvertUserItemDataToDict(EquippedWeaponData) },
            { "EquippedShieldData", ConvertUserItemDataToDict(EquippedShieldData) },
            { "EquippedChestArmorData", ConvertUserItemDataToDict(EquippedChestArmorData) },
            { "EquippedBootsData", ConvertUserItemDataToDict(EquippedBootsData) },
            { "EquippedGlovesData", ConvertUserItemDataToDict(EquippedGlovesData) },
            { "EquippedAccessoryData", ConvertUserItemDataToDict(EquippedAccessoryData) },
            { "EquippedCharacterData", ConvertUserItemDataToDict(EquippedCharacterData) },
            { "InventoryItemDataList", ConvertInventoryListToDict(InventoryItemDataList) },
        };

        return dict;
    }

    private Dictionary<string, object> ConvertUserItemDataToDict(UserItemData userItemData)
    {
        if(userItemData == null) // 아이템 미장착
        {
            return null;
        }

        // 장착 아이템이 있다면
        return new Dictionary<string, object> { { "SerialNumber", userItemData.SerialNumber }, { "ItemID", userItemData.ItemID } };
    }

    private List<Dictionary<string, object>> ConvertInventoryListToDict(List<UserItemData> inventoryItemDataList)
    {
        // List의 원소 -> 각 유저 아이템 데이터를 담는 Dictionary
        List<Dictionary<string, object>> convertedInventoryList = new List<Dictionary<string, object>>();

        foreach (var item in inventoryItemDataList)
        {
            convertedInventoryList.Add(ConvertUserItemDataToDict(item));
        }

        return convertedInventoryList;
    }

    public void SetData(Dictionary<string, object> firestoreDict)
    {
        ConvertFirestoreDictToData(firestoreDict);
    }

    private void ConvertFirestoreDictToData(Dictionary<string, object> dict)
    {
        EquippedWeaponData = ConvertDictToUserItemData((Dictionary<string, object>)dict["EquippedWeaponData"]);
        EquippedShieldData = ConvertDictToUserItemData((Dictionary<string, object>)dict["EquippedShieldData"]);
        EquippedChestArmorData = ConvertDictToUserItemData((Dictionary<string, object>)dict["EquippedChestArmorData"]);
        EquippedBootsData = ConvertDictToUserItemData((Dictionary<string, object>)dict["EquippedBootsData"]);
        EquippedGlovesData = ConvertDictToUserItemData((Dictionary<string, object>)dict["EquippedGlovesData"]);
        EquippedAccessoryData = ConvertDictToUserItemData((Dictionary<string, object>)dict["EquippedAccessoryData"]);
        EquippedCharacterData = ConvertDictToUserItemData((Dictionary<string, object>)dict["EquippedCharacterData"]);
        InventoryItemDataList = ConvertDictToInventoryList((List<object>)dict["InventoryItemDataList"]);

        if(dict.TryGetValue("EquippedWeaponData", out var equippedWeaponDataValue) && equippedWeaponDataValue is Dictionary<string, object> equippedWeaponDataDict)
        {
            EquippedWeaponData = ConvertDictToUserItemData(equippedWeaponDataDict);
        }

        if (dict.TryGetValue("EquippedShieldData", out var equippedShieldDataValue) && equippedShieldDataValue is Dictionary<string, object> equippedShieldDataDict)
        {
            EquippedShieldData = ConvertDictToUserItemData(equippedShieldDataDict);
        }

        if (dict.TryGetValue("EquippedChestArmorData", out var equippedChestArmorDataValue) && equippedChestArmorDataValue is Dictionary<string, object> equippedChestArmorDataDict)
        {
            EquippedChestArmorData = ConvertDictToUserItemData(equippedChestArmorDataDict);
        }

        if (dict.TryGetValue("EquippedBootsData", out var equippedBootsDataValue) && equippedBootsDataValue is Dictionary<string, object> equippedBootsDataDict)
        {
            EquippedBootsData = ConvertDictToUserItemData(equippedBootsDataDict);
        }

        if (dict.TryGetValue("EquippedGlovesData", out var equippedGlovesDataValue) && equippedGlovesDataValue is Dictionary<string, object> equippedGlovesDataDict)
        {
            EquippedGlovesData = ConvertDictToUserItemData(equippedGlovesDataDict);
        }

        if (dict.TryGetValue("EquippedAccessoryData", out var equippedAccessoryDataValue) && equippedAccessoryDataValue is Dictionary<string, object> equippedAccessoryDataDict)
        {
            EquippedAccessoryData = ConvertDictToUserItemData(equippedAccessoryDataDict);
        }

        if (dict.TryGetValue("EquippedCharacterData", out var equippedCharacterDataValue) && equippedCharacterDataValue is Dictionary<string, object> equippedCharacterDataDict)
        {
            EquippedCharacterData = ConvertDictToUserItemData(equippedCharacterDataDict);
        }

        if (dict.TryGetValue("InventoryItemDataList", out var inventoryItemDataListValue) && inventoryItemDataListValue is List<object> inventoryItemDataList)
        {
            InventoryItemDataList = ConvertDictToInventoryList(inventoryItemDataList);
        }
    }

    private UserItemData ConvertDictToUserItemData(Dictionary<string, object> dict)
    {
        if(dict == null)
        {
            return null;
        }

        long itemSerialNumber = 0;
        if (dict.TryGetValue("SerialNumber", out var serialNumberValue) && serialNumberValue is long serialNumber)
        {
            itemSerialNumber = serialNumber;
        }

        int itemId = 0;
        if(dict.TryGetValue("ItemID", out var itemIdValue) && itemIdValue != null)
        {
            itemId = Convert.ToInt32(itemIdValue);
        }

        if(itemSerialNumber == 0 || itemId == 0)
        {
            Logger.LogError($"Invalid ItemSerialNumber:{itemSerialNumber}, ItemID:{itemId}");
            return null;
        }

        return new UserItemData(itemSerialNumber, itemId);
    }

    private List<UserItemData> ConvertDictToInventoryList(List<object> list)
    {
        List<UserItemData> inventoryList = new List<UserItemData>();

        foreach (var item in list)
        {
            if(item is Dictionary<string, object> itemDict)
            {
                inventoryList.Add(ConvertDictToUserItemData(itemDict));
            }
        }

        return inventoryList;
    }

    public void AcquireItem(int itemID)
    {
        Logger.Log($"Item acquired. ItemID:{itemID}");

        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), itemID));
    }
}
