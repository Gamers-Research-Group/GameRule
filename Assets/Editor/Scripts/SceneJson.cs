using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneJson
{
    public string GameName;
    public float[] ScreenResolution;
    public float[] CameraPosition;
    public float[] CameraRotation;
    public float[] SunPosition;
    public float[] SunRotation;
    public byte[] SunColor;
    public byte[] SunAmbientColor;
    public byte[] BackgroundColor;
    public float[] Gravity;
    public string SoundTrack;
    public float[] Mouse;
    public float[] MouseWorld;
    public List<ActorJson> Cast;
    public List<CustomVariable> CustomVariables;
}

[System.Serializable]
public class ActorJson
{
    public string ActorName;
    public bool Active = true;
    public string PrefabName;
    public string Tag;
    public float[] Position;
    public float[] Rotation;
    public float[] Scale;
    public float[] Size;
    public float[] Velocity;
    public float[] AngularVelocity;
    public float Density;
    public float Friction;
    public float Bounciness;
    public float Drag;
    public List<string> Properties;
    public List<SentenceJson> Script = new List<SentenceJson>();
}

[System.Serializable]
public class SentenceJson
{
    public List<string> When;
    public List<string> Do;
}

[System.Serializable]
public class CustomVariable
{
    public string name;
    public string type;
    public int intValue;
    public float floatValue;
    public bool boolValue;
    public float[] arrayValue;

    public object GetValue()
    {
        switch (type.ToLower())
        {
            case "int": return intValue;
            case "float": return floatValue;
            case "bool": return boolValue;
            case "vector2":
                return arrayValue != null && arrayValue.Length >= 2 ?
                    new Vector2(arrayValue[0], arrayValue[1]) : Vector2.zero;

            case "vector3":
                return arrayValue != null && arrayValue.Length >= 3 ?
                    new Vector3(arrayValue[0], arrayValue[1], arrayValue[2]) : Vector3.zero;

            default: return null;
        }
    }
}