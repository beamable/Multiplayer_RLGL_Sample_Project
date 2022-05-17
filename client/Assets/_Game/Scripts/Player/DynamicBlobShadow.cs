using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DynamicBlobShadow : MonoBehaviour
{
    public enum DirectionType { DirectionalLight, SourceObject };
    public DirectionType directionType = DirectionType.SourceObject;
    public float shadowScale = 1f;
    private Light directionalLight;
    private GameObject sourceObject;
    private Vector3 lastDirectionalLightRotation;
    private RaycastHit groundHit;
    private Ray groundRay;
    //private float groundOffset = 0f;
    private UnityEngine.LayerMask layerMask;
    private new Renderer renderer;
    private void OnEnable()
    {
        layerMask = UnityEngine.LayerMask.GetMask("Ground");
        renderer = this.GetComponent<Renderer>();
        if (directionType == DirectionType.DirectionalLight)
        {

            Light[] lights = FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    directionalLight = lights[i];
                    lastDirectionalLightRotation = directionalLight.transform.eulerAngles;
                    break;
                }

            }

        }
        else if (directionType == DirectionType.SourceObject)
        {
            sourceObject = GameObject.FindGameObjectWithTag("MainLight");
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {

        //Set Rotation/Position/Scale for shadow quad (relative to angle of the directional light)
        if (directionType == DirectionType.DirectionalLight)
        {
            if (directionalLight)
            {

                Vector3 lookTarget = this.transform.position + directionalLight.transform.forward;
                lookTarget.y = this.transform.position.y;
                Quaternion lookRotation = Quaternion.LookRotation(lookTarget - this.transform.position);
                this.transform.rotation = lookRotation * Quaternion.Euler(90, 0, 0);

                float scalar = 1f;

                if (directionalLight.transform.localEulerAngles.x <= 90f)
                    scalar = Mathf.Lerp(3f, 1f, directionalLight.transform.localEulerAngles.x / 90f);
                else
                    scalar = Mathf.Lerp(3f, 1f, (90f - (directionalLight.transform.localEulerAngles.x - 270f)) / 90f);

                this.transform.localScale = new Vector3(shadowScale, scalar * shadowScale, shadowScale);
                this.transform.localPosition = Vector3.zero;
                this.transform.position += this.transform.up * ((scalar - 1f) * 0.33f);

            }
        }
        else if (directionType == DirectionType.SourceObject)
        {
            if (sourceObject)
            {
                Vector3 lightForward = (this.transform.parent.position - sourceObject.transform.position).normalized;
                Vector3 lightEuler = Quaternion.FromToRotation(Vector3.up, lightForward).eulerAngles;

                Vector3 lookTarget = this.transform.position + (lightForward);
                lookTarget.y = this.transform.position.y;
                Quaternion lookRotation = Quaternion.LookRotation(lookTarget - this.transform.position);
                this.transform.rotation = lookRotation * Quaternion.Euler(90, 0, 0);

                float scalar = 1f;

                if (lightEuler.x <= 90f)
                    scalar = Mathf.Lerp(1f, 3f, lightEuler.x / 90f);
                else
                    scalar = Mathf.Lerp(1f, 3f, (90f - (lightEuler.x - 270f)) / 90f);

                this.transform.localScale = new Vector3(shadowScale, scalar * shadowScale, shadowScale);
                this.transform.localPosition = Vector3.zero;


                groundRay = new Ray(this.transform.parent.position, lightForward);

                if (Physics.Raycast(this.transform.parent.position + Vector3.up * 0.5f, Vector3.down, out groundHit, 1f, layerMask))
                {
                    Vector3 offset = groundHit.point + groundHit.normal * 0.025f;
                    this.transform.position = offset;
                    Quaternion lookRot = Quaternion.LookRotation(groundHit.normal, this.transform.up);
                    this.transform.rotation = lookRot;
                    renderer.enabled = true;
                }
                else if (Physics.Raycast(groundRay, out groundHit, 10f, layerMask))
                {
                    Vector3 offset = groundHit.point + groundHit.normal * 0.025f;
                    this.transform.position = offset;
                    Quaternion lookRot = Quaternion.LookRotation(groundHit.normal, this.transform.up);
                    this.transform.rotation = lookRot;
                    renderer.enabled = true;
                }
                else
                {
                    renderer.enabled = false;
                }
                this.transform.position += this.transform.up * ((scalar - 1f) * 0.33f);


            }
        }

    }
}
