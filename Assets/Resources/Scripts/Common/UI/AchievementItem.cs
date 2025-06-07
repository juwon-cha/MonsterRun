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

        // ���� �޼� ���ο� ���� ��� �̹��� ����
        AchievedBg.SetActive(mAcheivementItemData.IsAchieved);
        UnachievedBg.SetActive(!mAcheivementItemData.IsAchieved);

        // ���� �̸� ����
        AchievementNameTxt.text = achievementData.AchievementName;

        // ���� ����� ����
        AchievementProgressSlider.value = (float)mAcheivementItemData.AchievementAmount / achievementData.AchievementGoal;
        AchievementProgressTxt.text = $"{mAcheivementItemData.AchievementAmount.ToString("N0")}/{achievementData.AchievementGoal.ToString("N0")}";

        // ���� ���� ����
        RewardAmountTxt.text = achievementData.AchievementRewardAmount.ToString("N0");

        // ���� �̹��� ����
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

        // ���� ���� ��ư ���ǿ� �°� Ȱ��ȭ/��Ȱ��ȭ
        // ���� �޼������� ���� ���� �������� ���� ��� -> ��ư Ȱ��ȭ
        ClaimBtn.enabled = mAcheivementItemData.IsAchieved && !mAcheivementItemData.IsRewardClaimed;
        ClaimBtnImg.color = ClaimBtn.enabled ? Color.white : Color.grey;
        ClaimBtnTxt.color = ClaimBtn.enabled ? Color.white : Color.grey;
    }

    public void OnClickClaimBtn()
    {
        // ���� �޼��� ���� ���߰ų� ������ �̹� �����ߴٸ� ��ư Ŭ�� �ȵ�
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

        // ���� ���� Ÿ�Կ� �´� ���� ���� ������ ������
        var userAchievedData = userAchievementData.GetUserAchievementProgressData(mAcheivementItemData.AchievementType);
        if(userAchievedData != null)
        {
            // ���� ���� ó��
            var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
            if(userGoodsData != null)
            {
                userAchievedData.IsRewardClaimed = true; // ���� ���� ���� �������� ���� ���� ����
                userAchievementData.SaveData();
                mAcheivementItemData.IsRewardClaimed = true; // ���� UI ���� �������� ���� ���� ����

                switch (achievementData.AchievementRewardType)
                {
                    case GlobalDefine.ERewardType.Gold:
                        userGoodsData.Gold += achievementData.AchievementRewardAmount;
                        // ��� ȹ�� �޽��� ����
                        var goldUpdateMsg = new GoldUpdateMsg();
                        goldUpdateMsg.IsAdd = true;
                        Messenger.Default.Publish(goldUpdateMsg);

                        // ��� ���� ������ ���� ó��(��带 �󸶳� ��Ҵ����� ���� ���� ����)
                        userAchievementData.ProgressAchievement(EAchievementType.CollectGold, achievementData.AchievementRewardAmount);
                        break;

                    case GlobalDefine.ERewardType.Gem:
                        userGoodsData.Gem += achievementData.AchievementRewardAmount;
                        // ���� ȹ�� �޽��� ����
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
