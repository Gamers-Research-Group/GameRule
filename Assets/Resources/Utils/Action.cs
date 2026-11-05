using B83.LogicExpressionParser;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class Action
{
    public static void Edit(string property, string valueExp, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float value = (float)parser.ParseNumber(valueExp).GetNumber();
        GameObject obj = scopeList[property];
        Utils.SetProperty(property, value, obj);
    }

    public static void Spawn(string prefabName, GameObject spawnerObj, string offsetXExp, string offsetYExp, string offsetZExp,
                            string offsetRxExp, string offsetRyExp, string offsetRzExp, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float ParseNum(string exp) => (float)parser.ParseNumber(exp).GetNumber();

        Vector3 offsetPos = new(ParseNum(offsetXExp), ParseNum(offsetYExp), ParseNum(offsetZExp));
        Vector3 offsetRot = new(ParseNum(offsetRxExp), ParseNum(offsetRyExp), ParseNum(offsetRzExp));

        var prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        if (!prefab)
        {
            Debug.LogWarning($"Prefab '{prefabName}' no encontrado en Resources/Prefabs.");
            return;
        }

        var newObj = Object.Instantiate(prefab);
        newObj.name = prefab.name;
        newObj.SetActive(true);

        newObj.transform.position = spawnerObj.transform.TransformPoint(offsetPos);
        newObj.transform.rotation = spawnerObj.transform.rotation * Quaternion.Euler(offsetRot);
        newObj.transform.localScale = prefab.transform.localScale;

        var scriptType = System.Type.GetType(prefabName);
        if (scriptType?.IsSubclassOf(typeof(MonoBehaviour)) == true)
        {
            var script = newObj.AddComponent(scriptType);
            scriptType.GetField("Active")?.SetValue(script, true);

            if (scriptType.GetField("propertyList")?.GetValue(script) is Dictionary<string, float> props)
                foreach (var (prop, val) in props)
                    scriptType.GetField(prop)?.SetValue(script, val);

            // Append the runtime actor to the central scheduler so it is
            // evaluated in order from next iteration (Algorithm 1, line 11).
            if (script is IGameRuleActor spawnedActor)
                ActorScheduler.RegisterSpawned(spawnedActor);
        }

        if (newObj.TryGetComponent(out LineRenderer lr))
        {
            Vector3 start = newObj.transform.position;
            Vector3 dir = newObj.transform.forward;
            Vector3 end = Physics.Raycast(start, dir, out RaycastHit hit, 100f)
                ? hit.point
                : start + dir * 100f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }

    public static void Animate(string animationName, GameObject obj)
    {
        Animator animator = obj.GetComponent<Animator>();
        if (animator != null)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (!currentState.IsName(animationName)) animator.Play(animationName);
        }
    }

    public static void Move(string vExp, string rxExp, string ryExp, string rzExp, GameObject obj, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float v = (float)parser.ParseNumber(vExp).GetNumber();
        float rx = (float)parser.ParseNumber(rxExp).GetNumber() * Mathf.Deg2Rad;
        float ry = (float)parser.ParseNumber(ryExp).GetNumber() * Mathf.Deg2Rad;
        float rz = (float)parser.ParseNumber(rzExp).GetNumber() * Mathf.Deg2Rad;

        Vector3 direction = new Vector3(Mathf.Cos(rx) * Mathf.Sin(ry), Mathf.Sin(rx), Mathf.Cos(rx) * Mathf.Cos(ry)).normalized;
        Vector3 delta = direction * v * Time.deltaTime;

        Utils.SetProperty("this.x", obj.transform.position.x + delta.x, obj);
        Utils.SetProperty("this.y", obj.transform.position.y + delta.y, obj);
        Utils.SetProperty("this.z", obj.transform.position.z + delta.z, obj);
    }

    public static void MoveTo(string speedExp, string xExp, string yExp, string zExp, GameObject obj, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float targetX = (float)parser.ParseNumber(xExp).GetNumber();
        float targetY = (float)parser.ParseNumber(yExp).GetNumber();
        float targetZ = (float)parser.ParseNumber(zExp).GetNumber();
        float speed = (float)parser.ParseNumber(speedExp).GetNumber();

        Vector3 currentPos = obj.transform.position;
        Vector3 targetPos = new Vector3(targetX, targetY, targetZ);
        Vector3 direction = (targetPos - currentPos).normalized;
        Vector3 step = direction * speed * Time.deltaTime;

        if (step.magnitude > Vector3.Distance(currentPos, targetPos))
            step = targetPos - currentPos;
        Vector3 newPos = currentPos + step;

        Utils.SetProperty("this.x", newPos.x, obj);
        Utils.SetProperty("this.y", newPos.y, obj);
        Utils.SetProperty("this.z", newPos.z, obj);
    }

    public static void NavigateTo(string speedExp, string xExp, string yExp, string zExp, GameObject obj, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float x = (float)parser.ParseNumber(xExp).GetNumber();
        float y = (float)parser.ParseNumber(yExp).GetNumber();
        float z = (float)parser.ParseNumber(zExp).GetNumber();
        float speed = (float)parser.ParseNumber(speedExp).GetNumber();

        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        if (agent == null) return;

        agent.speed = speed;
        Vector3 target = new Vector3(x, y, z); //If the NavMesh is on a plain terrain, y will be ignored

        if (agent.isOnNavMesh) agent.SetDestination(target);
        else Debug.LogWarning($"'{obj.name}' is not on the NavMesh, it cannot be navigated");
    }

    public static void LoadScene()
    {
        SceneManager.LoadScene(0);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }

    public static void PlaySound(string audioClip, GameObject obj)
    {
        AudioSource[] audios = obj.GetComponents<AudioSource>();
        foreach (AudioSource audio in audios)
        {
            if (audio.clip != null && audio.clip.name == audioClip && !audio.isPlaying)
            {
                audio.Play();
                return;
            }
        }

        AudioSource[] childAudios = obj.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audio in childAudios)
        {
            if (audio.gameObject == obj) continue;
            if (audio.clip != null && audio.clip.name == audioClip && !audio.isPlaying)
            {
                audio.Play();
                return;
            }
        }
    }

    public static void PlayParticles(string particleSystemName, GameObject obj)
    {
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null && obj.name == particleSystemName && !ps.isPlaying)
        {
            ps.Play();
            return;
        }

        ParticleSystem[] systems = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem childPs in systems)
        {
            if (childPs.gameObject == obj) continue;
            if (childPs.gameObject.name == particleSystemName && !childPs.isPlaying)
            {
                childPs.Play();
            }
        }
    }

    public static void Delete(GameObject me)
    {
        Utils.RemoveFromCollisions(me);
        // Deferred removal from the central scheduler (Algorithm 1, line 10).
        if (me.TryGetComponent(out IGameRuleActor actor))
            ActorScheduler.Unregister(actor);
        Object.Destroy(me);
    }

    public static void Rotate(string angleExp, string rxExp, string ryExp, string rzExp, GameObject obj, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float angleSpeed = (float)parser.ParseNumber(angleExp).GetNumber();
        float rx = (float)parser.ParseNumber(rxExp).GetNumber();
        float ry = (float)parser.ParseNumber(ryExp).GetNumber();
        float rz = (float)parser.ParseNumber(rzExp).GetNumber();

        float angleDelta = angleSpeed * Time.deltaTime;

        Vector3 localAxis = new Vector3(rx, ry, rz).normalized;
        if (localAxis == Vector3.zero) localAxis = Vector3.up;
        obj.transform.Rotate(localAxis, angleDelta, Space.Self);

        Utils.SetProperty(obj.name + ".rx", obj.transform.eulerAngles.x, obj);
        Utils.SetProperty(obj.name + ".ry", obj.transform.eulerAngles.y, obj);
        Utils.SetProperty(obj.name + ".rz", obj.transform.eulerAngles.z, obj);
    }

    public static void RotateTo(string aExp, string dxExp, string dyExp, string dzExp, string pxExp, string pyExp, string pzExp, GameObject obj, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float a = (float)parser.ParseNumber(aExp).GetNumber();
        float dx = (float)parser.ParseNumber(dxExp).GetNumber();
        float dy = (float)parser.ParseNumber(dyExp).GetNumber();
        float dz = (float)parser.ParseNumber(dzExp).GetNumber();
        float px = (float)parser.ParseNumber(pxExp).GetNumber();
        float py = (float)parser.ParseNumber(pyExp).GetNumber();
        float pz = (float)parser.ParseNumber(pzExp).GetNumber();

        Vector3 pivot = new Vector3(px, py, pz);
        Vector3 targetDir = new Vector3(dx, dy, dz) - pivot;
        Vector3 currentDir = obj.transform.position - pivot;

        Quaternion targetRot = Quaternion.LookRotation(targetDir.normalized, Vector3.up);
        obj.transform.rotation = Quaternion.RotateTowards(obj.transform.rotation, targetRot, a * Time.deltaTime);

        Vector3 rotatedPos = pivot + targetRot * currentDir.normalized * currentDir.magnitude;
        obj.transform.position = rotatedPos;

        Utils.SetProperty(obj.name + ".ry", obj.transform.eulerAngles.y, obj);
    }

    public static void Push(string forceExp, string rxExp, string ryExp, string rzExp, GameObject obj, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float force = (float)parser.ParseNumber(forceExp).GetNumber();
        float rx = (float)parser.ParseNumber(rxExp).GetNumber() * Mathf.Deg2Rad;
        float ry = (float)parser.ParseNumber(ryExp).GetNumber() * Mathf.Deg2Rad;
        float rz = (float)parser.ParseNumber(rzExp).GetNumber() * Mathf.Deg2Rad;

        Vector3 direction = new Vector3(
            Mathf.Cos(rx) * Mathf.Sin(ry),
            Mathf.Sin(rx),
            Mathf.Cos(rx) * Mathf.Cos(ry)
        ).normalized;

        Vector3 forceVector = direction * force;
        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb != null) rb.AddForce(forceVector, ForceMode.Force);
        else obj.transform.position += forceVector * 0.02f;
    }

    public static void PushTo(string forceExp, string xExp, string yExp, string zExp, GameObject obj, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float force = (float)parser.ParseNumber(forceExp).GetNumber();
        float targetX = (float)parser.ParseNumber(xExp).GetNumber();
        float targetY = (float)parser.ParseNumber(yExp).GetNumber();
        float targetZ = (float)parser.ParseNumber(zExp).GetNumber();

        Vector3 targetPos = new Vector3(targetX, targetY, targetZ);
        Vector3 direction = (targetPos - obj.transform.position).normalized;
        Vector3 forceVector = direction * force;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(forceVector, ForceMode.Force);
        else obj.transform.position += forceVector * 0.02f;
    }

    public static void Torque(string rxExp, string ryExp, string rzExp, GameObject obj, Dictionary<string, GameObject> scopeList)
    {
        Parser parser = new Parser();
        foreach (var pair in scopeList)
            parser.ExpressionContext[pair.Key].Set(Utils.GetProperty(pair));

        float rx = (float)parser.ParseNumber(rxExp).GetNumber();
        float ry = (float)parser.ParseNumber(ryExp).GetNumber();
        float rz = (float)parser.ParseNumber(rzExp).GetNumber();

        Vector3 torqueVector = new Vector3(rx, ry, rz);
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.AddTorque(torqueVector, ForceMode.Force);
        else obj.transform.Rotate(torqueVector * Time.deltaTime, Space.Self);

        Utils.SetProperty(obj.name + ".rx", obj.transform.eulerAngles.x, obj);
        Utils.SetProperty(obj.name + ".ry", obj.transform.eulerAngles.y, obj);
        Utils.SetProperty(obj.name + ".rz", obj.transform.eulerAngles.z, obj);
    }
}