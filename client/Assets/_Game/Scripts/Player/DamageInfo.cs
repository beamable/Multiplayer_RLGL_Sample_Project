using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BeamableExample.RedlightGreenLight.GameManager;

namespace BeamableExample.RedlightGreenLight
{
    public class DamageInformation
    {
        public int amount;
        public KillSource killSource;
        public WeaponType weaponType;
        public DamageModifiers damageMods;
        public PlayerRef attackingPlayerRef;
    }
}
