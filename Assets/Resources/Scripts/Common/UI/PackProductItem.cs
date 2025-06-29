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
        // �޽��� ���� ó��
        Messenger.Default.Subscribe<PackProductPurchasedMsg>(OnPackProductPurchased/*�޽��� �������� �� ������ �Լ�*/);
    }

    private void OnDisenable()
    {
        // �޽��� ���� ��� ó��
        Messenger.Default.Unsubscribe<PackProductPurchasedMsg>(OnPackProductPurchased);
    }

    public void SetInfo(string productID)
    {
        // ��ǰ ������ ��������
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

        // �� �� ���� �� ��Ȱ��ȭ
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
                            // ���� ���� ó��
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

                            // ���� ���� ó��
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
