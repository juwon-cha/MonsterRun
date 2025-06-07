using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSprite : MonoBehaviour
{
    public Sprite[] Sprites;
    private SpriteRenderer mSpriteRenderer;

    private void Awake()
    {
        mSpriteRenderer = GetComponent<SpriteRenderer>();
        Change();
    }

    public void Change()
    {
        int random = Random.Range(0, Sprites.Length);
        mSpriteRenderer.sprite = Sprites[random];
    }
}
