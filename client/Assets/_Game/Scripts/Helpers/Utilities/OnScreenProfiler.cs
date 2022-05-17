using TMPro;
using UnityEngine;

public class OnScreenProfiler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI frameRate;

    private void Update()
    {
        UpdateFrameRate();
    }

    private void UpdateFrameRate()
    {
        if (frameRate == null) return;
        frameRate.text = $"{(int)(Time.frameCount / Time.time)} FPS";
    }
}