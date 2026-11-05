using UnityEngine;
using System.Collections.Generic;

public class LevelArt : MonoBehaviour, IGameRuleActor {
    public bool Active = true;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
    }
    public void EvalUpdate(){
    }
    void Start() {
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
