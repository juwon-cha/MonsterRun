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
        // ���� ���� ���ʴ�� ������ �� �ֵ��� ��
        yield return new WaitForSeconds(0.1f + 0.08f * idx);

        // �� ������ ���� ��ġ�� �Դ��� üũ
        while(mTransform.position.y < mDestPos.y)
        {
            mTransform.position = Vector2.MoveTowards(mTransform.position, mDestPos, MoveSpeed * Time.deltaTime);
            // Ʈ������ ��ġ ���� -> RectTransform z���� ����� -> RectTransform z�� ���� �ʿ�
            var rectLocalPos = mRectTransform.localPosition;
            mRectTransform.localPosition = new Vector3(rectLocalPos.x, rectLocalPos.y, 0f);
            yield return null; // ���� �����ӿ��� ����
        }

        // ��ǥ ���� �� �ν��Ͻ� ����
        Destroy(gameObject);
    }
}
