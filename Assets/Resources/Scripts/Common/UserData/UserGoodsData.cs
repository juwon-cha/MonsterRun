using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UserGoodsData : IUserData
{
    public bool IsLoaded { get; set; }
    // ����
    public long Gem { get; set; }
    // ���
    public long Gold { get; set; }

    public void SetDefaultData()
    {
        Logger.Log($"{GetType()}::SetDefaultData");

        Gem = 0;
        Gold = 0;
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");

        FirebaseManager.Instance.LoadUserData<UserGoodsData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");

        FirebaseManager.Instance.SaveUserData<UserGoodsData>(ConvertDataToFirestoreDict());
    }

    private Dictionary<string, object> ConvertDataToFirestoreDict()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "Gem", Gem },
            { "Gold", Gold }
        };

        return dict;
    }

    public void SetData(Dictionary<string, object> firestoreDict)
    {
        ConvertFirestoreDictToData(firestoreDict);
    }

    private void ConvertFirestoreDictToData(Dictionary<string, object> dict)
    {
        // �����ͺ��̽��� �ش� �ʵ尡 ���ٸ� ���� ����
        //Gem = (long)dict["Gem"];
        //Gold = (long)dict["Gold"];

        if (dict.TryGetValue("Gem", out var gemValue) && gemValue is long gem)
        {
            Gem = gem;
        }

        if (dict.TryGetValue("Gold", out var goldValue) && goldValue is long gold)
        {
            Gold = gold;
        }
    }
}
