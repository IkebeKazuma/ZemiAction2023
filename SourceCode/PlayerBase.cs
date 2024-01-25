using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour {
    protected PlayerInputReceiver PlayerInput => GameManager.Instance.InputCtrl;
}