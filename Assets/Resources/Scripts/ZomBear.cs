using UnityEngine;
using System.Collections.Generic;

public class ZomBear : MonoBehaviour, IGameRuleActor {
    public bool Active = false;
    public float speed=3f;
    public float health=200f;
    public float damage=0.2f;
    public float moving=0f;
    public Dictionary<string, float> propertyList = new Dictionary<string, float>();
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.Edit("this.moving","1",scopeList);
            Action.NavigateTo("this.speed","Player.x","Player.y","Player.z",gameObject,scopeList);
        }
        if(Condition.Compare("Player.health<0",scopeList)){
            Action.Edit("this.moving","0",scopeList);
            Action.Edit("this.speed","0",scopeList);
        }
        if(Condition.Collision("Bullet",gameObject)){
            Action.Edit("this.health","this.health-Bullet.damage",scopeList);
            Action.PlaySound("ZomBearHurt",gameObject);
            Action.PlayParticles("HitParticles",gameObject);
        }
        if(Condition.Compare("this.health<=0",scopeList)){
            Action.Edit("#Score","#Score+10",scopeList);
            Action.Spawn("ZomBearDead", gameObject, "0", "0", "0", "0", "0", "0", scopeList);
            Action.Delete(gameObject);
        }
        if(!Condition.Check("this.moving",scopeList)){
            Action.Animate("Idle",gameObject);
        }
        if(Condition.Check("this.moving",scopeList)){
            Action.Animate("Move",gameObject);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Edit(this.moving,1);NavigateTo(this.speed,Player.x,Player.y,Player.z);Compare(Player.health<0);Edit(this.moving,0);Edit(this.speed,0);Collision(Bullet);Edit(this.health,this.health-Bullet.damage);PlaySound(ZomBearHurt);PlayParticles(HitParticles);Compare(this.health<=0);Edit(#Score,#Score+10);Spawn(ZomBearDead,this);Delete(this);Check(this.moving);Animate(Idle);Animate(Move)");
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
        propertyList = Utils.CreateProperties("speed=3;health=200;damage=0.2;moving=0");
        TagCollisions["Untagged"] = new HashSet<GameObject>();
        TagCollisions["Respawn"] = new HashSet<GameObject>();
        TagCollisions["Finish"] = new HashSet<GameObject>();
        TagCollisions["EditorOnly"] = new HashSet<GameObject>();
        TagCollisions["MainCamera"] = new HashSet<GameObject>();
        TagCollisions["Player"] = new HashSet<GameObject>();
        TagCollisions["GameController"] = new HashSet<GameObject>();
        TagCollisions["Obstacle"] = new HashSet<GameObject>();
        TagCollisions["Hellephant"] = new HashSet<GameObject>();
        TagCollisions["ZomBear"] = new HashSet<GameObject>();
        TagCollisions["ZomBunny"] = new HashSet<GameObject>();
        TagCollisions["Bullet"] = new HashSet<GameObject>();
    }
}
