using SuperMaxim.Messaging;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterClearUIData : BaseUIData
{
    public int Chapter;
    // 챕터 첫 클리어라서 보상 지급해야 하는지 아닌지(클리어 후 다시 클리어) 판단
    public bool EarnReward;
}

public class ChapterClearUI : BaseUI
{
    // 보상 관련 UI 요소들의 최상위 오브젝트
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

        // 보상을 받을지에 따라 Reward 오브젝트 활성화/비활성화
        Rewards.SetActive(mChapterClearUIData.EarnReward);
        if(mChapterClearUIData.EarnReward)
        {
            // 보상을 받게 된다면 수량 표시(챕터 데이터 테이블에 보상 정보 있음)
            GemRewardAmountTxt.text = chapterData.ChapterRewardGem.ToString("N0");
            GoldRewardAmountTxt.text = chapterData.ChapterRewardGold.ToString("N0");

            // 보상 지급
            UserGoodsData userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
            if(userGoodsData == null)
            {
                Logger.LogError("UserGoodsData does not exist.");
                return;
            }

            userGoodsData.Gold += chapterData.ChapterRewardGold;
            userGoodsData.Gem += chapterData.ChapterRewardGem;
            userGoodsData.SaveData();

            // 유저 보유 재화가 변동되었다는 메시지 발행
            GoldUpdateMsg goldUpdateMsg = new GoldUpdateMsg();
            goldUpdateMsg.IsAdd = true;
            Messenger.Default.Publish(goldUpdateMsg);

            // 유저 업적 진행(골드)
            var userAchievementData = UserDataManager.Instance.GetUserData<UserAchievementData>();
            if(userAchievementData != null)
            {
                userAchievementData.ProgressAchievement(EAchievementType.CollectGold, chapterData.ChapterRewardGold);
            }

            GemUpdateMsg gemUpdateMsg = new GemUpdateMsg();
            gemUpdateMsg.IsAdd = true;
            Messenger.Default.Publish(gemUpdateMsg);
        }

        // Reward 오브젝트 위치에 따른 홈버튼 오브젝트 위치 조정
        HomeBtn.GetComponent<RectTransform>().localPosition = new Vector3(0f, mChapterClearUIData.EarnReward ? -250f : 50f, 0f);

        for (int i = 0; i < ClearFX.Length; i++)
        {
            ClearFX[i].Play();
        }

        // 챕터번호를 파이어베이스로 로그 전송
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
