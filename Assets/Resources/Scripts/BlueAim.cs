using UnityEngine;
using System.Collections.Generic;

public class BlueAim : MonoBehaviour, IGameRuleActor {
    public bool Active = true;
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.Edit("this.x","BlueTank.x",scopeList);
            Action.Edit("this.y","BlueTank.y",scopeList);
            Action.Edit("this.z","BlueTank.z",scopeList);
            Action.Edit("this.ry","BlueTank.ry",scopeList);
            Action.Edit("this.sliderValue","BlueTank.currentAim",scopeList);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Edit(this.x,BlueTank.x);Edit(this.y,BlueTank.y);Edit(this.z,BlueTank.z);Edit(this.ry,BlueTank.ry);Edit(this.sliderValue,BlueTank.currentAim)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
