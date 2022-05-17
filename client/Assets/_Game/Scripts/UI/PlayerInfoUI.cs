using TMPro;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public class PlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private Transform _lookTarget;
        [SerializeField] private CanvasGroup _readupCanvasGroup;
        [SerializeField] private CanvasGroup _nameCanvasGroup;
        [SerializeField] private TextMeshProUGUI _nameText;

        // Start is called before the first frame update
        void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            if (_lookTarget == null)
                return;

            if (_readupCanvasGroup.alpha > 0.0f || _nameCanvasGroup.alpha > 0.0f)
            {
                Vector3 lookVec = Vector3.ProjectOnPlane(this.transform.position - _lookTarget.position, Vector3.up);
                this.transform.rotation = Quaternion.LookRotation(lookVec);
            }
        }

        public void Initialize()
        {
            ShowReady(false);
        }
        public void ShowReady(bool show)
        {
            _readupCanvasGroup.alpha = show ? 1.0f : 0.0f;
        }

        public void ShowName(bool show)
        {
            _nameCanvasGroup.alpha = show ? 1.0f : 0.0f;
        }

        public void SetLookTarget(Transform target)
        {
            _lookTarget = target;
        }

        public void SetNameText(string name)
        {
            _nameText.text = name;
        }
    }
}
