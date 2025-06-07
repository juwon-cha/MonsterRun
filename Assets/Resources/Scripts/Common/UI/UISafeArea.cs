using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UISafeArea : MonoBehaviour
{
    private RectTransform mRectTransform;

    void Start()
    {
        mRectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void LateUpdate()
    {
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // safeArea ���� ���� �ϴ��� �ȼ� ��ġ�� ���� ������ ��Ÿ��
        Vector2 safeAreaBottomLeftPos = safeArea.position;
        //Logger.Log($"safeAreaBottomLeftPos.x:{safeAreaBottomLeftPos.x} safeAreaBottomLeftPos.y:{safeAreaBottomLeftPos.y}");

        // safeArea ���� ���� ����� �ȼ� ��ġ�� ���� ������ ��Ÿ��
        Vector2 safeAreaTopRightPos = safeAreaBottomLeftPos + safeArea.size;
        //Logger.Log($"safeAreaTopRightPos.x:{safeAreaTopRightPos.x} safeAreaTopRightPos.y:{safeAreaTopRightPos.y}");

        // ��ü ȭ���� ����, ���� �ػ� �ȼ� ��
        //Logger.Log($"Screen.width:{Screen.width} Screen.height:{Screen.height}");

        Vector2 anchorMin = Vector2.zero;
        anchorMin.x = safeAreaBottomLeftPos.x / Screen.width; // 0 ~ 1 ������ ��(����)
        anchorMin.y = safeAreaBottomLeftPos.y / Screen.height;

        Vector2 anchorMax = Vector2.zero;
        anchorMax.x = safeAreaTopRightPos.x / Screen.width;
        anchorMax.y = safeAreaTopRightPos.y / Screen.height;

        mRectTransform.anchorMin = anchorMin;
        mRectTransform.anchorMax = anchorMax;
    }
}
