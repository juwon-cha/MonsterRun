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

    // 패키지 상품 구매 여부
    public Dictionary<string, bool> PurchasedPackProduct { get; set; } = new Dictionary<string, bool>();

    // 일일 무료 보석 광고를 시청한 시각(일일 무료 보석 수령 시각)
    public DateTime LastDailyFreeGemAdRewardedTime { get; set; }

    // PlayerPrefs에 저장하지 않음(게임 진행 중에만 현재 선택된 챕터 변수 관리)
    // -> 유저가 플레이할 수 있는 최고 챕터로 자동 이동
    public int SelectedChapter { get; set; } = 1;

    public void SetDefaultData()
    {
        Logger.Log($"{GetType()}::SetDefaultData");

        MaxClearedChapter = 0;
        SelectedChapter = 1;

        // 패키지 상품 구매 여부 초기화
        // 패키지 상품 데이터 가져오기
        var productList = DataTableManager.Instance.GetProductDataListByProductType(EProductType.Pack);
        if (productList.Count == 0)
        {
            Logger.LogError($"No products. ProductType:{EProductType.Pack}");
            return;
        }

        // 상품리스트 순회하면서 초기화
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
            // Dictionary<string, bool>는 Firestore의 map 타입으로 자동 변환
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
        //MaxClearedChapter = Convert.ToInt32(dict["MaxClearedChapter"]); // (int) 형변환은 오류

        if(dict.TryGetValue("MaxClearedChapter", out var maxClearedChapterValue) && maxClearedChapterValue != null)
        {
            MaxClearedChapter = Convert.ToInt32(maxClearedChapterValue);
        }

        if(dict.TryGetValue("LastDailyFreeGemAdRewardedTime", out var lastDailyFreeGemAdRewardedTimeValue))
        {
            if(lastDailyFreeGemAdRewardedTimeValue is Timestamp lastDailyFreeGemAdRewardedTime)
            {
                // 유저가 위치한 현지 시간으로 저장
                LastDailyFreeGemAdRewardedTime = lastDailyFreeGemAdRewardedTime.ToDateTime().ToLocalTime();
            }
        }

        // Firestore에서 PurchasedPackProduct 데이터를 불러와 C# 딕셔너리로 변환
        if (dict.TryGetValue("PurchasedPackProduct", out var purchasedValue) && purchasedValue is Dictionary<string, object> purchasedDict)
        {
            // Firestore에서 받은 Dictionary<string, object>를 Dictionary<string, bool>로 변환
            // 각 value가 bool 타입인지 확인하고 안전하게 변환하기 위해 LINQ를 사용하는 것이 좋다
            PurchasedPackProduct = purchasedDict.ToDictionary(kvp => kvp.Key, kvp => (bool)kvp.Value);
        }
    }
}
