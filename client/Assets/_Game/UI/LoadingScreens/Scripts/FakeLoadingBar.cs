using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FakeLoadingBar : MonoBehaviour
{
    [SerializeField] private Image fillBar;

    private bool _runCoroutine;

    private void OnEnable()
    {
        fillBar.fillAmount = 0;
        _runCoroutine = true;
        StartCoroutine(LoopLoadBar());
    }

    private IEnumerator LoopLoadBar()
    {
        while (_runCoroutine)
        {
            if (fillBar.fillOrigin == 0)
            {
                if (fillBar.fillAmount < 1)
                {
                    fillBar.fillAmount += .1f;
                    yield return new WaitForSeconds(.05f);
                }
                else
                {
                    fillBar.fillOrigin = 1;
                }
            }
            else
            {
                if (fillBar.fillAmount > 0)
                {
                    fillBar.fillAmount -= .1f;
                    yield return new WaitForSeconds(.05f);
                }
                else
                {
                    fillBar.fillOrigin = 0;
                }
            }
        }
    }

    private void OnDestroy()
    {
        _runCoroutine = false;
    }
}
