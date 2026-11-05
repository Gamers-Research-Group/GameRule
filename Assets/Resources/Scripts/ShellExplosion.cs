using UnityEngine;
using System.Collections.Generic;

public class ShellExplosion : MonoBehaviour, IGameRuleActor {
    public bool Active = false;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.PlayParticles("ShellExplosion",gameObject);
            Action.PlaySound("ShellExplosion",gameObject);
        }
        if(Condition.Timer("0.5",gameObject)){
            Action.Delete(gameObject);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"PlayParticles(ShellExplosion);PlaySound(ShellExplosion);Timer(0.5);Delete(this)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
