using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AdsManager : SingletonBehaviour<AdsManager>
{
    protected override void Init()
    {
        base.Init();

        InitAdsService();
        InitBannerAds();
        InitInterstitialAds();
        InitRewardedAds();
    }

    private void InitAdsService()
    {
        MobileAds.Initialize(initStatus =>
        {
            // initStatus를 통해 초기화가 잘 진행되었는지 확인
            bool isInitSuccess = true;
            var statusMap = initStatus.getAdapterStatusMap();
            foreach (var status in statusMap)
            {
                var className = status.Key; // status의 인스턴스명
                var adapterStatus = status.Value; // 현재 status의 상태
                Logger.Log($"Adapter: {className}, State: {adapterStatus.InitializationState}, Description: {adapterStatus.Description}");
                if(adapterStatus.InitializationState != AdapterState.Ready) // 초기화 상태가 Ready가 아니면 초기화 실패
                {
                    isInitSuccess = false;
                }
            }

            if(isInitSuccess)
            {
                Logger.Log($"Google Ads initialization successful.");
            }
            else
            {
                Logger.LogError($"Google Ads initialization failed.");
            }
        });
    }

    #region BannerAds
    private BannerView mTopBannerView;
    private string mTopBannerAdID = string.Empty;
    private const string AOS_BANNER_TEST_AD_ID = "ca-app-pub-3940256099942544/6300978111";
    private const string IOS_BANNER_TEST_AD_ID = "ca-app-pub-3940256099942544/2934735716";
    private const string AOS_TOP_BANNER_AD_ID = "";
    private const string IOS_TOP_BANNER_AD_ID = "";

    private void InitBannerAds()
    {
        // 빌드/플랫폼에 따라 광고 아이디 변수 세팅 함수 호출
        SetTopBannerAdID();
    }

    private void SetTopBannerAdID()
    {
#if DEV_VER
#if UNITY_ANDROID
        mTopBannerAdID = AOS_BANNER_TEST_AD_ID;
#elif UNITY_IOS
        mTopBannerAdID = IOS_BANNER_TEST_AD_ID;
#endif
#else
#if UNITY_ANDROID
        mTopBannerAdID = AOS_TOP_BANNER_AD_ID;
#elif UNITY_IOS
        mTopBannerAdID = IOS_TOP_BANNER_AD_ID;
#endif
#endif
    }

    public void EnableTopBannerAd(bool value)
    {
        Logger.Log($"EnableTopBannerAd value: {value}");

        if(value)
        {
            if(mTopBannerView == null)
            {
                AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                mTopBannerView = new BannerView(mTopBannerAdID, adaptiveSize, AdPosition.Top);

                // Create Ad Request
                AdRequest request = new AdRequest();

                // Load Banner
                mTopBannerView.LoadAd(request);
                ListenToTopBannerAdEvents();
            }
            else
            {
                // 배너 객체가 존재하지만 실제 화면에 표시안되는 경우
                mTopBannerView.Show();
            }
        }
        else
        {
            if(mTopBannerView != null)
            {
                mTopBannerView.Hide();
            }
        }
    }

    private void ListenToTopBannerAdEvents()
    {
        if (mTopBannerView == null)
        {
            Logger.LogError($"mTopBannerView is null");
            return;
        }

        mTopBannerView.OnBannerAdLoaded += () =>
        {
            Logger.Log($"mTopBannerView loaded an ad with response: {mTopBannerView.GetResponseInfo()}");
        };

        mTopBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Logger.LogError($"mTopBannerView failed to load an ad with error: {error}");
        };

        // 광고에서 수익이 발생할 때 호출되는 이벤트 리스너 등록
        mTopBannerView.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"mTopBannerView paid {adValue.Value}{adValue.CurrencyCode}");
        };

        // 광고 노출이 시작됐을 때 호출
        mTopBannerView.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"mTopBannerView recorded an impression");
        };

        // 광고 클릭
        mTopBannerView.OnAdClicked += () =>
        {
            Logger.Log($"mTopBannerView was clicked");
        };

        // 광고 클릭 후 광고 열렸을 때
        mTopBannerView.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"mTopBannerView full screen content opened");
        };

        // 열렸던 광고가 닫힐 때
        mTopBannerView.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"mTopBannerView full screen content closed");
        };
    }
    #endregion

    #region InterstitialAds
    private InterstitialAd mChapterClearInterstitial;
    private string mChapterClearInterstitialAdID = string.Empty;
    private const string AOS_INTERSTITIAL_TEST_AD_ID = "ca-app-pub-3940256099942544/1033173712";
    private const string IOS_INTERSTITIAL_TEST_AD_ID = "ca-app-pub-3940256099942544/4411468910";
    private const string AOS_CHAPTER_CLEAR_INTERSTITIAL_AD_ID = "";
    private const string IOS_CHAPTER_CLEAR_INTERSTITIAL_AD_ID = "";
    private Action mOnFinishChapterClearInterstitialAd = null;

    private void InitInterstitialAds()
    {
        // 빌드/플랫폼에 따라 광고 아이디 변수 세팅 함수 호출
        SetChapterClearInterstitialAdID();

        // 광고 로드 함수 호출
        LoadChapterClearInterstitialAd(); // 보여줄 광고를 미리 로드(광고를 화면에 표시하는 것은 아님)
    }

    private void SetChapterClearInterstitialAdID()
    {
#if DEV_VER
#if UNITY_ANDROID
        mChapterClearInterstitialAdID = AOS_INTERSTITIAL_TEST_AD_ID;
#elif UNITY_IOS
        mChapterClearInterstitialAdID = IOS_INTERSTITIAL_TEST_AD_ID;
#endif
#else
#if UNITY_ANDROID
        mChapterClearInterstitialAdID = AOS_CHAPTER_CLEAR_INTERSTITIAL_AD_ID;
#elif UNITY_IOS
        mChapterClearInterstitialAdID = IOS_CHAPTER_CLEAR_INTERSTITIAL_AD_ID;
#endif
#endif
    }

    // 전면 광고 로드 함수를 초기화할 때 호출하는 이유
    // -> 배너 광고와 달리 광고 로딩이 동기로 실행되는 것이 아니기 때문
    // -> 광고를 보여줘야할 때 로드를 하면 만약 광고가 로딩되지 않은 상태라면 광고가 재생되지 않을 수 있음
    // -> 비동기 로딩 필요(미리 로딩)
    private void LoadChapterClearInterstitialAd()
    {
        // ad request 생성
        var adRequest = new AdRequest();

        // 광고 로딩 요청
        InterstitialAd.Load(mChapterClearInterstitialAdID, adRequest,
            (InterstitialAd ad, LoadAdError error) => // 로딩이 끝났을 때 실행할 콜백함수
            {
                if(error != null || ad == null)
                {
                    Logger.LogError($"Interstitial ad failed to load. Error: {error}");
                    return;
                }

                Logger.Log($"Interstitial ad loaded successfully. Response: {ad.GetResponseInfo()}");
                mChapterClearInterstitial = ad;
                ListenToChapterClearInterstitialAdEvents();
            });
    }

    private void ListenToChapterClearInterstitialAdEvents()
    {
        if(mChapterClearInterstitial == null)
        {
            Logger.LogError($"mChapterClearInterstitial is null");
            return;
        }

        // 광고에서 수익이 발생할 때 호출되는 이벤트 리스너 등록
        mChapterClearInterstitial.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"mChapterClearInterstitial ad paid {adValue.Value}{adValue.CurrencyCode}");
        };

        // 광고 노출이 시작됐을 때 호출
        mChapterClearInterstitial.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"mChapterClearInterstitial ad recorded an impression");
        };

        // 광고 클릭
        mChapterClearInterstitial.OnAdClicked += () =>
        {
            Logger.Log($"mChapterClearInterstitial ad was clicked");
        };

        // 광고 클릭 후 광고 컨텐츠 열었을 때
        mChapterClearInterstitial.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"mChapterClearInterstitial ad full screen content opened");
        };

        // 광고 컨텐츠 닫고 앱으로 복귀
        mChapterClearInterstitial.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"mChapterClearInterstitial ad full screen content closed");

            // 전면 광고 객체는 재활용할 수 있는 것이 아니고 일회성 객체이기 때문에
            // 한 번 광고를 재생하고 나면 새로운 인스턴스를 로드해야 함
            LoadChapterClearInterstitialAd();

            // 광고 시청 이후 실행할 처리를 담은 액션 실행
            mOnFinishChapterClearInterstitialAd?.Invoke();
            mOnFinishChapterClearInterstitialAd = null;
        };

        // 광고 시청 중 오류 발생
        mChapterClearInterstitial.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Logger.LogError($"mChapterClearInterstitial ad failed to open full screen content. Error: {error}");
            LoadChapterClearInterstitialAd();
            mOnFinishChapterClearInterstitialAd?.Invoke();
            mOnFinishChapterClearInterstitialAd = null;
        };
    }

    public void ShowChapterClearInterstitialAd(Action onFinishChapterClearInterstitialAd = null)
    {
        if(mChapterClearInterstitial != null && mChapterClearInterstitial.CanShowAd())
        {
            Logger.Log($"Show stage clear interstitial ad");
            mChapterClearInterstitial.Show();
            mOnFinishChapterClearInterstitialAd = onFinishChapterClearInterstitialAd;
        }
        else
        {
            Logger.LogError($"Chapter clear interstitial ad is not ready yet");
        }
    }
    #endregion

    #region RewardedAds
    private RewardedAd mPackRewardedAd;
    private string mPackRewardedAdID;

    private RewardedAd mDailyFreeGemRewardedAd;
    private string mDailyFreeGemRewardedAdID;

    private const string AOS_REWARDED_TEST_AD = "ca-app-pub-3940256099942544/5224354917";
    private const string IOS_REWARDED_TEST_AD = "ca-app-pub-3940256099942544/1712485313";
    private const string AOS_PACK_REWARDED_AD_ID = "";
    private const string IOS_PACK_REWARDED_AD_ID = "";
    private const string AOS_DAILY_FREE_GEM_REWARDED_AD_ID = "";
    private const string IOS_DAILY_FREE_GEM_REWARDED_AD_ID = "";

    private void InitRewardedAds()
    {
        SetPackRewardedAd();
        LoadPackRewardedAd();

        SetDailyFreeGemRewardedAdID();
        LoadDailyFreeGemRewardedAd(); // 전면 광고와 똑같이 비동기 로드 필요
    }

    private void SetPackRewardedAd()
    {
#if DEV_VER
#if UNITY_ANDROID
        mPackRewardedAdID = AOS_REWARDED_TEST_AD;
#elif UNITY_IOS
        mPackRewardedAdID = IOS_REWARDED_TEST_AD;
#endif
#else
#if UNITY_ANDROID
        mPackRewardedAdID = AOS_PACK_REWARDED_AD_ID;
#elif UNITY_IOS
        mPackRewardedAdID = IOS_PACK_REWARDED_AD_ID;
#endif
#endif
    }

    private void LoadPackRewardedAd()
    {
        // ad request 생성
        var adRequest = new AdRequest();

        // 광고 로딩 요청
        RewardedAd.Load(mPackRewardedAdID, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Logger.LogError($"Rewarded ad failed to load. Error: {error}");
                    return;
                }

                Logger.Log($"Rewarded ad loaded successfully. Response: {ad.GetResponseInfo()}");
                mPackRewardedAd = ad;
                ListenToPackRewardedAdEvents();
            });
    }

    private void SetDailyFreeGemRewardedAdID()
    {
#if DEV_VER
#if UNITY_ANDROID
        mDailyFreeGemRewardedAdID = AOS_REWARDED_TEST_AD;
#elif UNITY_IOS
        mDailyFreeGemRewardedAdID = IOS_REWARDED_TEST_AD;
#endif
#else
#if UNITY_ANDROID
        mDailyFreeGemRewardedAdID = AOS_DAILY_FREE_GEM_REWARDED_AD_ID;
#elif UNITY_IOS
        mDailyFreeGemRewardedAdID = IOS_DAILY_FREE_GEM_REWARDED_AD_ID;
#endif
#endif
    }

    private void LoadDailyFreeGemRewardedAd()
    {
        // ad request 생성
        var adRequest = new AdRequest();

        // 광고 로딩 요청
        RewardedAd.Load(mDailyFreeGemRewardedAdID, adRequest,
            (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Logger.LogError($"Rewarded ad failed to load. Error: {error}");
                return;
            }

            Logger.Log($"Rewarded ad loaded successfully. Response: {ad.GetResponseInfo()}");
            mDailyFreeGemRewardedAd = ad;
            ListenToDailyFreeGemRewardedAdEvents();
        });
    }

    private void ListenToPackRewardedAdEvents()
    {
        if (mPackRewardedAd == null)
        {
            Logger.LogError($"mPackRewardedAd is null");
            return;
        }

        // 광고에서 수익이 발생할 때 호출되는 이벤트 리스너 등록
        mPackRewardedAd.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"mPackRewardedAd ad paid {adValue.Value}{adValue.CurrencyCode}");
        };

        // 광고 노출이 시작됐을 때 호출
        mPackRewardedAd.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"mPackRewardedAd ad recorded an impression");
        };

        // 광고 클릭
        mPackRewardedAd.OnAdClicked += () =>
        {
            Logger.Log($"mPackRewardedAd ad was clicked");
        };

        // 광고 클릭 후 광고 컨텐츠 열었을 때
        mPackRewardedAd.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"mPackRewardedAd ad full screen content opened");
        };

        // 광고 컨텐츠 닫고 앱으로 복귀
        mPackRewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"mPackRewardedAd ad full screen content closed");

            // 보상 광고 객체는 재활용할 수 있는 것이 아니고 일회성 객체이기 때문에
            // 한 번 광고를 재생하고 나면 새로운 인스턴스를 로드해야 함
            LoadPackRewardedAd();
        };

        // 광고 시청 중 오류 발생
        mPackRewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Logger.LogError($"mPackRewardedAd ad failed to open full screen content. Error: {error}");
            LoadPackRewardedAd();
        };
    }

    public void ShowPackRewardedAd(Action onPackProductAd = null)
    {
        Logger.Log($"Show PackRewardedAd");

        if (mPackRewardedAd != null && mPackRewardedAd.CanShowAd())
        {
            mPackRewardedAd.Show((Reward reward) =>
            {
                Logger.Log($"Reward PackProduct");

                // 광고 시청이 정상적으로 완료되었을 때 실행할 보상 처리 콜백함수
                onPackProductAd?.Invoke();
            });
        }
        else
        {
            Logger.LogError($"mPackRewardedAd is not ready yet");
        }
    }

    private void ListenToDailyFreeGemRewardedAdEvents()
    {
        if (mDailyFreeGemRewardedAd == null)
        {
            Logger.LogError($"mDailyFreeGemRewardedAd is null");
            return;
        }

        // 광고에서 수익이 발생할 때 호출되는 이벤트 리스너 등록
        mDailyFreeGemRewardedAd.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad paid {adValue.Value}{adValue.CurrencyCode}");
        };

        // 광고 노출이 시작됐을 때 호출
        mDailyFreeGemRewardedAd.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad recorded an impression");
        };

        // 광고 클릭
        mDailyFreeGemRewardedAd.OnAdClicked += () =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad was clicked");
        };

        // 광고 클릭 후 광고 컨텐츠 열었을 때
        mDailyFreeGemRewardedAd.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad full screen content opened");
        };

        // 광고 컨텐츠 닫고 앱으로 복귀
        mDailyFreeGemRewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad full screen content closed");

            // 보상 광고 객체는 재활용할 수 있는 것이 아니고 일회성 객체이기 때문에
            // 한 번 광고를 재생하고 나면 새로운 인스턴스를 로드해야 함
            LoadDailyFreeGemRewardedAd();
        };

        // 광고 시청 중 오류 발생
        mDailyFreeGemRewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Logger.LogError($"mDailyFreeGemRewardedAd ad failed to open full screen content. Error: {error}");
            LoadDailyFreeGemRewardedAd();
        };
    }

    public void ShowDailyFreeGemRewardedAd(Action onRewardDailyFreeGemAd = null)
    {
        Logger.Log($"Show DailyFreeGemRewardedAd");

        if (mDailyFreeGemRewardedAd != null && mDailyFreeGemRewardedAd.CanShowAd())
        {
            mDailyFreeGemRewardedAd.Show((Reward reward) =>
            {
                Logger.Log($"Reward DailyFreeGem");

                // 광고 시청이 정상적으로 완료되었을 때 실행할 보상 처리 콜백함수
                onRewardDailyFreeGemAd?.Invoke();
            });
        }
        else
        {
            Logger.LogError($"mDailyFreeGemRewardedAd is not ready yet");
        }
    }
    #endregion

    protected override void Dispose()
    {
        if(mTopBannerView != null)
        {
            mTopBannerView.Destroy();
            mTopBannerView = null;
        }

        if(mChapterClearInterstitial != null)
        {
            mChapterClearInterstitial.Destroy();
            mChapterClearInterstitial = null;
        }

        if(mDailyFreeGemRewardedAd != null)
        {
            mDailyFreeGemRewardedAd.Destroy();
            mDailyFreeGemRewardedAd = null;
        }

        base.Dispose();
    }
}
