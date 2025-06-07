using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestLootUIData : BaseUIData
{
    public string ChestID;
    public List<int> RewardItemIDList = new List<int>();
}

public class ChestLootUI : BaseUI
{
    public Image ChestImg;
    public GameObject RewardItemPrefab;
    public Transform RewardItemTrs;

    public ParticleSystem[] LootFx;

    private ChestLootUIData mChestLootUIData;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        // 뽑기 결과를 UI에 표시
        // 매개변수에서 받은 데이터를 상자뽑기UI데이터로 변환하여 대입
        mChestLootUIData = uiData as ChestLootUIData;
        if (mChestLootUIData == null)
        {
            Logger.LogError($"mChestLootUIData is invalid.");
            return;
        }

        // mChestLootUIData 데이터 변수에 있는 상자 ID로 ID에 맞는 이미지 로드, 이미지 변수에 대입
        var chestImgTexutre = Resources.Load<Texture2D>($"Textures/Shop_Icon_{mChestLootUIData.ChestID}_open");
        if(chestImgTexutre != null)
        {
            ChestImg.sprite = Sprite.Create(chestImgTexutre, new Rect(0, 0, chestImgTexutre.width, chestImgTexutre.height), new Vector2(1f, 1f));
        }

        // 뽑기 결과 얻은 아이템 표시
        // 결과 아이템이 배치되는 상위 오브젝트 트랜스폼 하위에 이미 세팅된 오브젝트가 있다면 삭제 처리
        foreach (Transform child in RewardItemTrs)
        {
            Destroy(child.gameObject);
        }

        // 뽑기 결과로 지급되는 아이템 리스트 순회
        foreach (var rewardItemID in mChestLootUIData.RewardItemIDList)
        {
            // 보상 아이템 프리팹으로 게임오브젝트 인스턴스 생성
            var rewardItemObject = Instantiate(RewardItemPrefab, Vector3.zero, Quaternion.identity);
            rewardItemObject.transform.SetParent(RewardItemTrs);
            rewardItemObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            rewardItemObject.transform.localScale = Vector3.one;
            var rewardItemUI = rewardItemObject.GetComponent<RewardItem>(); // 오브젝트 인스턴스에 연동된 RewardItem 스크립트 가져옴
            if(rewardItemUI != null)
            {
                rewardItemUI.SetInfo(rewardItemID);
            }
        }

        // 뽑기 이펙트 배열 순회하면서 재생
        for(int i = 0; i < LootFx.Length; ++i)
        {
            LootFx[i].Play();
        }
    }
}
