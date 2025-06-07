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

    // ���� ���� ���� ���� ��û�� �ð�(���� ���� ���� ���� �ð�)
    public DateTime LastDailyFreeGemAdRewardedTime { get; set; }

    // PlayerPrefs�� �������� ����(���� ���� �߿��� ���� ���õ� é�� ���� ����)
    // -> ������ �÷����� �� �ִ� �ְ� é�ͷ� �ڵ����� �̵�
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
    }
}
