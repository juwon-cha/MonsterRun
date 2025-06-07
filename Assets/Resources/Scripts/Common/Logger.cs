using System;
using System.Diagnostics;

// 1. �߰����� ���� ǥ��
// Ÿ�� ������: ���� ���࿡�� �� �ܰ��� ó���� �󸶳� �ɸ����� Ȯ��
// -> ��� �κ��� �켱������ ����ȭ�ؾ� ���� �Ǵ� ����
// -> ����ȭ�� �󸶳� ���� ����� �̷������� �Ǵ� ����

// 2. ��ÿ� ���带 ���� �α� ���� �����ϰ� �ϱ� ����
// ���߿� ���忡���� �α״� ���������
// ��ÿ� ���忡���� ���ɿ� ���ϰ� ���� �� �־ ���� �ʿ�(�ϰ������� ����)
public static class Logger
{
    [Conditional("DEV_VER")]
    public static void Log(string msg)
    {
        UnityEngine.Debug.LogFormat("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg);
    }

    [Conditional("DEV_VER")]
    public static void LogWarning(string msg)
    {
        UnityEngine.Debug.LogWarningFormat("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg);
    }

    public static void LogError(string msg)
    {
        UnityEngine.Debug.LogErrorFormat("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg);
    }
}
