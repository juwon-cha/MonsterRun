using System;
using System.Diagnostics;

// 1. 추가적인 정보 표현
// 타임 스탬프: 게임 진행에서 각 단계의 처리가 얼마나 걸리는지 확인
// -> 어느 부분을 우선순위로 최적화해야 할지 판단 가능
// -> 최적화로 얼마나 성능 향상이 이뤄졌는지 판단 가능

// 2. 출시용 빌드를 위한 로그 제거 용이하게 하기 위함
// 개발용 빌드에서는 로그는 상관없지만
// 출시용 빌드에서는 성능에 부하가 생길 수 있어서 제거 필요(일괄적으로 제거)
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
