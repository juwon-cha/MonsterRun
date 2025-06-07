using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EConfirmType
{
    OK, // Ȯ�� ��ư�� �ִ� UI
    OK_CANCEL, // Ȯ���� ���� �´��� Ȯ���ϴ� �˾�(Ȯ�ΰ� ��� ��ư �� �� ����)
}

public class ConfirmUIData : BaseUIData
{
    public EConfirmType ConfirmType;
    public string TitleTxt;
    public string DescTxt; // ȭ�� �߾� �ؽ�Ʈ
    public string OKBtnTxt; // Ȯ�� ��ư �ؽ�Ʈ
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
