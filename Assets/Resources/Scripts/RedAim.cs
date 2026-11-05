using UnityEngine;
using System.Collections.Generic;

public class RedAim : MonoBehaviour, IGameRuleActor {
    public bool Active = true;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.Edit("this.x","RedTank.x",scopeList);
            Action.Edit("this.y","RedTank.y",scopeList);
            Action.Edit("this.z","RedTank.z",scopeList);
            Action.Edit("this.ry","RedTank.ry",scopeList);
            Action.Edit("this.sliderValue","RedTank.currentAim",scopeList);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Edit(this.x,RedTank.x);Edit(this.y,RedTank.y);Edit(this.z,RedTank.z);Edit(this.ry,RedTank.ry);Edit(this.sliderValue,RedTank.currentAim)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
