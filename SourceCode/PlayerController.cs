using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using PMP.UnityLib;
using KanKikuchi.AudioManager;

public class PlayerController : PlayerBase {

    [Header("References")]
    [SerializeField] Rigidbody2D rb2d;
    [SerializeField] CapsuleCollider2D capsuleCollider2D;
    [SerializeField] Animator anim;
    [SerializeField] WallDetectionUtilities wDetection;
    [SerializeField] GroundChecker groundChecker;
    [SerializeField] PlayerParticleController particleController;
    [SerializeField] PlayerScaleEffectController scaleEffectController;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] UnityEngine.Rendering.Universal.Light2D pointLight;
    [SerializeField] SpriteRenderer glowSpriteRenderer;

    [Header("Move Settings")]
    [SerializeField] float normalMoveSpeed = 10f;
    [SerializeField] float airMoveSpeed = 3f;
    float moveSpeed;

    [Header("gravity Settings")]
    [SerializeField] float normalGravityScale = 35.0f;
    [SerializeField] float jumpGravityScale = 5.0f;
    float gravityScale;

    // ��ԊǗ�
    public enum State {
        None = -1,
        Normal,
        Freeze
    }
    public State state { get; private set; }

    private Vector2 moveVector;   // �ړ��x�N�g��

    public void ChangeState(State newState) {
        if (state != newState) state = newState;
    }

    bool isRight = true;
    public Vector2 forwardDir { get; private set; }

    public float verticalVelocity{ get; private set; } = 0f;

    // �L�����������Ă��邩
    bool isVisible = false;

    [Header("Jump Settings")]
    [SerializeField] private float jumpTimeout = 0.1f;
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private float jumpMaxTime = 1f;
    float _jumpTimeoutDelta;
    public bool isJumping { get; private set; } = false;
    float jumpElapsedTime = 0.0f;
    //bool releasedJumpInput = false;   // �W�����v�������i�������A���W�����v����j
    [SerializeField] float headHitRadius = 0.1f;
    [SerializeField] LayerMask headHitLayerMask = ~0;
    bool tmpCollidedHeadCast = false;
    enum JumpInputState {
        None = -1,
        Wait,
        Pressed,
        Pressing,
        Released
    }
    JumpInputState jumpInputState;
    // �W�����v�o�b�t�@
    bool requestJumpBuffering = false;
    [SerializeField] float jumpBufferingAcceptTime = 0.05f;
    float jumpBufferingStartTime = 0.0f;
    // �R���[�e�^�C��
    bool requestCoyoteTime = false;
    [SerializeField] float coyoteTimeAcceptTime = 0.05f;
    float coyoteTimeTimeoutDelta = 0.0f;
    bool rejectCoyoteTime = false;   // �W�����v����̃R���[�e�^�C���𐧌�����

    //[Header("���G")]
    float blinkStartTime = 0.0f;
    float blinkDuration = 1f;
    float blinkInterval = 0.05f;
    bool isBlinking;
    float blinkedElapsedTime = 0.0f;

    Vector2 prevPosition;
    float maxFallVelocityY = 0.0f;

    public void Initialize() {
        FreezeMovement();

        isJumping = false;
        gravityScale = normalGravityScale;

        groundChecker.onLand.AddListener(OnLand);
        groundChecker.onLeft.AddListener(OnLeft);

        isBlinking = false;

        prevPosition = transform.position;
    }

    private void FixedUpdate() {
        groundChecker.CheckGrounded();
        moveVector = CalcMoveVector();
        switch (state) {
            case State.Normal:
                Move();
                break;
            case State.Freeze:
                ResetMoveVeclocity();
                ResetVerticalVelocity();
                break;
        }
    }

    void Update() {
        // ��]
        Rotate();

        if (anim) anim.SetBool("Run", state != State.Freeze && Mathf.Abs(PlayerInput.move.x) > 0.01f);   

        if (PlayerInput.action) {
            PlayerInput.ActionInput(false);

            if (state == State.Freeze) return;

            // StageManager.Instance.OnPlayerStageAction();
            if (StageManager.Instance.stageType == StageManager.StageType.FlashStage && groundChecker.isGrounded)
                StageManager.Instance.flashStageController.Flash();
        }

        if (isBlinking) {
            float elapsedTime = Time.time - blinkStartTime;
            if (elapsedTime >= blinkDuration) {
                EndVisibleBlink();
            } else {
                if (elapsedTime - blinkedElapsedTime >= blinkInterval) {
                    blinkedElapsedTime = elapsedTime;
                    SetVisibleState(!isVisible);
                }
            }
        }

        JumpAndGravity();

        if (maxFallVelocityY == 0.0f || maxFallVelocityY > rb2d.velocity.y) maxFallVelocityY = rb2d.velocity.y;

        particleController.SetPlayStateRun(Mathf.Abs(PlayerInput.move.x) > 0.01f && groundChecker.isGrounded, rb2d.velocity.x);

        if (!GameManager.Instance.isRespawning && state != State.Freeze) {
            anim.SetFloat("VelocityY", rb2d.velocity.y.RoundOffToNDecimalPoint(2));
            anim.SetBool("IsGrounded", groundChecker.isGrounded);
            if (PlayerInput.HasMoveInput()) anim.SetFloat("HorizontalRaw", PlayerInput.actualMove.x);
        }
    }

    void OnLand() {
        if (isJumping || verticalVelocity > 0) return;
        if (GameManager.Instance.isRespawning) return;

        if (GameManager.Instance.isPlaying && maxFallVelocityY <= -5.0f) {
            scaleEffectController.Land();
            particleController.PlayLandEff(maxFallVelocityY);
            SEManager.Instance.Play(StageManager.Instance.stageType == StageManager.StageType.FlashStage ? SEPath.LAND_FLASH_STAGE : SEPath.LAND_REPEAT_STAGE);
        }
    }

    private void OnLeft() {
        maxFallVelocityY = 0.0f;
    }

    public void SetVisibleState(bool newState) {
        spriteRenderer.gameObject.SetActive(newState);
        pointLight.gameObject.SetActive(newState);
        glowSpriteRenderer.gameObject.SetActive(newState);
        isVisible = newState;
    }

    public void StartVisibleBlink(float duration, float interval) {
        blinkStartTime = Time.time;
        blinkDuration = duration;
        blinkInterval = interval;

        blinkedElapsedTime = 0.0f;

        isBlinking = true;
    }

    void EndVisibleBlink() {
        isBlinking = false;
        SetVisibleState(true);
    }

    Vector2 CalcMoveVector() {
        moveSpeed = GetMoveSpeed();

        forwardDir = Vector2.right * PlayerInput.actualMove.normalized.x.Extremize();
        Vector2 dir = forwardDir;
        if (groundChecker.isGrounded && groundChecker.OnSlope()) {
            //dir = dir - Vector2.Dot(dir, groundChecker.groundNormal) * groundChecker.groundNormal;
        }

        // �ǔ���
        if (wDetection.UpdateWallDetectionState(dir)) {
            //Debug.Log(wDetection.GetDistance());
            if (wDetection.CalcDistanceError(wDetection.GetDistance()) <= wDetection.distanceErrorTolerance) {
                //h = new Vector2();
            }
        }

        return dir.normalized * (moveSpeed * Mathf.Abs(PlayerInput.move.x));
    }

    float GetMoveSpeed() {
        if (groundChecker.isGrounded) {
            return normalMoveSpeed;
        } else {
            return airMoveSpeed;
        }
    }

    void Move() {
        rb2d.velocity = moveVector + new Vector2(0.0f, verticalVelocity) * Time.fixedDeltaTime;
    }

    void Rotate() {
        if (!PlayerInput.HasMoveInput()) return;
        if (state == State.Freeze) return;
        if (GameManager.Instance.isRespawning) return;

        float moveVal = PlayerInput.move.x;

        if (moveVal > 0.0f) {
            isRight = true;
        } else if (moveVal < -0.0f) {
            isRight = false;
        }

        var rot = transform.rotation;
        // transform.rotation = Quaternion.Euler(rot.x, isRight ? 0 : 180, rot.z);
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void ForceLookRight() {
        var rot = transform.rotation;
        transform.rotation = Quaternion.Euler(rot.x, 0, rot.z);
        anim.SetFloat("HorizontalRaw", 1);
    }

    public void ResetMoveVeclocity() { rb2d.velocity = new Vector2(); }

    void JumpAndGravity() {
        groundChecker.CheckGrounded();

        bool tempJumpPressed = CheckJumpInput();

        //if (!CheckJumpInput()) releasedJumpInput = true;

        jumpInputState = GetJumpInputState(tempJumpPressed);

        // Debug.Log(jumpInputState);

        if (jumpInputState == JumpInputState.Pressed) {
            //Debug.Log("�W�����v");

            if (state == State.Freeze) return;

            //if (releasedJumpInput == false) return;

            // ���͂��������ꍇ�o�b�t�@����J�n
            requestJumpBuffering = true;
            jumpBufferingStartTime = Time.time;

            requestCoyoteTime = true;

            // �W�����v���s
            TryJump();
        }

        if (isJumping || rb2d.velocity.y.RoundDownToNDecimalPoint(1) > 0) groundChecker.OverrideGroundedState(false);

        if (groundChecker.isGrounded) {
            if (verticalVelocity < 0.0f) {
                verticalVelocity = -2f;
            }

            isJumping = false;
            anim.SetBool("Jump", false);

            // �W�����v�o�b�t�@�m�F
            if (requestJumpBuffering && (Time.time - jumpBufferingStartTime) <= jumpBufferingAcceptTime) {
                // �W�����v���s
                if (TryJump()) Debug.Log("�W�����v�o�b�t�@");
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f) {
                _jumpTimeoutDelta -= Time.deltaTime;
            }

            // �R���[�e�^�C�����Z�b�g
            coyoteTimeTimeoutDelta = coyoteTimeAcceptTime;
            rejectCoyoteTime = false;
        } else {
            // reset the jump timeout timer
            _jumpTimeoutDelta = jumpTimeout;

            // if (anim) anim.SetBool("Jump", false);

            if (isJumping) {
                jumpElapsedTime += Time.deltaTime;

                // ���� overlap
                var headHits = Physics2D.OverlapCircleAll((Vector2)transform.position + new Vector2(0, capsuleCollider2D.size.y), headHitRadius, headHitLayerMask);
                bool collidedHeadCast = headHits != null && headHits.Length > 0;

                if (tmpCollidedHeadCast == false) {
                    if (collidedHeadCast) {
                        tmpCollidedHeadCast = true;
                    }
                } else {
                    if (collidedHeadCast == false) {
                        EndJump();
                        verticalVelocity = -2f;
                    }
                }

                // �ő厞�ԏI��
                if (jumpElapsedTime >= jumpMaxTime) {
                    EndJump();
                } else
                // ���͂��Ȃ��Ȃ���
                if (jumpInputState == JumpInputState.Released) {
                    EndJump();
                } else
                // ���Ԃ���
                if (collidedHeadCast && jumpElapsedTime <= 0.03f) {
                    EndJump();
                    verticalVelocity = -2f;
                }
            } else {
                if (coyoteTimeTimeoutDelta >= 0.0f)
                    coyoteTimeTimeoutDelta -= Time.deltaTime;

                if (!rejectCoyoteTime && (requestCoyoteTime && coyoteTimeTimeoutDelta > 0.0f)) {
                    _jumpTimeoutDelta = 0.0f;
                    if (TryJump()) Debug.Log("�R���[�e�^�C��");
                }
            }
        }

        if (isJumping) {
            gravityScale = jumpGravityScale;
        } else {
            gravityScale = normalGravityScale;
        }

        // �ڒn���Ă��� && �X���[�v�ɂ��� && �W�����v���ł͂Ȃ�
        if (groundChecker.isGrounded && groundChecker.OnSlope() && !isJumping) {
            // �d�͂𖳌���
            gravityScale = 0;
            verticalVelocity = 0f;
        }

        // apply gravity
        verticalVelocity += rb2d.mass * Physics.gravity.y * gravityScale * Time.deltaTime;
    }

    JumpInputState GetJumpInputState(bool tempJumpPressed) {
        if (tempJumpPressed) {
            if (jumpInputState == JumpInputState.Wait) {
                return JumpInputState.Pressed;
            } else if (jumpInputState == JumpInputState.Pressed) {
                return JumpInputState.Pressing;
            }
        } else {
            if (jumpInputState == JumpInputState.Pressed || jumpInputState == JumpInputState.Pressing) {
                return JumpInputState.Released;
            } else if (jumpInputState == JumpInputState.Released) {
                return JumpInputState.Wait;
            }
        }

        return jumpInputState;
    }

    bool CheckJumpInput() => PlayerInput.jump;

    bool TryJump() {// �W�����v����

        // ���� overlap
        var headHits = Physics2D.OverlapCircleAll((Vector2)transform.position + new Vector2(0, capsuleCollider2D.size.y), headHitRadius, headHitLayerMask);
        bool collidedHeadCast = headHits != null && headHits.Length > 0;

        if (collidedHeadCast) return false;

        if (!isJumping && _jumpTimeoutDelta <= 0.0f) {
            StartJump();
            return true;
        } else
            return false;
    }

    void StartJump() {
        groundChecker.OverrideGroundedState(false);

        gravityScale = jumpGravityScale;

        isJumping = true;
        //releasedJumpInput = false;
        tmpCollidedHeadCast = false;

        requestJumpBuffering = false;
        requestCoyoteTime = false;

        scaleEffectController.JumpUp();

        anim.SetBool("Jump", true);
        anim.Play("Jump up");

        // SEManager.Instance.Play(SEPath.JUMP);

        PerformJump();
    }

    void PerformJump() {
        // the square root of H * -2 * G = how much velocity needed to reach desired height
        verticalVelocity = Mathf.Sqrt(jumpPower * -2f * (rb2d.mass * Physics.gravity.y * gravityScale));

        jumpElapsedTime = 0.0f;
    }

    void EndJump() {
        isJumping = false;
        gravityScale = normalGravityScale;

        requestJumpBuffering = false;
        requestCoyoteTime = false;

        rejectCoyoteTime = true;

        anim.SetBool("Jump", false);
    }

    public void ResetVerticalVelocity() {
        verticalVelocity = 0f;
    }

    public void FreezeMovement() {
        ChangeState(State.Freeze);
        ResetMoveVeclocity();
        ResetVerticalVelocity();
    }

    public void UnfreezeMovement() {
        groundChecker.OverrideGroundedState(true, true);
        GameManager.Instance.InputCtrl.PlayerInput.SwitchCurrentActionMap("Player");
        ChangeState(State.Normal);
    }

    public void FreezeMovementTemp(float duration) {
        FreezeMovement();
        DOVirtual.DelayedCall(duration, () => { UnfreezeMovement(); });
    }

    public async UniTask Die(CancellationToken ct, float waitTime = 0) {
        await UniTask.Yield(ct);

        // ���S�G�t�F�N�g���Đ�
        EffectController.Instance.PlayPlayerDieEff((Vector2)transform.position + new Vector2(0, 0.5f));
        // SE
        SEManager.Instance.Play(SEPath.DEATH);
        // �v���C���[���\��
        SetVisibleState(false);
        // �v���C���[�̓������~
        FreezeMovement();

        if (waitTime > 0) await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: ct);

        await UniTask.Yield(ct);
    }

    /// <summary>
    /// ���X�|�[�����W�Ɉړ�������
    /// </summary>
    public void MoveToRespawnPoint() {
        transform.position = StageManager.Instance.GetSpawnPointFromController();
    }

    public async UniTask RecoverFromDeath(CancellationToken ct, float bindTime = 0) {
        await UniTask.Yield(ct);

        // �E�Ɍ�����
        ForceLookRight();
        // �v���C���[��\��
        SetVisibleState(true);

        SEManager.Instance.Play(SEPath.REVIVE);

        if (bindTime > 0) {
            StartVisibleBlink(bindTime, 0.05f);
            await UniTask.Delay(TimeSpan.FromSeconds(bindTime), cancellationToken: ct);
        }

        await UniTask.Yield(ct);

        UnfreezeMovement();
    }

    public void ResetPlayerStates() {
        // �E�Ɍ�����
        ForceLookRight();
        // �v���C���[��\��
        SetVisibleState(true);

        MoveToRespawnPoint();

        // �v���C���[�̓������~
        FreezeMovement();
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(0, capsuleCollider2D.size.y), headHitRadius);
    }
}