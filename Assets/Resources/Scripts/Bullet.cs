using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour, IGameRuleActor {
    public bool Active = false;
    public float speed=50f;
    public float damage=5f;
    public Dictionary<string, float> propertyList = new Dictionary<string, float>();
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    public void EvalFixedUpdate(){
        {
            Action.Move("this.speed","0","this.ry","0",gameObject,scopeList);
        }
        if(Condition.Collision("Hellephant",gameObject) || Condition.Collision("ZomBear",gameObject) || Condition.Collision("ZomBunny",gameObject) || Condition.Collision("Obstacle",gameObject)){
            Action.Delete(gameObject);
        }
    }
    public void EvalUpdate(){
    }
    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
    void Start() {
        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),"Move(this.speed,0,this.ry,0);Collision(Hellephant);Collision(ZomBear);Collision(ZomBunny);Collision(Obstacle);Delete(this)");
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
        propertyList = Utils.CreateProperties("speed=50;damage=5");
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
