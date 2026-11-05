using UnityEngine;
using System.Collections.Generic;

public class ZomBunnySpawner : MonoBehaviour, IGameRuleActor {
    public bool Active = true;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        if(Condition.Timer("3",gameObject)){
            Action.Spawn("ZomBunny", gameObject, "0", "0", "0", "0", "0", "0", scopeList);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Timer(3);Spawn(ZomBunny,this)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
