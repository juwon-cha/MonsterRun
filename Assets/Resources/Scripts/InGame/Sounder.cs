using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounder : MonoBehaviour
{
    public enum SFX
    {
        Jump,
        Land,
        Hit,
        Reset
    }

    public AudioClip[] Clips;
    private AudioSource mAudioSrc;

    private void Awake()
    {
        mAudioSrc = GetComponent<AudioSource>();
    }

    public void PlaySFX(SFX sfx)
    {
        mAudioSrc.clip = Clips[(int)sfx];
        mAudioSrc.Play();
    }
}
