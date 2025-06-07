using Gpm.Ui;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterScrollItemData : InfiniteScrollData
{
    public int ChapterNo;
}

public class ChapterScrollItem : InfiniteScrollItem
{
    public GameObject CurChapter;
    public RawImage CurChatperBg;
    public Image Dim;
    public Image LockIcon;
    public Image Round; // 테두리 이미지
    public ParticleSystem ComingSoonFx;
    public TextMeshProUGUI ComingSoonTxt;

    private ChapterScrollItemData mChapterScrollItemData;

    public override void UpdateData(InfiniteScrollData scrollData)
    {
        base.UpdateData(scrollData);

        mChapterScrollItemData = scrollData as ChapterScrollItemData;
        if(mChapterScrollItemData == null)
        {
            Logger.LogError("Invalid ChapterScrollItemData");
            return;
        }

        if(mChapterScrollItemData.ChapterNo > GlobalDefine.MAX_CHAPTER)
        {
            CurChapter.SetActive(false);
            ComingSoonFx.gameObject.SetActive(true);
            ComingSoonTxt.gameObject.SetActive(true);
        }
        else
        {
            CurChapter.SetActive(true);
            ComingSoonFx.gameObject.SetActive(false);
            ComingSoonTxt.gameObject.SetActive(false);

            var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
            if(userPlayData != null)
            {
                // 현재 최대로 클리어한 챕터와 비교 -> 챕터의 해금 여부 판단
                var isLocked = mChapterScrollItemData.ChapterNo > userPlayData.MaxClearedChapter + 1;
                Dim.gameObject.SetActive(isLocked); // 해금되지 않았으면 활성화
                LockIcon.gameObject.SetActive(isLocked);
                Round.color = isLocked ? new Color(0.5f, 0.5f, 1f) : Color.white;
            }

            // 챕터 번호에 맞는 배경 이미지 로드하고 설정
            var bgTexture = Resources.Load($"Textures/ChapterBG/ChapterBG_{mChapterScrollItemData.ChapterNo.ToString("D3")}") as Texture2D;
            if(bgTexture != null)
            {
                CurChatperBg.texture = bgTexture;
            }
        }
    }
}
