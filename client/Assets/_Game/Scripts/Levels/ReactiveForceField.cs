using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public class ReactiveForceField : MonoBehaviour
    {

        private List<Vector4> collisions = new List<Vector4>();
        private Vector4[] positions = new Vector4[256];
        private List<float> collisionTimers = new List<float>();
        private float[] timers = new float[256];
        public new Renderer renderer;
        private bool CanCollide = true;
        private float colliderTimer = 0f;
        public float speed = 5f;
        public float interval = 2f;
        public float velocityThreshold = 5f;
        void Update()
        {
            if (collisionTimers.Count > 0)
            {
                for (int i = collisionTimers.Count - 1; i >= 0; i--)
                {
                    if (collisionTimers[i] > 0)
                        collisionTimers[i] -= Time.deltaTime * speed;
                    else
                    {
                        collisionTimers.RemoveAt(i);
                        collisions.RemoveAt(i);
                    }

                }

                positions = collisions.ToArray();
                timers = collisionTimers.ToArray();

                System.Array.Resize<Vector4>(ref positions, 1023);
                System.Array.Resize<float>(ref timers, 1023);


                renderer.material.SetFloatArray("_Timers", timers);
                renderer.material.SetVectorArray("_Positions", positions);
            }

            renderer.material.SetInt("_PositionCount", collisions.Count);



            if (!CanCollide)
            {
                colliderTimer += Time.deltaTime;
                if (colliderTimer > interval)
                {
                    CanCollide = true;
                }
            }

        }

        void OnCollisionEnter(Collision collision)
        {
            if (CanCollide)
            {
                if (collision.transform.root.TryGetComponent(out CharacterSoundManager characterSound))
                {
                    characterSound.PlaySound("ForceFieldHit");
                }

                if (collision.transform.root.TryGetComponent(out CharacterFXManager characterFX) && collision.contactCount > 0)
                {
                    characterFX.PlayEffectAtPosition("Basic Hit", collision.contacts[0].point);
                }


                foreach (var contact in collision.contacts)
                {
                    Vector3 point = contact.point;

                    if (collisions.Count < 256)
                    {

                        collisions.Add(point);
                        collisionTimers.Add(collision.relativeVelocity.magnitude / velocityThreshold);
                    }

                    CanCollide = false;
                    colliderTimer = 0f;
                }
            }

        }
    }
}
