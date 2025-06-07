#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public enum EBuildType
{
    DEV, // apk
    TEST, // 테스트 aab
    RELEASE, // 출시용 aab 
}

public class BuildManager : Editor
{
    public const string DEV_SCRIPTING_DEFINE_SYMBOLS = "DEV_VER";
    public const string RELEASE_SCRIPTING_DEFINE_SYMBOLS = "";

    // mBuildType이 DEV로만 설정되는 문제
    // -> 정적 변수이지만 Editor 스크립트에서는 Unity가 다시 컴파일하거나 에디터가 재시작될 때 값이 초기화된다.
    // -> EditorPrefs 사용
    //private static EBuildType mBuildType;

    // EditorPrefs를 통해 BuildType 저장 및 불러오기
    private const string BuildTypeKey = "BuildManager_BuildType";
    private static EBuildType mBuildType
    {
        get
        {
            // 만약 저장된 값이 없다면 기본값 (int)BuildType.DEV -> 0을 반환
            int type = EditorPrefs.GetInt("BuildManager_BuildType", (int)EBuildType.DEV);
            return (EBuildType)type;
        }

        set
        {
            EditorPrefs.SetInt("BuildManager_BuildType", (int)value);
        }
    }

    // 안드로이드 개발용 빌드 설정
    [MenuItem("Build/Set AOS DEV Build Settings")]
    public static void SetAOSDevBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = false; // apk 파일로 빌드
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, DEV_SCRIPTING_DEFINE_SYMBOLS);

        mBuildType = EBuildType.DEV;

        Logger.Log("Android DEV 설정 완료");
    }

    // 안드로이드 테스트용 빌드 설정
    [MenuItem("Build/Set AOS TEST Build Settings")]
    public static void SetAOSTestBuildSettings()
    {
        // 구글 플레이에 업로드해야하므로 apk가 아닌 aab로 빌드
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, DEV_SCRIPTING_DEFINE_SYMBOLS);

        mBuildType = EBuildType.TEST;

        Logger.Log("Android TEST 설정 완료");
    }

    // 안드로이드 출시용 빌드 설정
    [MenuItem("Build/Set AOS RELEASE Build Settings")]
    public static void SetAOSReleaseBuildSettings()
    {
        // 구글 플레이에 업로드해야하므로 apk가 아닌 aab로 빌드
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, RELEASE_SCRIPTING_DEFINE_SYMBOLS);

        mBuildType = EBuildType.RELEASE;

        Logger.Log("Android RELEASE 설정 완료");
    }

    // 빌드 추출 실행
    [MenuItem("Build/Start AOS Build")]
    public static void StartAOSBuild()
    {
        // keystore 설정
        PlayerSettings.Android.keystoreName = "Builds/AOS/CHACHACHA.keystore";
        PlayerSettings.Android.keystorePass = "CHACHACHA";
        PlayerSettings.Android.keyaliasName = "monsterrun";
        PlayerSettings.Android.keyaliasPass = "monsterrun";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[]
        {
            "Assets/Scenes/Title.unity",
            "Assets/Scenes/Lobby.unity",
            "Assets/Scenes/InGame.unity",
        };
        buildPlayerOptions.target = BuildTarget.Android;

        // 빌드 파일 확장자명, 압축 방법 설정
        string fileExtension = string.Empty;
        BuildOptions compressOption = BuildOptions.None;

        // 개발용 -> apk, Lz4
        // 테스트용, 출시용 -> aab, Lz4HC
        switch (mBuildType)
        {
            case EBuildType.DEV:
                fileExtension = "apk";
                compressOption = BuildOptions.CompressWithLz4;
                break;

            case EBuildType.TEST:
            case EBuildType.RELEASE:
                fileExtension = "aab";
                compressOption = BuildOptions.CompressWithLz4HC;
                break;

            default:
                break;
        }

        buildPlayerOptions.locationPathName = $"Builds/AOS/MonsterRun_{Application.version}_{DateTime.Now.ToString("yyMMdd_HHmmss")}.{fileExtension}";
        buildPlayerOptions.options = compressOption;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        if(summary.result == BuildResult.Succeeded)
        {
            Logger.Log($"Build succeeded. {summary.totalSize} bytes.");
        }
        else
        {
            Logger.LogError($"Build failed.");
        }
    }
}
#endif