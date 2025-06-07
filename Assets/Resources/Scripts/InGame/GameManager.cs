using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public PlayerController PlayerController { get; private set; }

    const float ORIGIN_SPEED = 3;

    public float GlobalSpeed;
    public float Score; // TODO: ���� é�� ������ Firestore�� ����??
    public bool IsRunning;
    public bool IsGameOver;
    public GameObject UI_GameOver;

    private Transform mPlayerTrs; // ĳ���� �������� �θ� Ʈ������

    protected override void Init()
    {
        mbIsDestroyOnLoad = true;
        IsRunning = true;
        IsGameOver = false;

        mPlayerTrs = GameObject.Find("Player").transform;

        base.Init();
    }

    private void Start()
    {
        IsRunning = true;
        IsGameOver = false;

        //if (PlayerPrefs.HasKey("Score") == false)
        //{
        //    PlayerPrefs.SetFloat("Score", 0);
        //}

        // ������ ĳ���Ϳ� ���� �÷��̾� ������ ����
        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if(userInventoryData.EquippedCharacterData == null) // ĳ���� ������� ���� ���� �Ұ�
        {
            ChapterManager.Instance.PauseGame();

            var uiData = new ConfirmUIData();
            uiData.ConfirmType = EConfirmType.OK;
            uiData.TitleTxt = "";
            uiData.DescTxt = "Please equip any character to play.";
            uiData.OKBtnTxt = "OK";
            uiData.OnClickOKBtn = () =>
            {
                SceneLoader.Instance.LoadScene(ESceneType.Lobby);
            };
            UIManager.Instance.OpenUI<ConfirmUI>(uiData);
        }
        else
        {
            var itemData = DataTableManager.Instance.GetItemData(userInventoryData.EquippedCharacterData.ItemID);

            var playerObject = Instantiate(Resources.Load($"Prefabs/InGame/Character/{itemData.ItemName}", typeof(GameObject))) as GameObject;
            playerObject.transform.SetParent(mPlayerTrs);
            playerObject.transform.localScale = Vector3.one;
            playerObject.transform.localPosition = new Vector3(-1.5f, 0, 0);
            playerObject.SetActive(true);

            PlayerController = FindObjectOfType<PlayerController>();
            if (PlayerController == null)
            {
                Logger.LogError("PlayerController does not exist.");
                return;
            }

            PlayerController.Init();
            PlayerController.OnHit.AddListener(GameOver);
        }
    }

    private void Update()
    {
        if(IsRunning)
        {
            Score += Time.deltaTime * 2;
            GlobalSpeed = ORIGIN_SPEED + Score * 0.1f;
        }
    }

    public void GameOver()
    {
        if(ChapterManager.Instance.IsChapterCleared)
        {
            return;
        }

        UI_GameOver.SetActive(true);
        IsRunning = false;
        IsGameOver = true;

        //float highScore = PlayerPrefs.GetFloat("Score");
        //PlayerPrefs.SetFloat("Score", Mathf.Max(highScore, Score));
    }

    public void OnClickRestart()
    {
        UIManager.Instance.Fade(Color.black, 0f, 1f, 0.5f, 0f, false, () =>
        {
            SceneLoader.Instance.LoadScene(ESceneType.InGame);
        });

        //SceneLoader.Instance.LoadScene(ESceneType.InGame);
        Score = 0;
        IsRunning = true;
        IsGameOver = false;
    }
}
