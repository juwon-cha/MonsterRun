using TMPro;
using UnityEngine;

public class SettingsUI : BaseUI
{
    public TextMeshProUGUI GameVersionTxt;

    public GameObject BGMOnToggle;
    public GameObject BGMOffToggle;

    public GameObject SFXOnToggle;
    public GameObject SFXOffToggle;

    private const string PRIVACY_POLICY_URL = "https://sites.google.com/view/chachacha-monster-run/%ED%99%88";

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        SetGameVersion();

        var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        if(userSettingsData != null)
        {
            SetBGMSetting(userSettingsData.BGM);
            SetSFXSetting(userSettingsData.SFX);
        }
    }

    private void SetGameVersion()
    {
        GameVersionTxt.text = $"Version:{Application.version}";
    }

    private void SetBGMSetting(bool bgm)
    {
        BGMOnToggle.SetActive(bgm);
        BGMOffToggle.SetActive(!bgm);
    }

    private void SetSFXSetting(bool sfx)
    {
        SFXOnToggle.SetActive(sfx);
        SFXOffToggle.SetActive(!sfx);
    }    

    public void OnClickBGMOnToggle()
    {
        Logger.Log($"{GetType()}::OnClickSoundOnToggle");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        if (userSettingsData != null)
        {
            userSettingsData.BGM = false;
            userSettingsData.SaveData();
            AudioManager.Instance.MuteBGM();
            SetBGMSetting(userSettingsData.BGM);
        }
    }

    public void OnClickBGMOffToggle()
    {
        Logger.Log($"{GetType()}::OnClickSoundOffToggle");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        if (userSettingsData != null)
        {
            userSettingsData.BGM = true;
            userSettingsData.SaveData();
            AudioManager.Instance.UnMuteBGM();
            SetBGMSetting(userSettingsData.BGM);
        }
    }

    public void OnClickSFXOnToggle()
    {
        Logger.Log($"{GetType()}::OnClickSFXOnToggle");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        if (userSettingsData != null)
        {
            userSettingsData.SFX = false;
            userSettingsData.SaveData();
            AudioManager.Instance.MuteSFX();
            SetSFXSetting(userSettingsData.SFX);
        }
    }

    public void OnClickSFXOffToggle()
    {
        Logger.Log($"{GetType()}::OnClickSFXOffToggle");

        //AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        if (userSettingsData != null)
        {
            userSettingsData.SFX = true;
            userSettingsData.SaveData();
            AudioManager.Instance.UnMuteSFX();
            SetSFXSetting(userSettingsData.SFX);
        }
    }

    public void OnClickPrivacyPolicyURL()
    {
        Logger.Log($"{GetType()}::OnClickPrivacyPolicyURL");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);
        Application.OpenURL(PRIVACY_POLICY_URL);
    }

    public void OnClickLogoutBtn()
    {
        Logger.Log($"{GetType()}::OnClickLogoutBtn");

        FirebaseManager.Instance.SignOut();
    }

    public void OnClickLanguageBtn()
    {
        Logger.Log($"{GetType()}::OnClickLanguageBtn");
        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var uiData = new BaseUIData();
        UIManager.Instance.OpenUIFromAA<LanguageUI>(uiData);
    }
}
