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
    public TextMeshProUGUI TimerTxt; // 일일 무료 보석 광고를 볼 수 있기까지 얼마나 시간이 남았는지 보여줌
    private Button mButton; // 보상 수령 여부에 따라 버튼 클릭 여부 제어하기 위한 변수
    private const float AD_COOLTIME_INTERVAL = 1f; // 다음 광고까지 남은 시간 텍스트를 얼마의 시간 주기로 업데이트 할 것인지에 대한 상수
    private Coroutine mAdCoolTimeCo; // 광고 타이머를 돌리는 코루틴을 저장할 변수

    private ProductData mProductData;

    private void OnDisable()
    {
        // mAdCoolTimeCo 코루틴 정지
        // 상점을 열면 타이머가 실행되는데 이후 상점을 닫으면 실행하고 있는 타이머에 대한 멈춤 처리
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
            mButton = GetComponent<Button>(); // GemProductItem 스크립트가 붙어있는 동일한 오브젝트에서 Button 컴포넌트 가져옴
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

        // 버튼 컴포넌트와 타이머 텍스트 비활성화
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

    // 마지막으로 광고 시청한 시간과 현재 시간 비교 -> 광고 다시 시청 가능한지 판단
    // 시청 가능하면 상품 활성화/ 시청 불가능하면 상품 비활성화 처리 및 남은 시간 타이머 실행
    private async void SetAdCoolTime()
    {
        DateTime currentDateTime = await FirebaseManager.Instance.GetCurrentDateTime();
        
        // 오늘 날짜 기준으로 00시 값을 만들어줌
        DateTime today = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 0, 0, 0);

        var userPlayData = UserDataManager.Instance.GetUserData<UserPlayData>();
        if(userPlayData == null)
        {
            Logger.LogError($"UserPlayData is null");
            return;
        }

        // 유저 플레이 데이터에서 마지막으로 광고를 시청하고 보상 받은 시간과 현재 시간 비교
        if(userPlayData.LastDailyFreeGemAdRewardedTime >= today)
        {
            mButton.enabled = false;
            AdIcon.gameObject.SetActive(false);
            TimerTxt.gameObject.SetActive(true);
            // AdCoolTimerCo 코루틴 함수에 다시 광고를 시청할 수 있을때까지 남은 시간을 매개변수로 넘겨줌
            mAdCoolTimeCo = StartCoroutine(AdCoolTimerCo(today.AddDays(1)/*오늘 시간에 하루 더함(다음 날의 00시)*/ - userPlayData.LastDailyFreeGemAdRewardedTime));
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
            TimerTxt.text = $"{remainTime.Hours:D2}:{remainTime.Minutes:D2}:{remainTime.Seconds:D2}"; // 00:00:00 표시됨
            yield return new WaitForSeconds(AD_COOLTIME_INTERVAL);
            remainTime = remainTime.Subtract(TimeSpan.FromSeconds(AD_COOLTIME_INTERVAL));
        }

        // 남은 시간 0 이하
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
                        AdsManager.Instance.ShowDailyFreeGemRewardedAd(async/*비동기 GetCurrentDateTime 함수를 사용하기 위함*/ () =>
                        {
                            // 보상 지급 처리
                            StartCoroutine(GetProductRewardCo());
                            // 유저 플레이 데이터의 마지막 광고 시청 시간에 현재 시간 대입
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

    // 리워드 광고 시청 후 보상 연출이 재생되지 않는 현상 -> 다음 프레임에 연출이 재생되도록 코루틴 사용
    private IEnumerator GetProductRewardCo()
    {
        yield return null;
        ShopManager.Instance.GetProductReward(mProductData.ProductID);
    }
}
