using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

namespace BeamableExample.RedlightGreenLight
{
    [RequireComponent(typeof(AudioSource))]
    public class LevelSoundManager : NetworkBehaviour
    {
        [SerializeField] private AudioClip introLoop;
        [SerializeField] private AudioClip mainLoop;
        [SerializeField] private AudioClip countdownTone;
        [SerializeField] private AudioClip stopTone;
        [SerializeField] private AudioClip goTone;
        [SerializeField] private List<AudioSource> alternatingAudioSources;
        private int _alternatingAudioSourceIndex = 0;
        private AudioSource currentAudioSource = null;
        private AudioSource previousAudioSource = null;
        public enum MusicTransitionState { Ready, IntroLoop, BeginTransition, Transitioning, MainLoop }
        public MusicTransitionState musicTransitionState = MusicTransitionState.Ready;
        public static LevelSoundManager Instance { get; private set; }

        [Networked] float _syncedTrackTime { get; set; }

        public delegate void Callback();

        public void InitNetworkState()
        {

        }

        public override void Spawned()
        {
            currentAudioSource = alternatingAudioSources[_alternatingAudioSourceIndex];
            if (Object.HasStateAuthority)
            {
                currentAudioSource.Stop();
                SetClipAndPlay(currentAudioSource, introLoop);
                _syncedTrackTime = 0.0f;
            }
            else
            {
                currentAudioSource.Stop();
                SetClipAndPlayTime(currentAudioSource, introLoop, _syncedTrackTime);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
                _syncedTrackTime = currentAudioSource.time;
        }

        private void OnEnable()
        {
            if (Instance == null) Instance = this;

        }

        private void PlayOneShot(AudioClip clip)
        {
            currentAudioSource.PlayOneShot(clip);
        }

        private void SetClipAndPlay(AudioSource audioSource, AudioClip clip)
        {
            if (audioSource)
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
        private void SetClipAndPlayTime(AudioSource audioSource, AudioClip clip, float time)
        {
            if (audioSource)
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.time = time;
                audioSource.Play();
            }
        }
        private void SetClipAndPlayScheduled(AudioSource audioSource, AudioClip clip, double time)
        {
            if (audioSource)
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.PlayScheduled(time);
            }
        }

        private void IterateAudioSource()
        {
            previousAudioSource = currentAudioSource;

            if (_alternatingAudioSourceIndex < alternatingAudioSources.Count - 1)
                _alternatingAudioSourceIndex++;
            else
                _alternatingAudioSourceIndex = 0;

            currentAudioSource = alternatingAudioSources[_alternatingAudioSourceIndex];
        }

        public IEnumerator TransitionToMainLoop(Callback callback)
        {
            float timeRemaining = introLoop.length - currentAudioSource.time;
            currentAudioSource.loop = false;

            IterateAudioSource();
            SetClipAndPlayScheduled(currentAudioSource, mainLoop, AudioSettings.dspTime + (double)timeRemaining);

            yield return new WaitForSeconds(timeRemaining);

            yield return new WaitForSeconds(3.0f);

            HUDAlertUI.Instance.DisplayAlert("Get to the center!");

            previousAudioSource.Stop();
            callback?.Invoke();
        }

        public void PlayCountdownTone()
        {
            PlayOneShot(countdownTone);
        }

        public void PlayGoTone()
        {
            PlayOneShot(goTone);
        }

        public void PlayStopTone()
        {
            PlayOneShot(stopTone);
        }




    }
}
