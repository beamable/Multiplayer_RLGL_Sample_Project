using System.Collections;
using System.Collections.Generic;
using BeamableExample.RedlightGreenLight.Character;
using UnityEngine;
namespace BeamableExample.RedlightGreenLight
{
    public class CullingManager : MonoBehaviour
    {
        private static CullingGroup cullingGroup;
        private static List<BoundingSphere> cullingBounds;
        public static void AddCullingBounds()
        {
            if (cullingGroup == null)
            {
                cullingGroup = new CullingGroup();
                cullingGroup.targetCamera = Camera.main;
                cullingGroup.SetBoundingDistances(new float[] { 25, 150 });
                cullingGroup.SetDistanceReferencePoint(Camera.main.transform);
                cullingGroup.onStateChanged = OnLodCullingGroupOnStateChangedHandler;
                cullingBounds = new List<BoundingSphere>();
                Debug.Log("Culling bounds added");
            }

        }

        public static void Clear()
        {
            cullingGroup = null;
            cullingBounds = null;
        }

        public static void AddPlayerToCulling(PlayerCharacter player)
        {
            if (cullingGroup == null)
                AddCullingBounds();
            if (cullingGroup != null)
            {
                if (!player.IsLocal)
                {
                    cullingBounds.Add(new BoundingSphere(player.transform.position, 1f));
                    cullingGroup.SetBoundingSpheres(cullingBounds.ToArray());
                    cullingGroup.SetBoundingSphereCount(PlayerManager.allPlayers.Count);
                }
            }
        }
        public static void RemovePlayerFromCulling(PlayerCharacter player)
        {
            if (cullingGroup != null)
            {
                int index = PlayerManager.allPlayers.IndexOf(player);
                cullingBounds.RemoveAt(index);

                cullingGroup.SetBoundingSpheres(cullingBounds.ToArray());
                cullingGroup.SetBoundingSphereCount(PlayerManager.allPlayers.Count);
            }
        }
        public static void UpdateCullingBounds()
        {
            if (cullingGroup != null && cullingBounds != null)
            {
                int numPlayers = PlayerManager.allPlayers.Count - 1;

                if (numPlayers == 0 || numPlayers > cullingBounds.Count) return;

                for (int i = 0; i < numPlayers; ++i)
                {
                    PlayerCharacter player = PlayerManager.allPlayers[i];
                    BoundingSphere bounds = cullingBounds[i];
                    bounds.position = player.transform.position;
                    bounds.radius = 1f;
                    cullingBounds[i] = bounds;
                }
            }
        }

        public static void DestroyCullingGroup()
        {
            if (cullingGroup != null)
            {
                cullingGroup.Dispose();
                cullingGroup = null;
            }
        }

        public void OnDestroy()
        {
            DestroyCullingGroup();
        }

        private static void OnLodCullingGroupOnStateChangedHandler(CullingGroupEvent evt)
        {
            if (evt.index > -1 && evt.index < PlayerManager.allPlayers.Count)
            {
                PlayerCharacter player = PlayerManager.allPlayers[evt.index];
                if (player != null && player.characterPhysics != null)
                    player.characterPhysics.SetLOD(evt.currentDistance, evt.isVisible);
            }

        }
    }
}