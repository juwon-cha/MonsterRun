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
            // initStatus�� ���� �ʱ�ȭ�� �� ����Ǿ����� Ȯ��
            bool isInitSuccess = true;
            var statusMap = initStatus.getAdapterStatusMap();
            foreach (var status in statusMap)
            {
                var className = status.Key; // status�� �ν��Ͻ���
                var adapterStatus = status.Value; // ���� status�� ����
                Logger.Log($"Adapter: {className}, State: {adapterStatus.InitializationState}, Description: {adapterStatus.Description}");
                if(adapterStatus.InitializationState != AdapterState.Ready) // �ʱ�ȭ ���°� Ready�� �ƴϸ� �ʱ�ȭ ����
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
        // ����/�÷����� ���� ���� ���̵� ���� ���� �Լ� ȣ��
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
                // ��� ��ü�� ���������� ���� ȭ�鿡 ǥ�þȵǴ� ���
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

        // ������ ������ �߻��� �� ȣ��Ǵ� �̺�Ʈ ������ ���
        mTopBannerView.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"mTopBannerView paid {adValue.Value}{adValue.CurrencyCode}");
        };

        // ���� ������ ���۵��� �� ȣ��
        mTopBannerView.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"mTopBannerView recorded an impression");
        };

        // ���� Ŭ��
        mTopBannerView.OnAdClicked += () =>
        {
            Logger.Log($"mTopBannerView was clicked");
        };

        // ���� Ŭ�� �� ���� ������ ��
        mTopBannerView.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"mTopBannerView full screen content opened");
        };

        // ���ȴ� ���� ���� ��
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
        // ����/�÷����� ���� ���� ���̵� ���� ���� �Լ� ȣ��
        SetChapterClearInterstitialAdID();

        // ���� �ε� �Լ� ȣ��
        LoadChapterClearInterstitialAd(); // ������ ���� �̸� �ε�(���� ȭ�鿡 ǥ���ϴ� ���� �ƴ�)
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

    // ���� ���� �ε� �Լ��� �ʱ�ȭ�� �� ȣ���ϴ� ����
    // -> ��� ����� �޸� ���� �ε��� ����� ����Ǵ� ���� �ƴϱ� ����
    // -> ���� ��������� �� �ε带 �ϸ� ���� ���� �ε����� ���� ���¶�� ���� ������� ���� �� ����
    // -> �񵿱� �ε� �ʿ�(�̸� �ε�)
    private void LoadChapterClearInterstitialAd()
    {
        // ad request ����
        var adRequest = new AdRequest();

        // ���� �ε� ��û
        InterstitialAd.Load(mChapterClearInterstitialAdID, adRequest,
            (InterstitialAd ad, LoadAdError error) => // �ε��� ������ �� ������ �ݹ��Լ�
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

        // ������ ������ �߻��� �� ȣ��Ǵ� �̺�Ʈ ������ ���
        mChapterClearInterstitial.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"mChapterClearInterstitial ad paid {adValue.Value}{adValue.CurrencyCode}");
        };

        // ���� ������ ���۵��� �� ȣ��
        mChapterClearInterstitial.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"mChapterClearInterstitial ad recorded an impression");
        };

        // ���� Ŭ��
        mChapterClearInterstitial.OnAdClicked += () =>
        {
            Logger.Log($"mChapterClearInterstitial ad was clicked");
        };

        // ���� Ŭ�� �� ���� ������ ������ ��
        mChapterClearInterstitial.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"mChapterClearInterstitial ad full screen content opened");
        };

        // ���� ������ �ݰ� ������ ����
        mChapterClearInterstitial.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"mChapterClearInterstitial ad full screen content closed");

            // ���� ���� ��ü�� ��Ȱ���� �� �ִ� ���� �ƴϰ� ��ȸ�� ��ü�̱� ������
            // �� �� ���� ����ϰ� ���� ���ο� �ν��Ͻ��� �ε��ؾ� ��
            LoadChapterClearInterstitialAd();

            // ���� ��û ���� ������ ó���� ���� �׼� ����
            mOnFinishChapterClearInterstitialAd?.Invoke();
            mOnFinishChapterClearInterstitialAd = null;
        };

        // ���� ��û �� ���� �߻�
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
        LoadDailyFreeGemRewardedAd(); // ���� ����� �Ȱ��� �񵿱� �ε� �ʿ�
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
        // ad request ����
        var adRequest = new AdRequest();

        // ���� �ε� ��û
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
        // ad request ����
        var adRequest = new AdRequest();

        // ���� �ε� ��û
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

        // ������ ������ �߻��� �� ȣ��Ǵ� �̺�Ʈ ������ ���
        mPackRewardedAd.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"mPackRewardedAd ad paid {adValue.Value}{adValue.CurrencyCode}");
        };

        // ���� ������ ���۵��� �� ȣ��
        mPackRewardedAd.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"mPackRewardedAd ad recorded an impression");
        };

        // ���� Ŭ��
        mPackRewardedAd.OnAdClicked += () =>
        {
            Logger.Log($"mPackRewardedAd ad was clicked");
        };

        // ���� Ŭ�� �� ���� ������ ������ ��
        mPackRewardedAd.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"mPackRewardedAd ad full screen content opened");
        };

        // ���� ������ �ݰ� ������ ����
        mPackRewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"mPackRewardedAd ad full screen content closed");

            // ���� ���� ��ü�� ��Ȱ���� �� �ִ� ���� �ƴϰ� ��ȸ�� ��ü�̱� ������
            // �� �� ���� ����ϰ� ���� ���ο� �ν��Ͻ��� �ε��ؾ� ��
            LoadPackRewardedAd();
        };

        // ���� ��û �� ���� �߻�
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

                // ���� ��û�� ���������� �Ϸ�Ǿ��� �� ������ ���� ó�� �ݹ��Լ�
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

        // ������ ������ �߻��� �� ȣ��Ǵ� �̺�Ʈ ������ ���
        mDailyFreeGemRewardedAd.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad paid {adValue.Value}{adValue.CurrencyCode}");
        };

        // ���� ������ ���۵��� �� ȣ��
        mDailyFreeGemRewardedAd.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad recorded an impression");
        };

        // ���� Ŭ��
        mDailyFreeGemRewardedAd.OnAdClicked += () =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad was clicked");
        };

        // ���� Ŭ�� �� ���� ������ ������ ��
        mDailyFreeGemRewardedAd.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad full screen content opened");
        };

        // ���� ������ �ݰ� ������ ����
        mDailyFreeGemRewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"mDailyFreeGemRewardedAd ad full screen content closed");

            // ���� ���� ��ü�� ��Ȱ���� �� �ִ� ���� �ƴϰ� ��ȸ�� ��ü�̱� ������
            // �� �� ���� ����ϰ� ���� ���ο� �ν��Ͻ��� �ε��ؾ� ��
            LoadDailyFreeGemRewardedAd();
        };

        // ���� ��û �� ���� �߻�
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

                // ���� ��û�� ���������� �Ϸ�Ǿ��� �� ������ ���� ó�� �ݹ��Լ�
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
