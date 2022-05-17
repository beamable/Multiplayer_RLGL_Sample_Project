using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.UI
{
    public class PlatformSpecificObject : MonoBehaviour
    {
        [SerializeField]
        private List<RuntimePlatform> targetPlatforms;

        private void Awake()
        {
            if (!targetPlatforms.Contains(Application.platform))
            {
                gameObject.SetActive(false);
            }
        }
    }
}