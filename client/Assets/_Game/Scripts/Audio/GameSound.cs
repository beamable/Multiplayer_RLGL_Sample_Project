using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
	[CreateAssetMenu(fileName = "Game Sound", menuName = "ScriptableObjects/Game Sound")]
    public class GameSound : ScriptableObject
    {
        public AudioClip audioClip;
        public CharacterFaceState faceState;
        public string soundName;
    }
}
