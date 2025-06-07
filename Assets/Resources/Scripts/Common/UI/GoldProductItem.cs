using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldProductItem : MonoBehaviour
{
    public Image GoldImg;
    public TextMeshProUGUI AmountTxt;
    public TextMeshProUGUI CostTxt;

    private ProductData mProductData;

    public void SetInfo(string productID)
    {
        mProductData = DataTableManager.Instance.GetProductData(productID);
        if(mProductData == null)
        {
            Logger.LogError($"Product does not exist. ProductID:{productID}");
            return;
        }

        var goldImageTexture = Resources.Load<Texture2D>($"Textures/Shop_Icon_{mProductData.ProductID}");
        if(goldImageTexture != null)
        {
            GoldImg.sprite = Sprite.Create(goldImageTexture, new Rect(0, 0, goldImageTexture.width, goldImageTexture.height), new Vector2(1f, 1f));
        }

        AmountTxt.text = mProductData.RewardGold.ToString("N0");
        CostTxt.text = mProductData.PurchaseCost.ToString("N0");
    }

    public void OnClickItem()
    {
        ShopManager.Instance.PurchaseProduct(mProductData.ProductID);
    }
}
