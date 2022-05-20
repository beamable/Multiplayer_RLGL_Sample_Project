using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private float _scrollSpeed = 1f;

    public UnityEvent OnScrollComplete;

    public void OnEnable()
    {
        _scrollRect.content.localPosition = new Vector3(0,
            _scrollRect.transform.localPosition.y - _scrollRect.viewport.rect.height - _scrollRect.content.rect.height/2, 0);
        StartCoroutine(Scroll());
    }

    public IEnumerator Scroll()
    {
        while (_scrollRect.content.localPosition.y <
               _scrollRect.viewport.rect.height / 2 + _scrollRect.content.rect.height / 2)
        {
            _scrollRect.content.localPosition = new Vector3(0, _scrollRect.content.localPosition.y + _scrollSpeed, 0);
            yield return null;
        }
        OnScrollComplete?.Invoke();
    }
}
