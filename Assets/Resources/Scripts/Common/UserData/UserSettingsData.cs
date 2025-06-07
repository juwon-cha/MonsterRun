using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

// 언어 변경 담당 클래스
public static class LanguageSettingsHelper
{
    private static Dictionary<SystemLanguage, string> languageCodeDic = new Dictionary<SystemLanguage, string>
    {
        { SystemLanguage.English, "en-US" },
        { SystemLanguage.Korean, "ko-KR" },
    };

    public static void ChangeLanguage(SystemLanguage systemLanguage)
    {
        if(languageCodeDic.TryGetValue(systemLanguage, out string localeCode))
        {
            Locale newLocale = LocalizationSettings.AvailableLocales.GetLocale(systemLanguage);
            if(newLocale != null)
            {
                LocalizationSettings.SelectedLocale = newLocale;
                Logger.Log($"Language changed to: {newLocale.Identifier.Code}");
            }
            else
            {
                Logger.LogError($"Locale with code {localeCode} not found");
            }
        }
        else
        {
            Logger.LogError($"System language {systemLanguage} not mapped to a locale code");
        }
    }
}

public class UserSettingsData : IUserData
{
    public bool IsLoaded { get; set; }
    public bool BGM { get; set; }
    public bool SFX { get; set; }

    public SystemLanguage Language { get; set; } = SystemLanguage.English;

    public void SetDefaultData()
    {
        Logger.Log($"{GetType()}::SetDefaultData");

        BGM = true;
        SFX = true;
        Language = Application.systemLanguage;
        LanguageSettingsHelper.ChangeLanguage(Language);
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");

        FirebaseManager.Instance.LoadUserData<UserSettingsData>(() =>
        {
            LanguageSettingsHelper.ChangeLanguage(Language);
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");

        FirebaseManager.Instance.SaveUserData<UserSettingsData>(ConvertDataToFirestoreDict());
    }

    // DB(Firestore)에 저장
    private Dictionary<string, object> ConvertDataToFirestoreDict()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "BGM", BGM },
            { "SFX", SFX },
            { "Language", Language.ToString() }
        };

        return dict;
    }

    public void SetData(Dictionary<string, object> firestoreDict)
    {
        ConvertFirestoreDictToData(firestoreDict);
    }

    // DB(Firestore)에서 로드
    private void ConvertFirestoreDictToData(Dictionary<string, object> dict)
    {
        //BGM = (bool)dict["BGM"];
        //SFX = (bool)dict["SFX"];

        if (dict.TryGetValue("BGM", out var bgmValue) && bgmValue is bool bgm)
        {
            BGM = bgm;
        }

        if (dict.TryGetValue("SFX", out var sfxValue) && sfxValue is bool sfx)
        {
            SFX = sfx;
        }

        if (dict.TryGetValue("Language", out var languageValue) && languageValue is string language)
        {
            Language = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), language);
        }
    }
}
