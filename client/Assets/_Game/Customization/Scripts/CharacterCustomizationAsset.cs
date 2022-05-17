using UnityEngine;

[System.Serializable]
public class CharacterCustomizationAsset
{
    public Renderer renderer = null;
    [HideInInspector]
    public Transform[] transforms;
    [HideInInspector]
    public Vector3[] initialPositions;
    [HideInInspector]
    public Quaternion[] initialRotations;
    [HideInInspector]
    public ConfigurableJoint[] joints;
    [HideInInspector]
    public Vector3[] connectedAnchors;
    public Color currentColor = Color.white;
    public string beamableID = "";
}
