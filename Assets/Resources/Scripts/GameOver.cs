using UnityEngine;
using System.Collections.Generic;

public class GameOver : MonoBehaviour, IGameRuleActor {
    public bool Active = false;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        if(Condition.Timer("2",gameObject)){
            Action.Animate("GameOver",gameObject);
        }
        if(Condition.Timer("3",gameObject)){
            Action.Edit("#Score","0",scopeList);
            Action.LoadScene();
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Timer(2);Animate(GameOver);Timer(3);Edit(#Score,0);LoadScene()");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
