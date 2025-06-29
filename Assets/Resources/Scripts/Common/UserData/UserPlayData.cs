using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using static Sounder;

public class UserPlayData : IUserData
{
    public bool IsLoaded { get; set; }
    public int MaxClearedChapter { get; set; }

    // ��Ű�� ��ǰ ���� ����
    public Dictionary<string, bool> PurchasedPackProduct { get; set; } = new Dictionary<string, bool>();

    // ���� ���� ���� ���� ��û�� �ð�(���� ���� ���� ���� �ð�)
    public DateTime LastDailyFreeGemAdRewardedTime { get; set; }

    // PlayerPrefs�� �������� ����(���� ���� �߿��� ���� ���õ� é�� ���� ����)
    // -> ������ �÷����� �� �ִ� �ְ� é�ͷ� �ڵ� �̵�
    public int SelectedChapter { get; set; } = 1;

    public void SetDefaultData()
    {
        Logger.Log($"{GetType()}::SetDefaultData");

        MaxClearedChapter = 0;
        SelectedChapter = 1;

        // ��Ű�� ��ǰ ���� ���� �ʱ�ȭ
        // ��Ű�� ��ǰ ������ ��������
        var productList = DataTableManager.Instance.GetProductDataListByProductType(EProductType.Pack);
        if (productList.Count == 0)
        {
            Logger.LogError($"No products. ProductType:{EProductType.Pack}");
            return;
        }

        // ��ǰ����Ʈ ��ȸ�ϸ鼭 �ʱ�ȭ
        foreach (var product in productList)
        {
            PurchasedPackProduct.Add(product.ProductID, false);
        }
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");

        FirebaseManager.Instance.LoadUserData<UserPlayData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");

        FirebaseManager.Instance.SaveUserData<UserPlayData>(ConvertDataToFirestoreDict());
    }

    private Dictionary<string, object> ConvertDataToFirestoreDict()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "MaxClearedChapter", MaxClearedChapter },
            { "LastDailyFreeGemAdRewardedTime", Timestamp.FromDateTime(LastDailyFreeGemAdRewardedTime) },
            // Dictionary<string, bool>�� Firestore�� map Ÿ������ �ڵ� ��ȯ
            { "PurchasedPackProduct", PurchasedPackProduct }
        };

        return dict;
    }

    public void SetData(Dictionary<string, object> firestoreDict)
    {
        ConvertFirestoreDictToData(firestoreDict);
    }

    private void ConvertFirestoreDictToData(Dictionary<string, object> dict)
    {
        //MaxClearedChapter = Convert.ToInt32(dict["MaxClearedChapter"]); // (int) ����ȯ�� ����

        if(dict.TryGetValue("MaxClearedChapter", out var maxClearedChapterValue) && maxClearedChapterValue != null)
        {
            MaxClearedChapter = Convert.ToInt32(maxClearedChapterValue);
        }

        if(dict.TryGetValue("LastDailyFreeGemAdRewardedTime", out var lastDailyFreeGemAdRewardedTimeValue))
        {
            if(lastDailyFreeGemAdRewardedTimeValue is Timestamp lastDailyFreeGemAdRewardedTime)
            {
                // ������ ��ġ�� ���� �ð����� ����
                LastDailyFreeGemAdRewardedTime = lastDailyFreeGemAdRewardedTime.ToDateTime().ToLocalTime();
            }
        }

        // Firestore���� PurchasedPackProduct �����͸� �ҷ��� C# ��ųʸ��� ��ȯ
        if (dict.TryGetValue("PurchasedPackProduct", out var purchasedValue) && purchasedValue is Dictionary<string, object> purchasedDict)
        {
            // Firestore���� ���� Dictionary<string, object>�� Dictionary<string, bool>�� ��ȯ
            // �� value�� bool Ÿ������ Ȯ���ϰ� �����ϰ� ��ȯ�ϱ� ���� LINQ�� ����ϴ� ���� ����
            PurchasedPackProduct = purchasedDict.ToDictionary(kvp => kvp.Key, kvp => (bool)kvp.Value);
        }
    }
}
