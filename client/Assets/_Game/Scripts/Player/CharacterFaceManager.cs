using System.Collections;
using System.Collections.Generic;
using BeamableExample.RedlightGreenLight.Character;
using Fusion;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public enum CharacterFaceState { Angry, Shades, Smile, AngrySmile, Confused, SadisticSmile, Exclamation, Error, Excited, Shocked, Melting, Scared, Tired, Disgusted, Warning, Sad }
    [ExecuteInEditMode]
    public class CharacterFaceManager : MonoBehaviour
    {
        public CharacterFaceState inspectorFaceState = CharacterFaceState.Smile;
        private CharacterFaceState faceState = CharacterFaceState.Smile;
        public Renderer faceRenderer;
        public float restTimer = 0f;
        public float restThreshold = 1f;//Time to neutral
        private bool IsEmoting = false;
 
        public void Update()
        {
            if (inspectorFaceState != faceState && !Application.isPlaying)
            {
                SetFaceState(inspectorFaceState);
            }

            if (IsEmoting && Application.isPlaying)
            {
                restTimer += Time.deltaTime;
                if (restTimer > restThreshold)
                {
                    ResetFaceState();
                }
            }


        }
        public void SetFaceState(CharacterFaceState newFaceState)
        {
            if (faceState != newFaceState)
            {
                OnFaceStateChanged(newFaceState);
            }
            faceState = newFaceState;
            inspectorFaceState = newFaceState;
            restTimer = 0f;
            IsEmoting = true;
        }

        public void ResetFaceState()
        {
            CharacterFaceState newFaceState = CharacterFaceState.Smile;
            faceState = newFaceState;
            inspectorFaceState = newFaceState;
            OnFaceStateChanged(newFaceState);
            IsEmoting = false;

        }

        public void OnFaceStateChanged(CharacterFaceState newState)
        {
            if (!faceRenderer) return;
            Vector2 offset = new Vector2(0.25f, -0.25f);//Smile default
            switch (newState)
            {
                case CharacterFaceState.Angry:
                    offset = new Vector2(-0.25f, -0.25f);
                    break;
                case CharacterFaceState.Shades:
                    offset = new Vector2(0, -0.25f);
                    break;
                case CharacterFaceState.Smile:
                    offset = new Vector2(0.25f, -0.25f);
                    break;
                case CharacterFaceState.AngrySmile:
                    offset = new Vector2(0.5f, -0.25f);
                    break;
                case CharacterFaceState.Confused:
                    offset = new Vector2(-0.25f, -0.5f);
                    break;
                case CharacterFaceState.SadisticSmile:
                    offset = new Vector2(0f, -0.5f);
                    break;
                case CharacterFaceState.Exclamation:
                    offset = new Vector2(0.25f, -0.5f);
                    break;
                case CharacterFaceState.Error:
                    offset = new Vector2(0.5f, -0.5f);
                    break;
                case CharacterFaceState.Excited:
                    offset = new Vector2(-0.25f, -0.75f);
                    break;
                case CharacterFaceState.Shocked:
                    offset = new Vector2(-0.0f, -0.75f);
                    break;
                case CharacterFaceState.Melting:
                    offset = new Vector2(0.25f, -0.75f);
                    break;
                case CharacterFaceState.Scared:
                    offset = new Vector2(0.5f, -0.75f);
                    break;
                case CharacterFaceState.Tired:
                    offset = new Vector2(-0.25f, -1f);
                    break;
                case CharacterFaceState.Disgusted:
                    offset = new Vector2(-0.0f, -1f);
                    break;
                case CharacterFaceState.Warning:
                    offset = new Vector2(0.25f, -1f);
                    break;
                case CharacterFaceState.Sad:
                    offset = new Vector2(0.5f, -1f);
                    break;
            }
            if (Application.isPlaying)
                faceRenderer.material.SetVector("_FaceOffset", offset);
            else
                faceRenderer.sharedMaterial.SetVector("_FaceOffset", offset);
        }

    }
}
