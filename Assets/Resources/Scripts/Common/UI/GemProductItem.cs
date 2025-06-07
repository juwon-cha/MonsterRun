using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GemProductItem : MonoBehaviour
{
    public Image GemImg;
    public TextMeshProUGUI AmountTxt;
    public TextMeshProUGUI CostTxt;
    public Image AdIcon;
    public TextMeshProUGUI TimerTxt; // ���� ���� ���� ���� �� �� �ֱ���� �󸶳� �ð��� ���Ҵ��� ������
    private Button mButton; // ���� ���� ���ο� ���� ��ư Ŭ�� ���� �����ϱ� ���� ����
    private const float AD_COOLTIME_INTERVAL = 1f; // ���� ������� ���� �ð� �ؽ�Ʈ�� ���� �ð� �ֱ�� ������Ʈ �� �������� ���� ���
    private Coroutine mAdCoolTimeCo; // ���� Ÿ�̸Ӹ� ������ �ڷ�ƾ�� ������ ����

    private ProductData mProductData;

    private void OnDisable()
    {
        // mAdCoolTimeCo �ڷ�ƾ ����
        // ������ ���� Ÿ�̸Ӱ� ����Ǵµ� ���� ������ ������ �����ϰ� �ִ� Ÿ�̸ӿ� ���� ���� ó��
        if(mAdCoolTimeCo != null)
        {
            StopCoroutine(mAdCoolTimeCo);
            mAdCoolTimeCo = null;
        }
    }

    public void SetInfo(string productID)
    {
        mProductData = DataTableManager.Instance.GetProductData(productID);
        if (mProductData == null)
        {
            Logger.LogError($"Product does not exist. ProductID:{productID}");
            return;
        }

        if(mButton == null)
        {
            mButton = GetComponent<Button>(); // GemProductItem ��ũ��Ʈ�� �پ��ִ� ������ ������Ʈ���� Button ������Ʈ ������
            if(mButton == null)
            {
                Logger.LogError($"No button component");
                return;
            }
        }

        var gemImgTexture = Resources.Load<Texture2D>($"Textures/Shop_Icon_{mProductData.ProductID}");
        if (gemImgTexture != null)
        {
            GemImg.sprite = Sprite.Create(gemImgTexture, new Rect(0, 0, gemImgTexture.width, gemImgTexture.height), new Vector2(1f, 1f));
        }

        // ��ư ������Ʈ�� Ÿ�̸� �ؽ�Ʈ ��Ȱ��ȭ
        mButton.enabled = false;
        TimerTxt.gameObject.SetActive(false);
        AmountTxt.text = mProductData.RewardGem.ToString("N0");

        switch (mProductData.PurchaseType)
        {
            case EPurchaseType.IAP:
                CostTxt.gameObject.SetActive(true);
                AdIcon.gameObject.SetActive(false);
                mButton.enabled = true;
                break;

            case EPurchaseType.Ad:
                CostTxt.gameObject.SetActive(false);
                AdIcon.gameObject.SetActive(true);
                SetAdCoolTime();
                break;

            default:
                break;
        }
    }

    // ���������� ���� ��û�� �ð��� ���� �ð� �� -> ���� �ٽ� ��û �������� �Ǵ�
    // ��û �����ϸ� ��ǰ Ȱ��ȭ/ ��û �Ұ����ϸ� ��ǰ ��Ȱ��ȭ ó�� �� ���� �ð� Ÿ�̸� ����
    private async void SetAdCoolTime()
    {
        DateTime currentDateTime = await FirebaseManager.Instance.GetCurrentDateTime();
        
        // ���� ��¥ �������� 00�� ���� �������
        DateTime today = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 0, 0, 0);

        var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
        if(userPlayData == null)
        {
            Logger.LogError($"UserPlayData is null");
            return;
        }

        // ���� �÷��� �����Ϳ��� ���������� ���� ��û�ϰ� ���� ���� �ð��� ���� �ð� ��
        if(userPlayData.LastDailyFreeGemAdRewardedTime >= today)
        {
            mButton.enabled = false;
            AdIcon.gameObject.SetActive(false);
            TimerTxt.gameObject.SetActive(true);
            // AdCoolTimerCo �ڷ�ƾ �Լ��� �ٽ� ���� ��û�� �� ���������� ���� �ð��� �Ű������� �Ѱ���
            mAdCoolTimeCo = StartCoroutine(AdCoolTimerCo(today.AddDays(1)/*���� �ð��� �Ϸ� ����(���� ���� 00��)*/ - userPlayData.LastDailyFreeGemAdRewardedTime));
        }
        else
        {
            mButton.enabled = true;
            AdIcon.gameObject.SetActive(true);
            TimerTxt.gameObject.SetActive(false);
        }
    }

    private IEnumerator AdCoolTimerCo(TimeSpan remainTime)
    {
        while(remainTime.TotalSeconds > 0)
        {
            TimerTxt.text = $"{remainTime.Hours:D2}:{remainTime.Minutes:D2}:{remainTime.Seconds:D2}"; // 00:00:00 ǥ�õ�
            yield return new WaitForSeconds(AD_COOLTIME_INTERVAL);
            remainTime = remainTime.Subtract(TimeSpan.FromSeconds(AD_COOLTIME_INTERVAL));
        }

        // ���� �ð� 0 ����
        TimerTxt.text = "00:00:00";
        yield return new WaitForSeconds(1f);
        mAdCoolTimeCo = null;
        SetAdCoolTime();
    }

    public void OnClickItem()
    {
        //ShopManager.Instance.PurchaseProduct(mProductData.ProductID);

        Logger.Log($"{GetType()}::OnClickItem");

        AudioManager.Instance.PlaySFX(ESFX.UI_Button_Click);

        switch (mProductData.PurchaseType)
        {
            case EPurchaseType.IAP:
                break;
            case EPurchaseType.Ad:
                {
                    var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
                    if(userPlayData != null)
                    {
                        AdsManager.Instance.ShowDailyFreeGemRewardedAd(async/*�񵿱� GetCurrentDateTime �Լ��� ����ϱ� ����*/ () =>
                        {
                            // ���� ���� ó��
                            StartCoroutine(GetProductRewardCo());
                            // ���� �÷��� �������� ������ ���� ��û �ð��� ���� �ð� ����
                            userPlayData.LastDailyFreeGemAdRewardedTime = await FirebaseManager.Instance.GetCurrentDateTime();
                            userPlayData.SaveData();
                            SetAdCoolTime();
                        });
                    }
                }
                break;
            default:
                break;
        }
    }

    // ������ ���� ��û �� ���� ������ ������� �ʴ� ���� -> ���� �����ӿ� ������ ����ǵ��� �ڷ�ƾ ���
    private IEnumerator GetProductRewardCo()
    {
        yield return null;
        ShopManager.Instance.GetProductReward(mProductData.ProductID);
    }
}
