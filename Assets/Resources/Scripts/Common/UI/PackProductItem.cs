using SuperMaxim.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackProductItem : MonoBehaviour
{
    public Image PurchasedDim;
    private ProductData mProductData;

    private void OnEnable()
    {
        // 메시지 구독 처리
        Messenger.Default.Subscribe<PackProductPurchasedMsg>(OnPackProductPurchased/*메시지 발행했을 때 실행할 함수*/);
    }

    private void OnDisenable()
    {
        // 메시지 구독 취소 처리
        Messenger.Default.Unsubscribe<PackProductPurchasedMsg>(OnPackProductPurchased);
    }

    public void SetInfo(string productID)
    {
        // 상품 데이터 가져오기
        mProductData = DataTableManager.Instance.GetProductData(productID);
        if(mProductData == null)
        {
            Logger.LogError($"Product does not exist. ProductID:{productID}");
            return;
        }

        bool bHasPurchased = ShopManager.Instance.HadPurchasedPackProduct(productID);
        PurchasedDim.gameObject.SetActive(bHasPurchased);

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.enabled = !bHasPurchased;
        }
    }

    private void OnPackProductPurchased(PackProductPurchasedMsg packProductPurchasedMsg)
    {
        Logger.Log($"{GetType()}::OnPackProductPurchased");

        // 한 번 구매 후 비활성화
        if(mProductData != null && mProductData.ProductID == packProductPurchasedMsg.productID)
        {
            PurchasedDim.gameObject.SetActive(false);

            var button = GetComponent<Button>();
            if(button != null)
            {
                button.enabled = false;
            }
        }
    }

    public void OnClickItem()
    {
        Logger.Log($"{GetType()::OnClickItem}");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        switch (mProductData.PurchaseType)
        {
            case EPurchaseType.IAP:
                break;
            case EPurchaseType.Ad:
                {
                    var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
                    if (userPlayData != null)
                    {
                        AdsManager.Instance.ShowPackRewardedAd(() =>
                        {
                            // 구매 여부 처리
                            string id = mProductData.ProductID;
                            if(userPlayData.PurchasedPackProduct.ContainsKey(id))
                            {
                                userPlayData.PurchasedPackProduct[id] = true;
                                userPlayData.SaveData();
                            }
                            else
                            {
                                Logger.LogError($"Product ID does not exist. {id}");
                            }

                            // 보상 지급 처리
                            ShopManager.Instance.GetProductReward(id);
                        });
                    }
                }
                break;
            default:
                break;
        }
    }
}
