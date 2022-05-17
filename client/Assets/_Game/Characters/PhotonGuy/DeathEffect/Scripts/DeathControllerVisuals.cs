using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BeamableExample.RedlightGreenLight
{
    public class DeathControllerVisuals : MonoBehaviour
    {
        public GameObject ashPile;
        public GameObject laserObject;
        public ParticleSystem cartoonExplosion;
        public Renderer[] hairRenderers;
        public LineRenderer[] smokeRenderers;
        public GameObject[] weaponRenderers;
        public Renderer[] characterRenderers;
        public ParticleSystem crumbleParticles;
        public ParticleSystem ashExplosion;
        public float animationFrameRate = 60;
    }
}