using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIController : MonoBehaviour
{
    public Image CharacterImg;
    public Image LobbyBg;

    public TextMeshProUGUI CurChapterNameTxt;
    public RawImage CurChapterBg;

    public Image AchievementAlarm;
    public TextMeshProUGUI AchievementAlarmTxt;

    public void Init()
    {
        UIManager.Instance.EnableGoodsUI(true);
        SetCurChapter();
        SetCharacterImg();
        SetAchievementAlarm();
    }

    private void Update()
    {
        HandleInput();
    }

    private void LateUpdate()
    {
        SetAchievementAlarm();
        SetCharacterImg();
    }

    private void HandleInput()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

            var frontUI = UIManager.Instance.GetCurrentFrontUI();
            if(frontUI != null)
            {
                frontUI.CloseUI();
            }
            else
            {
                var uiData = new ConfirmUIData();
                uiData.ConfirmType = EConfirmType.OK_CANCEL;
                uiData.TitleTxt = "Quit";
                uiData.DescTxt = "Do you want to quit game?";
                uiData.OKBtnTxt = "Quit";
                uiData.CancelBtnTxt = "Cancel";
                uiData.OnClickOKBtn = () =>
                {
                    Application.Quit();
                };
                UIManager.Instance.OpenUI<ConfirmUI>(uiData);
            }
        }
    }

    public void SetCurChapter()
    {
        var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
        if(userPlayData == null)
        {
            Logger.LogError("UserPlayData does not exist.");
            return;
        }

        var curChapterData = DataTableManager.Instance.GetChapterData(userPlayData.SelectedChapter);
        if(curChapterData == null)
        {
            Logger.LogError("CurChapterData does not exist.");

            // ������ ���̺� ���� é�Ϳ� ���� ������ ���� �� ����ó��
            curChapterData = DataTableManager.Instance.GetChapterData(GlobalDefine.MAX_CHAPTER);
            ChangeChapterSettings(userPlayData, curChapterData);

            return;
        }

        // �����Ϸ��� �ϴ� é�Ͱ� MAX_CHAPTER���� ������ ����ó��
        if(userPlayData.SelectedChapter > GlobalDefine.MAX_CHAPTER)
        {
            curChapterData = DataTableManager.Instance.GetChapterData(GlobalDefine.MAX_CHAPTER);
        }

        ChangeChapterSettings(userPlayData, curChapterData);
    }

    private void ChangeChapterSettings(UserPlayData userPlayData, ChapterData curChapterData)
    {
        CurChapterNameTxt.text = curChapterData.ChapterName;
        
        var bgTexture = Resources.Load($"Textures/ChapterBG/ChapterBG_{curChapterData.ChapterNo.ToString("D3")}") as Texture2D;
        if (bgTexture != null)
        {
            CurChapterBg.texture = bgTexture;
            LobbyBg.sprite = Sprite.Create(bgTexture, new Rect(0, 0, bgTexture.width, bgTexture.height), new Vector2(1f, 1f));
        }
    }

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
                color.a = 1f; // ĳ���͸� �ٽ� �׷��ֱ� ���� ���� 1�� ����
                CharacterImg.color = color;

                CharacterImg.sprite = Sprite.Create(itemIconTexture, new Rect(0, 0, itemIconTexture.width, itemIconTexture.height), new Vector2(1f, 1f));
            }
        }
        else // ĳ���� �������� �ʾ��� ��� �̹��� �� ���̰� ����
        {
            CharacterImg.sprite = null;
            var color = CharacterImg.color;
            color.a = 0f; // ������ �����ϰ� ����
            CharacterImg.color = color;
        }
    }

    private void SetAchievementAlarm()
    {
        int achievementCount = 0;

        var achievementDataList = DataTableManager.Instance.GetAchievementDataList();
        var userAchievementData = UserDataManager.Instance.GetUserData<UserAchievementData>();
        if (achievementDataList != null && userAchievementData != null)
        {
            // ���� ������ ��� ��ȸ
            foreach (var achievement in achievementDataList)
            {
                var userAchievedData = userAchievementData.GetUserAchievementProgressData(achievement.AchievementType);
                if (userAchievedData != null)
                {
                    if(userAchievedData.IsAchieved)
                    {
                        ++achievementCount;
                    }
                    
                    if(userAchievedData.IsRewardClaimed)
                    {
                        --achievementCount;
                    }
                }
            }
        }

        if(achievementCount == 0)
        {
            AchievementAlarm.color = new Color(1f, 1f, 1f, 0f);
            AchievementAlarmTxt.color = new Color(1f, 1f, 1f, 0f);
        }
        else
        {
            AchievementAlarm.color = Color.white;
            AchievementAlarmTxt.color = Color.white;

            AchievementAlarmTxt.text = achievementCount.ToString();
        }
    }

    public void OnClickSettingsBtn()
    {
        Logger.Log($"{GetType()}::OnClickSettingsBtn");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var uiData = new BaseUIData();
        UIManager.Instance.OpenUIFromAA<SettingsUI>(uiData);
    }

    public void OnClickProfileBtn()
    {
        Logger.Log($"{GetType()}::OnClickProfileBtn");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var uiData = new BaseUIData();
        UIManager.Instance.OpenUI<InventoryUI>(uiData);
    }

    public void OnClickCurChapter()
    {
        Logger.Log($"{GetType()}::OnClickCurChapter");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var uiData = new BaseUIData();
        UIManager.Instance.OpenUI<ChapterListUI>(uiData);
    }

    public void OnClickStartBtn()
    {
        Logger.Log($"{GetType()}::OnClickStartBtn");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);
        AudioManager.Instance.StopBGM();

        LobbyManager.Instance.StartInGame();
    }

    public void OnClickAchievementBtn()
    {
        Logger.Log($"{GetType()}::OnClickAchievementBtn");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var uiData = new BaseUIData();
        UIManager.Instance.OpenUI<AchievementUI>(uiData);
    }

    public void OnClickShopBtn()
    {
        Logger.Log($"{GetType()}::OnClickShopBtn");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        var uiData = new BaseUIData();
        UIManager.Instance.OpenUI<ShopUI>(uiData);
    }
}
