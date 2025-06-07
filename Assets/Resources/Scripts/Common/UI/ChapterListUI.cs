using Gpm.Ui;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterListUI : BaseUI
{
    public InfiniteScroll ChapterScrollList;
    public GameObject SelectedChapterName; // 선택 중인 챕터
    public TextMeshProUGUI SelectedChapterNameTxt;
    public Button SelectBtn;

    private int mSelectedChapter; // 현재 선택 중인 챕터 번호

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
        if(userPlayData == null)
        {
            Logger.LogError("UserPlayData does not exist.");
            return;
        }

        mSelectedChapter = userPlayData.SelectedChapter;

        SetSelectedChatper();
        SetChapterScrollList();

        ChapterScrollList.MoveTo(mSelectedChapter - 1, InfiniteScroll.MoveToType.MOVE_TO_CENTER);
        ChapterScrollList.OnSnap = (currentSnappedIndex/*현재 중앙으로 표시된 챕터의 인덱스*/) =>
        {
            var chapterListUI = UIManager.Instance.GetActiveUI<ChapterListUI>() as ChapterListUI; // 현재 열려있는 UI 화면을 받아와서
            if (chapterListUI != null)
            {
                chapterListUI.OnSnap(currentSnappedIndex + 1); // 현재 중앙에 위치한 챕터 번호를 매개변수로 넘겨줌
            }
        };
    }

    private void SetSelectedChatper()
    {
        if(mSelectedChapter <= GlobalDefine.MAX_CHAPTER)
        {
            // UI 요소 활성화
            SelectedChapterName.SetActive(true);
            SelectBtn.gameObject.SetActive(true);

            var itemData = DataTableManager.Instance.GetChapterData(mSelectedChapter);
            if(itemData != null)
            {
                SelectedChapterNameTxt.text = itemData.ChapterName;
            }
        }
        else
        {
            // 게임 내 추가되지 않은 챕터라면 UI 비활성화
            SelectedChapterName.SetActive(false);
            SelectBtn.gameObject.SetActive(false);
        }
    }

    private void SetChapterScrollList()
    {
        ChapterScrollList.Clear();

        for (int i = 1; i <= GlobalDefine.MAX_CHAPTER + 1; i++)
        {
            var chapterItemData = new ChapterScrollItemData();
            chapterItemData.ChapterNo = i;
            ChapterScrollList.InsertData(chapterItemData);
        }
    }

    public void OnSnap(int selectedChapter)
    {
        mSelectedChapter = selectedChapter;

        SetSelectedChatper();
    }

    public void OnClickSelect()
    {
        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
        if(userPlayData == null)
        {
            Logger.LogError("UserPlayData does not exist.");
            return;
        }

        // 해금된 챕터라면 선택할 수 있도록 설정
        if(mSelectedChapter <= userPlayData.MaxClearedChapter + 1)
        {
            userPlayData.SelectedChapter = mSelectedChapter;
            LobbyManager.Instance.LobbyUIController.SetCurChapter();
            CloseUI(); // ChapterListUI는 꺼줌
        }
    }
}
