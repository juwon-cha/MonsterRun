using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestProductItem : MonoBehaviour
{
    public Image ChestImg;
    public TextMeshProUGUI CostTxt;
    public Image AdIcon;

    private ProductData mProductData;

    public void SetInfo(string productID)
    {
        mProductData = DataTableManager.Instance.GetProductData(productID);
        if(mProductData == null)
        {
            Logger.LogError($"Product does not exist. ProductID:{productID}");
            return;
        }

        var chestImgTexture = Resources.Load<Texture2D>($"Textures/Shop_Icon_{mProductData.ProductID}");
        if(chestImgTexture != null)
        {
            ChestImg.sprite = Sprite.Create(chestImgTexture, new Rect(0, 0, chestImgTexture.width, chestImgTexture.height), new Vector2(1f, 1f));
        }

        switch (mProductData.PurchaseType)
        {
            case EPurchaseType.Gem:
                CostTxt.gameObject.SetActive(true);
                AdIcon.gameObject.SetActive(false);
                CostTxt.text = mProductData.PurchaseCost.ToString("N0");
                break;

            case EPurchaseType.Ad:
                CostTxt.gameObject.SetActive(false);
                AdIcon.gameObject.SetActive(true);
                break;

            default:
                break;
        }
    }

    public void OnClickItem()
    {
        ShopManager.Instance.PurchaseProduct(mProductData.ProductID);
    }
}
