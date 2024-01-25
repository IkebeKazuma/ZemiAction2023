using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AllTilesReplacer : EditorWindow {

    RuleTile tile;
    GameObject respawnTriggerPrefab;
    GameObject spikePrefab;

    string log;

    [MenuItem("Tools/All Tiles Relpacer")]
    [MenuItem("Window/All Tiles Relpacer")]
    static void Open() {
        AllTilesReplacer window = GetWindow<AllTilesReplacer>();
        window.title = "All Tiles Replacer";
        window.Show();
    }

    private void OnGUI() {
        tile = EditorGUILayout.ObjectField("Tile", tile, typeof(RuleTile), false) as RuleTile;

        if (GUILayout.Button("GO")) {
            if (!tile) return;

            var selectedObjs = Selection.gameObjects;
            if (selectedObjs == null) return;

            foreach (var selected in selectedObjs) {
                Tilemap tilemap = selected.GetComponent<Tilemap>();
                if (!tilemap) return;

                log += tilemap.name + "\n";

                List<Vector3> tileWorldLocations = new List<Vector3>();

                foreach (var pos in tilemap.cellBounds.allPositionsWithin) {
                    Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
                    Vector3 place = tilemap.CellToWorld(localPlace);
                    if (tilemap.HasTile(localPlace)) {
                        tileWorldLocations.Add(place);
                    }
                }

                foreach (var pos in tileWorldLocations) {
                    tilemap.SetTile(tilemap.WorldToCell(pos), tile);
                }

                var rts = tilemap.transform.GetComponentsInChildren<Transform>().Where(t => t.name.Contains("Respawn Trigger")).ToList();

                if (rts != null) {
                    foreach (var rt in rts) {
                        if (!rt.name.Contains("Respawn Trigger")) return;
                        if (!respawnTriggerPrefab) return;
                        if (!spikePrefab) return;

                        var posLS = rt.transform.localPosition;

                        var prefab = PrefabUtility.InstantiatePrefab(respawnTriggerPrefab, selected.transform) as GameObject;
                        prefab.transform.localPosition = posLS;
                        var spike = PrefabUtility.InstantiatePrefab(spikePrefab, prefab.transform) as GameObject;
                        spike.transform.localPosition = new Vector3Int();

                        DestroyImmediate(rt.gameObject);
                    }
                }

                EditorUtility.SetDirty(tilemap);
                AssetDatabase.SaveAssetIfDirty(tilemap);
            }
        }

        respawnTriggerPrefab = EditorGUILayout.ObjectField("Respawn Trigger Prefab", respawnTriggerPrefab, typeof(GameObject), false) as GameObject;
        spikePrefab = EditorGUILayout.ObjectField("Spike Prefab", spikePrefab, typeof(GameObject), false) as GameObject;

        if (GUILayout.Button("Replace")) {
            var selectedObjs = Selection.gameObjects;
            if (selectedObjs == null) return;

            foreach (var selected in selectedObjs) {
                if (!selected.name.Contains("Respawn Trigger")) return;

                var posLS = selected.transform.localPosition;

                if (!respawnTriggerPrefab) return;
                if (!spikePrefab) return;

                var prefab = PrefabUtility.InstantiatePrefab(respawnTriggerPrefab, selected.transform.parent) as GameObject;
                prefab.transform.localPosition = posLS;
                var spike = PrefabUtility.InstantiatePrefab(spikePrefab, prefab.transform) as GameObject;
                spike.transform.localPosition = new Vector3Int();

                EditorUtility.SetDirty(selected.transform.root);

                DestroyImmediate(selected);
            }
        }

        GUILayout.Label(log);
    }
}