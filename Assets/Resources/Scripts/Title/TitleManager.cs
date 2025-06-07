using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public enum RemoteResourceGroup
{
    DataTable,
    UI,
}

public class TitleManager : MonoBehaviour
{
    // �ΰ�
    public Animation LogoAnim;
    public TextMeshProUGUI LogoText;

    // Ÿ��Ʋ
    public GameObject Title;
    public Slider LoadingSlider;
    public TextMeshProUGUI LoadingProgressText;

    private AsyncOperation mAsyncOperation;
    private bool mbIsResourceDownloaded;

    private void Awake()
    {
        LogoAnim.gameObject.SetActive(true);
        Title.gameObject.SetActive(false);
    }

    private void Start()
    {
        UIManager.Instance.EnableGoodsUI(false);
        AdsManager.Instance.EnableTopBannerAd(false);

        StartCoroutine(LoadGameCo());
    }

    private IEnumerator LoadGameCo()
    {
        Logger.Log($"{GetType()/*Ŭ������ ���*/}::LoadGameCo");

        // �ΰ� �÷���
        LogoAnim.Play();
        yield return new WaitForSeconds(LogoAnim.clip.length);

        LogoAnim.gameObject.SetActive(false);
        Title.gameObject.SetActive(true);

        // ������Ƽ ���� �ʱ�ȭ Ȯ��
        if(!CheckThirdPartyServiceInit())
        {
            // �ʱ�ȭ ���� -> ���� �ε� ����
            yield break;
        }

        // �� ���� ����
        if(!ValidateAppVersion())
        {
            // ��ȿ���� ���� �� �����̸� ���� �ε� ����
            yield break;
        }

        // �α��� üũ
        if(!FirebaseManager.Instance.IsSignedIn())
        {
            var uiData = new BaseUIData();
            UIManager.Instance.OpenUI<LoginUI>(uiData);
        }

        // �α����� ���������� ���
        while(!FirebaseManager.Instance.IsSignedIn())
        {
            yield return null;
        }

        // ��巹���� �ʱ�ȭ
        Addressables.InitializeAsync().Completed += (handle) =>
        {
            if(handle.Status == AsyncOperationStatus.Succeeded)
            {
                // �ʱ�ȭ �Ϸ��ϸ� �ٿ�ε��ؾ� �� ���ҽ� Ȯ��
                Logger.Log($"Addressables initialization succeeded");

                StartCoroutine(CheckResourceCo());
            }
            else
            {
                Logger.LogError($"Failed to initializ Addressables");
            }
        };

        // ���ҽ� �ٿ�ε� �Ϸ�� ������ ���
        while(!mbIsResourceDownloaded)
        {
            yield return null;
        }

        // ������ ���̺� �ε�
        DataTableManager.Instance.LoadDataTables();

        // �α��� ������ ������ ���� ������ �ε�
        UserDataManager.Instance.LoadUserData();

        // ��� ���� �����Ͱ� �ε�Ǿ����� üũ
        while(!UserDataManager.Instance.IsUserDataLoaded())
        {
            yield return null;
        }

        yield return StartCoroutine(LoadLobbyCo());
    }

    private bool CheckThirdPartyServiceInit()
    {
        return FirebaseManager.Instance.IsInit();
    }

    private bool ValidateAppVersion()
    {
        bool result = false;

        if (Application.version == FirebaseManager.Instance.GetAppVersion())
        {
            result = true;
        }
        else
        {
            // �� ������Ʈ ���� �˾� UI
            var uiData = new ConfirmUIData();
            uiData.ConfirmType = EConfirmType.OK_CANCEL;
            uiData.TitleTxt = string.Empty;
            uiData.DescTxt = LocalizationSettings.StringDatabase.GetLocalizedString(GlobalDefine.LOCALIZATION_DATA_TABLE, "app_update_desc");
            uiData.OKBtnTxt = LocalizationSettings.StringDatabase.GetLocalizedString(GlobalDefine.LOCALIZATION_DATA_TABLE, "update");
            uiData.CancelBtnTxt = LocalizationSettings.StringDatabase.GetLocalizedString(GlobalDefine.LOCALIZATION_DATA_TABLE, "cancel");
            uiData.OnClickOKBtn = () =>
            {
#if UNITY_ANDROID
                Application.OpenURL(GlobalDefine.GOOGLE_PLAY_STORE);
#elif UNITY_IOS
                Application.OpenURL(GlobalDefine.APPLE_APP_STORE);
#endif
            };

            uiData.OnClickCancelBtn = () =>
            {
                Application.Quit();
            };

            UIManager.Instance.OpenUI<ConfirmUI>(uiData);
        }
        
        return result;
    }

    private IEnumerator CheckResourceCo()
    {
        long totalResourcesDownloadSize = 0;
        List<string> groupsToDownload = new List<string>();

        foreach (RemoteResourceGroup group in Enum.GetValues(typeof(RemoteResourceGroup)))
        {
            // �� �׷쿡 ���� �ٿ�� �޾ƾ��� ���ҽ� �뷮�� Ȯ��
            AsyncOperationHandle<long> getSizeHandle = Addressables.GetDownloadSizeAsync(group.ToString());
            yield return getSizeHandle; // �۾��� ���������� ���

            if (getSizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Log($"Download siz for {group}: {getSizeHandle.Result}");

                if (getSizeHandle.Result > 0)
                {
                    totalResourcesDownloadSize += getSizeHandle.Result;
                    groupsToDownload.Add(group.ToString()); // ���Ӱ� �ٿ�޾ƾ��� �׷� ��Ͽ� �׷�� �߰�
                }
            }
        }

        // ���ö������̼� ���� �ٿ�ε�
        foreach (string localizationLabel in GlobalDefine.LocalizationLabels)
        {
            AsyncOperationHandle<long> getSizeHandle = Addressables.GetDownloadSizeAsync(localizationLabel);
            yield return getSizeHandle; // �۾��� ���������� ���

            if(getSizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Log($"Download size for {localizationLabel}: {getSizeHandle.Result} bytes");
                if(getSizeHandle.Result > 0)
                {
                    totalResourcesDownloadSize += getSizeHandle.Result;
                    groupsToDownload.Add(localizationLabel); // ���Ӱ� �ٿ�޾ƾ��� �׷� ��Ͽ� �׷�� �߰�
                }
            }
        }

        // �ٿ�޾ƾ��� ���ҽ� �ѷ��� 0���� ũ�� �ٿ�ε� Ȯ�� UI ǥ��
        if(totalResourcesDownloadSize > 0)
        {
            // �ٿ�ε� �뷮�� 1MB �̻��̸� MB�� ǥ�� 1MB �̸��̸� KB�� ǥ��
            string downloadSizeStr = totalResourcesDownloadSize >= 1000000 ? $"{totalResourcesDownloadSize / 1000000}MB" : $"{totalResourcesDownloadSize / 1000}KB";

            var uiData = new ConfirmUIData();
            uiData.ConfirmType = EConfirmType.OK_CANCEL;
            uiData.TitleTxt = $"Download";
            uiData.DescTxt = $"Download resources?\n{downloadSizeStr}";
            uiData.OKBtnTxt = $"OK";
            uiData.CancelBtnTxt = $"Cancel";

            uiData.OnClickOKBtn = () =>
            {
                StartCoroutine(DownloadResourcesCo(groupsToDownload));
            };
            uiData.OnClickCancelBtn = () =>
            {
                Application.Quit();
            };

            UIManager.Instance.OpenUI<ConfirmUI>(uiData);
        }
        else
        {
            Logger.Log($"No resource download required");
            mbIsResourceDownloaded = true;
        }
    }

    private IEnumerator DownloadResourcesCo(List<string> groupsToDownload)
    {
        foreach (var group in groupsToDownload)
        {
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(group); // �ٿ�ε� ����
            while(!downloadHandle.IsDone)
            {
                float progress = downloadHandle.PercentComplete;
                LoadingSlider.value = progress;
                LoadingProgressText.text = $"Downloading: {(int)(progress * 100)}%";
                yield return null;
            }

            if(downloadHandle.Status == AsyncOperationStatus.Succeeded) // �ٿ�ε� �Ϸ�
            {
                Logger.Log($"{group} download complete successfully");
                // ���ҽ� �ٿ�ε� �� �� �� �ϳ��� ���ҽ��� �ε��Ϸ��� �� �� downloadHandle�� �����Ǿ� ���� ������ ���� �߻� ���ɼ� ����
                Addressables.Release(downloadHandle); // �ٿ�ε� �۾��� �޸𸮿��� ����
            }
            else
            {
                Logger.LogError($"Failed to download resources. Group: {group}");
                yield break;
            }
        }

        // �ٿ�ε� ��� ���������� �Ϸ�
        mbIsResourceDownloaded = true;
    }

    private IEnumerator LoadLobbyCo()
    {
        mAsyncOperation = SceneLoader.Instance.LoadSceneAsync(ESceneType.Lobby);
        if (mAsyncOperation == null)
        {
            Logger.Log("Lobby Async Loading Error");
            yield break;
        }

        // ���� true��� �ڷ�ƾ�� ��ٸ��� ��Ȳ������ �ε��� ������ ������ �κ������ ��ȯ�Ǳ� ������
        // �̸� �����ϱ� ���� allowSceneActivation = false �� ����
        mAsyncOperation.allowSceneActivation = false;

        LoadingSlider.value = 0.5f; // �ʹ� �ε��� ���� ��츦 ���� ������ ä��� ����
        LoadingProgressText.text = $"{((int)(LoadingSlider.value * 100)).ToString()}%";
        yield return new WaitForSeconds(0.5f);

        // �ε��� ���� ���� �� 
        while (!mAsyncOperation.isDone)
        {
            LoadingSlider.value = mAsyncOperation.progress < 0.5f ? 0.5f : mAsyncOperation.progress;
            LoadingProgressText.text = $"{((int)(LoadingSlider.value * 100)).ToString()}%";

            // �� �ε� �Ϸ� -> �κ�� ��ȯ�ϰ� �ڷ�ƾ ����
            if (mAsyncOperation.progress >= 0.9f) // allowSceneActivation�� false�� �� �ε��� 90���ο��� ���߰� �Ǿ�����(����Ƽ Ư¡)
            {
                mAsyncOperation.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }
    }
}
