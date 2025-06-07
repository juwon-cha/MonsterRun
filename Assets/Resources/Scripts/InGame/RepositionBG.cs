using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RepositionBG : MonoBehaviour
{
    public UnityEvent OnMove;

    private void LateUpdate()
    {
        if(transform.position.x > -10)
        {
            return;
        }

        transform.Translate(36, 0, 0, Space.Self);
        OnMove.Invoke();
    }
}
