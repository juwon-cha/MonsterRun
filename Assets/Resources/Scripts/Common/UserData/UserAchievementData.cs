using SuperMaxim.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// �� ���� ������ ���� ��Ȳ�� �����ϴ� Ŭ����
[Serializable]
public class UserAchievementProgressData
{
    public EAchievementType AchievementType;
    public int AchievementAmount;
    public bool IsAchieved;
    public bool IsRewardClaimed;
}

[Serializable]
public class UserAchievementProgressDataListWrapper
{
    public List<UserAchievementProgressData> AchievementProgressDataList;
}

// ���� ���� ��Ȳ�� �����Ҷ����� �޽��� ����
public class AchievementProgressMsg
{

}

public class UserAchievementData : IUserData
{
    public bool IsLoaded { get; set; }
    public List<UserAchievementProgressData> AchievementProgressDataList { get; set; } = new List<UserAchievementProgressData>();

    public void SetDefaultData()
    {
        // TEST
        //UserAchievementProgressData userAchievementProgressData = new UserAchievementProgressData();
        //userAchievementProgressData.AchievementType = EAchievementType.CollectGold;
        //userAchievementProgressData.AchievementAmount = 1000;
        //userAchievementProgressData.IsAchieved = true;
        //userAchievementProgressData.IsRewardClaimed = false;
        //AchievementProgressDataList.Add(userAchievementProgressData);
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");

        FirebaseManager.Instance.LoadUserData<UserAchievementData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");

        FirebaseManager.Instance.SaveUserData<UserAchievementData>(ConvertDataToFirestoreDict());
    }

    public UserAchievementProgressData GetUserAchievementProgressData(EAchievementType achievementType)
    {
        return AchievementProgressDataList.Where(item => item.AchievementType == achievementType).FirstOrDefault();
    }

    public void ProgressAchievement(EAchievementType achievementType, int achieveAmount)
    {
        var achievementData = DataTableManager.Instance.GetAchievementData(achievementType);
        if(achievementData == null)
        {
            Logger.LogError("AchievementData does not exist.");
            return;
        }

        UserAchievementProgressData userAchievementProgressData = GetUserAchievementProgressData(achievementType);
        if(userAchievementProgressData == null)
        {
            // ����� ���� �����Ͱ� ���ٸ� ���� ����
            userAchievementProgressData = new UserAchievementProgressData();
            userAchievementProgressData.AchievementType = achievementType;
            AchievementProgressDataList.Add(userAchievementProgressData);
        }

        // ���� �޼� ���� Ȯ��
        // -> �޼����� �ʾ����� ���� ���� ��ġ ����
        if(!userAchievementProgressData.IsAchieved)
        {
            // �߰��� �޼��� ��ġ��ŭ ����
            userAchievementProgressData.AchievementAmount += achieveAmount;

            // �޼� ��ġ �ʰ��ϸ� �ִ� �޼� ��ġ�� ����
            if(userAchievementProgressData.AchievementAmount > achievementData.AchievementGoal)
            {
                userAchievementProgressData.AchievementAmount = achievementData.AchievementGoal;
            }

            // ���� �޼�
            if(userAchievementProgressData.AchievementAmount == achievementData.AchievementGoal)
            {
                userAchievementProgressData.IsAchieved = true;
            }

            SaveData();

            // ���� ���� ��Ȳ ���ŵǾ��ٴ� �޽��� ����
            var achievementProgressMsg = new AchievementProgressMsg();
            Messenger.Default.Publish(achievementProgressMsg);
        }
    }

    private Dictionary<string, object> ConvertDataToFirestoreDict()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        List<Dictionary<string, object>> convertedAchievementProgressDataList = new List<Dictionary<string, object>>();

        foreach(var item in AchievementProgressDataList)
        {
            var convertedItem = new Dictionary<string, object>()
            {
                { "AchievementType", item.AchievementType },
                { "AchievementAmount", item.AchievementAmount },
                { "IsAchieved", item.IsAchieved },
                { "IsRewardClaimed", item.IsRewardClaimed },
            };

            convertedAchievementProgressDataList.Add(convertedItem);
        }

        dict["AchievementProgressDataList"] = convertedAchievementProgressDataList;

        return dict;
    }

    public void SetData(Dictionary<string, object> firestoreDict)
    {
        ConvertFirestoreDictToData(firestoreDict);
    }

    private void ConvertFirestoreDictToData(Dictionary<string, object> dict)
    {
        if(dict.TryGetValue("AchievementProgressDataList", out object achievementDataListObj) && achievementDataListObj is List<object> achievementList)
        {
            foreach(var item in achievementList)
            {
                if(item is Dictionary<string, object> itemDict)
                {
                    UserAchievementProgressData achievementProgressData = new UserAchievementProgressData();
                    //{
                    //    AchievementType = (EAchievementType)Convert.ToInt32(itemDict["AchievementType"]),
                    //    AchievementAmount = Convert.ToInt32(itemDict["AcievementAmount"]),
                    //    IsAchieved = (bool)itemDict["IsAchieved"],
                    //    IsRewardClaimed = (bool)itemDict["IsRewardClaimed"],
                    //};

                    if(itemDict.TryGetValue("AchievementType", out var achievementTypeValue) && achievementTypeValue is EAchievementType achievementType)
                    {
                        achievementProgressData.AchievementType = achievementType;
                    }

                    if (itemDict.TryGetValue("AchievementAmount", out var achievementAmountValue) && achievementAmountValue != null)
                    {
                        achievementProgressData.AchievementAmount = Convert.ToInt32(achievementAmountValue);
                    }

                    if (itemDict.TryGetValue("IsAchieved", out var isAchievedValue) && isAchievedValue is bool isAchieved)
                    {
                        achievementProgressData.IsAchieved = isAchieved;
                    }

                    if (itemDict.TryGetValue("IsRewardClaimed", out var isRewardClaimedValue) && isRewardClaimedValue is bool isRewardClaimed)
                    {
                        achievementProgressData.IsRewardClaimed = isRewardClaimed;
                    }

                    AchievementProgressDataList.Add(achievementProgressData);
                }
            }
        }
    }
}
