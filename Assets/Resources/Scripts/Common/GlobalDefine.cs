using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalDefine
{
    public const string GOOGLE_PLAY_STORE = "https://play.google.com/store/apps/detail?id=com.CHACHACHA.MonsterRun";
    public const string APPLE_APP_STORE = "";

    public const float THIRD_PARTY_SERVICE_INIT_TIME = 1f; // �� �ð��� �Ѿ �ʱ�ȭ �ȵǸ� ���� �߻� ��Ŵ

    public const int MAX_CHAPTER = 5;

    public enum ERewardType
    {
        Gold,
        Gem,
    }

    public const string LOCALIZATION_DATA_TABLE = "LocalizationDataTable";
    public static List<string> LocalizationLabels = new List<string>
    {
        "Locale", 
        "Locale-en",
        "Locale-ko"
    };
}
