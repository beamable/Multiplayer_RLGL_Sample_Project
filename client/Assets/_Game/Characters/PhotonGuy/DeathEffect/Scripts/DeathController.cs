using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BeamableExample.RedlightGreenLight
{

    public class DeathController : MonoBehaviour
    {
        public DeathControllerVisuals visuals;

        [SerializeField]
        private float crumbleParticleEmission = 150f;
        [SerializeField]
        private float characterHeight = 1.8f;
        [SerializeField]
        private float smokeDissolveOffset = 1.5f;
        [SerializeField]
        private float timeToDissolve = 3f;
        private RaycastHit hit;
        private Vector3 hitPoint = Vector3.zero;
        private bool dissolve = false;
        private float dissolveTimer = 0f;
        private RedLightManager _redLightManager;
        private ParticleSystem.EmissionModule crumbleEmission;

        private void Start()
        {
            _redLightManager = FindObjectOfType<RedLightManager>();
        }
        public void Update()
        {
            if (hitPoint != Vector3.zero)
                visuals.ashPile.transform.position = hitPoint;
            if (dissolve && dissolveTimer <= timeToDissolve)
            {
                dissolveTimer += Time.deltaTime * 6f;

                foreach (Renderer rend in visuals.characterRenderers)
                {
                    rend.material.SetFloat("_VerticalCutoff", dissolveTimer);
                }

                foreach (Renderer renderer in visuals.hairRenderers)
                {
                    if (renderer.gameObject.activeInHierarchy)
                        renderer.material.SetFloat("_VerticalCutoff", dissolveTimer);
                }

                foreach (LineRenderer renderer in visuals.smokeRenderers)
                {
                    renderer.material.SetFloat("_VerticalCutoff", dissolveTimer - smokeDissolveOffset);
                }
            }
            //once I am no longer visable disable smoke for performance
            else if (dissolveTimer >= timeToDissolve)
            {
                if(visuals.smokeRenderers[0].transform.parent.gameObject.activeSelf == true)
                {
                    visuals.smokeRenderers[0].transform.parent.gameObject.SetActive(false);
                }
            }
        }

        public void SetBurned(float value)
        {
            Vector3 thisPos = this.transform.position + Vector3.up * 1.3f;

            foreach (Renderer rend in visuals.characterRenderers)
            {
                rend.material.SetFloat("_IsBurned", value);
            }

            foreach (Renderer renderer in visuals.hairRenderers)
            {
                if (renderer.gameObject.activeInHierarchy)
                    renderer.material.SetFloat("_IsBurned", value);
            }
            visuals.cartoonExplosion.transform.position = thisPos;
            visuals.cartoonExplosion.Play();


            if (_redLightManager != null && visuals.laserObject)
            {
                float magnitude = (_redLightManager.aimLOC.position - thisPos).magnitude;
                Vector3 midPos = (_redLightManager.aimLOC.position + thisPos) * 0.5f;



                visuals.laserObject.transform.up = (_redLightManager.aimLOC.position - thisPos).normalized;
                visuals.laserObject.transform.localScale = new Vector3(1, magnitude, 1);
                visuals.laserObject.transform.position = midPos;

                visuals.laserObject.SetActive(true);
            }

        }

        public void EnableAshExplosion()
        {
            visuals.ashExplosion.gameObject.SetActive(true);
        }

        public void EnableCharredSmoke()
        {
            visuals.smokeRenderers[0].transform.parent.gameObject.SetActive(true);
        }

        public void DisableLaserObject()
        {
            visuals.laserObject.SetActive(false);
        }

        public void DropWeapon()
        {
            return;
            foreach (GameObject obj in visuals.weaponRenderers)
            {
                if (obj.activeInHierarchy)
                {
                    obj.AddComponent<MeshCollider>().convex = true;
                    obj.AddComponent<Rigidbody>();
                    obj.transform.SetParent(null);
                }
            }
        }

        public void StartDissolve()
        {
            dissolve = true;
        }

        public void SetAshPileToGround()
        {
            // LayerMask layerMask = LayerMask.NameToLayer("Ground");
            if (Physics.Raycast(new Ray(this.transform.position, Vector3.down), out hit, 10f))
            {
                hitPoint = hit.point;
            }
        }

        public void EnableAshPile(int frames)
        {
            visuals.ashPile.gameObject.SetActive(true);

            Debug.Log(frames);

            StartCoroutine(AshPileAnimation(frames));
        }

        private IEnumerator AshPileAnimation(int frames)
        {
            for (float t = 0; t < frames / visuals.animationFrameRate; t += Time.deltaTime)
            {
                float scale = Mathf.Lerp(0, 1f, t / (frames / visuals.animationFrameRate));

                visuals.ashPile.transform.localScale = new Vector3(scale, scale, scale);

                Debug.Log(t / (frames / visuals.animationFrameRate) + "% done ashpile");

                yield return null;
            }

            //enable smoke
            visuals.ashPile.transform.GetChild(1).gameObject.SetActive(true);
        }


        public void StartCrumbleParticles(int frames)
        {
            Debug.Log(frames);

            visuals.crumbleParticles.transform.gameObject.SetActive(true);

            crumbleEmission = visuals.crumbleParticles.emission;
            crumbleEmission.rateOverTime = crumbleParticleEmission;

            StartCoroutine(CrumbleAnimation(frames));
        }

        private IEnumerator CrumbleAnimation(int frames)
        {
            for (float t = 0; t < frames / visuals.animationFrameRate; t += Time.deltaTime)
            {
                //move up
                float yPos = Mathf.Lerp(0, characterHeight, t / (frames / visuals.animationFrameRate));

                visuals.crumbleParticles.transform.position = new Vector3(0, yPos, 0);

                Debug.Log(t / (frames / visuals.animationFrameRate) + "% done crumble");

                yield return null;
            }

            crumbleEmission.rateOverTime = 0f;          
        }
    }

}