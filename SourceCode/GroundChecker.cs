using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Image;

public class GroundChecker : MonoBehaviour {

    // 接地しているか
    public bool isGrounded { get; private set; }

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
    }

    public bool CheckGrounded() {
        bool result = false;

        groundNormal = Vector2.up;
        groundAngle = 0;
        castHit = false;

        CheckCast();

        if (hitInfo2D.collider && Mathf.Abs(hitInfo2D.point.y - footOrigin.position.y) <= reactiveRange) {
            if (OnSlope()) {
                if (groundAngle <= slopeAngleLimit) {
                    // 斜面
                    //Debug.Log("斜面");
                    result = true;
                }
            } else {
                // 平面
                //Debug.Log("平面");
                result = true;
            }
        }

        if (isGrounded != result) {
            // 接地ステート切り替わり
            if (result == true) {
                onLand?.Invoke();
            } else {
                onLeft?.Invoke();
            }
        }

        return isGrounded = result;
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