#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LightProbeGroup))]
[ExecuteInEditMode]
public class AutoLightProbe : MonoBehaviour
{
    public float XRange;
    public float YMin;
    public float YMax;
    public float YOffset;
    public float ZRange;
    public float spacing = 5;
    public float overlapRadius;
    public float emptyRadius;
    private List<Vector3> positions;
    private LightProbeGroup lightProbeGroup;
    void OnEnable()
    {
        lightProbeGroup = this.GetComponent<LightProbeGroup>();
    }
    [ContextMenu("Create Light Probes")]
    public void CreateLightProbes()
    {
        positions = new List<Vector3>();

        for (float x = -XRange; x < XRange; x += spacing)
        {
            for (float y = -YMin; y < YMax; y += spacing)
            {
                for (float z = -ZRange; z < ZRange; z += spacing)
                {
                    Vector3 pos = new Vector3(x, y + YOffset, z);
                    if (!IsOverlappingColliders(pos, overlapRadius) && IsOverlappingColliders(pos, emptyRadius))
                        positions.Add(pos);
                }
            }
        }

        lightProbeGroup.probePositions = positions.ToArray();
    }

    private bool IsOverlappingColliders(Vector3 pos, float radius)
    {
        bool contains = Physics.OverlapSphere(pos, radius).Length != 0;
        return contains;
    }
}
#endif