using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public bool IsHighScore;

    private float mHighScore;
    private TextMeshProUGUI mUiTxt;

    void Start()
    {
        mUiTxt = GetComponent<TextMeshProUGUI>();

        if(IsHighScore)
        {
            mHighScore = PlayerPrefs.GetFloat("Score");
            mUiTxt.text = mHighScore.ToString("F0");
        }
    }

    void LateUpdate()
    {
        if(GameManager.Instance.IsRunning == false)
        {
            return;
        }

        if(IsHighScore && GameManager.Instance.Score < mHighScore)
        {
            return;
        }

        mUiTxt.text = GameManager.Instance.Score.ToString("F0");
    }
}
