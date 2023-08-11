using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PMP.UnityLib;

public class WallDetectionUtilities : MonoBehaviour {

    [Header("References")]
    [SerializeField] CapsuleCollider2D capsuleCollider;

    [Header("Settings")]
    public float range = 1.5f;
    public float capsuleRadiusMargin = 0.025f;
    public float capsuleHeightMargin = 0.01f;
    public float distanceErrorTolerance = 0.1f;

    [Header("Cast Settings")]
    [SerializeField] LayerMask layerMask;

    Vector2 castDirection;
    RaycastHit2D hitInfo;
    public RaycastHit2D GetHitInfo() => hitInfo;
    Vector2 capsuleCenter => (Vector2)transform.position + capsuleCollider.offset;
    Vector2 capsuleSize => new Vector2((capsuleCollider.size.x - capsuleRadiusMargin) / 2, capsuleCollider.size.y - capsuleHeightMargin);

    public bool collided => hitInfo.collider != null ? true : false;

    public float GetDistance() {
        if (collided) {
            float distance = hitInfo.distance - (capsuleSize.x / 2);            
            return distance.RoundDownToNDecimalPoint(2);
        } else
            return -1;
    }

    public bool UpdateWallDetectionState(Vector2 direction) {
        castDirection = direction;
        range = castDirection.magnitude;
        hitInfo = Physics2D.CapsuleCast(
            origin: capsuleCenter,
            size: capsuleSize,
            angle: 0,
            capsuleDirection: CapsuleDirection2D.Vertical,
            direction: castDirection.normalized,
            distance: range,
            layerMask: layerMask
        );
        return collided;
    }

    public float CalcDistanceError(float distance) {
        if (collided) {
            return Mathf.Abs(distance - capsuleCollider.size.x);
        } else
            return -1;
    }

    private void OnDrawGizmos() {
#if UNITY_EDITOR

        var defaultCol = Gizmos.color;

        //Gizmos.color = Color.white;
        //Gizmos.DrawMesh(CapsuleMeshDrawer.GetMesh(capsuleRadius, capsuleHeight), capsuleCenter);

        float alpha = 0.25f;

        {
            Gizmos.color = new Color(1, 1, 1, alpha);

            Vector2 from = capsuleCenter;
            Vector2 to = capsuleCenter + (castDirection * range);
            Gizmos.DrawLine(from, to);
            Vector2 targetPos = from + (castDirection * range);
            //Gizmos.DrawMesh(CapsuleMeshDrawer.GetMesh(capsuleSize.x, capsuleSize.y), targetPos);
            Utils.GizmosExtensions.DrawWireCapsule(targetPos, capsuleSize.x, capsuleSize.y);
        }


        if (collided) {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, alpha);
            // 衝突地点のカプセル
            var hitPointCapCenter = capsuleCenter + (castDirection * GetDistance());
            //Gizmos.DrawMesh(CapsuleMeshDrawer.GetMesh(capsuleSize.x, capsuleSize.y), hitPointCapCenter);
            Utils.GizmosExtensions.DrawWireCapsule(hitPointCapCenter, capsuleSize.x, capsuleSize.y);
        }

        if (collided) {
            Gizmos.color = Color.red;
            //Gizmos.DrawLine(transform.position, (Vector2)transform.position + wallSlideVector);
            Gizmos.DrawSphere(hitInfo.point, 0.1f);
            Gizmos.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal);
        }

        Gizmos.color = defaultCol;
#endif
    }

    public static class CapsuleMeshDrawer {

        private static  Vector2Int _divide = new Vector2Int(10, 10);

        public static Mesh GetMesh(float radius, float height) {
            int divideH = _divide.x;
            int divideV = _divide.y;

            height -= radius*2f;

            var data = CreateCapsule(divideH, divideV, height, radius);
            var mesh = new Mesh();
            mesh.SetVertices(data.vertices);
            mesh.SetIndices(data.indices, MeshTopology.Triangles, 0);
            mesh.RecalculateNormals();

            return mesh;
        }

        struct MeshData {
            public Vector3[] vertices;
            public int[] indices;
        }

        /// <summary>  
        /// カプセルメッシュデータを作成  
        /// </summary>  
        static MeshData CreateCapsule(int divideH, int divideV, float height, float radius) {
            divideH = divideH < 4 ? 4 : divideH;
            divideV = divideV < 4 ? 4 : divideV;
            radius = radius <= 0 ? 0.001f : radius;

            // 偶数のみ有効  
            if (divideV % 2 != 0) {
                divideV++;
            }

            int cnt = 0;

            // =============================  
            // 頂点座標作成  
            // =============================  

            int vertCount = divideH * divideV + 2;
            var vertices = new Vector3[vertCount];

            // 中心角  
            float centerEulerRadianH = 2f * Mathf.PI / (float)divideH;
            float centerEulerRadianV = 2f * Mathf.PI / (float)divideV;

            float offsetHeight = height * 0.5f;

            // 天面  
            vertices[cnt++] = new Vector3(0, radius + offsetHeight, 0);

            // カプセル上部  
            for (int vv = 0; vv < divideV / 2; vv++) {
                var vRadian = (float)(vv + 1) * centerEulerRadianV / 2f;

                // 1辺の長さ  
                var tmpLen = Mathf.Abs(Mathf.Sin(vRadian) * radius);

                var y = Mathf.Cos(vRadian) * radius;
                for (int vh = 0; vh < divideH; vh++) {
                    var pos = new Vector3(
                        tmpLen * Mathf.Sin((float)vh * centerEulerRadianH),
                        y + offsetHeight,
                        tmpLen * Mathf.Cos((float)vh * centerEulerRadianH)
                    );
                    // サイズ反映  
                    vertices[cnt++] = pos;
                }
            }

            // カプセル下部  
            int offset = divideV / 2;
            for (int vv = 0; vv < divideV / 2; vv++) {
                var yRadian = (float)(vv + offset) * centerEulerRadianV / 2f;

                // 1辺の長さ  
                var tmpLen = Mathf.Abs(Mathf.Sin(yRadian) * radius);

                var y = Mathf.Cos(yRadian) * radius;
                for (int vh = 0; vh < divideH; vh++) {
                    var pos = new Vector3(
                        tmpLen * Mathf.Sin((float)vh * centerEulerRadianH),
                        y - offsetHeight,
                        tmpLen * Mathf.Cos((float)vh * centerEulerRadianH)
                    );
                    // サイズ反映  
                    vertices[cnt++] = pos;
                }
            }

            // 底面  
            vertices[cnt] = new Vector3(0, -radius - offsetHeight, 0);

            // =============================  
            // インデックス配列作成  
            // =============================  

            int topAndBottomTriCount = divideH * 2;
            // 側面三角形の数  
            int aspectTriCount = divideH * (divideV - 2 + 1) * 2;

            int[] indices = new int[(topAndBottomTriCount + aspectTriCount) * 3];

            //天面  
            int offsetIndex = 0;
            cnt = 0;
            for (int i = 0; i < divideH * 3; i++) {
                if (i % 3 == 0) {
                    indices[cnt++] = 0;
                } else if (i % 3 == 1) {
                    indices[cnt++] = 1 + offsetIndex;
                } else if (i % 3 == 2) {
                    var index = 2 + offsetIndex++;
                    // 蓋をする  
                    index = index > divideH ? indices[1] : index;
                    indices[cnt++] = index;
                }
            }

            // 側面Index  

            /* 頂点を繋ぐイメージ  
             * 1 - 2  
             * |   |  
             * 0 - 3  
             *  
             * 0, 1, 2  
             * 0, 2, 3  
             *  
             * 注意 : 1周した時にClampするのを忘れないように。  
             */

            // 開始Index番号  
            int startIndex = indices[1];

            // 天面、底面を除いたカプセルIndex要素数  
            int sideIndexLen = aspectTriCount * 3;

            int lap1stIndex = 0;

            int lap2ndIndex = 0;

            // 一周したときのindex数  
            int lapDiv = divideH * 2 * 3;

            int createSquareFaceCount = 0;

            for (int i = 0; i < sideIndexLen; i++) {
                // 一周の頂点数を超えたら更新(初回も含む)  
                if (i % lapDiv == 0) {
                    lap1stIndex = startIndex;
                    lap2ndIndex = startIndex + divideH;
                    createSquareFaceCount++;
                }

                if (i % 6 == 0 || i % 6 == 3) {
                    indices[cnt++] = startIndex;
                } else if (i % 6 == 1) {
                    indices[cnt++] = startIndex + divideH;
                } else if (i % 6 == 2 || i % 6 == 4) {
                    if (i > 0 &&
                        (i % (lapDiv * createSquareFaceCount - 2) == 0 ||
                         i % (lapDiv * createSquareFaceCount - 4) == 0)
                    ) {
                        // 1周したときのClamp処理  
                        // 周回ポリゴンの最後から2番目のIndex  
                        indices[cnt++] = lap2ndIndex;
                    } else {
                        indices[cnt++] = startIndex + divideH + 1;
                    }
                } else if (i % 6 == 5) {
                    if (i > 0 && i % (lapDiv * createSquareFaceCount - 1) == 0) {
                        // 1周したときのClamp処理  
                        // 周回ポリゴンの最後のIndex  
                        indices[cnt++] = lap1stIndex;
                    } else {
                        indices[cnt++] = startIndex + 1;
                    }

                    // 開始Indexの更新  
                    startIndex++;
                } else {
                    Debug.LogError("Invalid : " + i);
                }
            }


            // 底面Index  
            offsetIndex = vertices.Length - 1 - divideH;
            lap1stIndex = offsetIndex;
            var finalIndex = vertices.Length - 1;
            int len = divideH * 3;

            for (int i = len - 1; i >= 0; i--) {
                if (i % 3 == 0) {
                    // 底面の先頂点  
                    indices[cnt++] = finalIndex;
                    offsetIndex++;
                } else if (i % 3 == 1) {
                    indices[cnt++] = offsetIndex;
                } else if (i % 3 == 2) {
                    var value = 1 + offsetIndex;
                    if (value >= vertices.Length - 1) {
                        value = lap1stIndex;
                    }

                    indices[cnt++] = value;
                }
            }


            return new MeshData() {
                indices = indices,
                vertices = vertices
            };
        }
    }
}