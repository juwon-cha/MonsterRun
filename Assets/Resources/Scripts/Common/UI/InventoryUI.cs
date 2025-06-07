using Gpm.Ui;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EInventorySortType
{
    ItemGrade,
    ItemType,
}

public class InventoryUI : BaseUI
{
    public Image CharacterImg;

    public EquippedItemSlot WeaponSlot;
    public EquippedItemSlot ShieldSlot;
    public EquippedItemSlot ChestArmorSlot;
    public EquippedItemSlot BootsSlot;
    public EquippedItemSlot GlovesSlot;
    public EquippedItemSlot AccessorySlot;
    public EquippedItemSlot CharacterSlot;

    public InfiniteScroll InventoryScrollList;
    public TextMeshProUGUI SortBtnTxt;

    public TextMeshProUGUI AttackPowerAmountTxt;
    public TextMeshProUGUI DefenseAmountTxt;

    private EInventorySortType mInventorySortType = EInventorySortType.ItemGrade;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        SetUserStats();
        SetEquippedItems();
        SetInventory();
        SortInventory();
    }

    private void SetUserStats()
    {
        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if (userInventoryData == null)
        {
            Logger.LogError("UserInventoryData does not exist.");
            return;
        }

        UserItemStats userTotalItemStats = userInventoryData.GetUserTotalItemStats();

        AttackPowerAmountTxt.text = $"+{userTotalItemStats.AttackPower.ToString("N0")}";
        DefenseAmountTxt.text = $"+{userTotalItemStats.Defense.ToString("N0")}";
    }

    // 인벤토리 창에서 캐릭터 이미지 설정
    private void SetCharacterImg()
    {
        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if (userInventoryData == null)
        {
            Logger.LogError("UserInventoryData does not exist.");
            return;
        }

        if (userInventoryData.EquippedCharacterData != null)
        {
            StringBuilder sb = new StringBuilder(userInventoryData.EquippedCharacterData.ItemID.ToString());
            sb[1] = '1';
            var itemIconName = sb.ToString();

            var itemIconTexture = Resources.Load<Texture2D>($"Textures/{itemIconName}");
            if (itemIconTexture != null)
            {
                var color = CharacterImg.color;
                color.a = 1f; // 캐릭터를 다시 그려주기 위해 투명도 1로 복원
                CharacterImg.color = color;

                CharacterImg.sprite = Sprite.Create(itemIconTexture, new Rect(0, 0, itemIconTexture.width, itemIconTexture.height), new Vector2(1f, 1f));
            }
        }
        else // 캐릭터 장착하지 않았을 경우 이미지 안 보이게 설정
        {
            CharacterImg.sprite = null;
            var color = CharacterImg.color;
            color.a = 0f; // 완전히 투명하게 설정
            CharacterImg.color = color;
        }
    }

    private void SetEquippedItems()
    {
        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if(userInventoryData == null)
        {
            Logger.LogError("UserInventoryData does not exist.");
            return;
        }

        if (userInventoryData.EquippedWeaponData != null)
        {
            WeaponSlot.SetItem(userInventoryData.EquippedWeaponData);
        }
        else
        {
            WeaponSlot.ClearItem();
        }

        if (userInventoryData.EquippedShieldData != null)
        {
            ShieldSlot.SetItem(userInventoryData.EquippedShieldData);
        }
        else
        {
            ShieldSlot.ClearItem();
        }

        if (userInventoryData.EquippedChestArmorData != null)
        {
            ChestArmorSlot.SetItem(userInventoryData.EquippedChestArmorData);
        }
        else
        {
            ChestArmorSlot.ClearItem();
        }

        if (userInventoryData.EquippedBootsData != null)
        {
            BootsSlot.SetItem(userInventoryData.EquippedBootsData);
        }
        else
        {
            BootsSlot.ClearItem();
        }

        if (userInventoryData.EquippedGlovesData != null)
        {
            GlovesSlot.SetItem(userInventoryData.EquippedGlovesData);
        }
        else
        {
            GlovesSlot.ClearItem();
        }

        if (userInventoryData.EquippedAccessoryData != null)
        {
            AccessorySlot.SetItem(userInventoryData.EquippedAccessoryData);
        }
        else
        {
            AccessorySlot.ClearItem();
        }

        if (userInventoryData.EquippedCharacterData != null)
        {
            CharacterSlot.SetItem(userInventoryData.EquippedCharacterData);

            // 인벤토리 창에서 캐릭터 이미지 변경
            SetCharacterImg();
        }
        else
        {
            CharacterSlot.ClearItem();
            SetCharacterImg();
        }
    }

    private void SetInventory()
    {
        InventoryScrollList.Clear();

        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if (userInventoryData != null)
        {
            foreach (var itemData in userInventoryData.InventoryItemDataList)
            {
                // 아이템 장착 되어있으면 인벤토리에서 안보이도록 예외 처리
                if(userInventoryData.IsEquipped(itemData.SerialNumber))
                {
                    continue;
                }

                var itemSlotData = new InventoryItemSlotData();
                itemSlotData.SerialNumber = itemData.SerialNumber;
                itemSlotData.ItemID = itemData.ItemID;
                InventoryScrollList.InsertData(itemSlotData);
            }
        }
    }

    private void SortInventory()
    {
        switch(mInventorySortType)
        {
            case EInventorySortType.ItemGrade:
                SortBtnTxt.text = "GRADE";

                InventoryScrollList.SortDataList((a, b) =>
                {
                    var itemA = a.data as InventoryItemSlotData;
                    var itemB = b.data as InventoryItemSlotData;

                    // sort by item grade
                    int compareResult = ((itemB.ItemID / 1000) % 10).CompareTo(((itemA.ItemID / 1000) % 10));

                    // if same grade, sort by item type
                    if(compareResult == 0)
                    {
                        var itemAIdStr = itemA.ItemID.ToString();
                        var itemAComp = itemAIdStr.Substring(0, 1) + itemAIdStr.Substring(2, 3); // 예. 11001 -> 1001로 변환

                        var itemBIdStr = itemB.ItemID.ToString();
                        var itemBComp = itemBIdStr.Substring(0, 1) + itemBIdStr.Substring(2, 3);

                        compareResult = itemAComp.CompareTo(itemBComp);
                    }

                    return compareResult;
                });
                break;

            case EInventorySortType.ItemType:
                SortBtnTxt.text = "TYPE";

                InventoryScrollList.SortDataList((a, b) =>
                {
                    var itemA = a.data as InventoryItemSlotData;
                    var itemB = b.data as InventoryItemSlotData;

                    // sort by item type
                    var itemAIdStr = itemA.ItemID.ToString();
                    var itemAComp = itemAIdStr.Substring(0, 1) + itemAIdStr.Substring(2, 3); // 예. 11001 -> 1001로 변환

                    var itemBIdStr = itemB.ItemID.ToString();
                    var itemBComp = itemBIdStr.Substring(0, 1) + itemBIdStr.Substring(2, 3);

                    int compareResult = itemAComp.CompareTo(itemBComp);

                    // if same type, sort by grade
                    if(compareResult == 0)
                    {
                        compareResult = ((itemB.ItemID / 1000) % 10).CompareTo(((itemA.ItemID / 1000) % 10));
                    }

                    return compareResult;
                });
                break;

            default:
                break;
        }
    }

    public void OnClickSortBtn()
    {
        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        switch (mInventorySortType)
        {
            case EInventorySortType.ItemGrade:
                mInventorySortType = EInventorySortType.ItemType;
                break;

            case EInventorySortType.ItemType:
                mInventorySortType = EInventorySortType.ItemGrade;
                break;

            default:
                break;
        }

        SortInventory();
    }

    public void OnEquipItem(int itemID)
    {
        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if(userInventoryData == null)
        {
            Logger.LogError("UserInventoryData does not exist.");
            return;
        }

        var itemType = (EItemType)(itemID / 10000);
        switch (itemType)
        {
            case EItemType.Weapon:
                WeaponSlot.SetItem(userInventoryData.EquippedWeaponData);
                break;
            case EItemType.Shield:
                ShieldSlot.SetItem(userInventoryData.EquippedShieldData);
                break;
            case EItemType.ChestArmor:
                ChestArmorSlot.SetItem(userInventoryData.EquippedChestArmorData);
                break;
            case EItemType.Gloves:
                GlovesSlot.SetItem(userInventoryData.EquippedGlovesData);
                break;
            case EItemType.Boots:
                BootsSlot.SetItem(userInventoryData.EquippedBootsData);
                break;
            case EItemType.Accessory:
                AccessorySlot.SetItem(userInventoryData.EquippedAccessoryData);
                break;
            case EItemType.Character:
                CharacterSlot.SetItem(userInventoryData.EquippedCharacterData);
                SetCharacterImg();
                break;
            default:
                break;
        }

        SetUserStats();
        SetInventory();
        SortInventory();
    }

    public void OnUnequipItem(int itemID)
    {
        var itemType = (EItemType)(itemID / 10000);
        switch (itemType)
        {
            case EItemType.Weapon:
                WeaponSlot.ClearItem();
                break;
            case EItemType.Shield:
                ShieldSlot.ClearItem();
                break;
            case EItemType.ChestArmor:
                ChestArmorSlot.ClearItem();
                break;
            case EItemType.Gloves:
                GlovesSlot.ClearItem();
                break;
            case EItemType.Boots:
                BootsSlot.ClearItem();
                break;
            case EItemType.Accessory:
                AccessorySlot.ClearItem();
                break;
            case EItemType.Character:
                CharacterSlot.ClearItem();
                SetCharacterImg();
                break;
            default:
                break;
        }

        SetUserStats();
        SetInventory();
        SortInventory();
    }
}
