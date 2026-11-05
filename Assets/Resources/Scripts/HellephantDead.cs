using UnityEngine;
using System.Collections.Generic;

public class HellephantDead : MonoBehaviour, IGameRuleActor {
    public bool Active = false;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.Animate("Death",gameObject);
            Action.PlaySound("HellephantDeath",gameObject);
            Action.PlayParticles("DeathParticles",gameObject);
        }
        if(Condition.Timer("1",gameObject)){
            Action.Delete(gameObject);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Animate(Death);PlaySound(HellephantDeath);PlayParticles(DeathParticles);Timer(1);Delete(this)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
