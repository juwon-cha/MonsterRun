using SuperMaxim.Messaging;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Purchasing;

public class PackProductPurchasedMsg
{
    public string productID;
}

public class ShopManager : SingletonBehaviour<ShopManager>
{
    public void PurchaseProduct(string productID)
    {
        var productData = DataTableManager.Instance.GetProductData(productID);
        if(productData == null)
        {
            Logger.LogError($"No proudct data. ProductID:{productID}");
            return;
        }

        switch (productData.PurchaseType)
        {
            case EPurchaseType.IAP:
                break;

            case EPurchaseType.Ad:
                break;

            case EPurchaseType.Gem:
                {
                    var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
                    if(userGoodsData == null)
                    {
                        Logger.LogError($"No user data.");
                        return;
                    }

                    // 유저 재화 데이터 가져와서 상품을 살 보석이 충분하면 구매 가능
                    if(userGoodsData.Gem >= productData.PurchaseCost)
                    {
                        userGoodsData.Gem -= productData.PurchaseCost;
                        userGoodsData.SaveData();
                        var gemUpdateMsg = new GemUpdateMsg();
                        Messenger.Default.Publish(gemUpdateMsg);

                        // 구매에 대한 지급
                        GetProductReward(productID);
                    }
                    else // 충분하지 못하면 보석 부족 UI 팝업 띄움(구매 못함)
                    {
                        var uiData = new ConfirmUIData();
                        uiData.ConfirmType = EConfirmType.OK;
                        uiData.TitleTxt = "Purchase Fail";
                        uiData.DescTxt = "Not enough gem";
                        uiData.OKBtnTxt = "OK";
                        UIManager.Instance.OpenUI<ConfirmUI>(uiData);
                    }
                }
                break;

            default:
                break;
        }
    }

    public void GetProductReward(string productID)
    {
        var productData = DataTableManager.Instance.GetProductData(productID);
        if(productData == null)
        {
            Logger.LogError($"No product data. ProductID:{productID}");
            return;
        }

        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if (userGoodsData == null)
        {
            Logger.LogError($"No user data.");
            return;
        }

        switch (productData.ProductType)
        {
            case EProductType.Pack:
                OpenPack(productID);
                break;

            case EProductType.Chest:
                OpenChest(productID);
                break;

            case EProductType.Gem:
                {
                    userGoodsData.Gem += productData.RewardGem;
                    userGoodsData.SaveData();
                    var gemUpdateMsg = new GemUpdateMsg();
                    gemUpdateMsg.IsAdd = true;
                    Messenger.Default.Publish(gemUpdateMsg);
                }
                break;

            case EProductType.Gold:
                {
                    userGoodsData.Gold += productData.RewardGold;
                    userGoodsData.SaveData();
                    var goldUpdateMsg = new GoldUpdateMsg();
                    goldUpdateMsg.IsAdd = true;
                    Messenger.Default.Publish(goldUpdateMsg);
                }
                break;

            default:
                break;
        }
    }

    private void OpenPack(string productID)
    {
        var productData = DataTableManager.Instance.GetProductData(productID);
        if (productData == null)
        {
            Logger.LogError($"No proudct data. ProductId: {productID}");
            return;
        }

        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if (userGoodsData != null)
        {
            if (productData.RewardGem > 0)
            {
                userGoodsData.Gem += productData.RewardGem;
                var gemUpdatMsg = new GemUpdateMsg();
                gemUpdatMsg.IsAdd = true;
                Messenger.Default.Publish(gemUpdatMsg);
            }

            if (productData.RewardGold > 0)
            {
                userGoodsData.Gold += productData.RewardGold;
                var goldUpdateMsg = new GoldUpdateMsg();
                goldUpdateMsg.IsAdd = true;
                Messenger.Default.Publish(goldUpdateMsg);
            }

            userGoodsData.SaveData();
        }

        if (productData.RewardItemID > 0)
        {
            var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
            if (userInventoryData != null)
            {
                userInventoryData.AcquireItem(productData.RewardItemID);
                userInventoryData.SaveData();

                // UI로 결과 표시
                var chestUIData = new ChestLootUIData();
                chestUIData.ChestID = productID;
                chestUIData.RewardItemIDList.Add(productData.RewardItemID);
                UIManager.Instance.OpenUI<ChestLootUI>(chestUIData);
            }
        }

        // 한 번 구매하면 다시 구매 못하도록 메시지 발행
        // 메시지로 productID를 발행해 PackProductItem에서 게임오브젝트 비활성화
        var packProductPurchased = new PackProductPurchasedMsg();
        packProductPurchased.productID = productID;
        Messenger.Default.Publish(packProductPurchased);
    }

    private void OpenChest(string productID)
    {
        var chestRewardProbabilityData = DataTableManager.Instance.GetChestRewardProbabilityData(productID);
        var totalProbability = 0;

        foreach (var probabilityData in chestRewardProbabilityData)
        {
            totalProbability += probabilityData.LootProbability;
        }

        if(totalProbability != 100)
        {
            Logger.LogWarning($"Total Probability does not sum up to 100. ProductID:{productID}");
        }

        var resultValue = Random.Range(0, totalProbability); // 0 ~ totalProbability-1 범위
        var cumulativeProbability = 0;

        foreach (var probabilityData in chestRewardProbabilityData)
        {
            cumulativeProbability += probabilityData.LootProbability;
            if(resultValue < cumulativeProbability)
            {
                var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
                if(userInventoryData != null)
                {
                    userInventoryData.AcquireItem(probabilityData.ItemID);
                    userInventoryData.SaveData();

                    // ChestLootUI 화면 표시(결과 표시)
                    var chestUIData = new ChestLootUIData();
                    chestUIData.ChestID = productID;
                    chestUIData.RewardItemIDList.Add(probabilityData.ItemID);
                    UIManager.Instance.OpenUI<ChestLootUI>(chestUIData);
                }
                break;
            }
        }
    }

    public bool HadPurchasedPackProduct(string productID)
    {
        bool bHasPurchased;

        var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
        if (userPlayData == null)
        {
            Logger.LogError($"UserPlayData is null");
            return false;
        }

        userPlayData.PurchasedPackProduct.TryGetValue(productID, out bHasPurchased);

        return bHasPurchased;
    }
}
