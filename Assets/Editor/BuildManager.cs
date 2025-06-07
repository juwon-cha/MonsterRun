#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public enum EBuildType
{
    DEV, // apk
    TEST, // �׽�Ʈ aab
    RELEASE, // ��ÿ� aab 
}

public class BuildManager : Editor
{
    public const string DEV_SCRIPTING_DEFINE_SYMBOLS = "DEV_VER";
    public const string RELEASE_SCRIPTING_DEFINE_SYMBOLS = "";

    // mBuildType�� DEV�θ� �����Ǵ� ����
    // -> ���� ���������� Editor ��ũ��Ʈ������ Unity�� �ٽ� �������ϰų� �����Ͱ� ����۵� �� ���� �ʱ�ȭ�ȴ�.
    // -> EditorPrefs ���
    //private static EBuildType mBuildType;

    // EditorPrefs�� ���� BuildType ���� �� �ҷ�����
    private const string BuildTypeKey = "BuildManager_BuildType";
    private static EBuildType mBuildType
    {
        get
        {
            // ���� ����� ���� ���ٸ� �⺻�� (int)BuildType.DEV -> 0�� ��ȯ
            int type = EditorPrefs.GetInt("BuildManager_BuildType", (int)EBuildType.DEV);
            return (EBuildType)type;
        }

        set
        {
            EditorPrefs.SetInt("BuildManager_BuildType", (int)value);
        }
    }

    // �ȵ���̵� ���߿� ���� ����
    [MenuItem("Build/Set AOS DEV Build Settings")]
    public static void SetAOSDevBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = false; // apk ���Ϸ� ����
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, DEV_SCRIPTING_DEFINE_SYMBOLS);

        mBuildType = EBuildType.DEV;

        Logger.Log("Android DEV ���� �Ϸ�");
    }

    // �ȵ���̵� �׽�Ʈ�� ���� ����
    [MenuItem("Build/Set AOS TEST Build Settings")]
    public static void SetAOSTestBuildSettings()
    {
        // ���� �÷��̿� ���ε��ؾ��ϹǷ� apk�� �ƴ� aab�� ����
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, DEV_SCRIPTING_DEFINE_SYMBOLS);

        mBuildType = EBuildType.TEST;

        Logger.Log("Android TEST ���� �Ϸ�");
    }

    // �ȵ���̵� ��ÿ� ���� ����
    [MenuItem("Build/Set AOS RELEASE Build Settings")]
    public static void SetAOSReleaseBuildSettings()
    {
        // ���� �÷��̿� ���ε��ؾ��ϹǷ� apk�� �ƴ� aab�� ����
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, RELEASE_SCRIPTING_DEFINE_SYMBOLS);

        mBuildType = EBuildType.RELEASE;

        Logger.Log("Android RELEASE ���� �Ϸ�");
    }

    // ���� ���� ����
    [MenuItem("Build/Start AOS Build")]
    public static void StartAOSBuild()
    {
        // keystore ����
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

        // ���� ���� Ȯ���ڸ�, ���� ��� ����
        string fileExtension = string.Empty;
        BuildOptions compressOption = BuildOptions.None;

        // ���߿� -> apk, Lz4
        // �׽�Ʈ��, ��ÿ� -> aab, Lz4HC
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