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

        // 업적 목록 세팅
        SetAchievementList();
        SortAchievementList();
    }

    private void SetAchievementList()
    {
        // 스크롤뷰 이전에 생성된 아이템 제거
        AchievementScrollList.Clear();

        var achievementDataList = DataTableManager.Instance.GetAchievementDataList();
        var userAchievementData = UserDataManager.Instance.GetUserData<UserAchievementData>();
        if(achievementDataList != null && userAchievementData != null)
        {
            // 업적 데이터 목록 순회 -> 업적 UI에 필요한 데이터 세팅
            foreach (var achievement in achievementDataList)
            {
                var achievementItemData = new AchievementItemData();
                achievementItemData.AchievementType = achievement.AchievementType;

                // 유저 업적 데이터에도 해당 업적 데이터가 있다면 업적이 얼마나 진행됐는지 세팅
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

            // 아직 보상을 받지 않은 업적을 가장 상위로 정렬
            var AComp = achievementA.IsAchieved && !achievementA.IsRewardClaimed;
            var BComp = achievementB.IsAchieved && !achievementB.IsRewardClaimed;

            int compareResult = BComp.CompareTo(AComp);
            if(compareResult == 0)
            {
                // 조건 동일 -> 달성하지 못한 업적을 달성한 업적보다 상위로
                compareResult = achievementA.IsAchieved.CompareTo(achievementB.IsAchieved);
                if(compareResult == 0)
                {
                    // 위 조건도 동일하다면 업적 타입에 따라 정렬
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
