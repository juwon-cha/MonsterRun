using static GlobalDefine;

public enum EAchievementType
{
    CollectGold,
    ClearChapter1,
    ClearChapter2,
    ClearChapter3,
}

public class AchievementData
{
    public EAchievementType AchievementType;
    public string AchievementName;
    public int AchievementGoal;
    public ERewardType AchievementRewardType;
    public int AchievementRewardAmount;
}
