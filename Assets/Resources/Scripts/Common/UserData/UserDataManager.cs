using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// ���̳ʸ� ������ ��ȯ Ŭ����
public static class DataConvertHelper
{
    // ������ -> ���̳ʸ�
    public static string ConvertDataToBinary<T>(T data)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter(); // �����͸� ���̳ʸ��� ��ȯ�ϴ� ��� ���
        MemoryStream memoryStream = new MemoryStream(); // ���̳ʸ��� ��ȯ�� �����͸� �ӽ÷� ������ ����
        binaryFormatter.Serialize(memoryStream, data); // data�� ���̳ʸ��� ��ȯ
        return Convert.ToBase64String(memoryStream.ToArray()); // ToBase64String -> ���̳ʸ� �����͸� ���ڿ��� ��ȯ�� �� �ս��� �ȵǰ� �ϱ� ����
    }

    // ���̳ʸ� -> ���ϴ� Ÿ��
    public static T ConvertBinaryToData<T>(string binaryData)
    {
        byte[] bytes = Convert.FromBase64String(binaryData); // ���ڿ��� ���ڵ��ߴ� �����͸� ���̳ʸ� �����ͷ� ��ȯ
        MemoryStream memoryStream = new MemoryStream(bytes); // �޸𸮽�Ʈ���� ����
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        return (T)binaryFormatter.Deserialize(memoryStream);
    }
}

public class UserDataManager : SingletonBehaviour<UserDataManager>
{
    // ����� ���� ������ ���� ����
    public bool ExistsSaveData { get; private set; }

    // ��� ���� ������ �ν��Ͻ��� �����ϴ� �����̳�
    public List<IUserData> UserDataList { get; private set; } = new List<IUserData>();

    protected override void Init()
    {
        base.Init();

        // ��� ���� �����͸� UserDataList�� �߰�
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
