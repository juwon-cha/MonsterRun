using Gpm.Ui;
using SuperMaxim.Messaging;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AchievementItemData : InfiniteScrollData
{
    public EAchievementType AchievementType;
    public int AchievementAmount;
    public bool IsAchieved;
    public bool IsRewardClaimed;
}

public class AchievementItem : InfiniteScrollItem
{
    public GameObject AchievedBg;
    public GameObject UnachievedBg;
    public TextMeshProUGUI AchievementNameTxt;

    public Slider AchievementProgressSlider;
    public TextMeshProUGUI AchievementProgressTxt;

    public Image RewardIcon;
    public TextMeshProUGUI RewardAmountTxt;

    public Button ClaimBtn;
    public Image ClaimBtnImg;
    public TextMeshProUGUI ClaimBtnTxt;

    private AchievementItemData mAcheivementItemData;

    public override void UpdateData(InfiniteScrollData scrollData)
    {
        base.UpdateData(scrollData);

        mAcheivementItemData = scrollData as AchievementItemData;
        if(mAcheivementItemData == null)
        {
            Logger.LogError("AcheivementItemData is invalid.");
            return;
        }

        var achievementData = DataTableManager.Instance.GetAchievementData(mAcheivementItemData.AchievementType);
        if(achievementData == null)
        {
            Logger.LogError("AcheivementData does not exist.");
            return;
        }

        // 업적 달성 여부에 따라 배경 이미지 설정
        AchievedBg.SetActive(mAcheivementItemData.IsAchieved);
        UnachievedBg.SetActive(!mAcheivementItemData.IsAchieved);

        // 업적 이름 설정
        AchievementNameTxt.text = achievementData.AchievementName;

        // 업적 진행바 설정
        AchievementProgressSlider.value = (float)mAcheivementItemData.AchievementAmount / achievementData.AchievementGoal;
        AchievementProgressTxt.text = $"{mAcheivementItemData.AchievementAmount.ToString("N0")}/{achievementData.AchievementGoal.ToString("N0")}";

        // 보상 수량 설정
        RewardAmountTxt.text = achievementData.AchievementRewardAmount.ToString("N0");

        // 보상 이미지 설정
        var rewardTextrueName = string.Empty;
        switch (achievementData.AchievementRewardType)
        {
            case GlobalDefine.ERewardType.Gold:
                rewardTextrueName = "IconGolds";
                break;

            case GlobalDefine.ERewardType.Gem:
                rewardTextrueName = "IconGems";
                break;

            default:
                break;
        }

        var rewardTexture = Resources.Load<Texture2D>($"Textures/{rewardTextrueName}");
        if(rewardTexture != null)
        {
            RewardIcon.sprite = Sprite.Create(rewardTexture, new Rect(0, 0, rewardTexture.width, rewardTexture.height), new Vector2(1f, 1f));
        }

        // 보상 수령 버튼 조건에 맞게 활성화/비활성화
        // 업적 달성했지만 아직 보상 수령하지 않은 경우 -> 버튼 활성화
        ClaimBtn.enabled = mAcheivementItemData.IsAchieved && !mAcheivementItemData.IsRewardClaimed;
        ClaimBtnImg.color = ClaimBtn.enabled ? Color.white : Color.grey;
        ClaimBtnTxt.color = ClaimBtn.enabled ? Color.white : Color.grey;
    }

    public void OnClickClaimBtn()
    {
        // 업적 달성을 하지 못했거나 보상을 이미 수령했다면 버튼 클릭 안됨
        if(!mAcheivementItemData.IsAchieved || mAcheivementItemData.IsRewardClaimed)
        {
            return;
        }

        var userAchievementData = UserDataManager.Instance.GetUserData<UserAchievementData>();
        if(userAchievementData == null)
        {
            Logger.LogError("UserAchievementData does not exist.");
            return;
        }

        var achievementData = DataTableManager.Instance.GetAchievementData(mAcheivementItemData.AchievementType);
        if (achievementData == null)
        {
            Logger.LogError("AchievementData does not exist.");
            return;
        }

        // 현재 업적 타입에 맞는 유저 진행 데이터 가져옴
        var userAchievedData = userAchievementData.GetUserAchievementProgressData(mAcheivementItemData.AchievementType);
        if(userAchievedData != null)
        {
            // 보상 지급 처리
            var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
            if(userGoodsData != null)
            {
                userAchievedData.IsRewardClaimed = true; // 유저 업적 진행 데이터의 보상 수령 여부
                userAchievementData.SaveData();
                mAcheivementItemData.IsRewardClaimed = true; // 현재 UI 전용 데이터의 보상 수령 여부

                switch (achievementData.AchievementRewardType)
                {
                    case GlobalDefine.ERewardType.Gold:
                        userGoodsData.Gold += achievementData.AchievementRewardAmount;
                        // 골드 획득 메시지 발행
                        var goldUpdateMsg = new GoldUpdateMsg();
                        goldUpdateMsg.IsAdd = true;
                        Messenger.Default.Publish(goldUpdateMsg);

                        // 골드 수령 업적에 대한 처리(골드를 얼마나 모았는지에 대한 업적 갱신)
                        userAchievementData.ProgressAchievement(EAchievementType.CollectGold, achievementData.AchievementRewardAmount);
                        break;

                    case GlobalDefine.ERewardType.Gem:
                        userGoodsData.Gem += achievementData.AchievementRewardAmount;
                        // 보석 획득 메시지 발행
                        var gemUpdateMsg = new GemUpdateMsg();
                        gemUpdateMsg.IsAdd = true;
                        Messenger.Default.Publish(gemUpdateMsg);
                        break;

                    default:
                        break;
                }

                userGoodsData.SaveData();
            }
        }
    }
}
