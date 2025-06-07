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
    // SSL Connection Error�� ��� ����
    private const string TIME_URL = "https://worldtimeapi.org/api/ip";

    // �񵿱�� ��¥�� �޾ƿ;��ϴ� ���� -> �� ��û�� ��Ʈ��ũ �������� ���� �ð��� �ɸ� �� �ֱ� ����
    // -> �񵿱�� �ۼ��ؾ� �� �۾��� �Ϸ�ɶ����� ���α׷��� �ߴܵ��� ����(�ٸ� �۾� ��� ����)
    public async Task<DateTime> GetCurrentDateTime()
    {
        // using ��� -> �߰�ȣ ����� request ��ü�� �޸𸮿��� ����
        using (UnityWebRequest request = UnityWebRequest.Get(TIME_URL))
        {
            // �� ��û
            var operation = request.SendWebRequest();

            // operation �۾��� �Ϸ�ɶ����� ���
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            // operation �۾� ������ ��� �޾ƿ� -> ����� ������ �ִ��� üũ
            if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Logger.LogError($"Error: {request.error}");
                return DateTime.MinValue; // 0001-01-01 00:00:00(�ּҰ�)
            }

            // ������� �޾ƿ� ���� �ð��� TimeDataWrapper�� ��ȯ
            Logger.Log($"Raw JSON: {request.downloadHandler.text}");
            TimeDataWrapper timeData = JsonUtility.FromJson<TimeDataWrapper>(request.downloadHandler.text);
            // timeData�� DateTime Ÿ������ ��ȯ
            var currentDateTime = ParseDateTime(timeData.dateTime);
            Logger.Log($"Current datetime: {currentDateTime}");

            return currentDateTime;
        }
    }

    private DateTime ParseDateTime(string dateTimeString)
    {
        // 2025-05-02T15:54:21.5798445
        string date = Regex.Match(dateTimeString, @"^\d{4}-\d{2}-\d{2}").Value; // ��¥ ����
        string time = Regex.Match(dateTimeString, @"\d{2}:\d{2}:\d{2}").Value; // �ð� ����

        return DateTime.Parse($"{date} {time}");
    }
}
