using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class CharacterIdleController : MonoBehaviour
{
    private NetworkMecanimAnimator networkAnimator;
    private Animator animator;
    private float previousIdleIndex = 0f;
    private float idleIndex = 0f;
    private bool DoTransition = false;
    private float transitionTimer = 0f;
    public int maxNumIdles = 7;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;
    private float currentWaitTime = 3f;
    // Start is called before the first frame update
    void OnEnable()
    {
        networkAnimator = transform.GetComponent<NetworkMecanimAnimator>();
        animator = transform.GetComponent<Animator>();
        RandomizeIdle();
    }

    // Update is called once per frame
    void Update()
    {
        if (networkAnimator)
        {
            UpdateIdleCycle(networkAnimator.Animator);
        }
        else if (animator)
        {
            UpdateIdleCycle(animator);
        }
    }

    private IEnumerator WaitForTime()
    {
        yield return new WaitForSeconds(currentWaitTime);
        currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
    }

    public void RandomizeIdle()
    {
        previousIdleIndex = idleIndex;
        idleIndex = (int)Random.Range(0, maxNumIdles);
        idleIndex = (Random.Range(0, 2) == 0) ? 0 : idleIndex;
        DoTransition = true;
        transitionTimer = 0f;
    }

    private void UpdateIdleCycle(Animator animator)
    {
        if(DoTransition)
        {
            transitionTimer += Time.deltaTime;
            animator.SetFloat("IdleIndex", Mathf.Lerp(previousIdleIndex, idleIndex, transitionTimer));
            if(transitionTimer >= 1f)
            {
                DoTransition = false;
            }
        }
    }
}
