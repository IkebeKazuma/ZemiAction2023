using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class StageManager : SingletonMonoBehaviour<StageManager> {

    private List<RespawnTrigger> respawnTriggers = new List<RespawnTrigger>();

    private UnityEvent _onPlayerAction = new UnityEvent();
    public UnityEvent onPlayerAction => _onPlayerAction;

    private void Start() {
        //_onPlayerAction = new UnityEvent();

        respawnTriggers = FindObjectsByType<RespawnTrigger>(FindObjectsSortMode.None).Where(i => i.UseRandomActive == true).ToList();
    }

    public void RandomizeAllRespawnTriggerActive() {
        foreach (var trigger in respawnTriggers) {
            trigger.RandomizeActive();
        }
    }

    public void PlayerStageAction() {
        onPlayerAction?.Invoke();
    }
}