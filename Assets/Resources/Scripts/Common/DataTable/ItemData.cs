using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EItemType
{
    Weapon = 1,
    Shield,
    ChestArmor,
    Gloves,
    Boots,
    Accessory,
    Character
}

public enum EItemGrade
{
    Common = 1,
    Uncommon,
    Rare,
    Epic,
    Legendary,
}

public class ItemData
{
    public int ItemID;
    public string ItemName;
    public int AttackPower;
    public int Defense;
}
