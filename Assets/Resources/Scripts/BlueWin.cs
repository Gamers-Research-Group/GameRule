using UnityEngine;
using System.Collections.Generic;

public class BlueWin : MonoBehaviour, IGameRuleActor {
    public bool Active = false;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        if(Condition.Timer("1",gameObject)){
            Action.LoadScene();
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Timer(1);LoadScene()");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
