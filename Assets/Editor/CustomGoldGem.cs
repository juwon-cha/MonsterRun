using SuperMaxim.Messaging;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class CustomGoldGem : Editor
{
    [MenuItem("Tools/Add User Gem(+100)")]
    public static void AddUserGem()
    {
        //var gem = long.Parse(PlayerPrefs.GetString("Gem"));
        //gem += 100;

        //PlayerPrefs.SetString("Gem", gem.ToString());
        //PlayerPrefs.Save();

        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if(userGoodsData != null)
        {
            userGoodsData.Gem += 100;
            userGoodsData.SaveData();
            var gemUpdateMsg = new GemUpdateMsg();
            gemUpdateMsg.IsAdd = true;
            Messenger.Default.Publish(gemUpdateMsg);
        }
    }

    [MenuItem("Tools/Add User Gold(+100)")]
    public static void AddUserGold()
    {
        //var gold = long.Parse(PlayerPrefs.GetString("Gold"));
        //gold += 100;

        //PlayerPrefs.SetString("Gold", gold.ToString());
        //PlayerPrefs.Save();

        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if (userGoodsData != null)
        {
            userGoodsData.Gold += 100;
            userGoodsData.SaveData();
            var goldUpdateMsg = new GoldUpdateMsg();
            goldUpdateMsg.IsAdd = true;
            Messenger.Default.Publish(goldUpdateMsg);
        }
    }
}
#endif