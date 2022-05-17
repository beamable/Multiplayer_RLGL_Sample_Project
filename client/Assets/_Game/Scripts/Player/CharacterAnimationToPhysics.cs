using System.Collections;
using System.Collections.Generic;
using BeamableExample.RedlightGreenLight.Character;
using UnityEngine;
namespace BeamableExample.RedlightGreenLight
{
    public class CharacterAnimationToPhysics : MonoBehaviour
    {
        public PlayerCharacter player;
        public GameObject physicsGO;
        public Transform animationSource;
        public Transform physicsTarget;
        [System.Serializable]
        public class TransformToJoint
        {
            public ConfigurableJoint targetJoint;
            public Quaternion originalRotation;
            public Transform sourceTransform;
            public Transform targetTransform;
            public bool UsePhysics = true;
        }
        private Rigidbody[] rigidbodies;
        private Collider[] colliders;
        public float speed;
        public bool DoPhysics = true;
        public bool DoAnimate = true;
        public TransformToJoint[] transformToJoints;
        //private bool initialized = false;
        // Start is called before the first frame update
        [ContextMenu("Initialize Joints")]
        public void InitializeJoints()
        {
            ConfigurableJoint[] joints = physicsTarget.GetComponentsInChildren<ConfigurableJoint>();
            Transform[] sourceTransforms = animationSource.GetComponentsInChildren<Transform>();
            Transform[] targetTransforms = physicsTarget.GetComponentsInChildren<Transform>();

            transformToJoints = new TransformToJoint[sourceTransforms.Length];

            for (int i = 0; i < sourceTransforms.Length; i++)
            {
                TransformToJoint transformToJoint = new TransformToJoint();
                for (int j = 0; j < joints.Length; j++)
                {
                    if (sourceTransforms[i].name == joints[j].transform.name)
                    {
                        transformToJoint.targetJoint = joints[j];
                        transformToJoint.originalRotation = Quaternion.Inverse(joints[j].transform.localRotation);
                    }
                }
                transformToJoint.sourceTransform = sourceTransforms[i];
                transformToJoint.targetTransform = targetTransforms[i];
                transformToJoints[i] = transformToJoint;
            }

        }

        [ContextMenu("DisableAllPhysics")]
        public void DisableAllPhysics()
        {
            foreach (TransformToJoint transformToJoint in transformToJoints)
            {
                transformToJoint.UsePhysics = false;
            }
        }

        [ContextMenu("EnableAllPhysics")]
        public void EnableAllPhysics()
        {
            foreach (TransformToJoint transformToJoint in transformToJoints)
            {
                transformToJoint.UsePhysics = true;
            }
        }

        void OnEnable()
        {
            GetPhysics();
            foreach (TransformToJoint transformToJoint in transformToJoints)
            {
                if (transformToJoint.targetJoint != null)
                {
                    transformToJoint.originalRotation = transformToJoint.sourceTransform.transform.localRotation;//Quaternion.Inverse(transformToJoint.sourceTransform.transform.localRotation);
                }
            }
            SetLOD(0, true);
        }


        // Update is called once per frame
        void LateUpdate()
        {
            if (!DoAnimate) return;
            foreach (TransformToJoint transformToJoint in transformToJoints)
            {
                if (transformToJoint.targetJoint != null && transformToJoint.UsePhysics && DoPhysics)
                {
                    transformToJoint.targetJoint.SetTargetRotationLocal(transformToJoint.sourceTransform.localRotation, transformToJoint.originalRotation);
                }
                else if (transformToJoint.sourceTransform && transformToJoint.targetTransform)
                {
                    transformToJoint.targetTransform.localRotation = Quaternion.Lerp(transformToJoint.targetTransform.localRotation, transformToJoint.sourceTransform.localRotation, Time.fixedDeltaTime * speed);
                    transformToJoint.targetTransform.localPosition = Vector3.Lerp(transformToJoint.targetTransform.localPosition, transformToJoint.sourceTransform.localPosition, Time.fixedDeltaTime * speed);
                }

            }
        }


        public void GetPhysics()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
        }

        public void SetLOD(int index, bool isVisible)
        {
            if (player)
                if (player.IsLocal) return;

            if (rigidbodies == null || colliders == null) return;
            if (isVisible)
            {
                if (index == 0)
                {

                    for (int i = 1; i < rigidbodies.Length; i++)
                    {
                        rigidbodies[i].velocity = Vector3.zero;
                        rigidbodies[i].isKinematic = false;
                    }
                    foreach (Collider collider in colliders)
                    {
                        collider.enabled = true;
                    }

                    DoAnimate = true;
                    DoPhysics = true;
                    if (player)
                        player.SetNetworkAnimatorActive(true);

                    return;
                }
                if (index == 1)
                {


                    foreach (Rigidbody rb in rigidbodies)
                    {
                        rb.velocity = Vector3.zero;
                        rb.isKinematic = true;
                        rb.Sleep();

                    }
                    foreach (Collider collider in colliders)
                    {
                        collider.enabled = false;
                    }

                    DoAnimate = true;
                    DoPhysics = false;
                    if (player)
                        player.SetNetworkAnimatorActive(true);

                    return;
                }
                if (index == 2)
                {


                    foreach (Rigidbody rb in rigidbodies)
                    {
                        rb.velocity = Vector3.zero;
                        rb.isKinematic = true;
                        rb.Sleep();

                    }
                    foreach (Collider collider in colliders)
                    {
                        collider.enabled = false;
                    }

                    DoAnimate = false;
                    DoPhysics = false;
                    if (player)
                        player.SetNetworkAnimatorActive(false);
                    return;
                }
            }
            else
            {
                foreach (Rigidbody rb in rigidbodies)
                {
                    rb.velocity = Vector3.zero;
                    rb.isKinematic = true;
                    rb.Sleep();

                }
                foreach (Collider collider in colliders)
                {
                    collider.enabled = false;
                }

                DoAnimate = false;
                DoPhysics = false;
                if (player)
                    player.SetNetworkAnimatorActive(false);
                return;
            }
        }
    }




    public static class ConfigurableJointExtensions
    {
        /// <summary>
        /// Sets a joint's targetRotation to match a given local rotation.
        /// The joint transform's local rotation must be cached on Start and passed into this method.
        /// </summary>
        public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
        {
            if (joint.configuredInWorldSpace)
            {
                Debug.LogError("SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
            }
            SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
        }

        /// <summary>
        /// Sets a joint's targetRotation to match a given world rotation.
        /// The joint transform's world rotation must be cached on Start and passed into this method.
        /// </summary>
        public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion startWorldRotation)
        {
            if (!joint.configuredInWorldSpace)
            {
                Debug.LogError("SetTargetRotation must be used with joints that are configured in world space. For local space joints, use SetTargetRotationLocal.", joint);
            }
            SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
        }

        static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
        {
            // Calculate the rotation expressed by the joint's axis and secondary axis
            var right = joint.axis;
            var forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
            var up = Vector3.Cross(forward, right).normalized;
            Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

            // Transform into world space
            Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

            // Counter-rotate and apply the new local rotation.
            // Joint space is the inverse of world space, so we need to invert our value
            if (space == Space.World)
            {
                resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
            }
            else
            {
                resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
            }

            // Transform back into joint space
            resultRotation *= worldToJointSpace;

            // Set target rotation to our newly calculated rotation
            joint.targetRotation = resultRotation;
        }

        /// <summary>
        /// Adjust ConfigurableJoint settings to closely match CharacterJoint behaviour
        /// </summary>
        public static void SetupAsCharacterJoint(this ConfigurableJoint joint)
        {
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            // joint.angularXMotion = ConfigurableJointMotion.Free;
            // joint.angularYMotion = ConfigurableJointMotion.Free;
            // joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;

            joint.rotationDriveMode = RotationDriveMode.Slerp;
            var slerpDrive = joint.slerpDrive;
            //slerpDrive.mode = JointDriveMode.Position;
            slerpDrive.maximumForce = Mathf.Infinity;
            joint.slerpDrive = slerpDrive;
        }
    }
}