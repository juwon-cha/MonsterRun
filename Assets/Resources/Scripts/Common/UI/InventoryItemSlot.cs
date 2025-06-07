using Gpm.Ui;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlotData : InfiniteScrollData
{
    public long SerialNumber;
    public int ItemID;
}

public class InventoryItemSlot : InfiniteScrollItem
{
    public Image ItemGradeBg;
    public Image ItemIcon;

    private InventoryItemSlotData mInventoryItemSlotData;

    public override void UpdateData(InfiniteScrollData scrollData)
    {
        base.UpdateData(scrollData);

        mInventoryItemSlotData = scrollData as InventoryItemSlotData;
        if (mInventoryItemSlotData == null)
        {
            Logger.Log("mInventoryItemSlotData is invalid.");
            return;
        }

        var itemGrade = (EItemGrade)((mInventoryItemSlotData.ItemID / 1000) % 10);
        var gradeBgTexture = Resources.Load<Texture2D>($"Textures/{itemGrade}");
        if (gradeBgTexture != null)
        {
            ItemGradeBg.sprite = Sprite.Create(gradeBgTexture, new Rect(0, 0, gradeBgTexture.width, gradeBgTexture.height), new Vector2(1f, 1f));
        }

        StringBuilder sb = new StringBuilder(mInventoryItemSlotData.ItemID.ToString());
        sb[1] = '1';
        var itemIconName = sb.ToString();

        var itemIconTexture = Resources.Load<Texture2D>($"Textures/{itemIconName}");
        if(itemIconTexture != null)
        {
            ItemIcon.sprite = Sprite.Create(itemIconTexture, new Rect(0, 0, itemIconTexture.width, itemIconTexture.height), new Vector2(1f, 1f));
        }
    }

    public void OnClickInventoryItemSlot()
    {
        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var uiData = new EquipmentUIData();
        uiData.SerialNumber = mInventoryItemSlotData.SerialNumber;
        uiData.ItemID = mInventoryItemSlotData.ItemID;
        UIManager.Instance.OpenUI<EquipmentUI>(uiData);
    }
}
