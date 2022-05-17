using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace BeamableExample.RedlightGreenLight
{
    public class CharacterFXManager : MonoBehaviour
    {
        public CharacterFXVisuals visuals;
        public void EnableEffect(string effect)
        {
            visuals.EnableEffect(effect);
        }
        public void PlayEffectAtPosition(string effect, Vector3 position)
        {
            visuals.PlayEffectAtPosition(effect, position);
        }

        public void PlayParticleEffect(string effect)
        {
            visuals.PlayParticleEffect(effect);
        }
        public void DisableEffect(string effect)
        {
            visuals.DisableEffect(effect);
        }
        public void DisableAllEffects()
        {
            visuals.DisableAllEffects();
        }


    }




}
