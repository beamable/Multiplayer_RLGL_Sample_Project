using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace BeamableExample.RedlightGreenLight
{
    [RequireComponent(typeof(AudioListener))]
    public class CinemachineAudioListener : MonoBehaviour
    {
        [SerializeField]
        private CinemachineBrain cinemachineBrain;
        private Transform sourceTransform;
        private Transform targetTransform;
        [Range(0, 1)]
        public float interpolationFactor = 0.666f;
        void OnEnable()
        {
            Initialize();
        }

        void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            if (cinemachineBrain.ActiveVirtualCamera == null) return;

            if (cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject)
                sourceTransform = cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.transform;
            if (cinemachineBrain.ActiveVirtualCamera.Follow)
                targetTransform = cinemachineBrain.ActiveVirtualCamera.Follow;
        }

        void LateUpdate()
        {
            if (!sourceTransform || !targetTransform) Initialize();
            if (sourceTransform && targetTransform)
            {
                transform.position = Vector3.Lerp(sourceTransform.position, targetTransform.position, interpolationFactor);
            }
        }
    }
}