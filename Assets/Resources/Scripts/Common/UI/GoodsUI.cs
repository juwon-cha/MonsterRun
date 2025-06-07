using SuperMaxim.Messaging;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 재화 변동 시 발행할 메시지 클래스(Pub-Sub 디자인패턴)
public class GoldUpdateMsg
{
    public bool IsAdd; // 재화가 증가할때만 획득 연출처리하기 위함
}

public class GemUpdateMsg
{
    public bool IsAdd;
}

public class GoodsUI : MonoBehaviour
{
    public Image GoldIcon;
    public TextMeshProUGUI GoldAmountTxt;

    public Image GemIcon;
    public TextMeshProUGUI GemAmountTxt;

    private Coroutine mGoldIncreaseCo; // 재화 증가 요청이 여러번 올 때 새 요청이 기존 요청을 덮어쓰기 위함
    private Coroutine mGemIncreaseCo;
    private const float GOODS_INCREASE_DURATION = 0.5f;

    // 인스턴스에 연동된 게임오브젝트가 활성화될 때 호출
    private void OnEnable()
    {
        // Pub-Sub 라이브러리(구독 등록)
        Messenger.Default.Subscribe<GoldUpdateMsg>(OnUpateGold);
        Messenger.Default.Subscribe<GemUpdateMsg>(OnUpateGem);
    }

    // 인스턴스에 연동된 게임오브젝트가 비활성화될 때 호출
    private void OnDisable()
    {
        // 구독 해제
        Messenger.Default.Unsubscribe<GoldUpdateMsg>(OnUpateGold);
        Messenger.Default.Unsubscribe<GemUpdateMsg>(OnUpateGem);
    }

    public void SetValues()
    {
        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if(userGoodsData == null)
        {
            Logger.LogError("No user goods data");
        }
        else
        {
            GoldAmountTxt.text = userGoodsData.Gold.ToString("N0");
            GemAmountTxt.text = userGoodsData.Gem.ToString("N0");
        }
    }

    // GoodsUI 인스턴스에서 GoldUpdateMsg를 받았을 때 이 함수가 실행됨
    private void OnUpateGold(GoldUpdateMsg goldUpdateMsg)
    {
        // 획득 연출
        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if(userGoodsData == null)
        {
            Logger.LogError("UserGoodsData does not eixts.");
            return;
        }

        AudioManager.Instance.PlaySFX(ESFX.UI_Get_Goods);

        if(goldUpdateMsg.IsAdd)
        {
            if(mGoldIncreaseCo != null) // 기존 획득 연출 코루틴 취소
            {
                StopCoroutine(IncreaseGoldCo());
            }

            // 골드 증가 연출 담당 코루틴 호출
            mGoldIncreaseCo = StartCoroutine(IncreaseGoldCo());
        }
        else
        {
            GoldAmountTxt.text = userGoodsData.Gold.ToString("N0");
        }
    }

    private IEnumerator IncreaseGoldCo()
    {
        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if (userGoodsData == null)
        {
            Logger.LogError("UserGoodsData does not eixts.");
            yield break;
        }

        // 재화 이동 연출
        int amount = 10;
        for (int i = 0; i < amount; i++)
        {
            GameObject goldObject = Instantiate(Resources.Load("Prefabs/UI/GoldMove", typeof(GameObject))) as GameObject;
            goldObject.transform.SetParent(transform.parent);
            goldObject.transform.localScale = Vector3.one;
            goldObject.transform.localPosition = Vector3.zero;
            goldObject.GetComponent<GoodsMove>().SetMove(i, GoldIcon.transform.position);
        }

        yield return new WaitForSeconds(1f);

        AudioManager.Instance.PlaySFX(ESFX.UI_Increase_Goods);

        // 재화 텍스트 증가 연출
        float elapsedTime = 0f;// 경과 시간
        long curTextValue = Convert.ToInt64(GoldAmountTxt.text.Replace(",", "")); // 현재 UI에 표시된 골드 수치
        long destValue = userGoodsData.Gold; // 증가되어야할 골드 수치

        while(elapsedTime < GOODS_INCREASE_DURATION)
        {
            // 매 프레임 경과 시간에 따라 연출 시간과의 비율 계산하여 현재 표시해야 할 텍스트 값 산출
            float curValue = Mathf.Lerp(curTextValue, destValue, elapsedTime / GOODS_INCREASE_DURATION);
            GoldAmountTxt.text = curValue.ToString("N0");
            elapsedTime += Time.deltaTime; // 경과 시간을 한 프레임 시간 만큼 증가
            yield return null;
        }

        // 텍스트 증가 연출 끝나면 도달 수치를 텍스트 컴포넌트에 표시
        GoldAmountTxt.text = destValue.ToString("N0");
    }

    private void OnUpateGem(GemUpdateMsg gemUpdateMsg)
    {
        // 획득 연출
        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if (userGoodsData == null)
        {
            Logger.LogError("UserGoodsData does not eixts.");
            return;
        }

        AudioManager.Instance.PlaySFX(ESFX.UI_Get_Goods);

        if (gemUpdateMsg.IsAdd)
        {
            if (mGoldIncreaseCo != null) // 기존 획득 연출 코루틴 취소
            {
                StopCoroutine(IncreaseGemCo());
            }

            // 젬 증가 연출 담당 코루틴 호출
            mGemIncreaseCo = StartCoroutine(IncreaseGemCo());
        }
        else
        {
            GemAmountTxt.text = userGoodsData.Gem.ToString("N0");
        }
    }

    private IEnumerator IncreaseGemCo()
    {
        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if (userGoodsData == null)
        {
            Logger.LogError("UserGoodsData does not eixts.");
            yield break;
        }

        // 재화 이동 연출
        int amount = 10;
        for (int i = 0; i < amount; i++)
        {
            GameObject gemObject = Instantiate(Resources.Load("Prefabs/UI/GemMove", typeof(GameObject))) as GameObject;
            gemObject.transform.SetParent(transform.parent);
            gemObject.transform.localScale = Vector3.one;
            gemObject.transform.localPosition = Vector3.zero;
            gemObject.GetComponent<GoodsMove>().SetMove(i, GemIcon.transform.position);
        }

        yield return new WaitForSeconds(1f);

        AudioManager.Instance.PlaySFX(ESFX.UI_Increase_Goods);

        // 재화 텍스트 증가 연출
        float elapsedTime = 0f;// 경과 시간
        long curTextValue = Convert.ToInt64(GemAmountTxt.text.Replace(",", "")); // 현재 UI에 표시된 젬 수치
        long destValue = userGoodsData.Gem; // 증가되어야할 젬 수치

        while (elapsedTime < GOODS_INCREASE_DURATION)
        {
            // 매 프레임 경과 시간에 따라 연출 시간과의 비율 계산하여 현재 표시해야 할 텍스트 값 산출
            float curValue = Mathf.Lerp(curTextValue, destValue, elapsedTime / GOODS_INCREASE_DURATION);
            GemAmountTxt.text = curValue.ToString("N0");
            elapsedTime += Time.deltaTime; // 경과 시간을 한 프레임 시간 만큼 증가
            yield return null;
        }

        // 텍스트 증가 연출 끝나면 도달 수치를 텍스트 컴포넌트에 표시
        GemAmountTxt.text = destValue.ToString("N0");
    }
}
