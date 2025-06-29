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
    // ���̾�̽��� �����ϴ� ��� ��ɿ� ���� �����ϵ��� �ϴ� Ŭ����
    private FirebaseApp mApp;

    // RemoteConfig
    private FirebaseRemoteConfig mRemoteConfig;
    private bool mbIsRemoteConfigInit = false;
    private Dictionary<string, object> mRemoteConfigDic = new Dictionary<string, object>(); // remoteconfig ���� ��ƿ��� ���� �����̳�

    // Auth
    private FirebaseAuth mAuth;
    private bool mbIsAuthInit = false;
    private const string GOOGLE_WEB_CLIENT_ID = "";
    private GoogleSignInConfiguration mGoogleSignInConfiguration;
    private FirebaseUser mFirebaseUser; // �α��ε� ���� ��ü
    private string mUnityEditorUserId = "";

    // ���� �α����� �̷� �ִ��� Ȯ�� �� ������ ���� ���� �ʱ�ȭ ���� �ڵ� �α��� ó��
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

                mApp = FirebaseApp.DefaultInstance; // ���̾�̽� ��ɵ鿡 �����ϱ� ���� �߾� ����
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

        float elapsedTime = 0f; // ��� �ð� ����
        while (elapsedTime < GlobalDefine.THIRD_PARTY_SERVICE_INIT_TIME)
        {
            if (IsInit())
            {
                // ��� �ʱ�ȭ ���������� �����
                Logger.Log($"{GetType()} initialization success.");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �ʱ�ȭ �ð� �ʰ��ߴµ��� �ʱ�ȭ���� �ʾҴٸ� �ʱ�ȭ ����
        Logger.LogError($"FirebaseApp initialization failed.");
    }

    // https://worldtimeapi.org/api/ip���� �ð� �޾ƿ��� ��� Firestore Database�κ��� ���� �ð� ������
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

        // RemoteConfig �Ű����� ������ ���̾�̽� �������� ������
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
                            // RemoteConfig �Ű����� �� �����ͼ� Dictionary�� ����
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
        mAuth = FirebaseAuth.DefaultInstance; // auth ������ ���� ���� ��ü ����
        if(mAuth == null)
        {
            Logger.LogError($"FirebaseApp initialization failed. FirebaseAuth is null");
            return;
        }

        // ���� ���� ��ȭ�� ���� �̺�Ʈ �߻� -> �������� �̺�Ʈ �Լ� ����
        mAuth.StateChanged += OnAuthStateChanged; // �α���, �α׾ƿ� �� ���� ��ȭ ���� �� -> �ڵ����� ȣ��

        mGoogleSignInConfiguration = new GoogleSignInConfiguration
        {
            WebClientId = GOOGLE_WEB_CLIENT_ID,
            RequestIdToken = true
        };

        mbIsAuthInit = true;

        // ���� ������ null(�α����� ������ ���ٸ�)�̰�,
        if (mAuth.CurrentUser == null)
        {
            // ���ſ� ���� �α����� �̷��� ������ �ڵ� �α��� ó��
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
            // ���� ���� ���ڸ��� �ٽ� ���� �����ϸ� ���� �α��� ���� �������
            // -> ���� ���� ������ ����
            mFirebaseUser = mAuth.CurrentUser;
        }
    }

    // ������ �α׾ƿ��ϰų� ���ͳ� ������ �����Ǿ� ���� ���� ��ȭ�� �Ͼ�� ������ Ÿ��Ʋ ȭ������ ���� ��α����ϰ� ����� ����
    private void OnAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        // ���� ���� Ÿ��Ʋ���� ��� ����ó��
        if(SceneLoader.Instance.GetCurrentScene() == ESceneType.Title)
        {
            return;
        }

        // �κ� ���̳� �ΰ��� ������ ���� ������ �α׾ƿ��ߴ��� ���ͳ� ������ �����Ǿ����� üũ
        // ���� ���� ��ü�� ���������� ���� ������ ���� ���� -> �α׾ƿ�, ���ͳ� �������� �Ǵ�
        if(mAuth != null && mAuth.CurrentUser == null)
        {
            Logger.Log("User signed out or disconnected.");
            mFirebaseUser = null;
            HasSignedInWithGoogle = false;
            HasSignedInWithApple = false;
            SaveData();
            AudioManager.Instance.StopBGM();
            UIManager.Instance.CloseAllOpenUI();
            SceneLoader.Instance.LoadScene(ESceneType.Title); // Ÿ��Ʋ ������ ���� �̵�
        }
    }

    public bool IsSignedIn()
    {
#if UNITY_EDITOR
        return true; // ����Ƽ �����Ϳ����� ����/���� ���� �α��� ���� ���� -> ������ �󿡼� ���� ���� ����(������ �α���)
#else
        return mFirebaseUser != null; // ���̾�̽� ���� ��ü�� null�� �ƴ��� üũ
#endif
    }

    public void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = mGoogleSignInConfiguration;
        // ���� ���� �α���
        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            // �α��� ����
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

            // �������� ���� �α��� ó�� -> ���̾�̽� ���� ó��
            // �α��ε� ������ ���� ���ӿ� ���� ���� ����
            GoogleSignInUser googleUser = task.Result;
            // �ڰ� ���� ��ü ����
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            // ���� ��û
            mAuth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                // ��� �ݹ� ���� authTask ������ ���� ��ҵǾ��ų� ���� �߻��ߴ��� Ȯ��
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

                // ���� ����
                mFirebaseUser = authTask.Result;
                Logger.Log($"User signed in successfully: {mFirebaseUser.DisplayName} ({mFirebaseUser.UserId})");

                HasSignedInWithGoogle = true;
                HasSignedInWithApple = true;
                SaveData(); // ���ñ�⿡ �α��� ���� ����
            });
        });
    }

    public void SignInWithApple()
    {
        // TODO: SignInWithApple
    }

    public void SignOut()
    {
        // ���� ���� �����ϸ� �α׾ƿ�
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
        SceneLoader.Instance.LoadScene(ESceneType.Title); // Ÿ��Ʋ ������ ���� �̵�
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
            // OK ������ �ٽ� �α��� UI�� �������� ó��
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

    // ������ ����
    // �� ���� ������ Ŭ���� ��ü�� �ϳ��� Collection�� �Ǿ� DB�� �����
    // Collection -> ������ ������ Document�� ���� �� ������ �ִ� �����̳�
    // �� ���� ������ Ŭ������ ���� Ŭ�������� ��ġ�ϴ� �÷����� ����� �� ������ ������ ���� ���̵�� ������ ��ť��Ʈ�� ����
    // ��ȭ �����ͷ� ���� ���, ��ť��Ʈ�� �� ������ ��ȭ �����͸� ��� �ִ� ��ť��Ʈ�̴�
    // ��ť��Ʈ �������� ���� ���� �ʵ尪�� �ִ�(�� �ʵ尡 ������ �����Ϸ��� ������ �������̴�)

    // ��)
    // [Collection]           [Document]          [Field]
    // UserGoodsData    ----   UserId 1    ----  Gem : 100
    //                                     ----  Gold : 100
    //                  ----   UserId 2    ----  Gem : 100
    //                                     ----  Gold : 100

    // UserSettingsData ----   UserId 1     ---- SFX : true
    //                                      ---- BGM : true
    //                  ----   UserId2      ---- SFX : false
    //                                      ---- BGM : true

    // T�� ���� ������ Ŭ���� -> T�� Ŭ�����̸鼭 IUserData �������̽��� �����ؾ���
    // �ε� �Ϸ��� �����ؾ� �� �۾��� �������� �� �ִ� Action������ �Ű������� ����
    public void LoadUserData<T>(Action onFinishLoad = null) where T : class, IUserData
    {
        Type type = typeof(T);
        // Ŭ���� �̸��� ���ڿ��� Collection�� �Ű������� �Ѱ���
        // Document(GetUserId()) -> Ư�� ���� ���̵��� ��ť��Ʈ�� ������
        // GetSnapshotAsync() ���� ������ ������
        mDatabase.Collection($"{type}").Document(GetUserId()).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            // ������ ��û �۾� �Ϸ� �� task ������ ����� ���������� �Ѿ����
            if (task.IsCompleted)
            {
                // �ε��ϰ��� �ߴ� T Ŭ���� ���������͸� ���� ������ �Ŵ������� �޾ƿ�
                IUserData userData = UserDataManager.Instance.GetUserData<T>();
                DocumentSnapshot snapshot = task.Result; // ������ ������ ����� DocumentSnapshot�� ����
                if(snapshot.Exists) // ã������ �����Ͱ�  DB�� �����ϴ��� üũ
                {
                    Logger.Log($"{type} loaded successfully.");

                    Dictionary<string, object> userDataDict = snapshot.ToDictionary(); // �ε��� �����͸� ��ųʸ��� ����
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
        // �����͸� ������ ���� ������ ��ü�� ������
        // ���� ������ ��ü -> �����ͺ��̽��� �����ϴ� �츮�� ������ �÷��� Ÿ�� �ȿ� ����� Ư�� ���� ���̵� ��ť��Ʈ�� �ǹ���
        DocumentReference docRef = mDatabase.Collection($"{type}").Document(GetUserId());

        // ��ť��Ʈ ��ü�� �Ű������� ���� userDataDict�� �����ϵ��� ��û
        docRef.SetAsync(userDataDict).ContinueWithOnMainThread(task =>
        {
            if(task.IsCompleted)
            {
                // ���� ����
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

        // ��) é�� Ŭ������ �� ���̾�̽��� �α� ����
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
