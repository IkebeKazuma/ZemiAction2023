using PMP.UnityLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GroundChecker : MonoBehaviour {

    // 接地しているか
    public bool isGrounded { get; private set; }
    public void OverrideGroundedState(bool newState, bool withPrevState = false) {
        isGrounded = newState;
        if (withPrevState) prevGroundedState = newState;
    }

    bool prevGroundedState;

    [Header("References")]
    [SerializeField] PlayerController playerCtrl;
    [SerializeField] Rigidbody2D rb2d;

    [Header("Settings")]
    [SerializeField] Transform castOrigin;
    Vector2 origin => castOrigin.position;
    Vector2 direction => -castOrigin.up;
    [SerializeField] float radius = 0.5f;
    [SerializeField] float range = 0.5f;
    [SerializeField] LayerMask layerMask = ~0;
    [SerializeField] float slopeAngleLimit = 50f;
    [SerializeField] Transform footOrigin;
    [SerializeField] float reactiveRange = 0.1f;

    public RaycastHit2D hitInfo2D { get; private set; }
    public bool castHit { get; private set; }
    public Vector2 groundNormal { get; private set; }
    public float groundAngle { get; private set; }
    public Vector2 projectOnPlane { get; private set; }
    public Transform GetFootOrigin () { return footOrigin; }

    public UnityEvent onLand { get; private set; }
    public UnityEvent onLeft { get; private set; }

    private void OnEnable() {
        onLand = new UnityEvent();
        onLeft = new UnityEvent();

        prevGroundedState = isGrounded = false;
    }

    public bool CheckGrounded() {
        bool result = false;

        groundNormal = Vector2.up;
        groundAngle = 0;
        castHit = false;

        CheckCast();

        if (playerCtrl.isJumping) return isGrounded = prevGroundedState = false;
        if (playerCtrl.verticalVelocity > 0) return isGrounded = prevGroundedState = false;

        if (hitInfo2D.collider && Mathf.Max(footOrigin.position.y - hitInfo2D.point.y, 0.0f) <= reactiveRange) {
            if (rb2d.velocity.y.RoundOffToNDecimalPoint(2) <= 0.0f) { result = true; }
        }

        isGrounded = result;

        // 接地ステート切り替わり
        if (isGrounded != prevGroundedState) {
            if (isGrounded) {
                hitInfo2D = Physics2D.CircleCast(origin, 0.1f, direction, 0.5f, layerMask);
                if (hitInfo2D.collider) {
                    onLand?.Invoke();
                }
            } else
                onLeft?.Invoke();

            prevGroundedState = isGrounded;
        }

        return isGrounded;
    }

    RaycastHit2D CheckCast() {
        hitInfo2D = Physics2D.CircleCast(origin, radius, direction, range, layerMask);
        if (hitInfo2D.collider) {
            groundNormal = hitInfo2D.normal;
            groundAngle = Vector2.Angle(Vector2.up, groundNormal);
            castHit = true;
        }
        return hitInfo2D;
    }

    /// <summary>
    /// 斜面にいるか
    /// </summary>
    public bool OnSlope() => (groundNormal != Vector2.up && groundAngle > 0) ? true : false;

    private void OnDrawGizmos() {
        float distance = range;

        // Gizmos.color = Color.white;
        Gizmos.DrawSphere(origin, 0.05f);
        Gizmos.DrawWireSphere(origin + (direction * range), radius);

        if (CheckCast().collider) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitInfo2D.point, 0.05f);
            Gizmos.DrawLine(hitInfo2D.point, hitInfo2D.point + (hitInfo2D.normal * 0.5f));

            Gizmos.DrawLine(footOrigin.position + (-castOrigin.right * 0.5f), footOrigin.position + (castOrigin.right * 0.5f));
            Gizmos.DrawLine((footOrigin.position + ((Vector3)direction * reactiveRange)) + (-castOrigin.right * 0.5f), (footOrigin.position + ((Vector3)direction * reactiveRange)) + (castOrigin.right * 0.5f));

            Gizmos.color = Color.green;
            distance = hitInfo2D.distance;
        } else {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawWireSphere(origin + (direction * distance), radius);
    }
}