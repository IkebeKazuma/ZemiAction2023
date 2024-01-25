using UnityEngine;
using System;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

    private static T instance;
    public static T Instance {
        get {
            if (instance == null) {
                Type t = typeof(T);
                instance = (T)FindObjectOfType(t);

                if (instance == null)
                    Debug.LogWarning(t + " をアタッチしているGameObjectはありません。");
            }

            return instance;
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    void Initialize() {
        // インスタンスチェック
        CheckInstance();
    }

    protected bool CheckInstance() {
        if (instance == null) {
            instance = this as T;
            return true;
        } else if (Instance == this) {
            return true;
        }
        Destroy(this);
        return false;
    }
}