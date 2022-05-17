using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

using BeamableExample.Helpers;

namespace BeamableExample.RedlightGreenLight
{
    public enum VirtualCameraType
    {
        FOLLOW,
        SPECTATOR
    }

    public class CinemachineManager : SingletonNonPersistant<CinemachineManager>
    {
        [Header("Cinemachine")]
        [Tooltip("The Virtual Camera to use")]
        public CinemachineVirtualCamera spectatorCamera;

        public CinemachineVirtualCamera thirdPersonCamera;
    }
}
