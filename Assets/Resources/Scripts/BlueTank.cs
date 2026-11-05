using UnityEngine;
using System.Collections.Generic;

public class BlueTank : MonoBehaviour, IGameRuleActor {
    public bool Active = true;
    public float speed=10f;
    public float angularSpeed=90f;
    public float health=100f;
    public float offsetY=1.7f;
    public float offsetZ=1.35f;
    public float maxAim=200f;
    public float currentAim=0f;
    public Dictionary<string, float> propertyList = new Dictionary<string, float>();
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        if(Condition.Collision("Shell",gameObject)){
            Action.Edit("this.health","this.health-Shell.damage",scopeList);
            Action.PushTo("-300","RedTank.x","RedTank.y","RedTank.z",gameObject,scopeList);
        }
        if(Condition.Compare("this.health<=0",scopeList)){
            Action.Edit("RedWin.Active","1",scopeList);
        }
        if(Condition.Compare("this.currentAim>=this.maxAim",scopeList)){
            Action.Edit("this.currentAim","this.maxAim",scopeList);
        }
    }
    public void EvalUpdate(){
        if(Condition.Keyboard("D","press")){
            Action.Rotate("this.angularSpeed","this.rx","this.ry","this.rz",gameObject,scopeList);
            Action.PlayParticles("DustTrail",gameObject);
        }
        if(Condition.Keyboard("A","press")){
            Action.Rotate("-this.angularSpeed","this.rx","this.ry","this.rz",gameObject,scopeList);
            Action.PlayParticles("DustTrail",gameObject);
        }
        if(Condition.Keyboard("W","press")){
            Action.Move("this.speed","0","this.ry","0",gameObject,scopeList);
        }
        if(Condition.Keyboard("S","press")){
            Action.Move("this.speed","0","this.ry+180","0",gameObject,scopeList);
        }
        if(Condition.Keyboard("Space","press")){
            Action.Edit("this.currentAim","this.currentAim+1",scopeList);
            Action.PlaySound("ShotCharging",gameObject);
        }
        if(Condition.Keyboard("Space","up")){
            Action.Spawn("Shell", gameObject, "0", "this.offsetY", "this.offsetZ", "0", "0", "0", scopeList);
            Action.Edit("this.currentAim","0",scopeList);
            Action.PlaySound("ShotFiring",gameObject);
        }
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Collision(Shell);Edit(this.health,this.health-Shell.damage);PushTo(-300,RedTank.x,RedTank.y,RedTank.z);Compare(this.health<=0);Edit(RedWin.Active,1);Compare(this.currentAim>=this.maxAim);Edit(this.currentAim,this.maxAim);Keyboard(D,press);Rotate(this.angularSpeed,this.rx,this.ry,this.rz);PlayParticles(DustTrail);Keyboard(A,press);Rotate(-this.angularSpeed,this.rx,this.ry,this.rz);Keyboard(W,press);Move(this.speed,0,this.ry,0);Keyboard(S,press);Move(this.speed,0,this.ry+180,0);Keyboard(Space,press);Edit(this.currentAim,this.currentAim+1);PlaySound(ShotCharging);Keyboard(Space,up);Spawn(Shell,this,0,this.offsetY,this.offsetZ);Edit(this.currentAim,0);PlaySound(ShotFiring)");
        if (Active) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
    public Dictionary<string, HashSet<GameObject>> TagCollisions = new Dictionary<string, HashSet<GameObject>>();
    void OnTriggerEnter(Collider other) {
        if (TagCollisions.ContainsKey(other.tag))
            TagCollisions[other.tag].Add(other.gameObject);
    }
    void OnTriggerExit(Collider other) {
        if (TagCollisions.ContainsKey(other.tag))
            TagCollisions[other.tag].Remove(other.gameObject);
    }
    void Awake() {
        propertyList = Utils.CreateProperties("speed=10;angularSpeed=90;health=100;offsetY=1.7;offsetZ=1.35;maxAim=200;currentAim=0");
        TagCollisions["Untagged"] = new HashSet<GameObject>();
        TagCollisions["Respawn"] = new HashSet<GameObject>();
        TagCollisions["Finish"] = new HashSet<GameObject>();
        TagCollisions["EditorOnly"] = new HashSet<GameObject>();
        TagCollisions["MainCamera"] = new HashSet<GameObject>();
        TagCollisions["Player"] = new HashSet<GameObject>();
        TagCollisions["GameController"] = new HashSet<GameObject>();
        TagCollisions["Obstacle"] = new HashSet<GameObject>();
        TagCollisions["BlueTank"] = new HashSet<GameObject>();
        TagCollisions["RedTank"] = new HashSet<GameObject>();
        TagCollisions["Shell"] = new HashSet<GameObject>();
    }
}
