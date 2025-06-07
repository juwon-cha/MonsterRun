using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class TimeDataWrapper
{
    public string dateTime;
}

public class NetworkController : SingletonBehaviour<NetworkController>
{
    // SSL Connection Error로 사용 못함
    private const string TIME_URL = "https://worldtimeapi.org/api/ip";

    // 비동기로 날짜를 받아와야하는 이유 -> 웹 요청은 네트워크 지연으로 인해 시간이 걸릴 수 있기 때문
    // -> 비동기로 작성해야 이 작업이 완료될때까지 프로그램이 중단되지 않음(다른 작업 계속 수행)
    public async Task<DateTime> GetCurrentDateTime()
    {
        // using 사용 -> 중괄호 벗어나면 request 객체는 메모리에서 삭제
        using (UnityWebRequest request = UnityWebRequest.Get(TIME_URL))
        {
            // 웹 요청
            var operation = request.SendWebRequest();

            // operation 작업이 완료될때까지 대기
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            // operation 작업 끝나면 결과 받아옴 -> 결과에 에러가 있는지 체크
            if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Logger.LogError($"Error: {request.error}");
                return DateTime.MinValue; // 0001-01-01 00:00:00(최소값)
            }

            // 결과에서 받아온 현재 시간을 TimeDataWrapper로 변환
            Logger.Log($"Raw JSON: {request.downloadHandler.text}");
            TimeDataWrapper timeData = JsonUtility.FromJson<TimeDataWrapper>(request.downloadHandler.text);
            // timeData를 DateTime 타입으로 변환
            var currentDateTime = ParseDateTime(timeData.dateTime);
            Logger.Log($"Current datetime: {currentDateTime}");

            return currentDateTime;
        }
    }

    private DateTime ParseDateTime(string dateTimeString)
    {
        // 2025-05-02T15:54:21.5798445
        string date = Regex.Match(dateTimeString, @"^\d{4}-\d{2}-\d{2}").Value; // 날짜 추출
        string time = Regex.Match(dateTimeString, @"\d{2}:\d{2}:\d{2}").Value; // 시간 추출

        return DateTime.Parse($"{date} {time}");
    }
}
