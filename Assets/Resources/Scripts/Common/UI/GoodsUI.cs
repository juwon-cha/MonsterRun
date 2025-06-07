using SuperMaxim.Messaging;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ��ȭ ���� �� ������ �޽��� Ŭ����(Pub-Sub ����������)
public class GoldUpdateMsg
{
    public bool IsAdd; // ��ȭ�� �����Ҷ��� ȹ�� ����ó���ϱ� ����
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

    private Coroutine mGoldIncreaseCo; // ��ȭ ���� ��û�� ������ �� �� �� ��û�� ���� ��û�� ����� ����
    private Coroutine mGemIncreaseCo;
    private const float GOODS_INCREASE_DURATION = 0.5f;

    // �ν��Ͻ��� ������ ���ӿ�����Ʈ�� Ȱ��ȭ�� �� ȣ��
    private void OnEnable()
    {
        // Pub-Sub ���̺귯��(���� ���)
        Messenger.Default.Subscribe<GoldUpdateMsg>(OnUpateGold);
        Messenger.Default.Subscribe<GemUpdateMsg>(OnUpateGem);
    }

    // �ν��Ͻ��� ������ ���ӿ�����Ʈ�� ��Ȱ��ȭ�� �� ȣ��
    private void OnDisable()
    {
        // ���� ����
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

    // GoodsUI �ν��Ͻ����� GoldUpdateMsg�� �޾��� �� �� �Լ��� �����
    private void OnUpateGold(GoldUpdateMsg goldUpdateMsg)
    {
        // ȹ�� ����
        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if(userGoodsData == null)
        {
            Logger.LogError("UserGoodsData does not eixts.");
            return;
        }

        AudioManager.Instance.PlaySFX(ESFX.UI_Get_Goods);

        if(goldUpdateMsg.IsAdd)
        {
            if(mGoldIncreaseCo != null) // ���� ȹ�� ���� �ڷ�ƾ ���
            {
                StopCoroutine(IncreaseGoldCo());
            }

            // ��� ���� ���� ��� �ڷ�ƾ ȣ��
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

        // ��ȭ �̵� ����
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

        // ��ȭ �ؽ�Ʈ ���� ����
        float elapsedTime = 0f;// ��� �ð�
        long curTextValue = Convert.ToInt64(GoldAmountTxt.text.Replace(",", "")); // ���� UI�� ǥ�õ� ��� ��ġ
        long destValue = userGoodsData.Gold; // �����Ǿ���� ��� ��ġ

        while(elapsedTime < GOODS_INCREASE_DURATION)
        {
            // �� ������ ��� �ð��� ���� ���� �ð����� ���� ����Ͽ� ���� ǥ���ؾ� �� �ؽ�Ʈ �� ����
            float curValue = Mathf.Lerp(curTextValue, destValue, elapsedTime / GOODS_INCREASE_DURATION);
            GoldAmountTxt.text = curValue.ToString("N0");
            elapsedTime += Time.deltaTime; // ��� �ð��� �� ������ �ð� ��ŭ ����
            yield return null;
        }

        // �ؽ�Ʈ ���� ���� ������ ���� ��ġ�� �ؽ�Ʈ ������Ʈ�� ǥ��
        GoldAmountTxt.text = destValue.ToString("N0");
    }

    private void OnUpateGem(GemUpdateMsg gemUpdateMsg)
    {
        // ȹ�� ����
        var userGoodsData = UserDataManager.Instance.GetUserData<UserGoodsData>();
        if (userGoodsData == null)
        {
            Logger.LogError("UserGoodsData does not eixts.");
            return;
        }

        AudioManager.Instance.PlaySFX(ESFX.UI_Get_Goods);

        if (gemUpdateMsg.IsAdd)
        {
            if (mGoldIncreaseCo != null) // ���� ȹ�� ���� �ڷ�ƾ ���
            {
                StopCoroutine(IncreaseGemCo());
            }

            // �� ���� ���� ��� �ڷ�ƾ ȣ��
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

        // ��ȭ �̵� ����
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

        // ��ȭ �ؽ�Ʈ ���� ����
        float elapsedTime = 0f;// ��� �ð�
        long curTextValue = Convert.ToInt64(GemAmountTxt.text.Replace(",", "")); // ���� UI�� ǥ�õ� �� ��ġ
        long destValue = userGoodsData.Gem; // �����Ǿ���� �� ��ġ

        while (elapsedTime < GOODS_INCREASE_DURATION)
        {
            // �� ������ ��� �ð��� ���� ���� �ð����� ���� ����Ͽ� ���� ǥ���ؾ� �� �ؽ�Ʈ �� ����
            float curValue = Mathf.Lerp(curTextValue, destValue, elapsedTime / GOODS_INCREASE_DURATION);
            GemAmountTxt.text = curValue.ToString("N0");
            elapsedTime += Time.deltaTime; // ��� �ð��� �� ������ �ð� ��ŭ ����
            yield return null;
        }

        // �ؽ�Ʈ ���� ���� ������ ���� ��ġ�� �ؽ�Ʈ ������Ʈ�� ǥ��
        GemAmountTxt.text = destValue.ToString("N0");
    }
}
