using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIData : BaseUIData
{
    public long SerialNumber;
    public int ItemID;
    public bool IsEquipped;
}

public class EquipmentUI : BaseUI
{
    public Image ItemGradeBg;
    public Image ItemIcon;
    public TextMeshProUGUI ItemGradeTxt;
    public TextMeshProUGUI ItemNameTxt;
    public TextMeshProUGUI AttackPowerAmountTxt;
    public TextMeshProUGUI DefenseAmountTxt;
    public TextMeshProUGUI EquipBtnTxt;

    private EquipmentUIData mEquipmentUIData;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        mEquipmentUIData = uiData as EquipmentUIData;
        if(mEquipmentUIData == null)
        {
            Logger.LogError("mEquipmentUIData is invalid.");
            return;
        }

        var itemData = DataTableManager.Instance.GetItemData(mEquipmentUIData.ItemID);
        if(itemData == null)
        {
            Logger.LogError($"Item data is invalid. ItemId:{mEquipmentUIData.ItemID}");
            return;
        }

        var itemGrade = (EItemGrade)((mEquipmentUIData.ItemID / 1000) % 10);
        var gradeBgTexture = Resources.Load<Texture2D>($"Textures/{itemGrade}");
        if(gradeBgTexture != null)
        {
            ItemGradeBg.sprite = Sprite.Create(gradeBgTexture, new Rect(0, 0, gradeBgTexture.width, gradeBgTexture.height), new Vector2(1f, 1f));
        }

        // 등급 텍스트 설정
        ItemGradeTxt.text = itemGrade.ToString();
        // 등급 텍스트 색상 설정
        var hexColor = string.Empty;
        switch (itemGrade)
        {
            case EItemGrade.Common:
                hexColor = "#1AB3FF";
                break;
            case EItemGrade.Uncommon:
                hexColor = "#51C52C";
                break;
            case EItemGrade.Rare:
                hexColor = "#EA5AFF";
                break;
            case EItemGrade.Epic:
                hexColor = "#FF9900";
                break;
            case EItemGrade.Legendary:
                hexColor = "#F24949";
                break;
            default:
                break;
        }

        Color color;
        if(ColorUtility.TryParseHtmlString(hexColor, out color))
        {
            ItemGradeTxt.color = color;
        }

        StringBuilder sb = new StringBuilder(mEquipmentUIData.ItemID.ToString());
        sb[1] = '1';
        var itemIconName = sb.ToString();

        var itemIconTexture = Resources.Load<Texture2D>($"Textures/{itemIconName}");
        if (itemIconTexture != null)
        {
            ItemIcon.sprite = Sprite.Create(itemIconTexture, new Rect(0, 0, itemIconTexture.width, itemIconTexture.height), new Vector2(1f, 1f));
        }

        ItemNameTxt.text = itemData.ItemName;
        AttackPowerAmountTxt.text = $"+{itemData.AttackPower}";
        DefenseAmountTxt.text = $"+{itemData.Defense}";

        // 아이템 장착 여부에 따라 텍스트 출력
        EquipBtnTxt.text = mEquipmentUIData.IsEquipped ? "Unequip" : "Equip";
    }

    public void OnClickEquipBtn()
    {
        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if(userInventoryData == null)
        {
            Logger.Log("UserInventoryData does not exist.");
            return;
        }

        if(mEquipmentUIData.IsEquipped)
        {
            userInventoryData.UnequipItem(mEquipmentUIData.SerialNumber, mEquipmentUIData.ItemID);
        }
        else
        {
            userInventoryData.EquipItem(mEquipmentUIData.SerialNumber, mEquipmentUIData.ItemID);
        }

        userInventoryData.SaveData();

        // 아이템 장착/탈착 후 인벤토리 UI 갱신
        var inventoryUI = UIManager.Instance.GetActiveUI<InventoryUI>() as InventoryUI;
        if(inventoryUI != null)
        {
            if(mEquipmentUIData.IsEquipped)
            {
                inventoryUI.OnUnequipItem(mEquipmentUIData.ItemID);
            }
            else
            {
                inventoryUI.OnEquipItem(mEquipmentUIData.ItemID);
            }
        }

        // 현재 이 UI 화면 닫아줌
        CloseUI();
    }
}
