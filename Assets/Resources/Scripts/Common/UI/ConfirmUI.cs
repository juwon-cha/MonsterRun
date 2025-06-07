using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EConfirmType
{
    OK, // 확인 버튼만 있는 UI
    OK_CANCEL, // 확인이 정말 맞는지 확인하는 팝업(확인과 취소 버튼 둘 다 있음)
}

public class ConfirmUIData : BaseUIData
{
    public EConfirmType ConfirmType;
    public string TitleTxt;
    public string DescTxt; // 화면 중앙 텍스트
    public string OKBtnTxt; // 확인 버튼 텍스트
    public Action OnClickOKBtn;
    public string CancelBtnTxt;
    public Action OnClickCancelBtn;
}

public class ConfirmUI : BaseUI
{
    public TextMeshProUGUI TitleTxt;
    public TextMeshProUGUI DescTxt;
    public Button OKBtn;
    public Button CancelBtn;
    public TextMeshProUGUI OKBtnTxt;
    public TextMeshProUGUI CancelBtnTxt;

    private ConfirmUIData mConfirmUIData;
    private Action mOnClickOKBtn;
    private Action mOnClickCancelBtn;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        mConfirmUIData = uiData as ConfirmUIData;

        TitleTxt.text = mConfirmUIData.TitleTxt;
        DescTxt.text = mConfirmUIData.DescTxt;
        OKBtnTxt.text = mConfirmUIData.OKBtnTxt;
        mOnClickOKBtn = mConfirmUIData.OnClickOKBtn;
        CancelBtnTxt.text = mConfirmUIData.CancelBtnTxt;
        mOnClickCancelBtn = mConfirmUIData.OnClickCancelBtn;

        OKBtn.gameObject.SetActive(true);
        CancelBtn.gameObject.SetActive(mConfirmUIData.ConfirmType == EConfirmType.OK_CANCEL);
    }

    public void OnClickOKBtn()
    {
        mOnClickOKBtn?.Invoke();
        mOnClickOKBtn = null;
        CloseUI();
    }

    public void OnClickCancelBtn()
    {
        mOnClickCancelBtn?.Invoke();
        mOnClickCancelBtn = null;
        CloseUI();
    }
}
