using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// 바이너리 데이터 변환 클래스
public static class DataConvertHelper
{
    // 데이터 -> 바이너리
    public static string ConvertDataToBinary<T>(T data)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter(); // 데이터를 바이너리로 변환하는 기능 담당
        MemoryStream memoryStream = new MemoryStream(); // 바이너리로 변환한 데이터를 임시로 저장할 변수
        binaryFormatter.Serialize(memoryStream, data); // data를 바이너리로 변환
        return Convert.ToBase64String(memoryStream.ToArray()); // ToBase64String -> 바이너리 데이터를 문자열로 변환할 때 손실이 안되게 하기 위함
    }

    // 바이너리 -> 원하는 타입
    public static T ConvertBinaryToData<T>(string binaryData)
    {
        byte[] bytes = Convert.FromBase64String(binaryData); // 문자열로 인코딩했던 데이터를 바이너리 데이터로 변환
        MemoryStream memoryStream = new MemoryStream(bytes); // 메모리스트림에 저장
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        return (T)binaryFormatter.Deserialize(memoryStream);
    }
}

public class UserDataManager : SingletonBehaviour<UserDataManager>
{
    // 저장된 유저 데이터 존재 여부
    public bool ExistsSaveData { get; private set; }

    // 모든 유저 데이터 인스턴스를 저장하는 컨테이너
    public List<IUserData> UserDataList { get; private set; } = new List<IUserData>();

    protected override void Init()
    {
        base.Init();

        // 모든 유저 데이터를 UserDataList에 추가
        UserDataList.Add(new UserSettingsData());
        UserDataList.Add(new UserGoodsData());
        UserDataList.Add(new UserInventoryData());
        UserDataList.Add(new UserPlayData());
        UserDataList.Add(new UserAchievementData());
    }

    public void SetDefaultUserData()
    {
        for(int i = 0; i < UserDataList.Count; ++i)
        {
            UserDataList[i].SetDefaultData();
        }
    }

    public void LoadUserData()
    {
        for (int i = 0; i < UserDataList.Count; ++i)
        {
            UserDataList[i].LoadData();
        }
    }

    public void SaveUserData()
    {
        for (int i = 0; i < UserDataList.Count; ++i)
        {
            UserDataList[i].SaveData();
        }
    }

    public T GetUserData<T>() where T : class, IUserData
    {
        return UserDataList.OfType<T>().FirstOrDefault();
    }

    public bool IsUserDataLoaded()
    {
        for (int i = 0; i < UserDataList.Count; i++)
        {
            if (!UserDataList[i].IsLoaded)
            {
                return false;
            }
        }

        return true;
    }
}
