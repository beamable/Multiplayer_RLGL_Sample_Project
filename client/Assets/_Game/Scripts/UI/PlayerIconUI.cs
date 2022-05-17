using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    
    public void SetIcon(Sprite icon)
    {
        iconImage.sprite = icon;
    }
}
