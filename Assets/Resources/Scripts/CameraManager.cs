using UnityEngine;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour, IGameRuleActor {
    public bool Active = true;
    public float despX=15f;
    public float offsetY=15f;
    public float offsetZ=10f;
    public Dictionary<string, float> propertyList = new Dictionary<string, float>();
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.Edit("#CameraPosition.x","(BlueTank.x+RedTank.x)/2 - this.despX",scopeList);
            Action.Edit("#CameraPosition.y","this.offsetY + (abs(BlueTank.x-RedTank.x)+abs(BlueTank.z-RedTank.z))/3",scopeList);
            Action.Edit("#CameraPosition.z","(BlueTank.z+RedTank.z)/2 - this.offsetZ",scopeList);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Edit(#CameraPosition.x,(BlueTank.x+RedTank.x)/2 - this.despX);Edit(#CameraPosition.y,this.offsetY + (abs(BlueTank.x-RedTank.x)+abs(BlueTank.z-RedTank.z))/3);Edit(#CameraPosition.z,(BlueTank.z+RedTank.z)/2 - this.offsetZ)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
    void Awake() {
        propertyList = Utils.CreateProperties("despX=15;offsetY=15;offsetZ=10");
    }
}
