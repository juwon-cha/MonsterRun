using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeObject : MonoBehaviour
{
    public GameObject[] Objects;

    void Start()
    {
        
    }

    public void Change()
    {
        int random = Random.Range(0, Objects.Length);

        for(int index = 0; index < Objects.Length; ++index)
        {
            transform.GetChild(index).gameObject.SetActive(random == index);
        }
    }
}
