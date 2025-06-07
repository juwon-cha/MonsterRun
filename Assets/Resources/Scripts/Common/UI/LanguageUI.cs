using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageUI : BaseUI
{
    public void OnClickEnglishBtn()
    {
        Logger.Log($"{GetType()}::OnClickEnglishBtn");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        LanguageSettingsHelper.ChangeLanguage(SystemLanguage.English);
        var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        if(userSettingsData != null)
        {
            userSettingsData.Language = SystemLanguage.English;
            userSettingsData.SaveData();
        }
    }

    public void OnClickKoreanBtn()
    {
        Logger.Log($"{GetType()}::OnClickKoreanBtn");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        LanguageSettingsHelper.ChangeLanguage(SystemLanguage.Korean);
        var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        if (userSettingsData != null)
        {
            userSettingsData.Language = SystemLanguage.Korean;
            userSettingsData.SaveData();
        }
    }

    public override void OnClickCloseButton()
    {
        SceneLoader.Instance.LoadScene(ESceneType.Lobby);
    }
}
