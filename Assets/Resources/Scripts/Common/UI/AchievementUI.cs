using Gpm.Ui;
using SuperMaxim.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementUI : BaseUI
{
    public InfiniteScroll AchievementScrollList;

    private void OnEnable()
    {
        Messenger.Default.Subscribe<AchievementProgressMsg>(OnAchievementProgressed);
    }

    private void OnDisable()
    {
        Messenger.Default.Unsubscribe<AchievementProgressMsg>(OnAchievementProgressed);
    }

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        // ���� ��� ����
        SetAchievementList();
        SortAchievementList();
    }

    private void SetAchievementList()
    {
        // ��ũ�Ѻ� ������ ������ ������ ����
        AchievementScrollList.Clear();

        var achievementDataList = DataTableManager.Instance.GetAchievementDataList();
        var userAchievementData = UserDataManager.Instance.GetUserData<UserAchievementData>();
        if(achievementDataList != null && userAchievementData != null)
        {
            // ���� ������ ��� ��ȸ -> ���� UI�� �ʿ��� ������ ����
            foreach (var achievement in achievementDataList)
            {
                var achievementItemData = new AchievementItemData();
                achievementItemData.AchievementType = achievement.AchievementType;

                // ���� ���� �����Ϳ��� �ش� ���� �����Ͱ� �ִٸ� ������ �󸶳� ����ƴ��� ����
                var userAchievedData = userAchievementData.GetUserAchievementProgressData(achievement.AchievementType);
                if(userAchievedData != null)
                {
                    achievementItemData.AchievementAmount = userAchievedData.AchievementAmount;
                    achievementItemData.IsAchieved = userAchievedData.IsAchieved;
                    achievementItemData.IsRewardClaimed = userAchievedData.IsRewardClaimed;
                }

                AchievementScrollList.InsertData(achievementItemData);
            }
        }
    }

    private void SortAchievementList()
    {
        AchievementScrollList.SortDataList((a, b) =>
        {
            var achievementA = a.data as AchievementItemData;
            var achievementB = b.data as AchievementItemData;

            // ���� ������ ���� ���� ������ ���� ������ ����
            var AComp = achievementA.IsAchieved && !achievementA.IsRewardClaimed;
            var BComp = achievementB.IsAchieved && !achievementB.IsRewardClaimed;

            int compareResult = BComp.CompareTo(AComp);
            if(compareResult == 0)
            {
                // ���� ���� -> �޼����� ���� ������ �޼��� �������� ������
                compareResult = achievementA.IsAchieved.CompareTo(achievementB.IsAchieved);
                if(compareResult == 0)
                {
                    // �� ���ǵ� �����ϴٸ� ���� Ÿ�Կ� ���� ����
                    compareResult = (achievementA.AchievementType).CompareTo(achievementB.AchievementType);
                }
            }

            return compareResult;
        });
    }

    private void OnAchievementProgressed(AchievementProgressMsg msg)
    {
        SetAchievementList();
        SortAchievementList();
    }
}
