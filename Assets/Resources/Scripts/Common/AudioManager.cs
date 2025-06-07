using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBGM
{
    Lobby,
    COUNT
}

public enum ESFX
{
    Chapter_Clear,
    Stage_Clear,
    UI_Button_Click,
    UI_Get_Goods,
    UI_Increase_Goods,
    COUNT
}

public class AudioManager : SingletonBehaviour<AudioManager>
{
    public Transform BGMTransform;
    public Transform SFXTransform;

    private const string AUDIO_PATH = "Audio";

    private Dictionary<EBGM, AudioSource> mBGMPlayer = new Dictionary<EBGM, AudioSource>();
    private AudioSource mCurrBGMSource;

    private Dictionary<ESFX, AudioSource> mSFXPlayer = new Dictionary<ESFX, AudioSource>();

    protected override void Init()
    {
        base.Init();

        LoadBGMPlayer();
        LoadSFXPlayer();
    }

    private void LoadBGMPlayer()
    {
        for(int i = 0; i < (int)EBGM.COUNT; ++i)
        {
            var audioName = ((EBGM)i).ToString();
            var pathStr = $"{AUDIO_PATH}/{audioName}";
            var audioClip = Resources.Load(pathStr, typeof(AudioClip)) as AudioClip;
            if(audioClip == null)
            {
                Logger.LogError($"{audioName} clip does not exist.");
                continue;
            }

            var newGO = new GameObject(audioName);
            var newAudioSource = newGO.AddComponent<AudioSource>();
            newAudioSource.clip = audioClip;
            newAudioSource.loop = true;
            newAudioSource.playOnAwake = false;
            newGO.transform.parent = BGMTransform;

            mBGMPlayer[(EBGM)i] = newAudioSource;
        }
    }

    private void LoadSFXPlayer()
    {
        for (int i = 0; i < (int)ESFX.COUNT; ++i)
        {
            var audioName = ((ESFX)i).ToString();
            var pathStr = $"{AUDIO_PATH}/{audioName}";
            var audioClip = Resources.Load(pathStr, typeof(AudioClip)) as AudioClip;
            if (audioClip == null)
            {
                Logger.LogError($"{audioName} clip does not exist.");
                continue;
            }

            var newGO = new GameObject(audioName);
            var newAudioSource = newGO.AddComponent<AudioSource>();
            newAudioSource.clip = audioClip;
            newAudioSource.loop = false;
            newAudioSource.playOnAwake = false;
            newGO.transform.parent = SFXTransform;

            mSFXPlayer[(ESFX)i] = newAudioSource;
        }
    }

    public void OnLoadLobby()
    {
        var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        if(userSettingsData != null)
        {
            if(!userSettingsData.BGM)
            {
                MuteBGM();
            }

            if(!userSettingsData.SFX)
            {
                MuteSFX();
            }
        }
    }

    public void PlayBGM(EBGM bgm)
    {
        if(mCurrBGMSource)
        {
            mCurrBGMSource.Stop();
            mCurrBGMSource = null;
        }

        if(!mBGMPlayer.ContainsKey(bgm))
        {
            Logger.LogError($"Invalid clip name. {bgm}");
            return;
        }

        mCurrBGMSource = mBGMPlayer[bgm];
        mCurrBGMSource.Play();
    }

    public void PauseBGM()
    {
        if(mCurrBGMSource)
        {
            mCurrBGMSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (mCurrBGMSource)
        {
            mCurrBGMSource.UnPause();
        }
    }

    public void StopBGM()
    {
        if (mCurrBGMSource)
        {
            mCurrBGMSource.Stop();
        }
    }

    public void PlaySFX(ESFX sfx)
    {
        if(!mSFXPlayer.ContainsKey(sfx))
        {
            Logger.LogError($"Invalid clip name. {sfx}");
            return;
        }

        mSFXPlayer[sfx].Play();
    }

    public void MuteBGM()
    {
        foreach(var audioSourceItem in mBGMPlayer)
        {
            audioSourceItem.Value.volume = 0f;
        }
    }

    public void MuteSFX()
    {
        foreach (var audioSourceItem in mSFXPlayer)
        {
            audioSourceItem.Value.volume = 0f;
        }
    }

    public void UnMuteBGM()
    {
        foreach (var audioSourceItem in mBGMPlayer)
        {
            audioSourceItem.Value.volume = 1f;
        }
    }

    public void UnMuteSFX()
    {
        foreach (var audioSourceItem in mSFXPlayer)
        {
            audioSourceItem.Value.volume = 1f;
        }
    }
}
