using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysicsSimulationHelper : MonoBehaviour
{
    private void OnEnable()
    {
        // BUG:: this is causing issues with Fusion physics simulation loop.
        // This needs to be false during gameplay and true during the menu.
        Physics.autoSimulation = true;
    }

    private void OnDisable()
    {
        // BUG:: this is causing issues with Fusion physics simulation loop.
        // This needs to be false during gameplay and true during the menu.
        Physics.autoSimulation = false;
    }
}
