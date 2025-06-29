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

    private BaseUI mFrontUI; // ���� ��ܿ� ǥ�õǴ� UI�� �����ϴ� ����
    private Dictionary<System.Type, GameObject> mOpenUIPool = new Dictionary<System.Type, GameObject>(); // ���� Ȱ��ȭ �Ǿ��ִ� UI�� ��� ��ųʸ�
    private Dictionary<System.Type, GameObject> mClosedUIPool = new Dictionary<System.Type, GameObject>(); // ���� ��Ȱ��ȭ �Ǿ��ִ� UI�� ��� ��ųʸ�

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
        // T�� �������ϴ� UI Ŭ����
        System.Type uiType = typeof(T);

        BaseUI ui = null;
        isAlreadyOpen = false;

        // UI�� �ѹ��̶� �����Ǿ��ٸ� Ǯ���� �����ͼ� ��Ȱ��
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
        else // UI�� �� ���� �������� �ʾҴٸ� ����
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

        // �˾��� UI���� ClosedUI�� GoodsUI ���̿� �׻� ������ -> ���� �ֱٿ� ������ UI�� GoodsUI �ٷ� ���� ������

        // Fade �̹��� �߰� -> �˾��� UI ȭ����� Depth ó�� ���� ����(-1���� -2��) -> Fade�� ��� UI���� �ֻ���
        var siblingIndex = UICanvasTransform.childCount - 2;
        ui.Init(UICanvasTransform);
        ui.transform.SetSiblingIndex(siblingIndex);
        ui.gameObject.SetActive(true);
        ui.SetInfo(uiData);
        ui.ShowUI();

        mFrontUI = ui;
        mOpenUIPool[uiType] = ui.gameObject;
    }

    // addressables ���� �������� �ε��Ͽ� �ν��Ͻ� ����
    // �񵿱� �ε� -> async Ű���� ���
    public async void OpenUIFromAA<T>(BaseUIData uiData)
    {
        Type uiType = typeof(T);

        if(mOpenUIPool.ContainsKey(uiType)) // UI�� �̹� �����ִٸ� ����ó��
        {
            Logger.LogError($"{uiType} is already open");
            return;
        }
        else if(mClosedUIPool.ContainsKey(uiType)) // �̹� �ѹ� UI�� ��� Ǯ�� UI�� ������ ������ Ǯ���� UI ������
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
        else // addressables���� ���Ӱ� ������ �ε��Ͽ� �ν��Ͻ� ����
        {
            // ���� �ּҰ�
            // ��) SettingsUI/SettingsUI.prefab
            AsyncOperationHandle<GameObject> operationHandle = Addressables.LoadAssetAsync<GameObject>($"{uiType}/{uiType}.prefab");
            await operationHandle.Task; // �񵿱� �۾��� �Ϸ�� ������ ��ٸ�

            // �񵿱� �۾� ����(�ε� ����)
            if(operationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Log($"UI asset loaded successfully");

                GameObject loadedUIPrefab = operationHandle.Result; // operationHandle�� Result���� �ε��ϰ��� �ϴ� ������(���� ������Ʈ)
                var uiObj = Instantiate(loadedUIPrefab); // ���� ������Ʈ �ν��Ͻ� ����
                var ui = uiObj.GetComponent<BaseUI>(); // ������Ʈ���� BaseUI ��ũ��Ʈ ������
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
        // ui �� ���� �Ŀ� �ֻ�ܿ� �����ִ� UI ������Ʈ
        var lastChild = UICanvasTransform.GetChild(UICanvasTransform.childCount - 3);
        if(lastChild)
        {
            // �����ִ� UI�� �ִٸ� �ֻ�� UI�� ����
            // ���� ������ UI ȭ���� ������ lastChild.gameObject�� ClosedUI ���� ������Ʈ�� �ȴ�
            // -> ClosedUI ������Ʈ���� BaseUI ������Ʈ�� ��� mFrontUI�� null�� ó����
            mFrontUI = lastChild.gameObject.GetComponent<BaseUI>();
        }
    }

    // Ư�� UI ȭ���� �����ִ��� Ȯ��. �����ִ� UIȭ��(��ü) ��ȯ
    public BaseUI GetActiveUI<T>()
    {
        var uiType = typeof(T);
        return mOpenUIPool.ContainsKey(uiType) ? mOpenUIPool[uiType].GetComponent<BaseUI>() : null;
    }

    // UIȭ���� �����ִ� ���� �ϳ��� �ִ��� Ȯ��
    public bool ExistsOpenUI()
    {
        return mFrontUI != null;
    }

    // ���� ���� �ֻ�� UI ȭ�� �ν��Ͻ� ��ȯ
    public BaseUI GetCurrentFrontUI()
    {
        return mFrontUI;
    }

    // ���� �ֻ�� UI �ν��Ͻ��� ����
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
            // ����� �ð� ������ŭ Fade ����(���İ�) ó��
            FadeImg.color = new Color(color.r, color.g, color.b, Mathf.Lerp(startAlpha, endAlpha, Time.realtimeSinceStartup - startTime) / duration);
            yield return null;
        }

        FadeImg.color = new Color(color.r, color.g, color.b, endAlpha);

        // Fade �̹��� ������Ʈ ��Ȱ��ȭ���� ���ο� ���� Fade ó�� ������ scale�� 0���� ó��
        if(deactivateOnFinish)
        {
            FadeImg.transform.localScale = Vector3.zero;
        }

        // Fadeó���� �������� ����Ǳ� ���ϴ� ���� ����
        onFinish?.Invoke();
    }
}
