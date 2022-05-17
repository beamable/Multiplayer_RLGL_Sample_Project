using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class TriggerAnimations : MonoBehaviour
{
    public NetworkMecanimAnimator NAnimator;

    public void KillWithLaser()
    {
        NAnimator.Animator.SetTrigger("DeathByLaser");
    }
}
