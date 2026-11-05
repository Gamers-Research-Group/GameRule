using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public static class Utils
{
    public static float GetProperty(KeyValuePair<string, GameObject> s)
    {
        GameObject obj = s.Value;
        float value = float.NaN;
        if (s.Key.StartsWith("#"))
        {
            return GetGameManagerProperty(s.Key.Substring(1), obj);
        }
        string[] elements = s.Key.Split(new string[] { "." }, StringSplitOptions.None);
        if (elements.Length < 2) return float.NaN;

        if (obj != null)
        {
            switch (elements[1])
            {
                case "x": value = obj.transform.position.x; break;
                case "y": value = obj.transform.position.y; break;
                case "z": value = obj.transform.position.z; break;
                case "rx": value = obj.transform.eulerAngles.x; break;
                case "ry": value = obj.transform.eulerAngles.y; break;
                case "rz": value = obj.transform.eulerAngles.z; break;
                case "sx": value = obj.transform.localScale.x; break;
                case "sy": value = obj.transform.localScale.y; break;
                case "sz": value = obj.transform.localScale.z; break;
                case "Active": value = obj.activeSelf ? 1f : 0f; break;
                case "value":
                    var slider = obj.GetComponent<UnityEngine.UI.Slider>();
                    if (slider != null) value = slider.value;
                    break;

                case "text":
                    var text = obj.GetComponent<UnityEngine.UI.Text>();
                    if (text != null && float.TryParse(text.text, out float parsed)) value = parsed;
                    break;

                default:
                    {
                        var script = obj.GetComponent(obj.name);
                        value = (float)script.GetType().GetField(elements[1]).GetValue(script);
                        break;
                    }
            }
        }
        return (value);
    }

    private static float GetGameManagerProperty(string propertyPath, GameObject gameManagerObj)
    {
        GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.Log("GameManager component not found");
            return float.NaN;
        }

        string[] parts = propertyPath.Split('.');

        // Simple property
        if (parts.Length == 1)
        {
            string mainProperty = parts[0];

            var field = typeof(GameManager).GetField(mainProperty);
            var property = typeof(GameManager).GetProperty(mainProperty);

            object value = null;

            if (field != null)
                value = field.GetValue(gameManager);
            else if (property != null)
                value = property.GetValue(gameManager);
            else
            {
                Debug.LogError($"Property '{mainProperty}' not found in GameManager");
                return float.NaN;
            }

            // Float
            if (value is float f) return f;

            // Int
            if (value is int i) return i;

            // Bool
            if (value is bool b) return b ? 1f : 0f;

            Debug.LogError($"Unsupported GameManager property type for '{mainProperty}': {value.GetType()}");
            return float.NaN;
        }

        // Compound property
        if (parts.Length == 2)
        {
            string mainProperty = parts[0];
            string subProperty = parts[1];

            var field = typeof(GameManager).GetField(mainProperty);
            var property = typeof(GameManager).GetProperty(mainProperty);

            object value = null;
            if (field != null)
                value = field.GetValue(gameManager);
            else if (property != null)
                value = property.GetValue(gameManager);
            else
            {
                Debug.LogError($"Property '{mainProperty}' not found in GameManager");
                return float.NaN;
            }

            // Vector3
            if (value is Vector3 vector3)
            {
                if (subProperty == "x") return vector3.x;
                if (subProperty == "y") return vector3.y;
                if (subProperty == "z") return vector3.z;
            }

            // Vector2
            if (value is Vector2 vector2)
            {
                if (subProperty == "x") return vector2.x;
                if (subProperty == "y") return vector2.y;
            }

            // Float
            if (value is float f) return f;

            // Int
            if (value is int i) return i;

            // Bool
            if (value is bool b) return b ? 1f : 0f;

            Debug.LogError($"Unsupported GameManager property type: {value.GetType()}");
            return float.NaN;
        }

        return float.NaN;
    }

    public static void SetProperty(string property, float value, GameObject obj)
    {
        if (property.StartsWith("#"))
        {
            SetGameManagerProperty(property.Substring(1), value, obj);
            return;
        }
        string[] elements = property.Split(new string[] { "." }, StringSplitOptions.None);
        if (obj != null)
        {
            var script = obj.GetComponent(obj.name);
            switch (elements[1])
            {
                case "x": obj.transform.position = new Vector3(value, obj.transform.position.y, obj.transform.position.z); break;
                case "y": obj.transform.position = new Vector3(obj.transform.position.x, value, obj.transform.position.z); break;
                case "z": obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, value); break;
                case "rx": obj.transform.eulerAngles = new Vector3(value, obj.transform.eulerAngles.y, obj.transform.eulerAngles.z); break;
                case "ry": obj.transform.eulerAngles = new Vector3(obj.transform.eulerAngles.x, value, obj.transform.eulerAngles.z); break;
                case "rz": obj.transform.eulerAngles = new Vector3(obj.transform.eulerAngles.x, obj.transform.eulerAngles.y, value); break;
                case "sx": obj.transform.localScale = new Vector3(value, obj.transform.localScale.y, obj.transform.localScale.z); break;
                case "sy": obj.transform.localScale = new Vector3(obj.transform.localScale.x, value, obj.transform.localScale.z); break;
                case "sz": obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y, value); break;
                case "Active":
                    obj.SetActive(value != 0);
                    if (script != null && script.GetType().GetField("Active") != null)
                        script.GetType().GetField("Active").SetValue(script, value != 0);
                    break;

                case "value":
                    var slider = obj.GetComponent<UnityEngine.UI.Slider>();
                    if (slider != null) slider.value = value;
                    break;

                case "text":
                    var text = obj.GetComponent<UnityEngine.UI.Text>();
                    if (text != null) text.text = value.ToString();
                    break;

                default:
                    {
                        script.GetType().GetField(elements[1]).SetValue(script, value); break;
                    }
            }
        }
    }

    private static void SetGameManagerProperty(string propertyPath, float value, GameObject gameManagerObj)
    {
        GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager component not found");
            return;
        }

        string[] parts = propertyPath.Split('.');

        // Simple property
        if (parts.Length == 1)
        {
            string mainProperty = parts[0];

            var field = typeof(GameManager).GetField(mainProperty);
            var propertyInfo = typeof(GameManager).GetProperty(mainProperty);

            // Float
            if (field != null && field.FieldType == typeof(float))
            {
                field.SetValue(gameManager, value);
                return;
            }
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(float))
            {
                propertyInfo.SetValue(gameManager, value);
                return;
            }

            // Int
            if (field != null && field.FieldType == typeof(int))
            {
                field.SetValue(gameManager, Mathf.RoundToInt(value));
                return;
            }
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(int))
            {
                propertyInfo.SetValue(gameManager, Mathf.RoundToInt(value));
                return;
            }

            // Bool
            if (field != null && field.FieldType == typeof(bool))
            {
                field.SetValue(gameManager, value != 0f);
                return;
            }
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
            {
                propertyInfo.SetValue(gameManager, value != 0f);
                return;
            }

            Debug.LogError($"Property '{mainProperty}' not found or unsupported type in GameManager.");
            return;
        }

        // Compound property
        if (parts.Length == 2)
        {
            string mainProperty = parts[0];
            string subProperty = parts[1];

            var field = typeof(GameManager).GetField(mainProperty);
            var propertyInfo = typeof(GameManager).GetProperty(mainProperty);

            object mainValue = null;

            if (field != null)
                mainValue = field.GetValue(gameManager);
            else if (propertyInfo != null)
                mainValue = propertyInfo.GetValue(gameManager);
            else
            {
                Debug.LogError($"Property '{mainProperty}' not found in GameManager");
                return;
            }

            // Vector3
            if (mainValue is Vector3 v3)
            {
                if (subProperty == "x") v3.x = value;
                else if (subProperty == "y") v3.y = value;
                else if (subProperty == "z") v3.z = value;

                if (field != null) field.SetValue(gameManager, v3);
                else propertyInfo.SetValue(gameManager, v3);

                return;
            }

            // Vector2
            if (mainValue is Vector2 v2)
            {
                if (subProperty == "x") v2.x = value;
                else if (subProperty == "y") v2.y = value;

                if (field != null) field.SetValue(gameManager, v2);
                else propertyInfo.SetValue(gameManager, v2);

                return;
            }

            // Float
            if (field != null && field.FieldType == typeof(float))
            {
                field.SetValue(gameManager, value);
                return;
            }
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(float))
            {
                propertyInfo.SetValue(gameManager, value);
                return;
            }

            // Int
            if (field != null && field.FieldType == typeof(int))
            {
                field.SetValue(gameManager, Mathf.RoundToInt(value));
                return;
            }
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(int))
            {
                propertyInfo.SetValue(gameManager, Mathf.RoundToInt(value));
                return;
            }

            // Bool
            if (field != null && field.FieldType == typeof(bool))
            {
                field.SetValue(gameManager, value != 0f);
                return;
            }
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
            {
                propertyInfo.SetValue(gameManager, value != 0f);
                return;
            }

            Debug.LogError($"Unsupported property type for '{mainProperty}'");
        }
    }

    public static Dictionary<string, GameObject> CreateScope(int objID, string scope)
    {
        Dictionary<string, GameObject> scopeList = new Dictionary<string, GameObject>();
        List<string> baseProperties = new List<string> { "x", "y", "z", "rx", "ry", "rz", "sx", "sy", "sz", "Active" };

        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                GameObject obj = t.gameObject;
                var script = obj.GetComponent(obj.name);
                List<string> properties = new List<string>(baseProperties);

                if (script != null && script.GetType().GetField("propertyList") != null)
                {
                    var propertyList = (Dictionary<string, float>)script.GetType().GetField("propertyList").GetValue(script);
                    properties.AddRange(propertyList.Keys);
                }

                if (obj.GetComponent<UnityEngine.UI.Slider>() != null)
                    properties.Add("value");

                if (obj.GetComponent<UnityEngine.UI.Text>() != null)
                    properties.Add("text");

                properties = properties.Distinct().ToList();

                foreach (string p in properties)
                {
                    string fullName = obj.name + "." + p;
                    if (scope.Contains(fullName) && !scopeList.ContainsKey(fullName))
                    {
                        scopeList.Add(fullName, obj);
                    }
                }

                if (obj.GetInstanceID() == objID)
                {
                    foreach (string p in properties)
                    {
                        string thisProp = "this." + p;
                        if (!scopeList.ContainsKey(thisProp))
                        {
                            scopeList.Add(thisProp, obj);
                        }
                    }
                }
            }
        }

        if (scope.Contains("#"))
        {
            GameManager gameManager = GameObject.FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                var gameManagerType = typeof(GameManager);
                var fields = gameManagerType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(Vector2))
                    {
                        string[] components = { "x", "y" };
                        foreach (string comp in components)
                        {
                            string fullName = "#" + field.Name + "." + comp;
                            if (scope.Contains(fullName) && !scopeList.ContainsKey(fullName))
                            {
                                scopeList.Add(fullName, gameManager.gameObject);
                            }
                        }
                    }
                    else if (field.FieldType == typeof(Vector3))
                    {
                        string[] components = { "x", "y", "z" };
                        foreach (string comp in components)
                        {
                            string fullName = "#" + field.Name + "." + comp;
                            if (scope.Contains(fullName) && !scopeList.ContainsKey(fullName))
                            {
                                scopeList.Add(fullName, gameManager.gameObject);
                            }
                        }
                    }
                    else
                    {
                        string fullName = "#" + field.Name;
                        if (scope.Contains(fullName) && !scopeList.ContainsKey(fullName))
                        {
                            scopeList.Add(fullName, gameManager.gameObject);
                        }
                    }
                }

                var properties = gameManagerType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var prop in properties)
                {
                    if (prop.PropertyType == typeof(Vector2))
                    {
                        string[] components = { "x", "y" };
                        foreach (string comp in components)
                        {
                            string fullName = "#" + prop.Name + "." + comp;
                            if (scope.Contains(fullName) && !scopeList.ContainsKey(fullName))
                            {
                                scopeList.Add(fullName, gameManager.gameObject);
                            }
                        }
                    }
                    else if (prop.PropertyType == typeof(Vector3))
                    {
                        string[] components = { "x", "y", "z" };
                        foreach (string comp in components)
                        {
                            string fullName = "#" + prop.Name + "." + comp;
                            if (scope.Contains(fullName) && !scopeList.ContainsKey(fullName))
                            {
                                scopeList.Add(fullName, gameManager.gameObject);
                            }
                        }
                    }
                    else
                    {
                        string fullName = "#" + prop.Name;
                        if (scope.Contains(fullName) && !scopeList.ContainsKey(fullName))
                        {
                            scopeList.Add(fullName, gameManager.gameObject);
                        }
                    }
                }
            }
        }

        return scopeList;
    }

    public static Dictionary<string, float> CreateProperties(string s)
    {
        Dictionary<string, float> properties = new Dictionary<string, float>();
        if (s != "")
        {
            var asign = s.Split(new string[] { ";" }, StringSplitOptions.None);
            foreach (string a in asign)
            {
                var elements = a.Split(new string[] { "=" }, StringSplitOptions.None);
                if (!properties.ContainsKey(elements[0]))
                {
                    float value = float.Parse(elements[1], System.Globalization.CultureInfo.InvariantCulture);
                    properties.Add(elements[0], value);
                }
            }
        }
        return (properties);
    }

    public static void RemoveFromCollisions(GameObject me)
    {
        var script = me.GetComponent(me.name);
        if (script == null) return;

        var tagDict = script.GetType().GetField("TagCollisions")?.GetValue(script) as Dictionary<string, HashSet<GameObject>>;
        if (tagDict == null) return;

        foreach (var others in tagDict.Values.ToList())
        {
            foreach (var other in others)
            {
                if (other == null) continue;

                var otherScript = other.GetComponent(other.name);
                var otherTagDict = otherScript?.GetType().GetField("TagCollisions")?.GetValue(otherScript) as Dictionary<string, HashSet<GameObject>>;
                if (otherTagDict == null) continue;

                foreach (var set in otherTagDict.Values)
                {
                    set.Remove(me);
                }
            }
        }
    }
}