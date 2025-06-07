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

        // safeArea 가장 좌측 하단의 픽셀 위치를 벡터 값으로 나타냄
        Vector2 safeAreaBottomLeftPos = safeArea.position;
        //Logger.Log($"safeAreaBottomLeftPos.x:{safeAreaBottomLeftPos.x} safeAreaBottomLeftPos.y:{safeAreaBottomLeftPos.y}");

        // safeArea 가장 우측 상단의 픽셀 위치를 벡터 값으로 나타냄
        Vector2 safeAreaTopRightPos = safeAreaBottomLeftPos + safeArea.size;
        //Logger.Log($"safeAreaTopRightPos.x:{safeAreaTopRightPos.x} safeAreaTopRightPos.y:{safeAreaTopRightPos.y}");

        // 전체 화면의 가로, 세로 해상도 픽셀 값
        //Logger.Log($"Screen.width:{Screen.width} Screen.height:{Screen.height}");

        Vector2 anchorMin = Vector2.zero;
        anchorMin.x = safeAreaBottomLeftPos.x / Screen.width; // 0 ~ 1 사이의 값(비율)
        anchorMin.y = safeAreaBottomLeftPos.y / Screen.height;

        Vector2 anchorMax = Vector2.zero;
        anchorMax.x = safeAreaTopRightPos.x / Screen.width;
        anchorMax.y = safeAreaTopRightPos.y / Screen.height;

        mRectTransform.anchorMin = anchorMin;
        mRectTransform.anchorMax = anchorMax;
    }
}
