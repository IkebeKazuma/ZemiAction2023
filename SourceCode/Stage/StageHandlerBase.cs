using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageHandlerBase : MonoBehaviour {

    [SerializeField] StageManager _manager;
    public StageManager manager {
        get {
            if (_manager == null) {
                _manager = StageManager.Instance;
            }
            return _manager;
        }
    }

}