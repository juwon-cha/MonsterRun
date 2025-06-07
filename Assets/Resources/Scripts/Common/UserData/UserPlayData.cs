using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using static Sounder;

public class UserPlayData : IUserData
{
    public bool IsLoaded { get; set; }
    public int MaxClearedChapter { get; set; }

    // 일일 무료 보석 광고를 시청한 시각(일일 무료 보석 수령 시각)
    public DateTime LastDailyFreeGemAdRewardedTime { get; set; }

    // PlayerPrefs에 저장하지 않음(게임 진행 중에만 현재 선택된 챕터 변수 관리)
    // -> 유저가 플레이할 수 있는 최고 챕터로 자동으로 이동
    public int SelectedChapter { get; set; } = 1;

    public void SetDefaultData()
    {
        Logger.Log($"{GetType()}::SetDefaultData");

        MaxClearedChapter = 0;
        SelectedChapter = 1;
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
            { "LastDailyFreeGemAdRewardedTime", Timestamp.FromDateTime(LastDailyFreeGemAdRewardedTime) }
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
    }
}
