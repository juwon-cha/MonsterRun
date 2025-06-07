using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UserGoodsData : IUserData
{
    public bool IsLoaded { get; set; }
    // 보석
    public long Gem { get; set; }
    // 골드
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
        // 데이터베이스에 해당 필드가 없다면 오류 위험
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
