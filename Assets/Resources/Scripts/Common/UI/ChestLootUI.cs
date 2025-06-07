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

        // �̱� ����� UI�� ǥ��
        // �Ű��������� ���� �����͸� ���ڻ̱�UI�����ͷ� ��ȯ�Ͽ� ����
        mChestLootUIData = uiData as ChestLootUIData;
        if (mChestLootUIData == null)
        {
            Logger.LogError($"mChestLootUIData is invalid.");
            return;
        }

        // mChestLootUIData ������ ������ �ִ� ���� ID�� ID�� �´� �̹��� �ε�, �̹��� ������ ����
        var chestImgTexutre = Resources.Load<Texture2D>($"Textures/Shop_Icon_{mChestLootUIData.ChestID}_open");
        if(chestImgTexutre != null)
        {
            ChestImg.sprite = Sprite.Create(chestImgTexutre, new Rect(0, 0, chestImgTexutre.width, chestImgTexutre.height), new Vector2(1f, 1f));
        }

        // �̱� ��� ���� ������ ǥ��
        // ��� �������� ��ġ�Ǵ� ���� ������Ʈ Ʈ������ ������ �̹� ���õ� ������Ʈ�� �ִٸ� ���� ó��
        foreach (Transform child in RewardItemTrs)
        {
            Destroy(child.gameObject);
        }

        // �̱� ����� ���޵Ǵ� ������ ����Ʈ ��ȸ
        foreach (var rewardItemID in mChestLootUIData.RewardItemIDList)
        {
            // ���� ������ ���������� ���ӿ�����Ʈ �ν��Ͻ� ����
            var rewardItemObject = Instantiate(RewardItemPrefab, Vector3.zero, Quaternion.identity);
            rewardItemObject.transform.SetParent(RewardItemTrs);
            rewardItemObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            rewardItemObject.transform.localScale = Vector3.one;
            var rewardItemUI = rewardItemObject.GetComponent<RewardItem>(); // ������Ʈ �ν��Ͻ��� ������ RewardItem ��ũ��Ʈ ������
            if(rewardItemUI != null)
            {
                rewardItemUI.SetInfo(rewardItemID);
            }
        }

        // �̱� ����Ʈ �迭 ��ȸ�ϸ鼭 ���
        for(int i = 0; i < LootFx.Length; ++i)
        {
            LootFx[i].Play();
        }
    }
}
