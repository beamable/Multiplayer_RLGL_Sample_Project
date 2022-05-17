using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TabInput : MonoBehaviour
{
    public Selectable[] selectables;
    private int selectedIndex = 0;
    UnityEvent selectedEvent = new UnityEvent();

    void Update()
    {
        if (Keyboard.current[Key.Tab].wasReleasedThisFrame)
        {
            UpdateCurrentSelected(true);
        }
        else if (Keyboard.current[Key.LeftShift].isPressed && Keyboard.current[Key.Tab].wasReleasedThisFrame)
        {
            UpdateCurrentSelected(false);
        }
    }

    void UpdateCurrentSelected(bool up)
    {
        if (selectables.Length == 0 || selectables == null) return;


        if (up)
        {
            if (selectedIndex < selectables.Length - 1)
                selectedIndex++;
            else
                selectedIndex = 0;

        }
        else
        {
            if (selectedIndex > 0)
                selectedIndex--;
            else
                selectedIndex = selectables.Length - 1;
        }

        selectables[selectedIndex].Select();
    }

}
