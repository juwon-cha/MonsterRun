using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUI : BaseUI
{
    public void OnClickResume()
    {
        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        ChapterManager.Instance.ResumeGame();

        CloseUI();
    }

    public void OnClickHome()
    {
        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        SceneLoader.Instance.LoadScene(ESceneType.Lobby);

        CloseUI();
    }
}
