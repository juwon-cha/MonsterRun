using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static GlobalDefine;

public class DataTableManager : SingletonBehaviour<DataTableManager>
{
    private const string DATA_PATH = "DataTable";

    public void LoadDataTables()
    {
        LoadChapterDataTable();
        LoadItemDataTable();
        LoadAchievementDataTable();
        LoadProductDataTable();
        LoadChestRewardProbabilityDataTable();
    }

    #region CHAPTER_DATA
    private const string CHAPTHER_DATA_TABLE = "ChapterDataTable"; // 파일명

    private List<ChapterData> mChapterDataTable = new List<ChapterData>();

    private async void LoadChapterDataTable()
    {
        var parseDataTable = await CSVReader.ReadFromAA($"{CHAPTHER_DATA_TABLE}");
        
        foreach(var data in parseDataTable)
        {
            var chapterData = new ChapterData
            {
                ChapterNo = Convert.ToInt32(data["chapter_no"]),
                ChapterName = data["chapter_name"].ToString(),
                ChapterRewardGem = Convert.ToInt32(data["chapter_reward_gem"]),
                ChapterRewardGold = Convert.ToInt32(data["chapter_reward_gold"]),
                ChapterClearScore = Convert.ToInt32(data["chapter_clear_score"])
            };

            mChapterDataTable.Add(chapterData);
        }
    }

    public ChapterData GetChapterData(int chapterNo)
    {
        //foreach(var item in mChapterDataTable)
        //{
        //    if(item.ChapterNo == chapterNo)
        //    {
        //        return item;
        //    }
        //}

        //return null;

        return mChapterDataTable.Where(item => item.ChapterNo == chapterNo).FirstOrDefault();
    }
    #endregion

    #region ITEM_DATA
    private const string ITEM_DATA_TABLE = "ItemDataTable";
    private List<ItemData> mItemDataTable = new List<ItemData>();

    private async void LoadItemDataTable()
    {
        var parsedDataTable = await CSVReader.ReadFromAA($"{ITEM_DATA_TABLE}");

        foreach(var data in parsedDataTable)
        {
            var itemData = new ItemData
            {
                ItemID = Convert.ToInt32(data["item_id"]),
                ItemName = data["item_name"].ToString(),
                AttackPower = Convert.ToInt32(data["attack_power"]),
                Defense = Convert.ToInt32(data["defense"])
            };

            mItemDataTable.Add(itemData);
        }
    }

    public ItemData GetItemData(int itemID)
    {
        return mItemDataTable.Where(item => item.ItemID == itemID).FirstOrDefault();
    }
    #endregion

    #region ACHIEVEMENT_DATA
    private const string ACHIEVEMENT_DATA_TABLE = "AchievementDataTable";
    private List<AchievementData> mAchievementDataTable = new List<AchievementData>();
    public List<AchievementData> GetAchievementDataList()
    {
        return mAchievementDataTable;
    }

    private async void LoadAchievementDataTable()
    {
        var parsedDataTable = await CSVReader.ReadFromAA($"{ACHIEVEMENT_DATA_TABLE}");

        foreach (var data in parsedDataTable)
        {
            var achievementData = new AchievementData
            {
                AchievementType = (EAchievementType)Enum.Parse(typeof(EAchievementType), data["achievement_type"].ToString()),
                AchievementName = data["achievement_name"].ToString(),
                AchievementGoal = Convert.ToInt32(data["achievement_goal"]),
                AchievementRewardType = (ERewardType)Enum.Parse(typeof(ERewardType), data["achievement_reward_type"].ToString()),
                AchievementRewardAmount = Convert.ToInt32(data["achievement_reward_amount"])
            };

            mAchievementDataTable.Add(achievementData);
        }
    }

    // 업적 타입에 맞는 데이터를 찾아 반환
    public AchievementData GetAchievementData(EAchievementType achievementType)
    {
        return mAchievementDataTable.Where(item => item.AchievementType == achievementType).FirstOrDefault();
    }
    #endregion

    #region PRODUCT_DATA
    private const string PRODUCT_DATA_TABLE = "ProductDataTable";
    private List<ProductData> mProductDataTable = new List<ProductData>();

    private async void LoadProductDataTable()
    {
        var parseDataTable = await CSVReader.ReadFromAA($"{PRODUCT_DATA_TABLE}");

        foreach (var data in parseDataTable)
        {
            var productData = new ProductData
            {
                ProductID = data["product_id"].ToString(),
                ProductType = (EProductType)Enum.Parse(typeof(EProductType), data["product_type"].ToString()),
                ProductName = data["product_name"].ToString(),
                PurchaseType = (EPurchaseType)Enum.Parse(typeof(EPurchaseType), data["purchase_type"].ToString()),
                PurchaseCost = Convert.ToInt32(data["purchase_cost"]),
                RewardGem = Convert.ToInt32(data["reward_gem"]),
                RewardGold = Convert.ToInt32(data["reward_gold"]),
                RewardItemID = Convert.ToInt32(data["reward_item_id"])
            };
            mProductDataTable.Add(productData);
        }
    }

    public ProductData GetProductData(string productID)
    {
        return mProductDataTable.Where(item => item.ProductID == productID).FirstOrDefault();
    }

    public List<ProductData> GetProductDataListByProductType(EProductType productType)
    {
        return mProductDataTable.Where(item => item.ProductType == productType).ToList();
    }
    #endregion

    #region CHEST_REWARD_PROBABILITY_DATA
    private const string CHEST_REWARD_PROBABILITY_DATA_TABLE = "ChestRewardProbabilityDataTable";
    private List<ChestRewardProbabilityData> mChestRewardProbabilityDataTable = new List<ChestRewardProbabilityData>();

    private async void LoadChestRewardProbabilityDataTable()
    {
        var parsedDataTable = await CSVReader.ReadFromAA($"{CHEST_REWARD_PROBABILITY_DATA_TABLE}");

        foreach (var data in parsedDataTable)
        {
            var chestRewardProbabilityData = new ChestRewardProbabilityData
            {
                ItemID = Convert.ToInt32(data["item_id"]),
                ChestID = data["chest_id"].ToString(),
                LootProbability = Convert.ToInt32(data["loot_probability"])
            };
            mChestRewardProbabilityDataTable.Add(chestRewardProbabilityData);
        }
    }

    // 특정 상자ID에서 나올 수 있는 모든 확률 데이터를 반환
    public List<ChestRewardProbabilityData> GetChestRewardProbabilityData(string chestID)
    {
        return mChestRewardProbabilityDataTable.Where(item => item.ChestID == chestID).ToList();
    }
    #endregion
}
