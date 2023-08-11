using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : PlayerBase {

    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator anim;
    [SerializeField] WallDetectionUtilities wDetection;
    [SerializeField] GroundChecker groundChecker;
    [SerializeField] PlayerScaleEffectController scaleEffectController;

    [Header("Move Settings")]
    [SerializeField] float normalMoveSpeed = 10f;
    [SerializeField] float airMoveSpeed = 3f;
    float moveSpeed;

    [Header("gravity Settings")]
    [SerializeField] float normalGravityScale = 35.0f;
    [SerializeField] float jumpGravityScale = 5.0f;
    float gravityScale;

    private bool isForceMoving;
    private Vector2 forceMoveVector;

    public void SetForceMoveState(bool newState) {
        isForceMoving= newState;
    }

    public void SetForceMoveVector(Vector2 newVector) {
        forceMoveVector = newVector;
    }

    private Vector2 moveVector;   // 移動ベクトル

    bool isRight = true;
    public Vector2 forwardDir { get; private set; }

    float verticalVelocity = 0f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpTimeout = 0.1f;
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private float jumpMaxTime = 1f;
    float _jumpTimeoutDelta;
    bool isJumping = false;
    float jumpElapsedTime = 0.0f;
    bool jumpedAndInAir = false;   // ジャンプ入力後、空中にいるか（地面から離れたか）
    bool releasedJumpInput=false;   // ジャンプしたか（長押し連続ジャンプ回避）

    // Start is called before the first frame update
    void Start() => Initialize();

    void Initialize() {
        isForceMoving = false;

        isJumping = false;
        gravityScale = normalGravityScale;

        groundChecker.onLand.AddListener(OnLand);
        groundChecker.onLeft.AddListener(OnLeft);
    }

    private void FixedUpdate() {
        if (isForceMoving) {
            ForceMove();
        } else {
            Move();
        }
    }

    // Update is called once per frame
    void Update() {
        groundChecker.CheckGrounded();

            moveVector = CalcMoveVector();

        if (PlayerInput.action) {
            PlayerInput.ActionInput(false);

            StageManager.Instance.PlayerStageAction();
        }

        /*if (PlayerInput.avoid) {
            PlayerInput.FireInput(false);

            if (!isAvoiding && canAvoid) {
                Vector2 dir = Vector2.zero;

                Vector2 _tmpInput = PlayerInput.move;
                if (Mathf.Abs(_tmpInput.y) <= 0.4f) {

                    avoidDirection = Vector2.Scale(_tmpInput, new Vector2(1, 0)).normalized;

                    StartAvoid();
                }
            }
        }*/

        JumpAndGravity();
    }

    void OnLand() {
        scaleEffectController.Land();
        return;
    }

    private void OnLeft() {
        jumpedAndInAir = true;
    }

    Vector2 CalcMoveVector() {
        moveSpeed = GetMoveSpeed();

        if (PlayerInput.HasMoveInput()) {
            float moveVal = PlayerInput.move.x;

            if (moveVal > 0.0f) {
                isRight = true;
            } else if (moveVal < -0.0f) {
                isRight = false;
            }

            var rot = transform.rotation;
            transform.rotation = Quaternion.Euler(rot.x, isRight ? 0 : 180, rot.z);
        }

        forwardDir = transform.right;
        Vector2 dir = forwardDir;
        if (groundChecker.isGrounded && groundChecker.OnSlope()) {
            dir = dir - Vector2.Dot(dir, groundChecker.groundNormal) * groundChecker.groundNormal;
        }

        // 壁判定
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
        rb.velocity = moveVector + new Vector2(0.0f, verticalVelocity) * Time.fixedDeltaTime;
    }

    void ForceMove() {
        Debug.Log("Force move");
        rb.velocity = forceMoveVector;
    }

    void JumpAndGravity() {

        if (!CheckJumpInput()) releasedJumpInput = true;

        if (groundChecker.isGrounded) {
            if (verticalVelocity < 0.0f) {
                verticalVelocity = -2f;
            }

            if (CheckJumpInput()) {   // ジャンプ入力監視

                if (releasedJumpInput == false) return;

                // ジャンプ処理
                if (!isJumping && _jumpTimeoutDelta <= 0.0f) {
                    StartJump();
                    //Debug.Log("StartJump");
                }
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f) {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        } else {
            // reset the jump timeout timer
            _jumpTimeoutDelta = jumpTimeout;

            // if (anim) anim.SetBool("Jump", false);

            if (isJumping) {
                jumpElapsedTime += Time.deltaTime;

                // 最大時間終了
                if (jumpElapsedTime >= jumpMaxTime) {
                    EndJump();
                } else
                // ジャンプ後地面を離れている && 入力がなくなった
                if (jumpedAndInAir && CheckJumpInput() == false) {
                    EndJump();
                }
            }
        }

        if (isJumping) {
            gravityScale = jumpGravityScale;
        } else {
            gravityScale = normalGravityScale;
        }

        // 接地している && スロープにいる && ジャンプ中ではない
        if (groundChecker.isGrounded && groundChecker.OnSlope() && !isJumping) {
            // 重力を無効化
            gravityScale = 0;
            verticalVelocity = 0f;
        }

        // apply gravity
        verticalVelocity += rb.mass * Physics.gravity.y * gravityScale * Time.deltaTime;
    }

    bool CheckJumpInput() => PlayerInput.jump;

    void StartJump() {
        jumpedAndInAir = false;
        gravityScale = jumpGravityScale;

        isJumping = true;
        releasedJumpInput = false;

        gravityScale = jumpGravityScale;

        scaleEffectController.JumpUp();

        PerformJump();
    }

    void PerformJump() {
        // the square root of H * -2 * G = how much velocity needed to reach desired height
        verticalVelocity = Mathf.Sqrt(jumpPower * -2f * (rb.mass * Physics.gravity.y * gravityScale));

        jumpElapsedTime = 0.0f;
    }

    void EndJump() {
        isJumping = false;
        gravityScale = normalGravityScale;
        jumpedAndInAir = false;
        //Debug.Log("キャンセル");
        // if (anim) anim.SetBool("Jump", false);
    }

    void ResetJumpStates() {
        isJumping = false;
        jumpElapsedTime = 0.0f;
        // reset the jump timeout timer
        _jumpTimeoutDelta = jumpTimeout;
    }

    public void ResetVerticalVelocity() {
        verticalVelocity= 0f;
    }
}