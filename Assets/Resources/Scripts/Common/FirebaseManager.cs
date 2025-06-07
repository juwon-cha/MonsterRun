using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.RemoteConfig;
using Google;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class FirebaseManager : SingletonBehaviour<FirebaseManager>
{
    // 파이어베이스가 제공하는 모든 기능에 접근 가능하도록 하는 클래스
    private FirebaseApp mApp;

    // RemoteConfig
    private FirebaseRemoteConfig mRemoteConfig;
    private bool mbIsRemoteConfigInit = false;
    private Dictionary<string, object> mRemoteConfigDic = new Dictionary<string, object>(); // remoteconfig 값을 담아오기 위한 컨테이너

    // Auth
    private FirebaseAuth mAuth;
    private bool mbIsAuthInit = false;
    private const string GOOGLE_WEB_CLIENT_ID = "";
    private GoogleSignInConfiguration mGoogleSignInConfiguration;
    private FirebaseUser mFirebaseUser; // 로그인된 유저 객체
    private string mUnityEditorUserId = "";

    // 과거 로그인한 이력 있는지 확인 후 있으면 인증 서비스 초기화 직후 자동 로그인 처리
    public bool HasSignedInWithGoogle { get; private set; } = false;
    public bool HasSignedInWithApple { get; private set; } = false;

    // Firestore Database
    private FirebaseFirestore mDatabase;
    private bool mbIsFirestoreInit = false;

    // Analytics
    private bool mbIsAnalyticsInit = false;

    protected override void Init()
    {
        base.Init();

        LoadData();
        StartCoroutine(InitFirebaseServiceCo());
    }

    public bool IsInit()
    {
        return mbIsRemoteConfigInit
            && mbIsAuthInit
            && mbIsFirestoreInit
            && mbIsAnalyticsInit;
    }

    private void LoadData()
    {
        HasSignedInWithGoogle = PlayerPrefs.GetInt("HasSignedInWithGoogle") == 1 ? true : false;
        HasSignedInWithApple = PlayerPrefs.GetInt("HasSignedInWithApple") == 1 ? true : false;
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("HasSignedInWithGoogle", HasSignedInWithGoogle ? 1 : 0);
        PlayerPrefs.SetInt("HasSignedInWithApple", HasSignedInWithApple ? 1 : 0);
        PlayerPrefs.Save();
    }

    private IEnumerator InitFirebaseServiceCo()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if(dependencyStatus == DependencyStatus.Available)
            {
                Logger.Log($"FirebaseApp initialization success.");

                mApp = FirebaseApp.DefaultInstance; // 파이어베이스 기능들에 접근하기 위한 중앙 지점
                InitRemoteConfig();
                InitAuth();
                InitFirestore();
                InitAnalytics();
            }
            else
            {
                Logger.LogError($"FirebaseApp initialization failed. DependencyStatus:{dependencyStatus}");
            }
        });

        float elapsedTime = 0f; // 경과 시간 추적
        while (elapsedTime < GlobalDefine.THIRD_PARTY_SERVICE_INIT_TIME)
        {
            if (IsInit())
            {
                // 모든 초기화 정상적으로 수행됨
                Logger.Log($"{GetType()} initialization success.");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 초기화 시간 초과했는데도 초기화되지 않았다면 초기화 실패
        Logger.LogError($"FirebaseApp initialization failed.");
    }

    // https://worldtimeapi.org/api/ip에서 시간 받아오는 대신 Firestore Database로부터 서버 시간 가져옴
    public async Task<DateTime> GetCurrentDateTime()
    {
        // Request server timestamp from Firestore DB
        DocumentReference serverTimeDoc = mDatabase.Collection("time").Document("server_time");
        await serverTimeDoc.SetAsync(new { timestamp = FieldValue.ServerTimestamp });

        // Get server timestamp
        DocumentSnapshot serverTimeSnapshot = await serverTimeDoc.GetSnapshotAsync();
        Timestamp serverTimestamp = serverTimeSnapshot.GetValue<Timestamp>("timestamp");

        // Parse server timestamp to local datetime
        DateTime serverDateTime = serverTimestamp.ToDateTime().ToLocalTime();
        Logger.Log($"CurrentDateTime: {serverDateTime}");
        return serverDateTime;
    }

    #region REMOTE_CONFIG
    private void InitRemoteConfig()
    {
        mRemoteConfig = FirebaseRemoteConfig.DefaultInstance;
        if(mRemoteConfig == null)
        {
            Logger.LogError($"FirebaseApp initialization failed. FirebaseRemoteConfig is null.");
            return;
        }

        mRemoteConfigDic.Add("dev_app_version", string.Empty);
        mRemoteConfigDic.Add("release_app_version", string.Empty);

        // RemoteConfig 매개변수 값들을 파이어베이스 서버에서 가져옴
        mRemoteConfig.SetDefaultsAsync(mRemoteConfigDic).ContinueWithOnMainThread(task =>
        {
            mRemoteConfig.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(fetchTask =>
            {
                if (fetchTask.IsCompleted)
                {
                    mRemoteConfig.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                    {
                        if(activateTask.IsCompleted)
                        {
                            // RemoteConfig 매개변수 값 가져와서 Dictionary에 저장
                            mRemoteConfigDic["dev_app_version"] = mRemoteConfig.GetValue("dev_app_version").StringValue;
                            mRemoteConfigDic["release_app_version"] = mRemoteConfig.GetValue("release_app_version").StringValue;
                            mbIsRemoteConfigInit = true;
                        }
                    });
                }
            });
        });
    }

    public string GetAppVersion()
    {
#if DEV_VER
        if(mRemoteConfigDic.ContainsKey("dev_app_version"))
        {
            return mRemoteConfigDic["dev_app_version"].ToString();
        }
#else
        if(mRemoteConfigDic.ContainsKey("release_app_version"))
        {
            return mRemoteConfigDic["release_app_version"].ToString();
        }
#endif

        return string.Empty;
    }
    #endregion

    #region AUTH
    private void InitAuth()
    {
        mAuth = FirebaseAuth.DefaultInstance; // auth 변수에 인증 서비스 객체 대입
        if(mAuth == null)
        {
            Logger.LogError($"FirebaseApp initialization failed. FirebaseAuth is null");
            return;
        }

        // 인증 상태 변화에 따라 이벤트 발생 -> 실행해줄 이벤트 함수 대입
        mAuth.StateChanged += OnAuthStateChanged; // 로그인, 로그아웃 등 상태 변화 있을 때 -> 자동으로 호출

        mGoogleSignInConfiguration = new GoogleSignInConfiguration
        {
            WebClientId = GOOGLE_WEB_CLIENT_ID,
            RequestIdToken = true
        };

        mbIsAuthInit = true;

        // 현재 유저가 null(로그인한 유저가 없다면)이고,
        if (mAuth.CurrentUser == null)
        {
            // 과거에 구글 로그인한 이력이 있으면 자동 로그인 처리
            if(HasSignedInWithGoogle)
            {
                SignInWithGoogle();
            }
            else if(HasSignedInWithApple)
            {
                SignInWithApple();
            }
        }
        else
        {
            // 게임 종료 하자마자 다시 게임 시작하면 이전 로그인 세션 살아있음
            // -> 이전 유저 정보를 대입
            mFirebaseUser = mAuth.CurrentUser;
        }
    }

    // 유저가 로그아웃하거나 인터넷 연결이 해제되어 인증 상태 변화가 일어나면 강제로 타이틀 화면으로 보내 재로그인하게 만들기 위함
    private void OnAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        // 현재 씬이 타이틀씬의 경우 예외처리
        if(SceneLoader.Instance.GetCurrentScene() == ESceneType.Title)
        {
            return;
        }

        // 로비 씬이나 인게임 씬에서 현재 유저가 로그아웃했는지 인터넷 연결이 해제되었는지 체크
        // 인증 서비스 객체는 존재하지만 현재 유저가 없는 상태 -> 로그아웃, 인터넷 끊김으로 판단
        if(mAuth != null && mAuth.CurrentUser == null)
        {
            Logger.Log("User signed out or disconnected.");
            mFirebaseUser = null;
            HasSignedInWithGoogle = false;
            HasSignedInWithApple = false;
            SaveData();
            AudioManager.Instance.StopBGM();
            UIManager.Instance.CloseAllOpenUI();
            SceneLoader.Instance.LoadScene(ESceneType.Title); // 타이틀 씬으로 강제 이동
        }
    }

    public bool IsSignedIn()
    {
#if UNITY_EDITOR
        return true; // 유니티 에디터에서는 구글/애플 계정 로그인 동작 안함 -> 에디터 상에서 인증 과정 무시(무조건 로그인)
#else
        return mFirebaseUser != null; // 파이어베이스 유저 객체가 null이 아닌지 체크
#endif
    }

    public void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = mGoogleSignInConfiguration;
        // 구글 계정 로그인
        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            // 로그인 실패
            if (task.IsCanceled || task.IsFaulted)
            {
                if (task.IsCanceled)
                {
                    Logger.LogError($"SignInWithGoogle was canceled.");
                }
                else if (task.IsFaulted)
                {
                    Logger.LogError($"SignInWithGoogle encountered an error: {task.Exception}.");
                }

                ShowLoginFailUI();
                return;
            }

            // 정상적인 구글 로그인 처리 -> 파이어베이스 인증 처리
            // 로그인된 계정을 통해 게임에 대한 유저 인증
            GoogleSignInUser googleUser = task.Result;
            // 자격 증명 객체 생성
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            // 인증 요청
            mAuth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                // 결과 콜백 오면 authTask 변수를 통해 취소되었거나 에러 발생했는지 확인
                if(authTask.IsCanceled || authTask.IsFaulted)
                {
                    if (authTask.IsCanceled)
                    {
                        Logger.LogError($"SignInWithCredentialAsync was canceled.");
                    }
                    else if (authTask.IsFaulted)
                    {
                        Logger.LogError($"SignInWithCredentialAsync encountered an error: {authTask.Exception}.");
                    }

                    ShowLoginFailUI();
                    return;
                }

                // 인증 성공
                mFirebaseUser = authTask.Result;
                Logger.Log($"User signed in successfully: {mFirebaseUser.DisplayName} ({mFirebaseUser.UserId})");

                HasSignedInWithGoogle = true;
                HasSignedInWithApple = true;
                SaveData(); // 로컬기기에 로그인 정보 저장
            });
        });
    }

    public void SignInWithApple()
    {
        // TODO: SignInWithApple
    }

    public void SignOut()
    {
        // 현재 유저 존재하면 로그아웃
        if(mFirebaseUser != null)
        {
            mAuth.SignOut();
            Logger.Log($"User signed out successfully.");
        }

#if UNITY_EDITOR
        Logger.Log("User signed out or disconnected.");
        mFirebaseUser = null;
        HasSignedInWithGoogle = false;
        HasSignedInWithApple = false;
        SaveData();
        AudioManager.Instance.StopBGM();
        UIManager.Instance.CloseAllOpenUI();
        SceneLoader.Instance.LoadScene(ESceneType.Title); // 타이틀 씬으로 강제 이동
#endif
    }

    private void ShowLoginFailUI()
    {
        var uiData = new ConfirmUIData();
        uiData.ConfirmType = EConfirmType.OK;
        uiData.TitleTxt = "Error";
        uiData.DescTxt = "Failed to sign in";
        uiData.OKBtnTxt = "OK";
        uiData.OnClickOKBtn = () =>
        {
            // OK 누르면 다시 로그인 UI가 열리도록 처리
            var uiData = new BaseUIData();
            UIManager.Instance.OpenUI<LoginUI>(uiData);
        };

        UIManager.Instance.OpenUI<ConfirmUI>(uiData);
    }

    private string GetUserId()
    {
#if UNITY_EDITOR
        return mUnityEditorUserId;
#else
        return mFirebaseUser != null ? mFirebaseUser.UserId : string.Empty;
#endif
    }
    #endregion

    #region FIRESTORE
    private void InitFirestore()
    {
        mDatabase = FirebaseFirestore.DefaultInstance;
        if(mDatabase == null)
        {
            Logger.LogError($"FirebaseFirestore initialization failed. FirebaseFirestore is null.");
            return;
        }

        mbIsFirestoreInit = true;
    }

    // 데이터 저장
    // 각 유저 데이터 클래스 객체는 하나의 Collection이 되어 DB에 저장됨
    // Collection -> 동일한 형태의 Document를 여러 개 가지고 있는 컨테이너
    // 각 유저 데이터 클래스에 대해 클래스명과 일치하는 컬렉션을 만들고 그 하위에 각각의 유저 아이디로 명명된 도큐먼트를 저장
    // 재화 데이터로 예를 들면, 도큐먼트는 각 유저의 재화 데이터를 담고 있는 도큐먼트이다
    // 도큐먼트 하위에는 여러 개의 필드값이 있다(이 필드가 실제로 저장하려는 유저의 데이터이다)

    // 예)
    // [Collection]           [Document]          [Field]
    // UserGoodsData    ----   UserId 1    ----  Gem : 100
    //                                     ----  Gold : 100
    //                  ----   UserId 2    ----  Gem : 100
    //                                     ----  Gold : 100

    // UserSettingsData ----   UserId 1     ---- SFX : true
    //                                      ---- BGM : true
    //                  ----   UserId2      ---- SFX : false
    //                                      ---- BGM : true

    // T는 유저 데이터 클래스 -> T는 클래스이면서 IUserData 인터페이스를 구현해야함
    // 로딩 완료후 진행해야 할 작업을 정의해줄 수 있는 Action변수를 매개변수로 받음
    public void LoadUserData<T>(Action onFinishLoad = null) where T : class, IUserData
    {
        Type type = typeof(T);
        // 클래스 이름을 문자열로 Collection에 매개변수로 넘겨줌
        // Document(GetUserId()) -> 특정 유저 아이디의 도큐먼트를 가져옴
        // GetSnapshotAsync() 실제 데이터 가져옴
        mDatabase.Collection($"{type}").Document(GetUserId()).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            // 데이터 요청 작업 완료 후 task 변수로 결과가 정상적으로 넘어오면
            if (task.IsCompleted)
            {
                // 로딩하고자 했던 T 클래스 유저데이터를 유저 데이터 매니저에서 받아옴
                IUserData userData = UserDataManager.Instance.GetUserData<T>();
                DocumentSnapshot snapshot = task.Result; // 가져온 데이터 결과를 DocumentSnapshot에 저장
                if(snapshot.Exists) // 찾으려는 데이터가  DB에 존재하는지 체크
                {
                    Logger.Log($"{type} loaded successfully.");

                    Dictionary<string, object> userDataDict = snapshot.ToDictionary(); // 로드한 데이터를 딕셔너리로 저장
                    userData.SetData(userDataDict);
                }
                else
                {
                    Logger.Log($"No {type} found. Setting default data.");

                    userData.SetDefaultData();
                    userData.SaveData();
                }

                onFinishLoad?.Invoke();
            }
            else
            {
                Logger.LogError($"Failed to load {type}: {task.Exception}");
            }
        });
    }

    public void SaveUserData<T>(Dictionary<string, object> userDataDict) where T : class, IUserData
    {
        Type type = typeof(T);
        // 데이터를 저장할 유저 데이터 객체를 가져옴
        // 유저 데이터 객체 -> 데이터베이스에 존재하는 우리가 저장할 컬렉션 타입 안에 저장된 특정 유저 아이디 도큐먼트를 의미함
        DocumentReference docRef = mDatabase.Collection($"{type}").Document(GetUserId());

        // 도큐먼트 객체에 매개변수로 받은 userDataDict를 저장하도록 요청
        docRef.SetAsync(userDataDict).ContinueWithOnMainThread(task =>
        {
            if(task.IsCompleted)
            {
                // 저장 성공
                Logger.Log($"{type} saved successfully.");

            }
            else
            {
                Logger.LogError($"Failed to save {type}: {task.Exception}");
            }
        });
    }
    #endregion

    #region ANALYTICS
    private void InitAnalytics()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        mbIsAnalyticsInit = true;
    }

    public void LogCustomEvent(string eventName, Dictionary<string, object> parameters)
    {
        List<Parameter> firebaseParameters = new List<Parameter>();
        foreach (var param in parameters)
        {
            firebaseParameters.Add(new Parameter(param.Key, param.Value.ToString()));
        }

        FirebaseAnalytics.LogEvent(eventName, firebaseParameters.ToArray());

        // 예) 챕터 클리어할 때 파이어베이스로 로그 전송
    }
    #endregion

    protected override void Dispose()
    {
        if (mAuth != null)
        {
            mAuth.StateChanged -= OnAuthStateChanged;
        }

        base.Dispose();
    }
}
