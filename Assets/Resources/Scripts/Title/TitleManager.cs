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
    // 로고
    public Animation LogoAnim;
    public TextMeshProUGUI LogoText;

    // 타이틀
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
        Logger.Log($"{GetType()/*클래스명 출력*/}::LoadGameCo");

        // 로고 플레이
        LogoAnim.Play();
        yield return new WaitForSeconds(LogoAnim.clip.length);

        LogoAnim.gameObject.SetActive(false);
        Title.gameObject.SetActive(true);

        // 서드파티 서비스 초기화 확인
        if(!CheckThirdPartyServiceInit())
        {
            // 초기화 실패 -> 게임 로드 중지
            yield break;
        }

        // 앱 버전 검증
        if(!ValidateAppVersion())
        {
            // 유효하지 않은 앱 버전이면 게임 로드 중지
            yield break;
        }

        // 로그인 체크
        if(!FirebaseManager.Instance.IsSignedIn())
        {
            var uiData = new BaseUIData();
            UIManager.Instance.OpenUI<LoginUI>(uiData);
        }

        // 로그인이 끝날때까지 대기
        while(!FirebaseManager.Instance.IsSignedIn())
        {
            yield return null;
        }

        // 어드레서블 초기화
        Addressables.InitializeAsync().Completed += (handle) =>
        {
            if(handle.Status == AsyncOperationStatus.Succeeded)
            {
                // 초기화 완료하면 다운로드해야 할 리소스 확인
                Logger.Log($"Addressables initialization succeeded");

                StartCoroutine(CheckResourceCo());
            }
            else
            {
                Logger.LogError($"Failed to initializ Addressables");
            }
        };

        // 리소스 다운로드 완료될 때까지 대기
        while(!mbIsResourceDownloaded)
        {
            yield return null;
        }

        // 데이터 테이블 로드
        DataTableManager.Instance.LoadDataTables();

        // 로그인 인증이 끝나면 유저 데이터 로드
        UserDataManager.Instance.LoadUserData();

        // 모든 유저 데이터가 로드되었는지 체크
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
            // 앱 업데이트 유도 팝업 UI
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
            // 각 그룹에 대해 다운르도 받아야할 리소스 용량을 확인
            AsyncOperationHandle<long> getSizeHandle = Addressables.GetDownloadSizeAsync(group.ToString());
            yield return getSizeHandle; // 작업이 끝날때까지 대기

            if (getSizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Log($"Download siz for {group}: {getSizeHandle.Result}");

                if (getSizeHandle.Result > 0)
                {
                    totalResourcesDownloadSize += getSizeHandle.Result;
                    groupsToDownload.Add(group.ToString()); // 새롭게 다운받아야할 그룹 목록에 그룹명 추가
                }
            }
        }

        // 로컬라이제이션 에셋 다운로드
        foreach (string localizationLabel in GlobalDefine.LocalizationLabels)
        {
            AsyncOperationHandle<long> getSizeHandle = Addressables.GetDownloadSizeAsync(localizationLabel);
            yield return getSizeHandle; // 작업이 끝날때까지 대기

            if(getSizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Log($"Download size for {localizationLabel}: {getSizeHandle.Result} bytes");
                if(getSizeHandle.Result > 0)
                {
                    totalResourcesDownloadSize += getSizeHandle.Result;
                    groupsToDownload.Add(localizationLabel); // 새롭게 다운받아야할 그룹 목록에 그룹명 추가
                }
            }
        }

        // 다운받아야할 리소스 총량이 0보다 크면 다운로드 확인 UI 표시
        if(totalResourcesDownloadSize > 0)
        {
            // 다운로드 용량이 1MB 이상이면 MB로 표시 1MB 미만이면 KB로 표시
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
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(group); // 다운로드 실행
            while(!downloadHandle.IsDone)
            {
                float progress = downloadHandle.PercentComplete;
                LoadingSlider.value = progress;
                LoadingProgressText.text = $"Downloading: {(int)(progress * 100)}%";
                yield return null;
            }

            if(downloadHandle.Status == AsyncOperationStatus.Succeeded) // 다운로드 완료
            {
                Logger.Log($"{group} download complete successfully");
                // 리소스 다운로드 후 그 중 하나의 리소스를 로드하려고 할 때 downloadHandle가 해제되어 있지 않으면 오류 발생 가능성 있음
                Addressables.Release(downloadHandle); // 다운로드 작업을 메모리에서 해제
            }
            else
            {
                Logger.LogError($"Failed to download resources. Group: {group}");
                yield break;
            }
        }

        // 다운로드 모두 정상적으로 완료
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

        // 만약 true라면 코루틴이 기다리는 상황에서도 로딩이 끝나면 강제로 로비씬으로 전환되기 때문에
        // 이를 방지하기 위해 allowSceneActivation = false 로 설정
        mAsyncOperation.allowSceneActivation = false;

        LoadingSlider.value = 0.5f; // 너무 로딩이 빠른 경우를 위해 절반을 채우고 시작
        LoadingProgressText.text = $"{((int)(LoadingSlider.value * 100)).ToString()}%";
        yield return new WaitForSeconds(0.5f);

        // 로딩이 진행 중일 때 
        while (!mAsyncOperation.isDone)
        {
            LoadingSlider.value = mAsyncOperation.progress < 0.5f ? 0.5f : mAsyncOperation.progress;
            LoadingProgressText.text = $"{((int)(LoadingSlider.value * 100)).ToString()}%";

            // 씬 로딩 완료 -> 로비로 전환하고 코루틴 종료
            if (mAsyncOperation.progress >= 0.9f) // allowSceneActivation이 false일 때 로딩이 90프로에서 멈추게 되어있음(유니티 특징)
            {
                mAsyncOperation.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }
    }
}
