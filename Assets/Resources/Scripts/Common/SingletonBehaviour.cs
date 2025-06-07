using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    // 씬 전환 시 싱글턴 객체를 삭제할지 여부
    protected bool mbIsDestroyOnLoad = false;

    // 이 클래스의 스태틱 인스턴스 변수
    protected static T mInstance;

    public static T Instance
    {
        get { return mInstance; }
    }

    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        if(mInstance == null)
        {
            mInstance = (T)this;

            if(!mbIsDestroyOnLoad)
            {
                DontDestroyOnLoad(this);
            }
        }
        else if(mInstance != this)
        {
            Destroy(gameObject);
        }
    }

    // 삭제 시 실행되는 함수
    protected virtual void OnDestroy()
    {
        Dispose();
    }

    // 삭제 시 추가로 처리해줘야할 작업들 처리
    protected virtual void Dispose()
    {
        if(mInstance == this)
        {
            mInstance = null;
        }
    }
}
