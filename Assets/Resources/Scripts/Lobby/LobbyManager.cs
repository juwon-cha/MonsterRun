using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    public LobbyUIController LobbyUIController { get; private set; }

    private bool mbIsLoadingInGame; // start ��ư�� ������ ������ �ΰ��� ���� ��û�� �� ���� �� �� �ֵ��� �ϱ� ����

    protected override void Init()
    {
        mbIsDestroyOnLoad = true;
        mbIsLoadingInGame = false;

        base.Init();
    }

    private void Start()
    {
        LobbyUIController = FindObjectOfType<LobbyUIController>();
        if(LobbyUIController == null)
        {
            Logger.Log("LobbyUIController does not exist.");
            return;
        }

        LobbyUIController.Init();
        AudioManager.Instance.OnLoadLobby();
        AudioManager.Instance.PlayBGM(EBGM.Lobby);

        AdsManager.Instance.EnableTopBannerAd(true);
    }

    public void StartInGame()
    {
        if(mbIsLoadingInGame)
        {
            return;
        }

        mbIsLoadingInGame = true;

        UIManager.Instance.Fade(Color.black, 0f, 1f, 0.5f, 0f, false, () =>
        {
            UIManager.Instance.CloseAllOpenUI();
            SceneLoader.Instance.LoadScene(ESceneType.InGame);
        });
    }
}
