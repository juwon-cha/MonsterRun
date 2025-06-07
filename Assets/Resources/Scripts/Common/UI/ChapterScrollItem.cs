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
    public Image Round; // �׵θ� �̹���
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
                // ���� �ִ�� Ŭ������ é�Ϳ� �� -> é���� �ر� ���� �Ǵ�
                var isLocked = mChapterScrollItemData.ChapterNo > userPlayData.MaxClearedChapter + 1;
                Dim.gameObject.SetActive(isLocked); // �رݵ��� �ʾ����� Ȱ��ȭ
                LockIcon.gameObject.SetActive(isLocked);
                Round.color = isLocked ? new Color(0.5f, 0.5f, 1f) : Color.white;
            }

            // é�� ��ȣ�� �´� ��� �̹��� �ε��ϰ� ����
            var bgTexture = Resources.Load($"Textures/ChapterBG/ChapterBG_{mChapterScrollItemData.ChapterNo.ToString("D3")}") as Texture2D;
            if(bgTexture != null)
            {
                CurChatperBg.texture = bgTexture;
            }
        }
    }
}
