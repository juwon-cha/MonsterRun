using Gpm.Ui;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterListUI : BaseUI
{
    public InfiniteScroll ChapterScrollList;
    public GameObject SelectedChapterName; // ���� ���� é��
    public TextMeshProUGUI SelectedChapterNameTxt;
    public Button SelectBtn;

    private int mSelectedChapter; // ���� ���� ���� é�� ��ȣ

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
        ChapterScrollList.OnSnap = (currentSnappedIndex/*���� �߾����� ǥ�õ� é���� �ε���*/) =>
        {
            var chapterListUI = UIManager.Instance.GetActiveUI<ChapterListUI>() as ChapterListUI; // ���� �����ִ� UI ȭ���� �޾ƿͼ�
            if (chapterListUI != null)
            {
                chapterListUI.OnSnap(currentSnappedIndex + 1); // ���� �߾ӿ� ��ġ�� é�� ��ȣ�� �Ű������� �Ѱ���
            }
        };
    }

    private void SetSelectedChatper()
    {
        if(mSelectedChapter <= GlobalDefine.MAX_CHAPTER)
        {
            // UI ��� Ȱ��ȭ
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
            // ���� �� �߰����� ���� é�Ͷ�� UI ��Ȱ��ȭ
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

        // �رݵ� é�Ͷ�� ������ �� �ֵ��� ����
        if(mSelectedChapter <= userPlayData.MaxClearedChapter + 1)
        {
            userPlayData.SelectedChapter = mSelectedChapter;
            LobbyManager.Instance.LobbyUIController.SetCurChapter();
            CloseUI(); // ChapterListUI�� ����
        }
    }
}
