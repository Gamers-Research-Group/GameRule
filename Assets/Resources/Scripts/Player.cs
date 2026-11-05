using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour, IGameRuleActor {
    public bool Active = true;
    public float speed=5f;
    public float rotSpeed=360f;
    public float lastHealth=100.0f;
    public float health=100.0f;
    public float offsetCam=7f;
    public float offsetX=0.28f;
    public float offsetY=0.3f;
    public float offsetZ=0.7f;
    public float moving=0f;
    public Dictionary<string, float> propertyList = new Dictionary<string, float>();
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.RotateTo("this.rotSpeed","#MouseWorld.x","#MouseWorld.y","#MouseWorld.z","this.x","this.y","this.z",gameObject,scopeList);
            Action.Edit("#CameraPosition.x","this.x",scopeList);
            Action.Edit("#CameraPosition.z","this.z-this.offsetCam",scopeList);
        }
        if(Condition.Collision("Hellephant",gameObject)){
            Action.Edit("this.health","this.health-Hellephant.damage",scopeList);
            Action.PlaySound("PlayerHurt",gameObject);
        }
        if(Condition.Collision("ZomBear",gameObject)){
            Action.Edit("this.health","this.health-ZomBear.damage",scopeList);
            Action.PlaySound("PlayerHurt",gameObject);
        }
        if(Condition.Collision("ZomBunny",gameObject)){
            Action.Edit("this.health","this.health-ZomBunny.damage",scopeList);
            Action.PlaySound("PlayerHurt",gameObject);
        }
        if(Condition.Compare("this.health<=0",scopeList)){
            Action.Edit("GameOver.Active","1",scopeList);
            Action.PlaySound("PlayerDeath",gameObject);
            Action.Edit("this.moving","0",scopeList);
            Action.Edit("this.speed","0",scopeList);
        }
        if(Condition.Compare("this.health==this.lastHealth",scopeList)){
            Action.Edit("DamageCanvas.Active","0",scopeList);
        }
        if(Condition.Compare("this.health<this.lastHealth",scopeList)){
            Action.Edit("DamageCanvas.Active","1",scopeList);
            Action.Edit("this.lastHealth","this.health",scopeList);
        }
        if(Condition.Check("this.moving",scopeList)){
            Action.Animate("Move",gameObject);
        }
        if(!Condition.Check("this.moving",scopeList) && Condition.Compare("this.health>0",scopeList)){
            Action.Animate("Idle",gameObject);
        }
        if(!Condition.Check("this.moving",scopeList) && Condition.Compare("this.health<=0",scopeList)){
            Action.Animate("Death",gameObject);
        }
    }
    public void EvalUpdate(){
        if(Condition.Keyboard("RightArrow","press")){
            Action.Move("this.speed"," 0"," 90"," 0",gameObject,scopeList);
            Action.Edit("this.moving","1",scopeList);
        }
        if(Condition.Keyboard("LeftArrow","press")){
            Action.Move("this.speed"," 0"," -90"," 0",gameObject,scopeList);
            Action.Edit("this.moving","1",scopeList);
        }
        if(Condition.Keyboard("UpArrow","press")){
            Action.Move("this.speed"," 0"," 0"," 0",gameObject,scopeList);
            Action.Edit("this.moving","1",scopeList);
        }
        if(Condition.Keyboard("DownArrow","press")){
            Action.Move("this.speed"," 0"," 180"," 0",gameObject,scopeList);
            Action.Edit("this.moving","1",scopeList);
        }
        if(Condition.Keyboard("RightArrow","up") || Condition.Keyboard("LeftArrow","up") || Condition.Keyboard("UpArrow","up") || Condition.Keyboard("DownArrow","up")){
            Action.Edit("this.moving","0",scopeList);
        }
        if(Condition.Touch("press","false",gameObject)){
            Action.Spawn("Bullet", gameObject, "this.offsetX", "this.offsetY", "this.offsetZ", "0", "0", "0", scopeList);
            Action.Spawn("Laser", gameObject, "this.offsetX", "this.offsetY", "this.offsetZ", "0", "0", "0", scopeList);
            Action.Edit("ShotLight.Active","1",scopeList);
            Action.PlaySound("PlayerGunShot",gameObject);
            Action.PlayParticles("GunBarrelEnd",gameObject);
        }
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"RotateTo(this.rotSpeed,#MouseWorld.x,#MouseWorld.y,#MouseWorld.z,this.x,this.y,this.z);Edit(#CameraPosition.x,this.x);Edit(#CameraPosition.z,this.z-this.offsetCam);Collision(Hellephant);Edit(this.health,this.health-Hellephant.damage);PlaySound(PlayerHurt);Collision(ZomBear);Edit(this.health,this.health-ZomBear.damage);Collision(ZomBunny);Edit(this.health,this.health-ZomBunny.damage);Compare(this.health<=0);Edit(GameOver.Active,1);PlaySound(PlayerDeath);Edit(this.moving,0);Edit(this.speed,0);Compare(this.health==this.lastHealth);Edit(DamageCanvas.Active,0);Compare(this.health<this.lastHealth);Edit(DamageCanvas.Active,1);Edit(this.lastHealth,this.health);Check(this.moving);Animate(Move);Compare(this.health>0);Animate(Idle);Animate(Death);Keyboard(RightArrow,press);Move(this.speed, 0, 90, 0);Edit(this.moving,1);Keyboard(LeftArrow,press);Move(this.speed, 0, -90, 0);Keyboard(UpArrow,press);Move(this.speed, 0, 0, 0);Keyboard(DownArrow,press);Move(this.speed, 0, 180, 0);Keyboard(RightArrow,up);Keyboard(LeftArrow,up);Keyboard(UpArrow,up);Keyboard(DownArrow,up);Touch(press,false);Spawn(Bullet,this,this.offsetX,this.offsetY,this.offsetZ);Spawn(Laser,this,this.offsetX,this.offsetY,this.offsetZ);Edit(ShotLight.Active,1);PlaySound(PlayerGunShot);PlayParticles(GunBarrelEnd)");
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
        propertyList = Utils.CreateProperties("speed=5;rotSpeed=360;lastHealth=100.0;health=100.0;offsetCam=7;offsetX=0.28;offsetY=0.3;offsetZ=0.7;moving=0");
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
