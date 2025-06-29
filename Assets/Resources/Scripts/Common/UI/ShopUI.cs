using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : BaseUI
{
    public ScrollRect mScrollRect;

    // Package
    public Transform PackProductsGroupTrs;

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
        mScrollRect.verticalNormalizedPosition = 1f; // ��ũ�Ѻ䰡 �׻� �� ���� �ֵ��� ����
    }

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        SetPackProducts();
        SetChestProducts();
        SetGemProducts();
        SetGoldProducts();
    }

    private void SetPackProducts()
    {
        // ������ ���õǾ��ִ� ��Ű�� ��ǰ ����
        // -> ���� ���ϸ� UI ���� ���� �� ���� ���õǾ� �ִ� ��ǰ�� �����ִ� ����
        foreach (Transform child in PackProductsGroupTrs)
        {
            Destroy(child.gameObject);
        }

        // ��Ű�� ��ǰ ������ ��������
        var productList = DataTableManager.Instance.GetProductDataListByProductType(EProductType.Pack);
        if (productList.Count == 0)
        {
            Logger.LogError($"No products. ProductType:{EProductType.Pack}");
            return;
        }

        // ��ǰ����Ʈ ��ȸ�ϸ鼭 �� ��ǰ�� �´� ������ UI �ε�
        foreach(var product in productList)
        {
            var productItemObj = Instantiate(Resources.Load($"Prefabs/UI/PackProductItem_{product.ProductID}", typeof(GameObject))) as GameObject;
            productItemObj.transform.SetParent(PackProductsGroupTrs);
            productItemObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var packProductItem = productItemObj.GetComponent<PackProductItem>();
            if(packProductItem != null)
            {
                packProductItem.SetInfo(product.ProductID);
            }
        }
    }

    private void SetChestProducts()
    {
        // ������ ���õǾ��ִ� ���� ��ǰ ����
        // -> ���� ���ϸ� UI ���� ���� �� ���� ���õǾ� �ִ� ��ǰ�� �����ִ� ����
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

        // ���� ��ǰ �������� �����Ǿ� ���� ���� ���
        if (ChestProductItemPrefab == null)
        {
            Logger.LogError($"No Prefab. ProductType:{EProductType.Chest}");
            return;
        }

        // ���� ��ǰ ��ȸ�ϸ鼭 ��� ������ �������� �̿��Ͽ� ���ӿ�����Ʈ �ν��Ͻ� ����
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
        // ������ ���õǾ��ִ� ���� ��ǰ ����
        // -> ���� ���ϸ� UI ���� ���� �� ���� ���õǾ� �ִ� ��ǰ�� �����ִ� ����
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

        // ���� ��ǰ �������� �����Ǿ� ���� ���� ���
        if (GemProductItemPrefab == null)
        {
            Logger.LogError($"No Prefab. ProductType:{EProductType.Gem}");
            return;
        }

        // ���� ��ǰ ��ȸ�ϸ鼭 ���� ������ �������� �̿��� ���ӿ�����Ʈ �ν��Ͻ� ����
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
        // ������ ���õǾ��ִ� ��� ��ǰ ����
        // -> ���� ���ϸ� UI ���� ���� �� ���� ���õǾ� �ִ� ��ǰ�� �����ִ� ����
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

        // ��� ��ǰ �������� �����Ǿ� ���� ���� ���
        if(GoldProductItemPrefab == null)
        {
            Logger.LogError($"No Prefab. ProductType:{EProductType.Gold}");
            return;
        }

        // ��� ��ǰ ��ȸ�ϸ鼭 ��� ������ �������� �̿��Ͽ� ���ӿ�����Ʈ �ν��Ͻ� ����
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
