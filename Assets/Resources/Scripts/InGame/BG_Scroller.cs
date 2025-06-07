using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BG_Scroller : MonoBehaviour
{
    public int Count;
    public float SpeedRate;

    void Start()
    {
        Count = transform.childCount;
    }

    void Update()
    {
        if(GameManager.Instance.IsRunning)
        {
            float totalSpeed = GameManager.Instance.GlobalSpeed * SpeedRate * Time.deltaTime * -1f;
            transform.Translate(totalSpeed, 0, 0);
        }
    }
}
