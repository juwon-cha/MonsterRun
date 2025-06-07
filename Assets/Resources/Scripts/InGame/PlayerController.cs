using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public enum ECharacterState
    {
        IDLE,
        RUN,
        JUMP,
        HIT
    }

    public float StartJumpPower;
    public float JumpPower;
    public bool IsGround = false;
    public bool IsJump;
    public bool IsHit = false;
    public UnityEvent OnHit;

    private Rigidbody2D mRigid;
    private Animator mAnim;
    private Sounder mSound;

    public void Init()
    {
        mRigid = GetComponent<Rigidbody2D>();
        mAnim = GetComponent<Animator>();
        mSound = GetComponent<Sounder>();
    }

    private void Start()
    {
        //mSound.PlaySFX(Sounder.SFX.Reset);
    }

    private void Update()
    {
        if(GameManager.Instance.IsRunning == false)
        {
            return;
        }

        // 점프(점프 파워)
        // UI 터치와 화면 터치 분리
        if (Input.GetMouseButtonDown(0) && IsGround && EventSystem.current.IsPointerOverGameObject() == false) // 기본 점프
        {
            mRigid.AddForce(Vector2.up * StartJumpPower, ForceMode2D.Impulse);
        }

        IsJump = Input.GetMouseButton(0);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsRunning == false)
        {
            return;
        }

        if (IsJump && !IsGround)
        {
            JumpPower = Mathf.Lerp(JumpPower, 0, 0.1f); // 롱 점프
            mRigid.AddForce(Vector2.up * JumpPower, ForceMode2D.Impulse);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        UpdateAnim(ECharacterState.JUMP);

        if(!IsHit) // 선인장에 부딪히면서 점프 소리가 나는 에러 처리
        {
            // 사운드
            mSound.PlaySFX(Sounder.SFX.Jump);
        }

        IsGround = false;
    }

    // 착지(물리 충돌 이벤트)
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 바닥에 최초로 닿았을 때만 애니메이션 변경
        if(!IsGround)
        {
            UpdateAnim(ECharacterState.RUN);

            // 사운드
            mSound.PlaySFX(Sounder.SFX.Land);

            JumpPower = 1;
        }

        IsGround = true;
    }

    // 장애물 터치(트리거 충돌 이벤트)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IsHit = true;
        mRigid.simulated = false;

        // 사운드
        mSound.PlaySFX(Sounder.SFX.Hit);

        UpdateAnim(ECharacterState.HIT);

        // GameOver(유니티 이벤트)
        OnHit.Invoke();
    }

    // 애니메이션
    private void UpdateAnim(ECharacterState state)
    {
        mAnim.SetInteger("State", (int)state);
    }
}
