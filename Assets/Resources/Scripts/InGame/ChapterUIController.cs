using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterUIController : MonoBehaviour
{
    public void Init()
    {
        
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            // 챕터 클리어나 게임오버 화면이 나오면 일시정지 안됨
            if (!ChapterManager.Instance.IsPaused && !ChapterManager.Instance.IsChapterCleared && !GameManager.Instance.IsGameOver)
            {
                var uiData = new BaseUIData();
                UIManager.Instance.OpenUI<PauseUI>(uiData);

                ChapterManager.Instance.PauseGame();
            }
        }
    }

    private void Update()
    {
        if(!ChapterManager.Instance.IsPaused && !ChapterManager.Instance.IsChapterCleared && !GameManager.Instance.IsGameOver)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

            var uiData = new BaseUIData();
            UIManager.Instance.OpenUI<PauseUI>(uiData);

            ChapterManager.Instance.PauseGame();
        }
    }

    public void OnClickPauseBtn()
    {
        // 게임 오버 UI가 나오면 일시정지 안됨
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var uiData = new BaseUIData();
        UIManager.Instance.OpenUI<PauseUI>(uiData);

        ChapterManager.Instance.PauseGame();
    }
}
