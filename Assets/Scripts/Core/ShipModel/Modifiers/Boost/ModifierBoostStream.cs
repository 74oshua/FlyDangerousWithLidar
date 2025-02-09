using Core.Player;
using UnityEngine;
using UnityEngine.VFX;

namespace Core.ShipModel.Modifiers.Boost {
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(VisualEffect))]
    public class ModifierBoostStream : MonoBehaviour, IModifier {
        [SerializeField] private float shipForceAdd = 200000;
        [SerializeField] private float shipSpeedAdd = 15;
        [SerializeField] private float shipThrustAdd = 5000;

        [SerializeField] private float lengthMeters = 15000;
        [SerializeField] private float streamCapsuleEndCapRadius = 500;
        [SerializeField] private Transform terrainGenEndpointMarker;

        private CapsuleCollider _capsuleCollider;
        private VisualEffect _visualEffect;

        public float TrailLengthMeters {
            get => lengthMeters;
            set {
                var scale = transform.lossyScale.z;
                var capsuleCollider = GetComponent<CapsuleCollider>();
                var vfx = GetComponent<VisualEffect>();

                lengthMeters = value;
                var lengthLocal = lengthMeters / scale;

                capsuleCollider.height = lengthLocal;
                capsuleCollider.center = new Vector3(0, 0, lengthLocal / 2 - capsuleCollider.radius);
                vfx.SetFloat("_streamLength", lengthLocal);
                terrainGenEndpointMarker.localPosition = new Vector3(0, 0, lengthLocal);
            }
        }

        public void ApplyModifierEffect(Rigidbody shipRigidBody, ref AppliedEffects effects) {
            var streamTransform = transform;

            // apply force along the funnel with force linear equivalent to the distance from the source
            var streamPosition = streamTransform.position;
            var shipPosition = shipRigidBody.transform.position;
            var distance = streamPosition - shipPosition;
            var effectOverDistanceNormalised = 1 - distance.magnitude / (lengthMeters - streamCapsuleEndCapRadius);

            var parameters = shipRigidBody.gameObject.GetComponent<ShipPlayer>().ShipPhysics.FlightParameters;
            if (parameters.use_old_boost)
            {
                effects.shipForce += Vector3.Lerp(Vector3.zero, streamTransform.forward * shipForceAdd, effectOverDistanceNormalised);

                // apply additional thrust and max speed if the ship vector is facing the correct direction
                if (Vector3.Dot(transform.forward, shipRigidBody.velocity) > 0) {
                    effects.shipDeltaSpeedCap += Mathf.Lerp(0, shipSpeedAdd, effectOverDistanceNormalised);
                    effects.shipDeltaThrust += Mathf.Lerp(0, shipThrustAdd, effectOverDistanceNormalised);
                }
            }
            else
            {
                // This whole mess of normalisation factors should probably be replaced with a ship parameter called speedAddFactor or something
                // for now it serves to ensure that the ships revector ability stays consistent for 
                var thrustAddFactor = ShipParameters.Defaults.maxThrust / parameters.maxThrust * ShipParameters.Defaults.thrustBoostMultiplier / parameters.thrustBoostMultiplier;
                var massNorm = parameters.mass / ShipParameters.Defaults.mass;

                effects.shipForce += Vector3.Lerp(Vector3.zero, massNorm * shipForceAdd * streamTransform.forward, effectOverDistanceNormalised);

                // apply additional thrust and max speed if the ship vector is facing the correct direction
                if (Vector3.Dot(transform.forward, shipRigidBody.velocity) > 0)
                {
                    effects.shipDeltaSpeedCap += Mathf.Lerp(0, shipSpeedAdd, effectOverDistanceNormalised);
                    effects.shipDeltaThrust += Mathf.Lerp(0, thrustAddFactor * massNorm * shipThrustAdd, effectOverDistanceNormalised);
                }
            }
        }

        public void ApplyInitialEffect(Rigidbody ship, ref AppliedEffects effects) 
        {}

        // for some reason the unity editor insists on resetting the vfx which makes building levels a PITA
        public void OnGUI() {
            TrailLengthMeters = lengthMeters;
        }
    }
}
