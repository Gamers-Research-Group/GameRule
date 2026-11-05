using UnityEngine;
using System.Collections.Generic;

public class ZomBearSpawner : MonoBehaviour, IGameRuleActor {
    public bool Active = true;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        if(Condition.Timer("2",gameObject)){
            Action.Spawn("ZomBear", gameObject, "0", "0", "0", "0", "0", "0", scopeList);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Timer(2);Spawn(ZomBear,this)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
