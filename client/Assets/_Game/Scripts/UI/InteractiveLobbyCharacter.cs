using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace BeamableExample.RedlightGreenLight
{
    public class InteractiveLobbyCharacter : MonoBehaviour
    {
        private NetworkMecanimAnimator networkAnimator;
        private Animator animator;
        [SerializeField] private float mouseRotationSpeed;
        [SerializeField] private float rotationSpeed = 1f;
        [SerializeField] private float dizzyThreshold = 1f;
        private Quaternion targetRotation = Quaternion.identity;
        private void OnEnable()
        {
            networkAnimator = transform.GetComponent<NetworkMecanimAnimator>();
            animator = transform.GetComponent<Animator>();
        }

        public void RotateCharacter(float yAmount)
        {
            targetRotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + yAmount * mouseRotationSpeed, transform.localEulerAngles.z));

            if (yAmount > dizzyThreshold)
            {
                if (networkAnimator)
                {
                    networkAnimator.Animator.SetBool("Spinning", true);
                }
                else if (animator)
                {
                    animator.SetBool("Spinning", true);
                }
            }
            else
            {
                if (networkAnimator)
                {
                    networkAnimator.Animator.SetBool("Spinning", false);
                }
                else if (animator)
                {
                    animator.SetBool("Spinning", false);
                }
            }
        }

        private void Update()
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

    }
}
