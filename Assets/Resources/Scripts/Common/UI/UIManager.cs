using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class UIManager : SingletonBehaviour<UIManager>
{
    public Transform UICanvasTransform;
    public Transform ClosedUITransform;

    public Image FadeImg;

    private BaseUI mFrontUI; // 가장 상단에 표시되는 UI를 참조하는 변수
    private Dictionary<System.Type, GameObject> mOpenUIPool = new Dictionary<System.Type, GameObject>(); // 현재 활성화 되어있는 UI를 담는 딕셔너리
    private Dictionary<System.Type, GameObject> mClosedUIPool = new Dictionary<System.Type, GameObject>(); // 현재 비활성화 되어있는 UI를 담는 딕셔너리

    private GoodsUI mGoodsUI;

    public Camera UICamera;

    protected override void Init()
    {
        base.Init();

        FadeImg.transform.localScale = Vector3.zero;

        mGoodsUI = FindObjectOfType<GoodsUI>();
        if(mGoodsUI == null)
        {
            Logger.LogError("No goods ui component found.");
        }
    }

    private BaseUI GetUI<T>(out bool isAlreadyOpen)
    {
        // T는 열고자하는 UI 클래스
        System.Type uiType = typeof(T);

        BaseUI ui = null;
        isAlreadyOpen = false;

        // UI가 한번이라도 생성되었다면 풀에서 가져와서 재활용
        if(mOpenUIPool.ContainsKey(uiType))
        {
            ui = mOpenUIPool[uiType].GetComponent<BaseUI>();
            isAlreadyOpen = true;
        }
        else if(mClosedUIPool.ContainsKey(uiType))
        {
            ui = mClosedUIPool[uiType].GetComponent<BaseUI>();
            mClosedUIPool.Remove(uiType);
        }
        else // UI가 한 번도 생성되지 않았다면 생성
        {
            var uiObj = Instantiate(Resources.Load($"Prefabs/UI/{uiType}", typeof(GameObject))) as GameObject;
            ui = uiObj.GetComponent<BaseUI>();
        }

        return ui;
    }

    public void OpenUI<T>(BaseUIData uiData)
    {
        System.Type uiType = typeof(T);

        Logger.Log($"{GetType()}::OpenUI({uiType})");

        bool isAlreadyOpen = false;
        var ui = GetUI<T>(out isAlreadyOpen);

        if(ui == null)
        {
            Logger.LogError($"{uiType} does not exist.");
            return;
        }

        if(isAlreadyOpen)
        {
            Logger.LogError($"{uiType} is already opened.");
            return;
        }

        // 팝업성 UI들은 ClosedUI와 GoodsUI 사이에 항상 생성됨 -> 가장 최근에 생성된 UI는 GoodsUI 바로 위에 생성됨

        // Fade 이미지 추가 -> 팝업성 UI 화면들의 Depth 처리 로직 수정(-1에서 -2로) -> Fade는 모든 UI보다 최상위
        var siblingIndex = UICanvasTransform.childCount - 2;
        ui.Init(UICanvasTransform);
        ui.transform.SetSiblingIndex(siblingIndex);
        ui.gameObject.SetActive(true);
        ui.SetInfo(uiData);
        ui.ShowUI();

        mFrontUI = ui;
        mOpenUIPool[uiType] = ui.gameObject;
    }

    // addressables 에셋 폴더에서 로드하여 인스턴스 만듦
    // 비동기 로드 -> async 키워드 사용
    public async void OpenUIFromAA<T>(BaseUIData uiData)
    {
        Type uiType = typeof(T);

        if(mOpenUIPool.ContainsKey(uiType)) // UI가 이미 열려있다면 예외처리
        {
            Logger.LogError($"{uiType} is already open");
            return;
        }
        else if(mClosedUIPool.ContainsKey(uiType)) // 이미 한번 UI를 열어서 풀에 UI를 가지고 있으면 풀에서 UI 가져옴
        {
            var ui = mClosedUIPool[uiType].GetComponent<BaseUI>();
            mClosedUIPool.Remove(uiType);

            var siblingIndex = UICanvasTransform.childCount - 2;
            ui.Init(UICanvasTransform);
            ui.transform.SetSiblingIndex(siblingIndex);
            ui.gameObject.SetActive(true);
            ui.SetInfo(uiData);
            ui.ShowUI();

            mFrontUI = ui;
            mOpenUIPool[uiType] = ui.gameObject;
        }
        else // addressables에서 새롭게 에셋을 로드하여 인스턴스 만듦
        {
            // 에셋 주소값
            // 예) SettingsUI/SettingsUI.prefab
            AsyncOperationHandle<GameObject> operationHandle = Addressables.LoadAssetAsync<GameObject>($"{uiType}/{uiType}.prefab");
            await operationHandle.Task; // 비동기 작업이 완료될 때까지 기다림

            // 비동기 작업 성공(로드 성공)
            if(operationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Log($"UI asset loaded successfully");

                GameObject loadedUIPrefab = operationHandle.Result; // operationHandle의 Result값은 로드하고자 하는 프리팹(게임 오브젝트)
                var uiObj = Instantiate(loadedUIPrefab); // 게임 오브젝트 인스턴스 만듦
                var ui = uiObj.GetComponent<BaseUI>(); // 오브젝트에서 BaseUI 스크립트 가져옴
                if(!ui)
                {
                    Logger.LogError($"{uiType} does not exist");
                    return;
                }

                var siblingIndex = UICanvasTransform.childCount - 2;
                ui.Init(UICanvasTransform);
                ui.transform.SetSiblingIndex(siblingIndex);
                ui.gameObject.SetActive(true);
                ui.SetInfo(uiData);
                ui.ShowUI();

                mFrontUI = ui;
                mOpenUIPool[uiType] = ui.gameObject;
            }
            else
            {
                Logger.LogError($"Failed to load UI asset");
            }
        }
    }

    public void CloseUI(BaseUI ui)
    {
        System.Type uiType = ui.GetType();

        Logger.Log($"{GetType()}::CloseUI({uiType})");

        ui.gameObject.SetActive(false);
        mOpenUIPool.Remove(uiType);
        mClosedUIPool[uiType] = ui.gameObject;
        ui.transform.SetParent(ClosedUITransform);

        mFrontUI = null;
        // ui 를 닫은 후에 최상단에 남아있는 UI 오브젝트
        var lastChild = UICanvasTransform.GetChild(UICanvasTransform.childCount - 3);
        if(lastChild)
        {
            // 남아있는 UI가 있다면 최상단 UI로 설정
            // 만약 마지막 UI 화면이 닫히면 lastChild.gameObject는 ClosedUI 게임 오브젝트가 된다
            // -> ClosedUI 오브젝트에는 BaseUI 컴포넌트가 없어서 mFrontUI가 null로 처리됨
            mFrontUI = lastChild.gameObject.GetComponent<BaseUI>();
        }
    }

    // 특정 UI 화면이 열려있는지 확인. 열려있는 UI화면(객체) 반환
    public BaseUI GetActiveUI<T>()
    {
        var uiType = typeof(T);
        return mOpenUIPool.ContainsKey(uiType) ? mOpenUIPool[uiType].GetComponent<BaseUI>() : null;
    }

    // UI화면이 열려있는 것이 하나라도 있는지 확인
    public bool ExistsOpenUI()
    {
        return mFrontUI != null;
    }

    // 현재 가장 최상단 UI 화면 인스턴스 반환
    public BaseUI GetCurrentFrontUI()
    {
        return mFrontUI;
    }

    // 가장 최상단 UI 인스턴스를 닫음
    public void CloseCurrentFrontUI()
    {
        mFrontUI.CloseUI();
    }

    public void CloseAllOpenUI()
    {
        while(mFrontUI)
        {
            mFrontUI.CloseUI(true);
        }
    }

    public void EnableGoodsUI(bool value)
    {
        mGoodsUI.gameObject.SetActive(value);

        if(value)
        {
            mGoodsUI.SetValues();
        }
    }

    public void Fade(Color color, float startAlpha, float endAlpha, float duration, float startDelay, bool deactivateOnFinish, Action onFinish = null)
    {
        StartCoroutine(FadeCo(color, startAlpha, endAlpha, duration, startDelay, deactivateOnFinish, onFinish));
    }

    private IEnumerator FadeCo(Color color, float startAlpha, float endAlpha, float duration, float startDelay, bool deactivateOnFinish, Action onFinish)
    {
        yield return new WaitForSeconds(startDelay);

        FadeImg.transform.localScale = Vector3.one;
        FadeImg.color = new Color(color.r, color.g, color.b, startAlpha);

        float startTime = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - startTime < duration)
        {
            // 경과한 시간 비율만큼 Fade 색상(알파값) 처리
            FadeImg.color = new Color(color.r, color.g, color.b, Mathf.Lerp(startAlpha, endAlpha, Time.realtimeSinceStartup - startTime) / duration);
            yield return null;
        }

        FadeImg.color = new Color(color.r, color.g, color.b, endAlpha);

        // Fade 이미지 컴포넌트 비활성화할지 여부에 따라 Fade 처리 끝나면 scale값 0으로 처리
        if(deactivateOnFinish)
        {
            FadeImg.transform.localScale = Vector3.zero;
        }

        // Fade처리가 끝났을때 수행되길 원하는 로직 실행
        onFinish?.Invoke();
    }
}
