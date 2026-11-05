using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;

public static class Scripts
{
    public static void Create(List<ActorJson> actorList)
    {
        foreach (ActorJson actor in actorList)
        {
            List<string> tags = new List<string>(InternalEditorUtility.tags);
            List<string> scope = new List<string>();
            List<string> spawns = new List<string>();
            List<string> properties = new List<string>();
            string scriptsPath = "Assets/Resources/Scripts/" + actor.ActorName + ".cs";
            StreamWriter outfile = new StreamWriter(scriptsPath);
            bool hasCollision = false;

            // Header
            outfile.WriteLine("using UnityEngine;");
            outfile.WriteLine("using System.Collections.Generic;");
            outfile.WriteLine("");
            outfile.WriteLine("public class " + actor.ActorName + " : MonoBehaviour, IGameRuleActor {");

            // Properties
            outfile.WriteLine("    public bool Active = " + actor.Active.ToString().ToLower() + ";");
            foreach (string p in actor.Properties)
            {
                properties.Add(p);
                outfile.WriteLine("    public float " + p + "f;");
            }

            // Structure Accumulators
            string joinProperties = string.Join(";", properties);
            string joinSpawns = string.Join(",", spawns);
            string joinScope = "";

            // Dictionaries
            if (joinProperties.Length > 0)
                outfile.WriteLine("    public Dictionary<string, float> propertyList = new Dictionary<string, float>();");
            outfile.WriteLine("    private Dictionary<string, float> timers = new Dictionary<string, float>();");

            // Split Update and FixedUpdate
            List<SentenceJson> updateSentences = new List<SentenceJson>();
            List<SentenceJson> fixedSentences = new List<SentenceJson>();

            foreach (SentenceJson s in actor.Script)
            {
                bool isUpdate = s.When.Any(w => w.Contains("Keyboard") || w.Contains("Touch"));
                if (isUpdate) updateSentences.Add(s);
                else fixedSentences.Add(s);
            }

            // EvalFixedUpdate: physics-phase rules, invoked by the central
            // scheduler (GameManager) in declaration order, not by Unity.
            // Always emitted (empty when the actor has no fixed rules) to
            // satisfy the IGameRuleActor contract.
            outfile.WriteLine("    public void EvalFixedUpdate(){");
            foreach (SentenceJson s in fixedSentences)
            {
                if (s.When.Any())
                {
                    outfile.Write("        if(");
                    string conditionExpression = ProcessCondition(s.When[0]);
                    outfile.Write(conditionExpression);
                    outfile.WriteLine("){");

                    foreach (string c in ExtractIndividualConditions(s.When[0]))
                    {
                        if (c.Contains("Collision")) hasCollision = true;
                        scope.Add(c);
                    }
                }
                else
                {
                    outfile.WriteLine("        {");
                }

                foreach (string a in s.Do)
                {
                    if (a.Contains("Spawn")) spawns.Add(StringToElement(a));
                    scope.Add(a);
                    outfile.WriteLine("            Action." + StringToCommand(a) + ";");
                }
                outfile.WriteLine("        }");
            }
            outfile.WriteLine("    }");

            // EvalUpdate: input-phase rules (Keyboard/Touch), invoked by the
            // central scheduler (GameManager) in declaration order, not by
            // Unity. Always emitted (empty when the actor has no input rules)
            // to satisfy the IGameRuleActor contract.
            outfile.WriteLine("    public void EvalUpdate(){");
            foreach (SentenceJson s in updateSentences)
            {
                if (s.When.Any())
                {
                    outfile.Write("        if(");
                    string conditionExpression = ProcessCondition(s.When[0]);
                    outfile.Write(conditionExpression);
                    outfile.WriteLine("){");

                    foreach (string c in ExtractIndividualConditions(s.When[0]))
                    {
                        if (c.Contains("Collision")) hasCollision = true;
                        scope.Add(c);
                    }
                }
                else
                {
                    outfile.WriteLine("        {");
                }

                foreach (string a in s.Do)
                {
                    if (a.Contains("Spawn")) spawns.Add(StringToElement(a));
                    scope.Add(a);
                    outfile.WriteLine("            Action." + StringToCommand(a) + ";");
                }
                outfile.WriteLine("        }");
            }
            outfile.WriteLine("    }");

            // Awake
            List<string> awakeLines = new List<string>();
            if (joinProperties.Length != 0)
                awakeLines.Add("        propertyList = Utils.CreateProperties(\"" + joinProperties + "\");");
            if (spawns.Count > 0)
            {
                string joinSpawnsNow = string.Join(",", spawns);
            }

            // Start
            scope = scope.Distinct().ToList();
            joinScope = string.Join(";", scope);
            if (joinScope.Length != 0)
                outfile.WriteLine("    public Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();");

            outfile.WriteLine("    void Start() {");
            if (joinScope.Length != 0)
                outfile.WriteLine("        scopeList = Utils.CreateScope(gameObject.GetInstanceID(),\"" + joinScope + "\");");
            outfile.WriteLine("        if (Active) gameObject.SetActive(true);");
            outfile.WriteLine("        else gameObject.SetActive(false);");
            outfile.WriteLine("    }");

            // Collisions
            if (hasCollision)
            {
                tags = tags.Distinct().ToList();
                outfile.WriteLine("    public Dictionary<string, HashSet<GameObject>> TagCollisions = new Dictionary<string, HashSet<GameObject>>();");
                foreach (string t in tags)
                    awakeLines.Add("        TagCollisions[\"" + t + "\"] = new HashSet<GameObject>();");
                outfile.WriteLine("    void OnTriggerEnter(Collider other) {");
                outfile.WriteLine("        if (TagCollisions.ContainsKey(other.tag))");
                outfile.WriteLine("            TagCollisions[other.tag].Add(other.gameObject);");
                outfile.WriteLine("    }");

                outfile.WriteLine("    void OnTriggerExit(Collider other) {");
                outfile.WriteLine("        if (TagCollisions.ContainsKey(other.tag))");
                outfile.WriteLine("            TagCollisions[other.tag].Remove(other.gameObject);");
                outfile.WriteLine("    }");
            }

            // Write Awake
            if (awakeLines.Any())
            {
                outfile.WriteLine("    void Awake() {");
                foreach (string line in awakeLines)
                    outfile.WriteLine(line);
                outfile.WriteLine("    }");
            }

            outfile.WriteLine("}");
            outfile.Close();
        }
    }

    private static string StringToCommand(string element)
    {
        // Traslate a game.json comand into a valid unity command
        int init = element.IndexOf("(");
        int end = element.LastIndexOf(")");
        string name = element.Substring(0, init);
        string command = name;
        string rest = element.Substring(init + 1, end - init - 1);
        string[] parameters = rest.Split(new string[] { "," }, StringSplitOptions.None);
        command += "(";
        int counter = 0;
        foreach (string s in parameters)
        {
            counter++;
            command += "\"" + s + "\"";
            if (parameters.Length != counter) command += ",";
        }
        if (name == "Compare" || name == "Edit" || name == "Check") command += ",scopeList)";
        else if (name == "Move" || name == "MoveTo" || name == "NavigateTo" || name == "RotateTo" || name == "Rotate" || name == "Push" || name == "PushTo" || name == "Torque") command += ",gameObject,scopeList)";
        else if (name == "Collision" || name == "Touch" || name == "Animate" || name == "PlaySound" || name == "StopSound" || name == "PlayParticles" || name == "StopParticles" || name == "Timer") command += ",gameObject)";
        else if (name == "Keyboard") command += ")";
        else if (name == "Delete") command = "Delete(gameObject)";
        else if (name == "QuitGame" || name == "LoadScene") command = name + "()";
        else if (name == "Spawn")
        {
            string prefab = parameters[0];
            command = $"Spawn(\"{prefab}\", gameObject";

            List<string> extraParams = new List<string>();
            for (int i = 2; i < parameters.Length; i++)
                extraParams.Add($"\"{parameters[i].Trim()}\"");

            while (extraParams.Count < 6)
                extraParams.Add("\"0\"");

            foreach (string param in extraParams)
                command += $", {param}";

            command += ", scopeList)";
        }
        return (command);
    }

    private static string StringToElement(string element)
    {
        int init = element.IndexOf("(");
        int end = element.LastIndexOf(")");
        string tag = element.Substring(init + 1, end - init - 1);
        return (tag);
    }

    private static string ProcessCondition(string condition)
    {
        string result = condition
            .Replace(" AND ", " && ")
            .Replace(" OR ", " || ")
            .Replace("NOT ", "!");

        var individualConditions = ExtractIndividualConditions(condition);
        foreach (string individualCondition in individualConditions)
        {
            string replacement = $"Condition.{StringToCommand(individualCondition)}";
            result = result.Replace(individualCondition, replacement);
        }

        return result;
    }

    private static List<string> ExtractIndividualConditions(string conditionExpression)
    {
        List<string> conditions = new List<string>();

        var matches = Regex.Matches(
            conditionExpression,
            @"[a-zA-Z_][a-zA-Z0-9_]*\([^)]+\)"
        );

        foreach (Match match in matches)
        {
            conditions.Add(match.Value);
        }

        return conditions;
    }

    public static void CreateGameManager(SceneJson scene, List<string> declarationOrder)
    {
        string path = "Assets/Resources/Scripts/GameManager.cs";

        using (StreamWriter outfile = new StreamWriter(path))
        {
            outfile.WriteLine("using UnityEngine;");
            outfile.WriteLine("using System.Collections.Generic;");
            outfile.WriteLine("using UnityEngine.SceneManagement;");
            outfile.WriteLine("");
            // Runs its Awake/Start/Update/FixedUpdate before default-order
            // scripts, so the actor list is built while every actor is still
            // active (actors deactivate themselves in their own Start).
            outfile.WriteLine("[DefaultExecutionOrder(-100)]");
            outfile.WriteLine("public class GameManager : MonoBehaviour");
            outfile.WriteLine("{");
            outfile.WriteLine("    public static GameManager Instance { get; private set; }");
            outfile.WriteLine("    private Camera mainCamera;");
            outfile.WriteLine("    private Light sunLight;");
            outfile.WriteLine("    private AudioSource audioSource;");
            outfile.WriteLine("");

            // Actor evaluation order = order of declaration in the descriptor.
            string bakedOrder = string.Join(", ", declarationOrder.Select(n => "\"" + n + "\""));
            outfile.WriteLine("    private static readonly string[] ActorOrder = new string[] { " + bakedOrder + " };");
            outfile.WriteLine("");

            outfile.WriteLine($"    public string GameName = \"{scene.GameName ?? "Unknown"}\";");

            // ScreenResolution 1920x1080 default
            if (scene.ScreenResolution != null && scene.ScreenResolution.Length >= 2)
                outfile.WriteLine($"    public Vector2 ScreenResolution = new Vector2({scene.ScreenResolution[0].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.ScreenResolution[1].ToString(System.Globalization.CultureInfo.InvariantCulture)}f);");
            else
                outfile.WriteLine("    public Vector2 ScreenResolution = new Vector2(1920f, 1080f);");

            // CameraPosition (0, 1, -10) default
            if (scene.CameraPosition != null && scene.CameraPosition.Length >= 3)
                outfile.WriteLine($"    public Vector3 CameraPosition = new Vector3({scene.CameraPosition[0].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.CameraPosition[1].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.CameraPosition[2].ToString(System.Globalization.CultureInfo.InvariantCulture)}f);");
            else
                outfile.WriteLine("    public Vector3 CameraPosition = new Vector3(0f, 1f, -10f);");

            // CameraRotation (0, 0, 0) default
            if (scene.CameraRotation != null && scene.CameraRotation.Length >= 3)
                outfile.WriteLine($"    public Vector3 CameraRotation = new Vector3({scene.CameraRotation[0].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.CameraRotation[1].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.CameraRotation[2].ToString(System.Globalization.CultureInfo.InvariantCulture)}f);");
            else
                outfile.WriteLine("    public Vector3 CameraRotation = new Vector3(0f, 0f, 0f);");

            // SunPosition (0, 3, 0) default
            if (scene.SunPosition != null && scene.SunPosition.Length >= 3)
                outfile.WriteLine($"    public Vector3 SunPosition = new Vector3({scene.SunPosition[0].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.SunPosition[1].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.SunPosition[2].ToString(System.Globalization.CultureInfo.InvariantCulture)}f);");
            else
                outfile.WriteLine("    public Vector3 SunPosition = new Vector3(0f, 3f, 0f);");

            // SunRotation (50, -30, 0) default
            if (scene.SunRotation != null && scene.SunRotation.Length >= 3)
                outfile.WriteLine($"    public Vector3 SunRotation = new Vector3({scene.SunRotation[0].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.SunRotation[1].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.SunRotation[2].ToString(System.Globalization.CultureInfo.InvariantCulture)}f);");
            else
                outfile.WriteLine("    public Vector3 SunRotation = new Vector3(50f, -30f, 0f);");

            // SunColor (255, 255, 255) default
            if (scene.SunColor != null && scene.SunColor.Length >= 3)
                outfile.WriteLine($"    public Color SunColor = new Color32({scene.SunColor[0]}, {scene.SunColor[1]}, {scene.SunColor[2]}, 255);");
            else
                outfile.WriteLine("    public Color SunColor = new Color32(255, 255, 255, 255);");

            // SunAmbientColor (128, 128, 128) default
            if (scene.SunAmbientColor != null && scene.SunAmbientColor.Length >= 3)
                outfile.WriteLine($"    public Color SunAmbientColor = new Color32({scene.SunAmbientColor[0]}, {scene.SunAmbientColor[1]}, {scene.SunAmbientColor[2]}, 255);");
            else
                outfile.WriteLine("    public Color SunAmbientColor = new Color32(128, 128, 128, 255);");

            // BackgroundColor (0, 0, 0) default
            if (scene.BackgroundColor != null && scene.BackgroundColor.Length >= 3)
                outfile.WriteLine($"    public Color BackgroundColor = new Color32({scene.BackgroundColor[0]}, {scene.BackgroundColor[1]}, {scene.BackgroundColor[2]}, 255);");
            else
                outfile.WriteLine("    public Color BackgroundColor = new Color32(0, 0, 0, 255);");

            // Gravity (0, -9.81, 0) default
            if (scene.Gravity != null && scene.Gravity.Length >= 3)
                outfile.WriteLine($"    public Vector3 Gravity = new Vector3({scene.Gravity[0].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.Gravity[1].ToString(System.Globalization.CultureInfo.InvariantCulture)}f, {scene.Gravity[2].ToString(System.Globalization.CultureInfo.InvariantCulture)}f);");
            else
                outfile.WriteLine("    public Vector3 Gravity = new Vector3(0f, -9.81f, 0f);");

            outfile.WriteLine("    public string SoundTrack");
            outfile.WriteLine("    {");
            outfile.WriteLine("        get");
            outfile.WriteLine("        {");
            outfile.WriteLine("            if (audioSource != null && audioSource.clip != null)");
            outfile.WriteLine("                return audioSource.clip.name;");
            outfile.WriteLine("            return \"\";");
            outfile.WriteLine("        }");
            outfile.WriteLine("    }");

            outfile.WriteLine("    public float FPS { get; private set; }");

            outfile.WriteLine("    public float Time { get; private set; }");
            outfile.WriteLine("    public float DeltaTime { get; private set; }");

            outfile.WriteLine("    public Vector3 Mouse = Vector3.zero;");
            outfile.WriteLine("    public Vector3 MouseWorld = Vector3.zero;");
            outfile.WriteLine("");

            // Custom variables
            if (scene.CustomVariables != null && scene.CustomVariables.Count > 0)
            {
                outfile.WriteLine("    // Custom Global Variables");

                foreach (var customVar in scene.CustomVariables)
                {
                    if (!string.IsNullOrEmpty(customVar.name) && !string.IsNullOrEmpty(customVar.type))
                    {
                        string fieldDeclaration = GenerateCustomVariableDeclaration(customVar);
                        if (!string.IsNullOrEmpty(fieldDeclaration))
                        {
                            outfile.WriteLine($"    {fieldDeclaration}");
                        }
                    }
                }
            }

            outfile.WriteLine("    void Start()");
            outfile.WriteLine("    {");
            outfile.WriteLine("        if (Instance == null)");
            outfile.WriteLine("        {");
            outfile.WriteLine("            Instance = this;");
            outfile.WriteLine("            DontDestroyOnLoad(gameObject);");
            outfile.WriteLine("        }");
            outfile.WriteLine("        else");
            outfile.WriteLine("        {");
            outfile.WriteLine("            Destroy(gameObject);");
            outfile.WriteLine("            return;");
            outfile.WriteLine("        }");
            outfile.WriteLine("        ");
            outfile.WriteLine("        mainCamera = GetComponentInChildren<Camera>();");
            outfile.WriteLine("        sunLight = GetComponentInChildren<Light>();");
            outfile.WriteLine("        audioSource = GetComponent<AudioSource>();");
            outfile.WriteLine("        ");
            outfile.WriteLine("        ApplyCameraSettings();");
            outfile.WriteLine("        ApplySunSettings();");
            outfile.WriteLine("        ApplyGlobalSettings();");
            outfile.WriteLine("        ");
            outfile.WriteLine("        SceneManager.sceneLoaded += OnSceneLoaded;");
            outfile.WriteLine("        ActorScheduler.Build(ActorOrder);");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            outfile.WriteLine("    void OnDestroy()");
            outfile.WriteLine("    {");
            outfile.WriteLine("        if (Instance == this)");
            outfile.WriteLine("            SceneManager.sceneLoaded -= OnSceneLoaded;");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            // Rebuild the ordered actor list on scene reload (e.g. LoadScene).
            // sceneLoaded fires after all actor Awake calls but before any
            // actor Start, so every actor is still active and resolvable.
            outfile.WriteLine("    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)");
            outfile.WriteLine("    {");
            outfile.WriteLine("        ActorScheduler.Build(ActorOrder);");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            outfile.WriteLine("    void Update()");
            outfile.WriteLine("    {");
            outfile.WriteLine("        UpdateRuntimeVariables();");
            outfile.WriteLine("        ActorScheduler.RunUpdate();");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            // Fresh engine input (mouse) before the actors read it; camera and
            // sun applied after, so they reflect writes made this same step.
            outfile.WriteLine("    void FixedUpdate()");
            outfile.WriteLine("    {");
            outfile.WriteLine("        UpdateMousePosition();");
            outfile.WriteLine("        ActorScheduler.RunFixedUpdate();");
            outfile.WriteLine("        ApplyCameraSettings();");
            outfile.WriteLine("        ApplySunSettings();");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            outfile.WriteLine("    private void UpdateRuntimeVariables()");
            outfile.WriteLine("    {");
            outfile.WriteLine("        Time = UnityEngine.Time.time;");
            outfile.WriteLine("        DeltaTime = UnityEngine.Time.deltaTime;");
            outfile.WriteLine("        FPS = 1.0f / UnityEngine.Time.deltaTime;");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            outfile.WriteLine("    private void UpdateMousePosition()");
            outfile.WriteLine("    {");
            outfile.WriteLine("        if (mainCamera != null)");
            outfile.WriteLine("        {");
            outfile.WriteLine("            var mouse = UnityEngine.InputSystem.Mouse.current;");
            outfile.WriteLine("            if (mouse != null)");
            outfile.WriteLine("            {");
            outfile.WriteLine("                Vector2 m = mouse.position.ReadValue();");
            outfile.WriteLine("                Mouse = new Vector3(m.x, m.y, 0);");
            outfile.WriteLine();
            outfile.WriteLine("                Ray ray = mainCamera.ScreenPointToRay(Mouse);");
            outfile.WriteLine("                Plane plane = new Plane(Vector3.up, Vector3.zero);");
            outfile.WriteLine();
            outfile.WriteLine("                if (plane.Raycast(ray, out float enter))");
            outfile.WriteLine("                    MouseWorld = ray.GetPoint(enter);");
            outfile.WriteLine("            }");
            outfile.WriteLine("        }");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            outfile.WriteLine("    private void ApplyCameraSettings()");
            outfile.WriteLine("    {");
            outfile.WriteLine("        if (mainCamera != null)");
            outfile.WriteLine("        {");
            outfile.WriteLine("            mainCamera.transform.position = CameraPosition;");
            outfile.WriteLine("            mainCamera.transform.eulerAngles = CameraRotation;");
            outfile.WriteLine("            mainCamera.backgroundColor = BackgroundColor;");
            outfile.WriteLine("        }");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            outfile.WriteLine("    private void ApplySunSettings()");
            outfile.WriteLine("    {");
            outfile.WriteLine("        if (sunLight != null)");
            outfile.WriteLine("        {");
            outfile.WriteLine("            sunLight.transform.position = SunPosition;");
            outfile.WriteLine("            sunLight.transform.eulerAngles = SunRotation;");
            outfile.WriteLine("            sunLight.color = SunColor;");
            outfile.WriteLine("            RenderSettings.ambientLight = SunAmbientColor;");
            outfile.WriteLine("        }");
            outfile.WriteLine("        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            outfile.WriteLine("    private void ApplyGlobalSettings()");
            outfile.WriteLine("    {");
            outfile.WriteLine("        Physics.gravity = Gravity;");
            outfile.WriteLine("        if (ScreenResolution != Vector2.zero)");
            outfile.WriteLine("            Screen.SetResolution((int)ScreenResolution.x, (int)ScreenResolution.y, true);");
            outfile.WriteLine("    }");
            outfile.WriteLine("");

            outfile.WriteLine("}");
        }
    }

    private static string GenerateCustomVariableDeclaration(CustomVariable customVar)
    {
        string name = customVar.name;
        string type = customVar.type.ToLower();

        switch (type)
        {
            case "int":
                return $"public int {name} = {customVar.intValue};";
            case "float":
                return $"public float {name} = {customVar.floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}f;";
            case "bool":
                return $"public bool {name} = {customVar.boolValue.ToString().ToLower()};";
            case "vector2":
                if (customVar.arrayValue != null && customVar.arrayValue.Length >= 2)
                {
                    string x = customVar.arrayValue[0].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    string y = customVar.arrayValue[1].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    return $"public Vector2 {name} = new Vector2({x}f, {y}f);";
                }
                break;

            case "vector3":
                if (customVar.arrayValue != null && customVar.arrayValue.Length >= 3)
                {
                    string x = customVar.arrayValue[0].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    string y = customVar.arrayValue[1].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    string z = customVar.arrayValue[2].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    return $"public Vector3 {name} = new Vector3({x}f, {y}f, {z}f);";
                }
                break;
        }
        return null;
    }
}