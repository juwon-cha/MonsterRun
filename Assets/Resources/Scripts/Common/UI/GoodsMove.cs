using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoodsMove : MonoBehaviour
{
    public float MoveSpeed = 5f;

    private Vector3 mDestPos;
    private Transform mTransform;
    private RectTransform mRectTransform;

    public void SetMove(int idx, Vector3 destPos)
    {
        mTransform = transform;
        mRectTransform = GetComponent<RectTransform>();
        mDestPos = new Vector3(destPos.x, destPos.y, 0);

        StartCoroutine(MoveCo(idx));
    }

    private IEnumerator MoveCo(int idx)
    {
        // 일정 간격 차례대로 움직일 수 있도록 함
        yield return new WaitForSeconds(0.1f + 0.08f * idx);

        // 매 프레임 목적 위치에 왔는지 체크
        while(mTransform.position.y < mDestPos.y)
        {
            mTransform.position = Vector2.MoveTowards(mTransform.position, mDestPos, MoveSpeed * Time.deltaTime);
            // 트랜스폼 위치 변경 -> RectTransform z값이 변경됨 -> RectTransform z값 고정 필요
            var rectLocalPos = mRectTransform.localPosition;
            mRectTransform.localPosition = new Vector3(rectLocalPos.x, rectLocalPos.y, 0f);
            yield return null; // 다음 프레임에도 실행
        }

        // 목표 도달 후 인스턴스 삭제
        Destroy(gameObject);
    }
}
