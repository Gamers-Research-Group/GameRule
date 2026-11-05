using UnityEngine;
using System.Collections.Generic;

public class ZomBunnyDead : MonoBehaviour, IGameRuleActor {
    public bool Active = false;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.Animate("Death",gameObject);
            Action.PlaySound("ZomBunnyDeath",gameObject);
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
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Animate(Death);PlaySound(ZomBunnyDeath);PlayParticles(DeathParticles);Timer(1);Delete(this)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
