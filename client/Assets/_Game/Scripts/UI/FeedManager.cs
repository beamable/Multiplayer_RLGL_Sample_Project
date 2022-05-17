using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using BeamableExample.RedlightGreenLight;
using UnityEngine;

public class FeedManager : MonoBehaviour
{
    public static FeedManager Instance;

    public Color highlightColor;

    [SerializeField] private GameObject _feedPrefab;
    [SerializeField] private Transform _content;
    [SerializeField] private float _dequeueSeconds = 5.0f;

    private List<FeedElement> _feedElements = new List<FeedElement>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddFeed(string message)
    {
        var feedElementGO = Instantiate(_feedPrefab, _content);
        feedElementGO.name = string.Format("FeedMessage_{0}", _feedElements.Count);

        var feedElement = feedElementGO.GetComponentInChildren<FeedElement>();
        if (feedElement != null)
        {
            _feedElements.Add(feedElement);

            feedElement.feedText.text = message;

            feedElement.OnTimerExpiredCallback += OnFeedElementTimeOut;
            feedElement.Initialize(_dequeueSeconds);
        }
    }

    private void OnFeedElementTimeOut(FeedElement feedElement)
    {
        if (_feedElements.Remove(feedElement))
        {
            Destroy(feedElement.gameObject);
        }
    }

    private void OnDestroy()
    {
        foreach (FeedElement feedElement in _feedElements)
        {
            Destroy(feedElement.gameObject);
        }
        _feedElements.Clear();
    }
}