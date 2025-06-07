using System;
using UnityEngine;

public class BaseUIData
{
    public Action OnShow;
    public Action OnClose;
}

public class BaseUI : MonoBehaviour
{
    public Animation mUIOpenAnim;

    private Action mOnShow;
    private Action mOnClose;

    public virtual void Init(Transform anchor)
    {
        Logger.Log($"{GetType()}::Init");

        mOnShow = null;
        mOnClose = null;

        transform.SetParent(anchor);

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    public virtual void SetInfo(BaseUIData uiData)
    {
        Logger.Log($"{GetType()}::SetInfo");

        mOnShow = uiData.OnShow;
        mOnClose = uiData.OnClose;
    }

    public virtual void ShowUI()
    {
        if(mUIOpenAnim)
        {
            mUIOpenAnim.Play();
        }

        mOnShow?.Invoke();
        mOnShow = null;
    }

    public virtual void CloseUI(bool isCloseAll = false)
    {
        if(!isCloseAll)
        {
            mOnClose?.Invoke();
        }
        mOnClose = null;

        UIManager.Instance.CloseUI(this);
    }

    public virtual void OnClickCloseButton()
    {
        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);
        CloseUI();
    }
}
