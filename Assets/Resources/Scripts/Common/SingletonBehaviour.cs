using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    // �� ��ȯ �� �̱��� ��ü�� �������� ����
    protected bool mbIsDestroyOnLoad = false;

    // �� Ŭ������ ����ƽ �ν��Ͻ� ����
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

    // ���� �� ����Ǵ� �Լ�
    protected virtual void OnDestroy()
    {
        Dispose();
    }

    // ���� �� �߰��� ó��������� �۾��� ó��
    protected virtual void Dispose()
    {
        if(mInstance == this)
        {
            mInstance = null;
        }
    }
}
