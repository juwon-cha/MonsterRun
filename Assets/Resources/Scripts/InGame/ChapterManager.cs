using Gpm.Common.ThirdParty.MessagePack.Resolvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Sounder;

public enum EChapterName
{
    Chapter_Desert = 1,
    Chapter_Forest,
    Chapter_NightDesert,
    Chapter_Plateau,
    Chapter_Dungeon,
}

public class ChapterManager : SingletonBehaviour<ChapterManager>
{
    public ChapterUIController InGameUIController { get; private set; }
    public bool IsChapterCleared { get; private set; }
    public bool IsPaused { get; private set; }

    private EChapterName mChapterName;
    private int mSelectedChapter;
    private ChapterData mCurChapterData;
    private const string CHAPTER_PATH = "Prefabs/InGame/Chapter";
    private Transform mChapterTrs;

    protected override void Init()
    {
        // 챕터 매니저는 인게임 씬을 벗어나면 삭제되어야 함
        mbIsDestroyOnLoad = true;

        base.Init();

        InitVariables();
        LoadChapter();

        UIManager.Instance.Fade(Color.black, 1f, 0f, 0.5f, 0f, true);
    }

    private void InitVariables()
    {
        Logger.Log($"{GetType()}::InitVariables");

        mChapterTrs = GameObject.Find("Chapter").transform;

        var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
        if (userPlayData == null)
        {
            Logger.LogError("UserPlayData does not exist.");
            return;
        }

        mSelectedChapter = userPlayData.SelectedChapter;

        mCurChapterData = DataTableManager.Instance.GetChapterData(mSelectedChapter);
        if (mCurChapterData == null)
        {
            Logger.LogError($"ChapterData does not exist. Chapter:{mSelectedChapter}");
            return;
        }

        mChapterName = (EChapterName)mCurChapterData.ChapterNo;
    }

    private void LoadChapter()
    {
        Logger.Log($"{GetType()}::LoadChapter");
        Logger.Log($"Chapter:{mSelectedChapter}");

        var chapterObject = Instantiate(Resources.Load($"{CHAPTER_PATH}/{mChapterName.ToString()}", typeof(GameObject))) as GameObject;
        chapterObject.transform.SetParent(mChapterTrs);
        chapterObject.transform.localScale = Vector3.one;
        chapterObject.transform.localPosition = Vector3.zero;
        chapterObject.SetActive(true);
    }

    private void Start()
    {
        InGameUIController = FindObjectOfType<ChapterUIController>();
        if(InGameUIController == null)
        {
            Logger.LogError("InGameUIController does not exist.");
            return;
        }

        InGameUIController.Init();

        AdsManager.Instance.EnableTopBannerAd(true);
    }

    private void Update()
    {
        CheckClearChapter();
    }

    private void CheckClearChapter()
    {
        if(IsChapterCleared)
        {
            return;
        }

        // TODO: 각 챕터의 클리어 점수를 DataTable에 저장? -> 챕터 별 클리어 점수를 로드해서 만족하면 클리어
        // 현재 플레이하고 있는 챕터의 점수를 Firestore에 저장할 필요가 있을까?
        //if (GameManager.Instance.Score >= 30/* mCurChapterData의 클리어 점수? */)
        if(GameManager.Instance.Score >= mCurChapterData.ChapterClearScore)
        {
            IsChapterCleared = true;
            ClearChapter();
        }
        else
        {
            IsChapterCleared = false;
        }
    }

    public void ClearChapter()
    {
        Logger.Log($"{GetType()}::ClearChapter");

        AudioManager.Instance.PlaySFX(ESFX.Chapter_Clear);

        var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
        if (userPlayData == null)
        {
            Logger.LogError("UserPlayData does not exist.");
            return;
        }

        var uiData = new ChapterClearUIData();
        uiData.Chapter = mSelectedChapter;
        // 보상 지급 여부 -> 현재 챕터가 클리어한 챕터보다 크면 보상 지급
        uiData.EarnReward = mSelectedChapter > userPlayData.MaxClearedChapter;
        UIManager.Instance.OpenUI<ChapterClearUI>(uiData);

        // 현재 챕터 클리어 처리
        if (mSelectedChapter > userPlayData.MaxClearedChapter)
        {
            userPlayData.MaxClearedChapter++;
            // 로비 화면에서 해금한 챕터가 선택한 챕터로 선택되도록 함
            userPlayData.SelectedChapter = userPlayData.MaxClearedChapter + 1;
            userPlayData.SaveData();
        }

        GameManager.Instance.IsRunning = false;

        // 챕터 클리어 업적 진행
        var userAchievementData = UserDataManager.Instance.GetUserData<UserAchievementData>();
        if(userAchievementData != null)
        {
            switch (mSelectedChapter)
            {
                case 1:
                    userAchievementData.ProgressAchievement(EAchievementType.ClearChapter1, 1);
                    break;

                case 2:
                    userAchievementData.ProgressAchievement(EAchievementType.ClearChapter2, 1);
                    break;

                case 3:
                    userAchievementData.ProgressAchievement(EAchievementType.ClearChapter3, 1);
                    break;

                default:
                    break;
            }
        }

        // 챕터 클리어 광고 재생
        AdsManager.Instance.ShowChapterClearInterstitialAd();
    }

    public void PauseGame()
    {
        IsPaused = true;

        GameManager.Instance.IsRunning = false;
    }

    public void ResumeGame()
    {
        IsPaused = false;

        GameManager.Instance.IsRunning = true;
    }
}
