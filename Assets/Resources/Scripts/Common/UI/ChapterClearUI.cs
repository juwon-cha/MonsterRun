using SuperMaxim.Messaging;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterClearUIData : BaseUIData
{
    public int Chapter;
    // é�� ù Ŭ����� ���� �����ؾ� �ϴ��� �ƴ���(Ŭ���� �� �ٽ� Ŭ����) �Ǵ�
    public bool EarnReward;
}

public class ChapterClearUI : BaseUI
{
    // ���� ���� UI ��ҵ��� �ֻ��� ������Ʈ
    public GameObject Rewards;
    public TextMeshProUGUI GemRewardAmountTxt;
    public TextMeshProUGUI GoldRewardAmountTxt;
    public Button HomeBtn;
    public ParticleSystem[] ClearFX;

    private ChapterClearUIData mChapterClearUIData;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        mChapterClearUIData = uiData as ChapterClearUIData;
        if(mChapterClearUIData == null)
        {
            Logger.LogError("ChapterClearUIData in invalid.");
            return;
        }

        ChapterData chapterData = DataTableManager.Instance.GetChapterData(mChapterClearUIData.Chapter);
        if(chapterData == null)
        {
            Logger.LogError($"ChapterData in invalid. Chapter:{mChapterClearUIData.Chapter}");
            return;
        }

        // ������ �������� ���� Reward ������Ʈ Ȱ��ȭ/��Ȱ��ȭ
        Rewards.SetActive(mChapterClearUIData.EarnReward);
        if(mChapterClearUIData.EarnReward)
        {
            // ������ �ް� �ȴٸ� ���� ǥ��(é�� ������ ���̺� ���� ���� ����)
            GemRewardAmountTxt.text = chapterData.ChapterRewardGem.ToString("N0");
            GoldRewardAmountTxt.text = chapterData.ChapterRewardGold.ToString("N0");

            // ���� ����
            UserGoodsData userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
            if(userGoodsData == null)
            {
                Logger.LogError("UserGoodsData does not exist.");
                return;
            }

            userGoodsData.Gold += chapterData.ChapterRewardGold;
            userGoodsData.Gem += chapterData.ChapterRewardGem;
            userGoodsData.SaveData();

            // ���� ���� ��ȭ�� �����Ǿ��ٴ� �޽��� ����
            GoldUpdateMsg goldUpdateMsg = new GoldUpdateMsg();
            goldUpdateMsg.IsAdd = true;
            Messenger.Default.Publish(goldUpdateMsg);

            // ���� ���� ����(���)
            var userAchievementData = UserDataManager.Instance.GetUserData<UserAchievementData>();
            if(userAchievementData != null)
            {
                userAchievementData.ProgressAchievement(EAchievementType.CollectGold, chapterData.ChapterRewardGold);
            }

            GemUpdateMsg gemUpdateMsg = new GemUpdateMsg();
            gemUpdateMsg.IsAdd = true;
            Messenger.Default.Publish(gemUpdateMsg);
        }

        // Reward ������Ʈ ��ġ�� ���� Ȩ��ư ������Ʈ ��ġ ����
        HomeBtn.GetComponent<RectTransform>().localPosition = new Vector3(0f, mChapterClearUIData.EarnReward ? -250f : 50f, 0f);

        for (int i = 0; i < ClearFX.Length; i++)
        {
            ClearFX[i].Play();
        }

        // é�͹�ȣ�� ���̾�̽��� �α� ����
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "chapter_no", mChapterClearUIData.Chapter.ToString() }
        };
        FirebaseManager.Instance.LogCustomEvent("chapter_clear", parameters);
    }

    public void OnClickHomeBtn()
    {
        SceneLoader.Instance.LoadScene(ESceneType.Lobby);
        CloseUI();
    }
}
