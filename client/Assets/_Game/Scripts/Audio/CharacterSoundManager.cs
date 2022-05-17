using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    [RequireComponent(typeof(AudioSource))]
    public class CharacterSoundManager : MonoBehaviour
    {
        public GameSound[] sounds;
        private AudioSource audioSource;
        private Camera mainCamera;
        private GameObject cameraListener;
        private void OnEnable()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
        public void PlaySound(string sound)
        {
            int index = Array.FindIndex(sounds, x => x.soundName == sound);
            if (index > -1)
                PlaySound(sounds[index]);
        }

        public void PlaySound(GameSound gameSound)
        {
            if (audioSource && gameSound != null && gameSound.audioClip)
            {
                audioSource.PlayOneShot(gameSound.audioClip);
            }
        }
        public void PlaySoundAtPosition(GameSound gameSound, Vector3 position)
        {
            if (audioSource && gameSound != null && gameSound.audioClip)
            {
                AudioSource.PlayClipAtPoint(gameSound.audioClip, position);
            }
        }

    }

}

