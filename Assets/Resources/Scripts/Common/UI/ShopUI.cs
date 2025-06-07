using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : BaseUI
{
    public ScrollRect mScrollRect;

    // Chest
    public GameObject ChestProductItemPrefab;
    public Transform ChestProductsGroupTrs;

    // Gem
    public GameObject GemProductItemPrefab;
    public Transform GemProductsGroup1Trs;
    public Transform GemProductsGroup2Trs;

    // Gold
    public GameObject GoldProductItemPrefab;
    public Transform GoldProductsGroupTrs;

    private void OnEnable()
    {
        mScrollRect.verticalNormalizedPosition = 1f; // 스크롤뷰가 항상 맨 위에 있도록 설정
    }

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        SetChestProducts();
        SetGemProducts();
        SetGoldProducts();
    }

    private void SetChestProducts()
    {
        // 이전에 세팅되어있던 보석 상품 삭제
        // -> 삭제 안하면 UI 열고 닫을 때 전에 세팅되어 있던 상품들 남아있는 문제
        foreach (Transform child in ChestProductsGroupTrs)
        {
            Destroy(child.gameObject);
        }

        var productList = DataTableManager.Instance.GetProductDataListByProductType(EProductType.Chest);
        if (productList.Count == 0)
        {
            Logger.LogError($"No products. ProductType:{EProductType.Chest}");
            return;
        }

        // 상자 상품 프리펩이 설정되어 있지 않은 경우
        if (ChestProductItemPrefab == null)
        {
            Logger.LogError($"No Prefab. ProductType:{EProductType.Chest}");
            return;
        }

        // 상자 상품 순회하면서 골드 아이템 프리팹을 이용하여 게임오브젝트 인스턴스 생성
        foreach (var product in productList)
        {
            var productItemObject = Instantiate(ChestProductItemPrefab, Vector3.zero, Quaternion.identity);
            productItemObject.transform.SetParent(ChestProductsGroupTrs);
            productItemObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            var chestProductItem = productItemObject.GetComponent<ChestProductItem>();
            if (chestProductItem != null)
            {
                chestProductItem.SetInfo(product.ProductID);
            }
        }
    }

    private void SetGemProducts()
    {
        // 이전에 세팅되어있던 보석 상품 삭제
        // -> 삭제 안하면 UI 열고 닫을 때 전에 세팅되어 있던 상품들 남아있는 문제
        foreach (Transform child in GemProductsGroup1Trs)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in GemProductsGroup2Trs)
        {
            Destroy(child.gameObject);
        }

        var productList = DataTableManager.Instance.GetProductDataListByProductType(EProductType.Gem);
        if (productList.Count == 0)
        {
            Logger.LogError($"No products. ProductType:{EProductType.Gem}");
            return;
        }

        // 보석 상품 프리펩이 설정되어 있지 않은 경우
        if (GemProductItemPrefab == null)
        {
            Logger.LogError($"No Prefab. ProductType:{EProductType.Gem}");
            return;
        }

        // 보석 상품 순회하면서 보석 아이템 프리팹을 이용해 게임오브젝트 인스턴스 생성
        for(int i = 0; i < productList.Count; ++i)
        {
            var productItemObject = Instantiate(GemProductItemPrefab, Vector3.zero, Quaternion.identity);
            productItemObject.transform.SetParent(i < 3 ? GemProductsGroup1Trs : GemProductsGroup2Trs);
            productItemObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            var gemProductItem = productItemObject.GetComponent<GemProductItem>();
            if (gemProductItem != null)
            {
                gemProductItem.SetInfo(productList[i].ProductID);
            }
        }
    }

    private void SetGoldProducts()
    {
        // 이전에 세팅되어있던 골드 상품 삭제
        // -> 삭제 안하면 UI 열고 닫을 때 전에 세팅되어 있던 상품들 남아있는 문제
        foreach (Transform child in GoldProductsGroupTrs)
        {
            Destroy(child.gameObject);
        }

        var productList = DataTableManager.Instance.GetProductDataListByProductType(EProductType.Gold);
        if(productList.Count == 0)
        {
            Logger.LogError($"No products. ProductType:{EProductType.Gold}");
            return;
        }

        // 골드 상품 프리펩이 설정되어 있지 않은 경우
        if(GoldProductItemPrefab == null)
        {
            Logger.LogError($"No Prefab. ProductType:{EProductType.Gold}");
            return;
        }

        // 골드 상품 순회하면서 골드 아이템 프리팹을 이용하여 게임오브젝트 인스턴스 생성
        foreach (var product in productList)
        {
            var productItemObject = Instantiate(GoldProductItemPrefab, Vector3.zero, Quaternion.identity);
            productItemObject.transform.SetParent(GoldProductsGroupTrs);
            productItemObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            var goldProductItem = productItemObject.GetComponent<GoldProductItem>();
            if(goldProductItem != null)
            {
                goldProductItem.SetInfo(product.ProductID);
            }
        }
    }
}
