using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace BeamableExample.RedlightGreenLight
{
    public class CharacterFXVisuals : MonoBehaviour
    {
        public List<CharacterEffect> effects = new List<CharacterEffect>();
        private Vector3 targetPosition;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            foreach (CharacterEffect effect in effects)
            {
                effect.Update();
                effect.targetPosition = targetPosition;
            }
        }

        public void EnableEffect(string _effect)
        {
            CharacterEffect effect = effects.Find(e => e.id == _effect);
            if (effect != null)
            {
                effect.SetActive(true);
            }

        }
        public void DisableEffect(string _effect)
        {
            CharacterEffect effect = effects.Find(e => e.id == _effect);
            if (effect != null)
                effect.SetActive(false);
        }
        public void DisableAllEffects()
        {
            foreach(CharacterEffect effect in effects)
            {
                effect.SetActive(false);
            }
        }

        public void PlayEffectAtPosition(string _effect, Vector3 position)
        {
            CharacterEffect effect = effects.Find(e => e.id == _effect);
            if (effect != null)
            {
                effect.targetPosition = position;
                effect.SetActive(true);
            }
        }
        public void PlayParticleEffect(string _effect)
        {
            CharacterEffect effect = effects.Find(e => e.id == _effect);
            if (effect != null)
            {
                effect.targetPosition = effect.particleSystem.transform.position;
                effect.SetActive(true);
            }
        }
    }
    [System.Serializable]
    public enum CharacterEffectType { TrailEffect, HitEffect }
    [System.Serializable]
    public class CharacterEffect
    {
        [HideInInspector]
        [SerializeField] public bool enabled = false;
        [SerializeField] public string id = "";
        [SerializeField] public bool foldout = true;
        [SerializeField] public CharacterEffectType type;
        [SerializeField] public TrailRenderer trailRenderer;
        [SerializeField] public ParticleSystem particleSystem;
        [SerializeField] public Vector3 targetPosition;
        [SerializeField] public GameObject targetObject;
        [SerializeField] public Vector3 targetOffset;
        [SerializeField] public Color effectColor;
        [SerializeField] public Gradient colorGradient;
        [SerializeField] public AnimationCurve trailWidth;
        public void Update()
        {
            if (enabled)
            {
                switch (type)
                {
                    case CharacterEffectType.TrailEffect:
                        SetTrailRendererTransform();
                        break;
                    case CharacterEffectType.HitEffect:
                        break;
                }
            }
        }
        public void SetActive(bool active)
        {
            switch (type)
            {
                case CharacterEffectType.TrailEffect:

                    SetTrailRendererTransform();
                    SetTrailRendererProperties();

                    trailRenderer.emitting = active;

                    break;
                case CharacterEffectType.HitEffect:

                    SetParticleSystemPosition();
                    particleSystem.Play();

                    break;
            }

            enabled = active;
        }

        private void SetTrailRendererProperties()
        {
            trailRenderer.Clear();
            trailRenderer.colorGradient = colorGradient;
            trailRenderer.widthCurve = trailWidth;
        }
        private void SetTrailRendererTransform()
        {
            trailRenderer.transform.up = targetObject.transform.up;
            trailRenderer.transform.position = targetObject.transform.position - targetObject.transform.right * targetOffset.x - targetObject.transform.up * targetOffset.y - targetObject.transform.forward * targetOffset.z;
        }
        private void SetParticleSystemPosition()
        {
            particleSystem.transform.position = targetPosition;
        }

    }
}
