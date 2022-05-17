using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingGameTipsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameTipText;
    [SerializeField] private List<string> gameTips;
    [SerializeField] private float tipTime = 2f;

    private bool _runCoroutine;

    public void OnEnable()
    {
        _runCoroutine = true;
        StartCoroutine(SetGameTip());
    }

    private IEnumerator SetGameTip()
    {
        var index = 0;
        while (_runCoroutine)
        {
            gameTipText.text = gameTips[index];
            yield return new WaitForSeconds(tipTime);
            index++;
            if (index == gameTips.Count)
            {
                index = 0;
            }
        }
    }

    private void OnDestroy()
    {
        _runCoroutine = false;
    }
}
