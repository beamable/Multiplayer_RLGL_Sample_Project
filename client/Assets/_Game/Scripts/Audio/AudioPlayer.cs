using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
	[RequireComponent(typeof(AudioSource))]
	public class AudioPlayer : MonoBehaviour
	{
		[SerializeField] private AudioClipData audioClip;
		private AudioSource audioSource;

		private void OnEnable()
		{
			if (audioSource == null)
				audioSource = GetComponent<AudioSource>();
		}

		private void OnDisable()
		{
			Stop();
		}

		public void Play()
		{
			PlayClip();
		}

		public void Stop()
		{
			StopClip();
		}

		public void PlayOneShot()
		{
			SetAudioClipAndPitch();
			audioSource.PlayOneShot(audioSource.clip);
		}

		public void PlayOneShot(AudioClipData audioClip)
		{
			if (audioClip == null)
				return;

			this.audioClip = audioClip;
			PlayOneShot();
		}

		private void PlayClip()
		{
			SetAudioClipAndPitch();
			audioSource.Play();
		}

		private void SetAudioClipAndPitch()
		{
			audioSource.clip = audioClip.GetAudioClip();
			audioSource.pitch = audioClip.GetPitchOffset();
		}

		private void StopClip()
		{
			audioSource.Stop();
		}
	}
}