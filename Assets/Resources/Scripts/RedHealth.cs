using UnityEngine;
using System.Collections.Generic;

public class RedHealth : MonoBehaviour, IGameRuleActor {
    public bool Active = true;
    public float offsetY=0.05f;
    public Dictionary<string, float> propertyList = new Dictionary<string, float>();
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.Edit("this.x","RedTank.x",scopeList);
            Action.Edit("this.y","RedTank.y+this.offsetY",scopeList);
            Action.Edit("this.z","RedTank.z",scopeList);
            Action.Edit("this.ry","RedTank.ry",scopeList);
            Action.Edit("this.sliderValue","RedTank.health",scopeList);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Edit(this.x,RedTank.x);Edit(this.y,RedTank.y+this.offsetY);Edit(this.z,RedTank.z);Edit(this.ry,RedTank.ry);Edit(this.sliderValue,RedTank.health)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
    void Awake() {
        propertyList = Utils.CreateProperties("offsetY=0.05");
    }
}
